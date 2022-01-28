#!/bin/bash

cd ./ConcurrentCaching

dotnet add package Microsoft.Extensions.Caching.Memory
dotnet add package Microsoft.SourceLink.GitHub

cd ../ConcurrentCaching.Test

dotnet add package Microsoft.NET.Test.Sdk
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add package coverlet.collector
