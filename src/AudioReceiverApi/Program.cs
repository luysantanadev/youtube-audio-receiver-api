using System.Security.Claims;
using System.Text.RegularExpressions;
using dotenv.net;
using AudioReceiverApi;
using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Authorization;
using Keycloak.AuthServices.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minio;
using Raven.Client.Documents;
using Serilog;

DotEnv.Load();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter())
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, cfg) =>
    cfg.ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter())
);

builder.Configuration.AddEnvironmentVariables();
builder.Services.AddOpenApi();
builder.Services.AddSerilog();

builder.Services.AddMinio(client =>
    client
        .WithEndpoint(builder.Configuration["MINIO_HOST"])
        .WithCredentials(builder.Configuration["MINIO_ACCESS_KEY"], builder.Configuration["MINIO_SECRET_KEY"])
        .WithSSL(false)
        .Build()
);

builder.Services.AddSingleton<IDocumentStore>(opt => new DocumentStore()
{
    Urls = [builder.Configuration["RAVENDB_HOST"]],
    Database = builder.Configuration["RAVENDB_DATABASE"]
}.Initialize());

builder.Services.AddKeycloakWebApiAuthentication(opt =>
{
    opt.AuthServerUrl = builder.Configuration["KEYCLOAK_SERVER"];
    opt.Realm = builder.Configuration["KEYCLOAK_REALM"]!;
    opt.Resource = builder.Configuration["KEYCLOAK_RESOURCE"]!;
    opt.Credentials = new KeycloakClientInstallationCredentials()
    {
        Secret = builder.Configuration["KEYCLOAK_CREDENTIALS_SECRET"]!,
    };
    opt.SslRequired = builder.Configuration["KEYCLOAK_REQUIRE_SSL"]!;
});

builder.Services.AddAuthorization().AddKeycloakAuthorization();

builder.Services.AddScoped<IAudioProcessor, AudioProcessor>();
builder.Services.AddScoped<IMinioUploader, MinioUploader>();
builder.Services.AddScoped<ITranscriber, WhisperTranscriber>();
builder.Services.AddScoped<IAudioRepository, AudioRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/upload-audio", [Authorize, IgnoreAntiforgeryToken] async (
    IFormFile file,
    ClaimsPrincipal user,
    IAudioProcessor audioProcessor,
    IMinioUploader uploader,
    ITranscriber transcriber,
    IAudioRepository repo
) =>
{
    if (file is null || file.Length == 0) return Results.BadRequest("Arquivo inválido");

    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;

    var streamWav = await audioProcessor.WavConverterAsync(file);
    var (pathMinio, etag) = await uploader.SaveAsync(file, userId);
    var (withoutTimestamp, withTimestamp) = await transcriber.ProcessAsync(streamWav);

    var audio = new Audio
    {
        SourceFileName = Regex.Replace(file.FileName, "\\s", "-").ToLower().Trim(),
        SourceFileContentType = file.ContentType.Trim(),
        MinioFilePath = pathMinio.Trim(),
        MinioEtag = etag.Trim().Replace("\"",""),
        UserId = userId.Trim(),
        TranscriptionWithoutTimestamp = withoutTimestamp.Trim(),
        TrascriptionWithTimestamp = withTimestamp.Trim(),
        CreatedAt = DateTimeOffset.Now
    };

    await repo.SaveAsync(audio);

    return Results.Ok(audio);
}).DisableAntiforgery();


app.Run();