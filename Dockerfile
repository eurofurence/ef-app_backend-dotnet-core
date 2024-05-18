FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-chiseled
WORKDIR /app
COPY ./artifacts/** /app/
ENTRYPOINT ["dotnet", "Eurofurence.App.Server.Web.dll", "http://*:30001"]
EXPOSE 30001
