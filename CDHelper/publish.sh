#!/bin/sh

echo "Publish START"

rm -rf "$PWD/bin/publish" -f -r -v

dotnet publish --self-contained -c Release -r linux-arm -o ./bin/publish/linux-arm64
dotnet publish --self-contained -c Release -r linux-x64 -o ./bin/publish/linux-x64
dotnet publish --self-contained -c Release -r win-x64 -o ./bin/publish/win-x64
dotnet publish --self-contained -c Release -r osx-x64 -o ./bin/publish/osx-x64

zip -r -j ./bin/publish/CDHelper-linux-arm64.zip ./bin/publish/linux-arm64/*
zip -r -j ./bin/publish/CDHelper-linux-x64.zip ./bin/publish/linux-x64/*
zip -r -j ./bin/publish/CDHelper-win-x64.zip ./bin/publish/win-x64/*
zip -r -j ./bin/publish/CDHelper-osx-x64.zip ./bin/publish/osx-x64/*

echo "Publish DONE"