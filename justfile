default:
  just --list

# Create file with given name and extension from $NAME.example.$EXTENSION
_create_from_example NAME EXTENSION:
	if [ ! -f "{{NAME}}.{{EXTENSION}}" ]; then cp "{{NAME}}.example.{{EXTENSION}}" "{{NAME}}.{{EXTENSION}}"; fi

# Start the docker compose stack
up: (_create_from_example "appsettings" "json") (_create_from_example "firebase" "json")
	docker-compose up

# Bring the docker compose stack down
down:
	docker-compose down

# Clean build and artifact
clean:
	dotnet clean
	rm -rf artifacts

# Perform a clean, build & publish with dotnet and docker-compose
build: clean
	dotnet restore
	dotnet build src/Eurofurence.App.Server.Web/Eurofurence.App.Server.Web.csproj --configuration Release 
	dotnet publish src/Eurofurence.App.Tools.CliToolBox/Eurofurence.App.Tools.CliToolBox.csproj --output "$(pwd)/artifacts" --configuration Release
	dotnet publish src/Eurofurence.App.Server.Web/Eurofurence.App.Server.Web.csproj --output "$(pwd)/artifacts" --configuration Release
	docker-compose build