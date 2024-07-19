FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG MYSQL_VERSION=10.11.8-MariaDB
COPY ./src/ /app/src/
COPY ./test/ /app/test/
COPY ./ef-app_backend-dotnet-core.sln /app/
COPY ./NuGet.config /app/
WORKDIR /app
RUN dotnet restore \
	&& dotnet build src/Eurofurence.App.Server.Web/Eurofurence.App.Server.Web.csproj --configuration Release \
	&& dotnet publish src/Eurofurence.App.Tools.CliToolBox/Eurofurence.App.Tools.CliToolBox.csproj --output "/app/artifacts" --configuration Release \
	&& dotnet publish src/Eurofurence.App.Server.Web/Eurofurence.App.Server.Web.csproj --output "/app/artifacts" --configuration Release
RUN dotnet tool install --global dotnet-ef \
	&& export PATH="$PATH:/root/.dotnet/tools" \
	&& dotnet ef migrations bundle -o "/app/artifacts/db-migration-bundle" -p src/Eurofurence.App.Server.Web
ENTRYPOINT dotnet artifacts/Eurofurence.App.Server.Web.dll http://*:30001
EXPOSE 30001

FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-chiseled AS prod
COPY --from=build /app/artifacts/* /app/
COPY /src/Eurofurence.App.Server.Web/wwwroot /app/wwwroot
WORKDIR /app
ENTRYPOINT ["dotnet", "Eurofurence.App.Server.Web.dll", "http://*:30001"]
EXPOSE 30001
