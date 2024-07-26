set dotenv-load := true

# List available recipes
default:
  just --list

# Create file with given name and extension from NAME.sample.EXTENSION in PROJECT
@_create_from_sample PROJECT NAME EXTENSION TARGET:
	if [ ! -f "{{TARGET}}.{{EXTENSION}}" ]; then echo "Creating {{TARGET}}.{{EXTENSION}} from sample…"; cp "src/{{PROJECT}}/{{NAME}}.sample.{{EXTENSION}}" "{{TARGET}}.{{EXTENSION}}"; else echo "Skipping {{TARGET}}.{{EXTENSION}} because it already exists."; fi

# Open a url (or file) and print it to stdout
@_open_url TITLE TARGET_URL:
	echo "Opening {{TITLE}} at {{TARGET_URL}} …"
	open {{TARGET_URL}}

# create an empty file if it doesn't exist
@_create_if_not_exists FILE:
	if [[ ! -e {{FILE}} ]]; then touch {{FILE}}; fi

# Initialize configuration files in root folder
init: (_create_from_sample "Eurofurence.App.Server.Web" "appsettings" "json" "appsettings") (_create_from_sample "Eurofurence.App.Server.Web" "firebase" "json" "firebase") (_create_from_sample "Eurofurence.App.Backoffice/wwwroot" "appsettings" "json" "appsettings-backoffice")

# Start the docker compose stack
up *ARGS: (init)
	docker compose up {{ARGS}}

# Bring the docker compose stack down and remove volumes
[confirm('This is a potentially destructive operation that will delete remove your docker compose volumes! Are you sure?')]
down:
	docker compose down -v --remove-orphans || true

# Stop the docker compose stack
stop:
	docker compose stop

# Clean build files, compose stack, container images and other artifacts
clean:
	dotnet clean
	rm -rf artifacts build
	find . -type d -name "bin" -print0 | xargs -0 rm -rf
	find . -type d -name "obj" -print0 | xargs -0 rm -rf
	docker compose down || true
	docker rmi ghcr.io/eurofurence/ef-app_backend-dotnet-core:dev-sdk || true
	docker rmi ghcr.io/eurofurence/ef-app_backend-dotnet-core:nightly || true

# Perform restore, build & publish with dotnet
build $MYSQL_VERSION=env_var('EF_MOBILE_APP_MYSQL_VERSION'): (_create_if_not_exists 'src/Eurofurence.App.Backoffice/wwwroot/appsettings.json')
	dotnet tool install --global dotnet-ef
	dotnet restore
	dotnet build src/Eurofurence.App.Server.Web/Eurofurence.App.Server.Web.csproj --configuration Release
	dotnet build src/Eurofurence.App.Backoffice/Eurofurence.App.Backoffice.csproj --configuration Release
	dotnet publish src/Eurofurence.App.Tools.CliToolBox/Eurofurence.App.Tools.CliToolBox.csproj --output "$(pwd)/artifacts/cli" --configuration Release
	dotnet ef migrations bundle -o "$(pwd)/artifacts/db-migration-bundle" -p src/Eurofurence.App.Server.Web
	dotnet publish src/Eurofurence.App.Server.Web/Eurofurence.App.Server.Web.csproj --output "$(pwd)/artifacts/backend" --configuration Release
	dotnet publish src/Eurofurence.App.Backoffice/Eurofurence.App.Backoffice.csproj --output "$(pwd)/artifacts/backoffice" --configuration Release

# Build just CLI tools as single, self-contained executable
build-cli:
	dotnet publish src/Eurofurence.App.Tools.CliToolBox/Eurofurence.App.Tools.CliToolBox.csproj --output "$(pwd)/artifacts" --configuration Release --sc -p:PublishSingleFile=true -p:DebugType=None -p:DebugSymbols=false -p:GenerateDocumentationFile=false

# Build release container using spec from docker-compose.yml
containerize *ARGS:
	docker compose build {{ARGS}}

# Build sdk container for backend without executing second stage
containerize-dev:
	docker build --target build -t ghcr.io/eurofurence/ef-app_backend-dotnet-core:dev-sdk -f Dockerfile .

# Build sdk container for backoffice without executing second stage
containerize-backoffice-dev:
	docker build --target build -t ghcr.io/eurofurence/ef-app_backend-dotnet-core-backoffice:dev-sdk -f Dockerfile-backoffice .

# Open Swagger UI in default browser
swagger: (_open_url "Swagger UI" ("http://localhost:"+env_var('EF_MOBILE_APP_BACKEND_PORT')+"/swagger/ui/index.html"))

# Open homepage in default browser
homepage: (_open_url "Homepage" ("http://localhost:"+env_var('EF_MOBILE_APP_BACKEND_PORT')+"/"))

# Open Backoffice UI in default browser
backoffice: (_open_url "Backoffice UI" ("https://localhost:"+env_var('EF_MOBILE_APP_BACKOFFICE_PORT')+"/"))
