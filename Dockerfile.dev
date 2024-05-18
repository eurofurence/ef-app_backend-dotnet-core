FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
COPY ./src /app/src
COPY ./test /app/test
COPY ./ef-app_backend-dotnet-core.sln /app
COPY ./appsettings.json /app
COPY ./firebase.json /app
COPY ./NuGet.config /app
WORKDIR /app
RUN ["dotnet", "restore"]
RUN dotnet build src/Eurofurence.App.Server.Web/Eurofurence.App.Server.Web.csproj --configuration Release 
RUN dotnet publish src/Eurofurence.App.Tools.CliToolBox/Eurofurence.App.Tools.CliToolBox.csproj --output "$(pwd)/artifacts" --configuration Release
RUN dotnet publish src/Eurofurence.App.Server.Web/Eurofurence.App.Server.Web.csproj --output "$(pwd)/artifacts" --configuration Release
ENTRYPOINT dotnet artifacts/Eurofurence.App.Server.Web.dll http://*:30001
EXPOSE 30001
