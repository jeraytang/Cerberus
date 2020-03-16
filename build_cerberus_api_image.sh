#!/usr/bin/env bash
rm -rf output
dotnet clean
docker build -f ./dockerfile/Cerberus.API -t cerberus-api .