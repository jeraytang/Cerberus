#!/usr/bin/env bash
rm -rf output
cd src/IdentityServer4.SecurityTokenService && yarn install && cd ../..
dotnet clean
docker build -f ./dockerfile/STS -t identityserver-sts .
