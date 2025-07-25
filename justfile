set dotenv-load := true


export BUILD_VERSION := `echo $(git describe --tags --abbrev=0 | sed -e 's/^v//')$(if [ -z "$(git tag --points-at HEAD)" ]; then echo "-local.$USER"; fi)`

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

# Initalize configurations files in their own project folders. This is required for running this project in a devcontainer
init-devcontainer: (_create_from_sample "Eurofurence.App.Server.Web" "appsettings" "json" "src/Eurofurence.App.Server.Web/appsettings") (_create_from_sample "Eurofurence.App.Server.Web" "firebase" "json" "src/Eurofurence.App.Server.Web/firebase") (_create_from_sample "Eurofurence.App.Backoffice/wwwroot" "appsettings" "json" "src/Eurofurence.App.Backoffice/wwwroot/appsettings")

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
	dotnet tool install --global dotnet-ef || true
	dotnet restore
	dotnet build src/Eurofurence.App.Server.Web/Eurofurence.App.Server.Web.csproj --configuration Release --property:Version=$BUILD_VERSION
	dotnet build src/Eurofurence.App.Backoffice/Eurofurence.App.Backoffice.csproj --configuration Release --property:Version=$BUILD_VERSION
	ASPNETCORE_ENVIRONMENT="sample" dotnet ef migrations bundle -o "$(pwd)/artifacts/db-migration-bundle" -p src/Eurofurence.App.Server.Web
	dotnet publish src/Eurofurence.App.Server.Web/Eurofurence.App.Server.Web.csproj --output "$(pwd)/artifacts/backend" --configuration Release --property:Version=$BUILD_VERSION
	dotnet publish src/Eurofurence.App.Backoffice/Eurofurence.App.Backoffice.csproj --output "$(pwd)/artifacts/backoffice" --configuration Release --property:Version=$BUILD_VERSION

# Build release container for service using spec from docker-compose.yml and refresh service if stack is running
containerize SERVICE="" *ARGS="":
	#!/bin/bash
	set -euxo pipefail
	docker compose build --build-arg BUILD_VERSION=$BUILD_VERSION {{ARGS}} {{SERVICE}}
	if [[ $(docker compose ps --services --status running | wc -w) -gt 0 ]]; then
		echo "Detected running stack! Refreshing service {{SERVICE}} …"
		just recreate {{SERVICE}}
	fi

recreate SERVICE="":
	docker compose create --force-recreate {{SERVICE}} && docker compose start {{SERVICE}}

# Build sdk container for backend without executing second stage
containerize-dev:
	docker build --build-arg BUILD_VERSION=$BUILD_VERSION --target build -t ghcr.io/eurofurence/ef-app_backend-dotnet-core:dev-sdk -f Dockerfile .

# Build sdk container for backoffice without executing second stage
containerize-backoffice-dev:
	docker build --build-arg BUILD_VERSION=$BUILD_VERSION --target build -t ghcr.io/eurofurence/ef-app_backend-dotnet-core-backoffice:dev-sdk -f Dockerfile-backoffice .

# Open Swagger UI in default browser
swagger: (_open_url "Swagger UI" ("http://localhost:"+env_var('EF_MOBILE_APP_BACKEND_PORT')+"/swagger/ui/index.html"))

# Open homepage in default browser
homepage: (_open_url "Homepage" ("http://localhost:"+env_var('EF_MOBILE_APP_BACKEND_PORT')+"/"))

# Open Backoffice UI in default browser
backoffice: (_open_url "Backoffice UI" ("https://localhost:"+env_var('EF_MOBILE_APP_BACKOFFICE_PORT')+env_var('EF_MOBILE_APP_BACKOFFICE_BASE_PATH')+"/"))

[doc('Verify an Apple App Site Association file for a given domain and path
(see https://developer.apple.com/documentation/technotes/tn3155-debugging-universal-links)')]
verify-aasa DOMAIN PATH:
	#!/usr/bin/env bash
	set -euxo pipefail
	echo "Attempting download of AASA file for {{DOMAIN}} via swcutil…"
	sudo swcutil dl -d {{DOMAIN}}
	echo "Downloading AASA file as JSON…"
	AASA_TEMP_PATH=$(mktemp)
	trap 'rm -f -- "$AASA_TEMP_PATH"' EXIT
	curl -o $AASA_TEMP_PATH https://{{DOMAIN}}/.well-known/apple-app-site-association
	echo "Performing verification of URL https://{{DOMAIN}}/{{PATH}} via swcutil…"
	sudo swcutil verify -d {{DOMAIN}} -j $AASA_TEMP_PATH -u https://{{DOMAIN}}/{{PATH}}

# Import knowledgebase from an EF27 backend into a latest version target via API.
import-kb-ef27 SOURCE_API TARGET_API TOKEN:
	#!/usr/bin/env bash
	set -euxo pipefail
	echo "Retrieving source data from {{SOURCE_API}} and patching compatibility…"
	SOURCE_KNOWLEDGE_GROUPS_TEMP_PATH=$(mktemp)
	trap 'rm -f -- "$SOURCE_KNOWLEDGE_GROUPS_TEMP_PATH"' EXIT
	curl {{SOURCE_API}}/Api/KnowledgeGroups | sed 's/"FontAwesomeIconCharacterUnicodeAddress": "[^"]*"/"FontAwesomeIconName": ""/' > $SOURCE_KNOWLEDGE_GROUPS_TEMP_PATH
	echo "Importing KnowledgeGroups to {{TARGET_API}}…"
	python ./scripts/import.py {{TARGET_API}} -p {{TOKEN}} -s $SOURCE_KNOWLEDGE_GROUPS_TEMP_PATH -t KnowledgeGroups
	echo "Importing KnowledgeEntries to {{TARGET_API}}…"
	python ./scripts/import.py {{TARGET_API}} -p {{TOKEN}} -s {{SOURCE_API}}/Api/KnowledgeEntries -t KnowledgeEntries --with-images-from {{SOURCE_API}}

# Import data from source URL or path to the target API.
import SOURCE TARGET_API TOKEN TYPE *ARGS:
	python ./scripts/import.py {{TARGET_API}} -p {{TOKEN}} -s {{SOURCE}} -t {{TYPE}} {{ARGS}}

# List types supported for import.
import-list-types TARGET_API:
	python ./scripts/import.py {{TARGET_API}} --list-types

# Create a new EntityFramework migration
create-migration MIGRATION_NAME $MYSQL_VERSION=env_var('EF_MOBILE_APP_MYSQL_VERSION'):
	#!/usr/bin/env bash
	set -euxo pipefail
	docker compose start db
	cd src/Eurofurence.App.Infrastructure.EntityFramework
	ASPNETCORE_ENVIRONMENT="sample" dotnet ef migrations add {{MIGRATION_NAME}}
