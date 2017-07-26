#!/bin/bash
dotnet restore
dotnet test Xabe.VideoConverter.Test/
if [[ -z "${TRAVIS_TAG}" ]]; then 
	exit 0
else

fi
