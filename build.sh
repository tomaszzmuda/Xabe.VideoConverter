#!/bin/bash
dotnet restore
dotnet test Xabe.VideoConverter.Test/
if [[ -z "${TRAVIS_TAG}" ]]; then 
	exit 0
else
	cd Xabe.VideoConverter
	dotnet clean -c Release
	dotnet publish -c Release /property:Version=$TRAVIS_TAG -o VideoConverter
	zip -r VideoConverter$TRAVIS_TAG.zip VideoConverter/*
	sshpass -p $SSH_PASSWORD scp VideoConverter$TRAVIS_TAG.zip $SSH_USER@$SSH_HOST:~/VideoConverter/VideoConverter$TRAVIS_TAG.zip
fi
