FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /app

COPY /Nexplorer.Connect/ /app/Nexplorer.Connect/
COPY /Nexplorer.Core/ /app/Nexplorer.Core/
COPY /Nexplorer.Data/ /app/Nexplorer.Data/
COPY /Nexplorer.Nexus/ /app/Nexplorer.Nexus/
COPY /Nexplorer.Node/ /app/Nexplorer.Node/
COPY /Nexplorer.Web/ /app/Nexplorer.Web/

RUN dotnet publish ./Nexplorer.Node/ -c Release -o out/Nexplorer.Node
RUN dotnet publish ./Nexplorer.Connect/ -c Release -o out/Nexplorer.Connect
RUN dotnet publish ./Nexplorer.Web/ -c Release -o out/Nexplorer.Web
