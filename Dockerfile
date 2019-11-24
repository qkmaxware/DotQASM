# --------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /app

# Copy over csproj and sln
COPY OpenQASM/*.csproj ./OpenQASM/
COPY OpenQASM.Tests/*.csproj ./OpenQASM.Tests/
COPY OpenQASM.Tools/*.csproj ./OpenQASM.Tools/
COPY OpenQASM.sln OpenQASM.sln
RUN dotnet restore

# Copy over everything else
COPY . .

# Build
RUN dotnet publish --self-contained --runtime linux-x64 -c Release -o out OpenQASM.Tools

# --------------------------------------------------------------------------------
FROM ubuntu:latest AS runtime

COPY out/Release/* /usr/local/bin*

ENTRYPOINT /bin/bash