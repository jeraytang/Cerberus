#!/usr/bin/env bash
rm -rf output
RUN cd src/Cerberus && yarn install && cd ../..
dotnet clean
docker build -f ./dockerfile/Cerberus -t cerberus .