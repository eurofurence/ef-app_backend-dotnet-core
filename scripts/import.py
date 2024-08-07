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


def parse_arguments():
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
		"--debug",
		dest="debug",
		help="Enable debug logging.",
		action="store_true",
		required=False,
	)

	args = parser.parse_args()

	args.api = args.api.removesuffix("/")

	return args


def main():
	args = parse_arguments()
	logging.basicConfig(
		level=logging.INFO, format="%(asctime)s - %(levelname)s - %(message)s", datefmt="%Y-%m-%dT%H:%M:%S%z"
	)

	with urllib3.connection_from_url(args.api) as api, urllib3.PoolManager() as http:
		try:
			openapi_schema_response = api.request("GET", "/swagger/api/swagger.json")
		except urllib3.exceptions.HTTPError:
			logger.error(f"Failed to retrieve OpenAPI schema from {args.api}.", exc_info=args.debug)
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
					"schema": verbs["post"]["requestBody"]["content"]["application/json"]["schema"][
						"$ref"
					].removeprefix("#/components/schemas/"),
					"path": api_path,
					"post": True,
					"delete": (
						"delete" in verbs
						and "parameters" in verbs["delete"]
						and verbs["delete"]["parameters"]["name"] == "id"
						and verbs["delete"]["parameters"]["in"] == "path"
					),
				}

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

		data = {}
		if args.source.startswith("https://") or args.source.startswith("http://"):
			try:
				data_response = http.request("GET", args.source)
			except urllib3.exceptions.HTTPError:
				logger.error(f"Failed to retrieve source file from {args.source}.", exc_info=args.debug)
				sys.exit(23)
			data = json.loads(data_response.data)
		else:
			with open(args.source, encoding="utf-8") as f:
				data = json.load(f)

		for item in data:
			try:
				validator.validate(item)
			except ValidationError:
				logger.error(
					f"Unable to import {args.type} with id {item["Id"]} due to failed validation.", exc_info=args.debug
				)
				continue

			try:
				api_response = api.request(
					"POST", types[args.type]["path"], json=item, headers={"Authorization": f"Bearer {args.token}"}
				)

				if api_response.status != 200 and api_response.status != 204:
					logger.error(
						f"Failed to import {args.type} with id {item["Id"]}: [{api_response.status}] {api_response.data.decode('UTF-8')}"
					)
					logger.debug(f"Problematic item: {json.dumps(item)}")
				else:
					logger.info(
						f"Successfully imported {args.type} with id {item["Id"]}: {api_response.data.decode('UTF-8')}."
					)
			except urllib3.exceptions.HTTPError:
				logger.error(f"Failed to POST {args.type} with id {item["Id"]}.", exc_info=args.debug)
				continue


if __name__ == "__main__":
	main()
