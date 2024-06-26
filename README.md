# Eurofurence Mobile Apps (Backend)

## Run locally using Docker Compose (or Podman)

1. Copy [`appsettings.sample.json`](src/Eurofurence.App.Server.Web/appsettings.sample.json) to `/appsettings.json` (defaults should suffice for local development).
2. Retrieve a JSON with Google Application Credentials for Firebase (see [Initialize the SDK in non-Google environments](https://firebase.google.com/docs/admin/setup#initialize_the_sdk_in_non-google_environments)).
3. Store credential file as `/firebase.json`.
4. Run `docker compose up`.
5. Wait until you see a message starting with `Startup complete` from the `backend` container.
6. Go [home](http://localhost:30001/swagger/ui).
7. …
8. Profit.

## Run locally using Docker Compose and [`just`](https://github.com/casey/just)

If you have [`just`](https://github.com/casey/just) available on your system, you can use the recipes shown by `just --list` to build, clean, containerize and launch the application:

```plain
just --list
Available recipes:
    build            # Perform restore, build & publish with dotnet
    build-cli        # Build just CLI tools as single, self-contained executable
    clean            # Clean build, stack, container images and artifacts
    containerize     # Build release container using spec from docker-compose.yml
    containerize-dev # Build sdk container without executing second stage
    default          # List available recipes
    down             # Bring the docker compose stack down and remove volumes
    stop             # Stop the docker compose stack
    swagger          # Open swagger UI in default browser
    up *ARGS         # Start the docker compose stack
```

## Potentially outdated information below (Here be dragons!)

- Backend is written in .NET Core an requires at least the Microsoft.NETCore.App 1.0.3 runtime.

- We use Swagger to document our API & data models.
  - You can us [SwaggerUI](https://app.eurofurence.org/swagger/v2/ui/) to interactively play with the backend in your browser.

- Our production backend is located at <https://app.eurofurence.org/api/v2/> - it will return data (~3 months before of a convention usually fresh data, otherwise from last year) to test and design against.
  - You are free to consume the API from your own website/project/application! Please keep the request rates sane and feel free to share your work with us, we'd love to see!

### How to set up

What you'll need

- A MongoDB server
- A server capable of running dotnet core
- Some time and patience

First steps

```bash
git clone https://github.com/eurofurence/ef-app_backend-dotnet-core.git
cd ef-app_backend-dotnet-core
dotnet restore
```

This downloads the project and it's dependencies. For the next steps you need to make sure you have a Mongo instance running.

#### API

##### Starting a testing API

```bash
cd src/*.Kestrelhost
dotnet run
```

##### Publishing

```bash
dotnet publish ef-app_backend-dotnet-core/src/Eurofurence.App.Server.KestrelHost/Eurofurence.App.Server.KestrelHost.csproj --output "pwd/artifacts" —configuration Release —framework netcoreapp2.0
```

##### Running the artifact

```bash
dotnet Eurofurence.App.Server.KestrelHost.dll
```

#### Importing Events

To import a timetable for events you will need the output in a csv format. It needs to following header row.

`event_id,slug,title,conference_track,abstract,description,conference_day,conference_day_name,start_time,end_time,duration,conference_room,pannel_hosts`

Following this should be a CSV row for each event. An example is below

```csv
1,Craftbeer,Craft beer,main,"Stout or porter? You'll learn the history, smell, colour and taste of the most famous beer styles, share your opinion and choose your favourite. Because the world is not just a    bout pilsner.","Stout or porter? You'll learn the history, smell, colour and taste of the most famous beer styles, share your opinion and choose your favourite. Because the world is not just about pilsner.",2017-03-03,6,14:00:00,15:30:00,90,B,NULL
```

Put the CSV file somewhere  where the current  user can read it. Run the following command with the CliToolbox to import

`toolbox events importCsvFile -inputPath=$CSVFILE`

#### Adding a map item

To add a map item, first insert a new entry into Maps.MapRecord

```json
{
    "_id" : NUUID("9f587d12-e92f-46a8-a053-b1596433ce88"),
    "LastChangeDateTimeUtc" : ISODate("2017-07-26T12:33:03.343Z"),
    "IsDeleted" : 0,
    "ImageId" : NUUID("453d0e1d-7a61-4494-a70e-37261f56c692"),
    "Description" : "Estrel Venue",
    "IsBrowseable" : true,
    "Entries" : []
}
```

Then, to change the image, run the following command

`toolbox map loadImage -id 9f587d12-e92f-46a8-a053-b1596433ce88 -imagePath /tmp/myMap.jpg`

*note* Make sure the Mongo inserted ID is NUUID and not the Mongo Default
