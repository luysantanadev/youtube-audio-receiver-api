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