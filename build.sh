#!/bin/bash
dotnet restore
dotnet test Xabe.VideoConverter.Test/
if [[ -z "${TRAVIS_TAG}" ]]; then 
	exit 0
else
	cd Xabe.VideoConverter
	dotnet clean -c Release
	dotnet publish -c Release /property:Version=$TRAVIS_TAG -o Xabe.VideoConverter
	cp settings.json Xabe.VideoConverter/
	zip -r Xabe.VideoConverter.zip Xabe.VideoConverter/*
fi
