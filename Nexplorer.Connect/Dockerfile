FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS runtime
WORKDIR /app
COPY --from=nexplorer_build:latest /app/out/Nexplorer.Connect ./
ENTRYPOINT ["dotnet", "Nexplorer.Connect.dll"]