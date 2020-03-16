#!/usr/bin/env bash
rm -rf output
cd src/IdentityServer4.Admin && yarn install && cd ../..
dotnet clean
docker build -f ./dockerfile/Admin -t identityserver-admin .