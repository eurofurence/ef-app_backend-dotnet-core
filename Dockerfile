FROM alpine:latest
ENV DOTNET_ROOT=/opt/dotnet-sdk
ENV DOTNET_DOWNLOAD_DST=/tmp/dotnet-sdk.tar.gz
ENV DOTNET_DOWNLOAD_SRC=https://download.visualstudio.microsoft.com/download/pr/f8834fef-d2ab-4cf6-abc3-d8d79cfcde11/0ee05ef4af5fe324ce2977021bf9f340/dotnet-sdk-3.1.426-linux-musl-x64.tar.gz
ENV PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools
RUN apk add bash icu-libs krb5-libs libgcc libintl libssl1.1 libstdc++ zlib libgdiplus &&\
  wget "${DOTNET_DOWNLOAD_SRC}" -O $DOTNET_DOWNLOAD_DST &&\
  mkdir -p $DOTNET_ROOT &&\
  tar -zxf $DOTNET_DOWNLOAD_DST -C $DOTNET_ROOT &&\
  rm $DOTNET_DOWNLOAD_DST
COPY ./src /app/src
COPY ./test /app/test
COPY ./ef-app_backend-dotnet-core.sln /app
COPY ./NuGet.config /app
WORKDIR /app
RUN ["dotnet", "restore"]
RUN dotnet build src/Eurofurence.App.Server.Web/Eurofurence.App.Server.Web.csproj --configuration Release 
RUN dotnet publish src/Eurofurence.App.Tools.CliToolBox/Eurofurence.App.Tools.CliToolBox.csproj --output "$(pwd)/artifacts" --configuration Release
RUN dotnet publish src/Eurofurence.App.Server.Web/Eurofurence.App.Server.Web.csproj --output "$(pwd)/artifacts" --configuration Release --framework netcoreapp3.1
ENTRYPOINT dotnet artifacts/Eurofurence.App.Server.Web.dll http://*:30001
EXPOSE 30001
