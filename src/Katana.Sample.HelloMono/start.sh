#!/bin/sh

echo "** Building Katana.Sample.HelloMono.csproj"

xbuild

echo "** Executing: mono bin/Katana.exe --port 9001 --boot aspnet"
echo "** You should browse to http://localhost:9001/ now"

mono bin/Katana.exe --port 9001 --boot aspnet
