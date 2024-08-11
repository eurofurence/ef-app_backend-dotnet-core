# Quality-of-Life Scripts

Some Python (3.12) scripts to make a developer's life a bit easier.
Make sure to have all dependencies from the [requirements.txt](./requirements.txt) installed via `pip install -r requirements.txt` (consider using [`venvs`](https://docs.python.org/3/library/venv.html)).

**ToDo:**

* provide an easier, programmatic way to obtain a token from the IDP

## [import.py](./import.py) – Import Data between APIs

The token required will be a bearer token obtained from the [IDP](https://identity.eurofurence.org), which is currently easiest by going to the backoffice and simply pulling it from the browser's session storage.

Currently the script has only been tested with `KnowledgeGroups` and `KnowledgeEntries`.

Just recipes are available for listing (potentially) supported types (`just import-list-types`) and running the importer (`just import`).

```text
usage: import.py [-h] [--token TOKEN] [--source SOURCE] [--type TYPE] [--list-types]
                 [--with-images-from IMAGE_SOURCE] [--debug]
                 api

Import data exported from the API (e.g. of another instance or a previous year) into the current backend via the API
to restore it or import test data.

positional arguments:
  api                   Base URL of the API (e.g. https://app.eurofurence.org/EFXX).

options:
  -h, --help            show this help message and exit
  --token TOKEN, -p TOKEN
                        Bearer token used to authenticate with the API.
  --source SOURCE, -s SOURCE
                        Path or URL to JSON containing the data to be imported.
  --type TYPE, -t TYPE  Name of the entity type to be imported.
  --list-types, -l      List available entity types from API.
  --with-images-from IMAGE_SOURCE
                        Retrieve images from given API, upload and reference them on imported items.
  --debug               Enable debug logging.

Please be aware that this may result in a high number of API requests and thus run into rate limit on big imports!
```

### Conversion of KnowledgeGroups between EF27 and EF28

The `FontAwesomeIconCharacterUnicodeAddress` field got replaced by `FontAwesomeIconName` between EF27 and EF28, so importing requires this field to be replaced in order for the importer to be able to validate the schema:

```bash
curl https://app.eurofurence.org/EF27/Api/KnowledgeGroups | sed 's/"FontAwesomeIconCharacterUnicodeAddress": "[^"]*"/"FontAwesomeIconName": ""/'
```

Or simply use `just import-kb-ef27` with appropriate arguments.

Icons will have to be set again manually after importing this! Feel free to add a mapper to the import script if you want though. (;
