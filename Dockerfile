FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
COPY . /app/
WORKDIR /app
RUN dotnet restore \
	&& dotnet build src/Eurofurence.App.Server.Web/Eurofurence.App.Server.Web.csproj --configuration Release \
	&& dotnet publish src/Eurofurence.App.Tools.CliToolBox/Eurofurence.App.Tools.CliToolBox.csproj --output "/app/artifacts" --configuration Release \
	&& dotnet publish src/Eurofurence.App.Server.Web/Eurofurence.App.Server.Web.csproj --output "/app/artifacts" --configuration Release
ENTRYPOINT dotnet artifacts/Eurofurence.App.Server.Web.dll http://*:30001
EXPOSE 30001

FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-chiseled
COPY --from=build /app/artifacts/* /app/
WORKDIR /app
ENTRYPOINT ["dotnet", "Eurofurence.App.Server.Web.dll", "http://*:30001"]
EXPOSE 30001
