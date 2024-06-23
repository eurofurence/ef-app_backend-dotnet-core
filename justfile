# List available recipes
default:
  just --list

# Create file with given name and extension from NAME.sample.EXTENSION in PROJECT
_create_from_sample PROJECT NAME EXTENSION:
	if [ ! -f "{{NAME}}.{{EXTENSION}}" ]; then cp "src/{{PROJECT}}/{{NAME}}.sample.{{EXTENSION}}" "{{NAME}}.{{EXTENSION}}"; fi

# Start the docker compose stack
up *ARGS: (_create_from_sample "Eurofurence.App.Server.Web" "appsettings" "json") (_create_from_sample "Eurofurence.App.Server.Web" "firebase" "json") 
	docker-compose up {{ARGS}}

# Bring the docker compose stack down and remove volumes
down:
	docker-compose down -v --remove-orphans

# Stop the docker compose stack
stop:
	docker-compose stop

# Clean build, stack, container images and artifacts
clean:
	dotnet clean
	rm -rf artifacts build
	find . -type d -name "bin" -print0 | xargs -0 rm -rf
	find . -type d -name "obj" -print0 | xargs -0 rm -rf
	docker-compose down || true
	docker rmi ghcr.io/eurofurence/ef-app_backend-dotnet-core:dev-sdk || true
	docker rmi ghcr.io/eurofurence/ef-app_backend-dotnet-core:dev || true

# Perform restore, build & publish with dotnet
build:
	dotnet restore
	dotnet build src/Eurofurence.App.Server.Web/Eurofurence.App.Server.Web.csproj --configuration Release
	dotnet publish src/Eurofurence.App.Tools.CliToolBox/Eurofurence.App.Tools.CliToolBox.csproj --output "$(pwd)/artifacts" --configuration Release
	dotnet publish src/Eurofurence.App.Server.Web/Eurofurence.App.Server.Web.csproj --output "$(pwd)/artifacts" --configuration Release

# Build just CLI tools as single, self-contained executable
build-cli:
	dotnet publish src/Eurofurence.App.Tools.CliToolBox/Eurofurence.App.Tools.CliToolBox.csproj --output "$(pwd)/artifacts" --configuration Release --sc -p:PublishSingleFile=true -p:DebugType=None -p:DebugSymbols=false -p:GenerateDocumentationFile=false

# Build release container using spec from docker-compose.yml
containerize:
	docker-compose build

# Build sdk container without executing second stage
containerize-dev:
	docker build --target build -t ghcr.io/eurofurence/ef-app_backend-dotnet-core:dev-sdk -f Dockerfile .

# Open swagger UI in default browser
swagger:
	open http://127.0.0.1:30001/swagger/ui/index.html
