# 🎙️ Minimal API .NET 9 para Transcrição de Áudio com Whisper, MinIO, RavenDB e Keycloak

Este projeto é uma Minimal API desenvolvida em **.NET 9**, focada em **receber áudios, convertê-los para `.wav` com FFMpeg, transcrevê-los com IA (Whisper), armazenar os arquivos no MinIO e salvar os dados estruturados no RavenDB** — tudo isso com autenticação segura via **Keycloak**.

> ⚡ Ideal para: plataformas educacionais, análise de feedbacks, automação de entrevistas, sistemas de legendas, entre outros.

---

## 📽️ Vídeo no YouTube

▶️ *Assista aqui como tudo funciona passo a passo*  
_(link em breve!)_

---

## 🚀 Funcionalidades

- ✅ Upload de arquivos `.mp3`, `.m4a`, `.ogg`, `.wav`, etc.
- 🔄 Conversão para `.wav` com FFMpeg
- 🧠 Transcrição automática com [Whisper (OpenAI)](https://huggingface.co/ggerganov/whisper.cpp/tree/main)
- ☁️ Upload no MinIO com caminho organizado por usuário
- 🗃️ Salvamento dos dados de transcrição no RavenDB
- 🔐 Autenticação e autorização com Keycloak
- 📊 Logging estruturado com Serilog (formato JSON, ideal para containers)

## 🧪 Tecnologias & Pacotes

| Tecnologia    | Uso Principal                         |
|---------------|----------------------------------------|
| [.NET 9 Minimal API](https://learn.microsoft.com/aspnet/core) | API leve e moderna |
| `dotenv.net`  | Leitura de variáveis de ambiente via `.env` |
| `FFMpegCore`  | Conversão de áudio para `.wav` |
| `Whisper.net` + `Runtime.Cuda` | Transcrição de áudio com suporte a GPU |
| `MinIO`       | Armazenamento de objetos estilo S3 |
| `RavenDB`     | Banco de dados NoSQL |
| `Keycloak.AuthServices` | Autenticação OAuth2/JWT |
| `Serilog` + Enrichers | Logging estruturado e pronto para produção |

---

## 🐳 Infraestrutura local com Docker

Suba os serviços necessários com os seguintes comandos:

```sh
# RavenDB
docker run -d --name yt-ravendb \
  -p 8091:8080 \
  -v ~/docker-data/yt-ravendb/data:/var/lib/ravendb/data \
  -e RAVEN_ARGS='--log-to-console' \
  ravendb/ravendb

# MinIO
docker run -d --name yt-minio \
  -p 8092:9000 -p 8093:9001 \
  -v ~/docker-data/yt-minio:/data \
  -e "MINIO_ROOT_USER=admin" -e "MINIO_ROOT_PASSWORD=admin_password" \
  quay.io/minio/minio server /data --console-address ":9001"

# Keycloak
docker run -d --name yt-keycloak \
 -p 8094:8080 \
 -e KC_BOOTSTRAP_ADMIN_USERNAME=admin -e KC_BOOTSTRAP_ADMIN_PASSWORD=admin \
 quay.io/keycloak/keycloak:26.2.5 start-dev
```

## 📌 Observações Importantes

- Para transcrição via Whisper, o modelo é salvo localmente no caminho definido em GGML_PATH.
- É necessário que o container tenha suporte a CUDA se for usar aceleração por GPU (Whisper.net.Runtime.Cuda).
- As credenciais de Keycloak, MinIO e RavenDB são carregadas via .env ou variáveis de ambiente.

## 💡 Próximas melhorias (sugestões)

- 📁 Exportação dos resultados da transcrição em .txt ou .srt
- 🧪 Testes automatizados para cada serviço
- 📈 Exposição de métricas com OpenTelemetry
- 🐳 Dockerfile e docker-compose.yml para rodar tudo com um comando
