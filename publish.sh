#!/usr/bin/env bash
tag=$(date +%Y%m%d)
echo build cerberus api
sh build_cerberus_api_image.sh
docker tag cerberus-api zlzforever/cerberus-api:$tag
docker tag cerberus-api zlzforever/cerberus-api:latest
docker tag cerberus-api registry-docker.pamirs.com/cerberus-api:$tag
docker tag cerberus-api registry-docker.pamirs.com/cerberus-api:latest
docker push zlzforever/cerberus-api:$tag
docker push zlzforever/cerberus-api:latest
docker push registry-docker.pamirs.com/cerberus-api:$tag
docker push registry-docker.pamirs.com/cerberus-api:latest

echo build cerberus
sh build_cerberus_image.sh
docker tag cerberus zlzforever/cerberus:$tag
docker tag cerberus zlzforever/cerberus:latest
docker tag cerberus registry-docker.pamirs.com/cerberus:$tag
docker tag cerberus registry-docker.pamirs.com/cerberus:latest
docker push zlzforever/cerberus:$tag
docker push zlzforever/cerberus:latest
docker push registry-docker.pamirs.com/cerberus:$tag
docker push registry-docker.pamirs.com/cerberus:latest

echo build identityserver sts
sh build_sts_image.sh
docker tag identityserver-sts zlzforever/identityserver-sts:$tag
docker tag identityserver-sts zlzforever/identityserver-sts:latest
docker tag identityserver-sts registry-docker.pamirs.com/identityserver-sts:$tag
docker tag identityserver-sts registry-docker.pamirs.com/identityserver-sts:latest
docker push zlzforever/identityserver-sts:$tag
docker push zlzforever/identityserver-sts:latest
docker push registry-docker.pamirs.com/identityserver-sts:$tag
docker push registry-docker.pamirs.com/identityserver-sts:latest

echo build identityserver admin
sh build_admin_image.sh
docker tag identityserver-admin zlzforever/identityserver-admin:$tag
docker tag identityserver-admin zlzforever/identityserver-admin:latest
docker tag identityserver-admin registry-docker.pamirs.com/identityserver-admin:$tag
docker tag identityserver-admin registry-docker.pamirs.com/identityserver-admin:latest
docker push zlzforever/identityserver-admin:$tag
docker push zlzforever/identityserver-admin:latest
docker push registry-docker.pamirs.com/identityserver-admin:$tag
docker push registry-docker.pamirs.com/identityserver-admin:latest