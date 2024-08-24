import argparse
import json
import logging
import sys
from functools import reduce

import urllib3
from jsonschema.exceptions import ValidationError

# TODO: migrate from RefResolver to Registry https://python-jsonschema.readthedocs.io/en/latest/referencing/#migrating-from-refresolver
from jsonschema.validators import RefResolver
from openapi_schema_validator import OAS30Validator

logger = logging.getLogger(__name__)
debug = False


class ProcessImagesError(Exception):
    """General error encountered when processing an entities image IDs."""


def parse_arguments():
    global debug
    """Parse command line arguments.

    Returns:
        argparse.Namespace: namespace populated with arguments provided via the command-line
    """

    parser = argparse.ArgumentParser(
        description="Import data exported from the API (e.g. of another instance or a previous year) into the current backend via the API to restore it or import test data.",
        epilog="Please be aware that this may result in a high number of API requests and thus run into rate limit on big imports!",
    )

    parser.add_argument(
        dest="api",
        help="Base URL of the API (e.g. https://app.eurofurence.org/EFXX).",
        type=str,
    )

    parser.add_argument(
        "--token",
        "-p",
        dest="token",
        help="Bearer token used to authenticate with the API.",
        type=str,
        required=False,
    )

    parser.add_argument(
        "--source",
        "-s",
        dest="source",
        help="Path or URL to JSON containing the data to be imported.",
        type=str,
        required=False,
    )

    parser.add_argument(
        "--type",
        "-t",
        dest="type",
        help="Name of the entity type to be imported.",
        type=str,
        required=False,
    )

    parser.add_argument(
        "--list-types",
        "-l",
        dest="list",
        help="List available entity types from API.",
        action="store_true",
        required=False,
    )

    parser.add_argument(
        "--with-images-from",
        dest="image_source",
        help="Retrieve images from given API, upload and reference them on imported items.",
        type=str,
        required=False,
    )

    parser.add_argument(
        "--debug",
        dest="debug",
        help="Enable debug logging.",
        action="store_true",
        required=False,
    )

    args = parser.parse_args()

    args.api = args.api.removesuffix("/")

    if args.image_source is not None:
        args.image_source = args.image_source.removesuffix("/")

    debug = args.debug

    return args


def loadSchemas(api: urllib3.connectionpool.ConnectionPool, api_base: str):
    try:
        openapi_schema_response = api.request("GET", f"{api_base}/swagger/api/swagger.json")
        if openapi_schema_response.status != 200:
            logger.error(
                f"Failed to get OpenAPI schema from {openapi_schema_response.url}: [{openapi_schema_response.status}]"
            )
            sys.exit(32)
    except urllib3.exceptions.HTTPError:
        logger.error(f"Failed to retrieve OpenAPI schema from {api.host}.", exc_info=debug)
        sys.exit(22)

    openapi_schema = json.loads(openapi_schema_response.data)
    schemas = openapi_schema["components"]["schemas"]
    types = {}

    for api_path, verbs in openapi_schema["paths"].items():
        type_name = api_path.removeprefix("/Api/")
        if "/" in type_name:
            continue

        if "post" in verbs and "application/json" in verbs["post"]["requestBody"]["content"]:
            types[type_name] = {
                "schema": verbs["post"]["requestBody"]["content"]["application/json"]["schema"]["$ref"].removeprefix(
                    "#/components/schemas/"
                ),
                "path": api_path,
                "post": True,
                "delete": (
                    "delete" in verbs
                    and "parameters" in verbs["delete"]
                    and verbs["delete"]["parameters"]["name"] == "id"
                    and verbs["delete"]["parameters"]["in"] == "path"
                ),
            }

    return schemas, types


def getSourceData(http: urllib3.connectionpool.ConnectionPool, source: str):
    if source.startswith("https://") or source.startswith("http://"):
        try:
            data_response = http.request("GET", source)
            if data_response.status != 200:
                logger.error(f"Failed to get source data from {source}: [{data_response.status}]")
                sys.exit(33)
        except urllib3.exceptions.HTTPError:
            logger.error(f"Failed to retrieve source file from {source}.", exc_info=debug)
            sys.exit(23)
        return json.loads(data_response.data)
    else:
        with open(source, encoding="utf-8") as f:
            return json.load(f)


def processImages(
    http: urllib3.connectionpool.ConnectionPool,
    api: urllib3.connectionpool.ConnectionPool,
    api_base: str,
    image_ids: list[str],
    image_source: str,
):
    image_ids_new = []
    for image_id in image_ids:
        image_meta_response = http.request("GET", f"{image_source}/Api/Images/{image_id}")
        if image_meta_response.status != 200:
            logger.warning(
                f"Failed to get image metadata for {image_id} from {image_source}: [{image_meta_response.status}]"
            )
            break

        image_meta_data = json.loads(image_meta_response.data)
        if "Url" in image_meta_data:
            image_content_response = http.request("GET", image_meta_data["Url"])
        else:
            image_content_response = http.request("GET", f"{image_source}/Api/Images/{image_id}/Content")

        if image_content_response.status != 200:
            logger.warning(
                f"Failed to get image data for {image_id} from {image_source}: [{image_content_response.status}]"
            )
            break

        image_post_response = api.request(
            "POST",
            f"{api_base}/Api/Images",
            fields={"file": (image_meta_data["InternalReference"], image_content_response.data)},
        )
        if image_post_response.status == 200:
            image_id_new = json.loads(image_post_response.data)["Id"]
            image_ids_new.append(image_id_new)
            logger.info(
                f"Successfully uploaded image {image_id} ({image_meta_data["InternalReference"]}) to {api.host} with new ID {image_id_new}."
            )
        else:
            logger.warning(
                f"Failed to upload {image_id} ({image_meta_data["InternalReference"]}) to {api.host}: [{image_post_response.status}]"
            )
            break

    if len(image_ids_new) != len(image_ids):
        logger.warning(
            f"Number of uploaded images ({len(image_ids_new)}) does not match expected number of images ({len(image_ids)}). Rolling back…"
        )
        for image_id in image_ids_new:
            image_delete_response = api.request("DELETE", f"{api_base}/Api/Images/{image_id}")
            if image_delete_response.status == 204:
                logger.info(
                    f"Successfully deleted image {image_id_new} ({image_meta_data["InternalReference"]}) from {image_source}."
                )
            else:
                logger.warning(
                    f"Failed to delete {image_id_new} ({image_meta_data["InternalReference"]}) from {api.host}: [{image_delete_response.status}]"
                )
        raise ProcessImagesError()
    else:
        return image_ids_new


def processData(
    http: urllib3.connectionpool.ConnectionPool,
    api: urllib3.connectionpool.ConnectionPool,
    api_base: str,
    data: dict,
    validator: OAS30Validator,
    types: dict,
    type_name: str,
    image_source: str,
):
    for item in data:
        try:
            validator.validate(item)
        except ValidationError:
            logger.error(f"Unable to import {type_name} with ID {item["Id"]} due to failed validation.", exc_info=debug)
            continue

        if image_source is not None and item["ImageIds"] is not None and len(item["ImageIds"]) > 0:
            if api.request("GET", f"{api_base}{types[type_name]["path"]}/{item["Id"]}").status != 404:
                logger.warning(f"{type_name} with ID {item["Id"]} seems to already exist, skipping image upload…")
                continue
            else:
                logger.info(f"Found {len(item["ImageIds"])} image IDs on {type_name} with ID {item["Id"]}.")
                try:
                    item["ImageIds"] = processImages(http, api, api_base, item["ImageIds"], image_source=image_source)
                except ProcessImagesError:
                    logger.warning(f"Failed to process images for {type_name} with ID {item["Id"]}, skipping…")
                    continue

        try:
            api_response = api.request("POST", f"{api_base}{types[type_name]["path"]}", json=item)

            if api_response.status != 200 and api_response.status != 204:
                logger.error(
                    f"Failed to import {type_name} with ID {item["Id"]}: [{api_response.status}] {api_response.data.decode("UTF-8")}"
                )
                logger.debug(f"Problematic item: {json.dumps(item)}")
            else:
                logger.info(
                    f"Successfully imported {type_name} with ID {item["Id"]}: {api_response.data.decode('UTF-8')}."
                )
        except urllib3.exceptions.HTTPError:
            logger.error(f"Failed to POST {type_name} with ID {item["Id"]}.", exc_info=debug)
            continue


def main():
    args = parse_arguments()
    logging.basicConfig(
        level=logging.INFO, format="%(asctime)s - %(levelname)s - %(message)s", datefmt="%Y-%m-%dT%H:%M:%S%z"
    )

    with (
        urllib3.connection_from_url(args.api, headers={"Authorization": f"Bearer {args.token}"}) as api,
        urllib3.PoolManager() as http,
    ):
        api_base = urllib3.util.parse_url(args.api).request_uri.removeprefix("/")
        schemas, types = loadSchemas(api, api_base)

        if args.list:
            type_info = reduce(
                lambda type_string,
                key: f"{type_string}\n - {key} ({";".join("{}={}".format(*i) for i in types[key].items())})",
                types,
                "",
            )
            logger.info(f"Listing API types…\nAvailable types from OpenAPI schema at {args.api}:{type_info}")
            return

        if args.type not in types:
            logger.error(
                f"Type {args.type} is not part of the OpenAPI schema. Use `--list-types` to see all available types."
            )
            return 1

        resolver = RefResolver.from_schema({"components": {"schemas": schemas}})
        validator = OAS30Validator(schemas[types[args.type]["schema"]], resolver=resolver)

        data = getSourceData(http, args.source)

        processData(http, api, api_base, data, validator, types, type_name=args.type, image_source=args.image_source)


if __name__ == "__main__":
    main()
