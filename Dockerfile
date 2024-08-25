FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG MYSQL_VERSION=10.11.8-MariaDB
WORKDIR /app
ENV NUGET_PACKAGES=/dotnet/packages

# Allow results of `dotnet restore` to be cached if there are no changes to dependencies.
COPY ./NuGet.config /app/NuGet.config
RUN --mount=type=cache,target=/dotnet/packages \
    --mount=type=cache,target=/dotnet/global-packages \
    --mount=type=bind,source=./ef-app_backend-dotnet-core.sln,target=/app/ef-app_backend-dotnet-core.sln \
    --mount=type=bind,source=./src/Eurofurence.App.Common/Eurofurence.App.Common.csproj,target=/app/src/Eurofurence.App.Common/Eurofurence.App.Common.csproj \
    --mount=type=bind,source=./src/Eurofurence.App.Tools.CliToolBox/Eurofurence.App.Tools.CliToolBox.csproj,target=/app/src/Eurofurence.App.Tools.CliToolBox/Eurofurence.App.Tools.CliToolBox.csproj \
    --mount=type=bind,source=./src/Eurofurence.App.Server.Services/Eurofurence.App.Server.Services.csproj,target=/app/src/Eurofurence.App.Server.Services/Eurofurence.App.Server.Services.csproj \
    --mount=type=bind,source=./src/Eurofurence.App.Server.Web/Eurofurence.App.Server.Web.csproj,target=/app/src/Eurofurence.App.Server.Web/Eurofurence.App.Server.Web.csproj \
    --mount=type=bind,source=./src/Eurofurence.App.Backoffice/Eurofurence.App.Backoffice.csproj,target=/app/src/Eurofurence.App.Backoffice/Eurofurence.App.Backoffice.csproj \
    --mount=type=bind,source=./src/Eurofurence.App.Infrastructure.EntityFramework/Eurofurence.App.Infrastructure.EntityFramework.csproj,target=/app/src/Eurofurence.App.Infrastructure.EntityFramework/Eurofurence.App.Infrastructure.EntityFramework.csproj \
    --mount=type=bind,source=./src/Eurofurence.App.Domain.Model/Eurofurence.App.Domain.Model.csproj,target=/app/src/Eurofurence.App.Domain.Model/Eurofurence.App.Domain.Model.csproj \
    --mount=type=bind,source=./test/Eurofurence.App.Server.Services.Tests/Eurofurence.App.Server.Services.Tests.csproj,target=/app/test/Eurofurence.App.Server.Services.Tests/Eurofurence.App.Server.Services.Tests.csproj \
    --mount=type=bind,source=./test/Eurofurence.App.Server.Web.Tests/Eurofurence.App.Server.Web.Tests.csproj,target=/app/test/Eurofurence.App.Server.Web.Tests/Eurofurence.App.Server.Web.Tests.csproj \
    --mount=type=bind,source=./test/Eurofurence.App.Tests.Common/Eurofurence.App.Tests.Common.csproj,target=/app/test/Eurofurence.App.Tests.Common/Eurofurence.App.Tests.Common.csproj \
    dotnet nuget config set repositoryPath /dotnet/packages --configfile /app/NuGet.config \
    && dotnet nuget config set globalPackagesFolder /dotnet/global-packages --configfile /app/NuGet.config \
    && dotnet restore

COPY ./src/ /app/src/
COPY ./test/ /app/test/
RUN --mount=type=cache,target=/dotnet/packages \
    --mount=type=cache,target=/dotnet/global-packages \
    --mount=type=cache,target=/dotnet/artifacts \
    --mount=type=bind,source=./ef-app_backend-dotnet-core.sln,target=/app/ef-app_backend-dotnet-core.sln \
    dotnet build src/Eurofurence.App.Server.Web/Eurofurence.App.Server.Web.csproj --artifacts-path /dotnet/artifacts --configuration Release \
    && dotnet publish src/Eurofurence.App.Server.Web/Eurofurence.App.Server.Web.csproj --artifacts-path /dotnet/artifacts --no-build --output "/app/artifacts" --configuration Release \
    && dotnet tool install --global dotnet-ef \
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
