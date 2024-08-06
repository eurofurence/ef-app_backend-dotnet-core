FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG MYSQL_VERSION=10.11.8-MariaDB
WORKDIR /app

# Allow results of `dotnet restore` to be cached if there are no changes to dependencies.
COPY ./NuGet.config /app/
COPY ./ef-app_backend-dotnet-core.sln /app/
COPY  ./src/Eurofurence.App.Common/Eurofurence.App.Common.csproj /app/src/Eurofurence.App.Common/Eurofurence.App.Common.csproj
COPY  ./src/Eurofurence.App.Tools.CliToolBox/Eurofurence.App.Tools.CliToolBox.csproj /app/src/Eurofurence.App.Tools.CliToolBox/Eurofurence.App.Tools.CliToolBox.csproj
COPY  ./src/Eurofurence.App.Server.Services/Eurofurence.App.Server.Services.csproj /app/src/Eurofurence.App.Server.Services/Eurofurence.App.Server.Services.csproj
COPY  ./src/Eurofurence.App.Server.Web/Eurofurence.App.Server.Web.csproj /app/src/Eurofurence.App.Server.Web/Eurofurence.App.Server.Web.csproj
COPY  ./src/Eurofurence.App.Backoffice/Eurofurence.App.Backoffice.csproj /app/src/Eurofurence.App.Backoffice/Eurofurence.App.Backoffice.csproj
COPY  ./src/Eurofurence.App.Infrastructure.EntityFramework/Eurofurence.App.Infrastructure.EntityFramework.csproj /app/src/Eurofurence.App.Infrastructure.EntityFramework/Eurofurence.App.Infrastructure.EntityFramework.csproj
COPY  ./src/Eurofurence.App.Domain.Model/Eurofurence.App.Domain.Model.csproj /app/src/Eurofurence.App.Domain.Model/Eurofurence.App.Domain.Model.csproj
COPY ./test/Eurofurence.App.Server.Services.Tests/Eurofurence.App.Server.Services.Tests.csproj /app/test/Eurofurence.App.Server.Services.Tests/Eurofurence.App.Server.Services.Tests.csproj
COPY ./test/Eurofurence.App.Server.Web.Tests/Eurofurence.App.Server.Web.Tests.csproj /app/test/Eurofurence.App.Server.Web.Tests/Eurofurence.App.Server.Web.Tests.csproj
COPY ./test/Eurofurence.App.Tests.Common/Eurofurence.App.Tests.Common.csproj /app/test/Eurofurence.App.Tests.Common/Eurofurence.App.Tests.Common.csproj
RUN dotnet restore

COPY ./src/ /app/src/
COPY ./test/ /app/test/
RUN dotnet build src/Eurofurence.App.Server.Web/Eurofurence.App.Server.Web.csproj --configuration Release \
	&& dotnet publish src/Eurofurence.App.Tools.CliToolBox/Eurofurence.App.Tools.CliToolBox.csproj --output "/app/artifacts" --configuration Release \
	&& dotnet publish src/Eurofurence.App.Server.Web/Eurofurence.App.Server.Web.csproj --output "/app/artifacts" --configuration Release
RUN dotnet tool install --global dotnet-ef \
	&& export PATH="$PATH:/root/.dotnet/tools" \
	&& export ASPNETCORE_ENVIRONMENT="sample" \
	&& dotnet ef migrations bundle -o "/app/artifacts/db-migration-bundle" -p src/Eurofurence.App.Infrastructure.EntityFramework
ENTRYPOINT dotnet artifacts/Eurofurence.App.Server.Web.dll http://*:30001
EXPOSE 30001

FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-chiseled AS prod
COPY --from=build /app/artifacts/* /app/
COPY /src/Eurofurence.App.Server.Web/wwwroot /app/wwwroot
WORKDIR /app
ENTRYPOINT ["dotnet", "Eurofurence.App.Server.Web.dll", "http://*:30001"]
EXPOSE 30001
