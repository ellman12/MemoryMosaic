#!/bin/bash
# https://stackoverflow.com/a/65170521

if [ "$#" -ne 1 ]; then
    echo "Usage: build outputDirectory"
    exit 1
fi

projects=("MemoryMosaic" "Initialization")

for project in "${projects[@]}"
do
    dotnet publish -c Release -r win10-x64 -p:PublishReadyToRun=true -p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=None -p:DebugSymbols=false --version-suffix "3.0.0" -o "$1/$project" $project
    echo
done
