FROM mcr.microsoft.com/dotnet/sdk:6.0
WORKDIR /source

COPY ./* .
WORKDIR /source/NetCore
RUN dotnet publish -c release -o /app

FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /app
RUN dotnet DOLServer.dll
