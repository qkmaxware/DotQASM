# --------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /app

# Copy over csproj and sln
COPY OpenQASM/*.csproj ./OpenQASM/
COPY OpenQASM.Tests/*.csproj ./OpenQASM.Tests/
COPY OpenQASM.Tools/*.csproj ./OpenQASM.Tools/
COPY OpenQASM.Desktop/*.csproj ./OpenQASM.Desktop/
COPY OpenQASM.sln OpenQASM.sln
RUN dotnet restore

# Copy over everything else
COPY . .

# Build
RUN dotnet publish --self-contained --runtime linux-x64 -c Release -o out OpenQASM.Tools

# --------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/core/runtime:3.0 AS runtime

COPY --from=build /app/out/ /usr/local/bin/qasm
ENV PATH="/usr/local/bin/qasm:${PATH}"

ENTRYPOINT /bin/bash