FROM microsoft/dotnet:1.1-sdk
COPY . /app
WORKDIR /app
RUN ["dotnet", "restore"]
RUN cd src/Eurofurence.App.Server.KestrelHost \
  && dotnet build 
ENTRYPOINT cd src/Eurofurence.App.Server.KestrelHost \
  &&  dotnet run --server-urls http://*:30001
EXPOSE 30001
