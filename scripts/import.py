import argparse
import json
import logging
from openapi_schema_validator import OAS30Validator
#TODO: migrate from RefResolver to Registry https://python-jsonschema.readthedocs.io/en/latest/referencing/#migrating-from-refresolver
from referencing import Registry, Resource
from referencing.jsonschema import DRAFT202012
from jsonschema.validators import RefResolver
from jsonschema.exceptions import ValidationError
import sys
import urllib3

logger = logging.getLogger(__name__)
logging.basicConfig(level=logging.INFO,format="%(asctime)s - %(levelname)s - %(message)s",datefmt="%Y-%m-%dT%H:%M:%S%z")


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
		'--token', '-p',
		dest="token",
		help="Bearer token used to authenticate with the API.",
		type=str,
		required=False,
	)

	parser.add_argument(
		'--source', '-s',
		dest="source",
		help="Path or URL to JSON containing the data to be imported.",
		type=str,
		required=False,
	)

	parser.add_argument(
		'--type', '-t',
		dest="type",
		help="Name of the entity type to be imported.",
		type=str,
		required=False,
	)

	parser.add_argument(
		'--list-types', '-l',
		dest="list",
		help="List available entity types from API.",
		action='store_true',
		required=False,
	)

	args = parser.parse_args()

	args.api = args.api.removesuffix("/")

	return args


def main():
	args = parse_arguments()

	with urllib3.connection_from_url(args.api) as api, urllib3.PoolManager() as http:
		openapi_schema_response = api.request(
			"GET", "/swagger/api/swagger.json")
		openapi_schema = json.loads(openapi_schema_response.data)
		schemas = openapi_schema["components"]["schemas"]

		if args.type not in schemas:
			logger.error(f"Type {args.type} is not part of the OpenAPI schema. Use `--list-types` to see all available types.")
			return 1

		if args.list:
			logger.info(f"Available types from OpenAPI schema at {args.api}:\n - {"\n - ".join(schemas.keys())}\nThis does not mean that there will be a POST endpoint for each of these types!")
			return

		resolver = RefResolver.from_schema({"components":{"schemas": schemas}})
		validator = OAS30Validator(schemas[args.type], resolver=resolver)

		data = {}
		if args.source.startswith("https://") or args.source.startswith("http://"):
			data_response = http.request("GET", args.source)
			data = json.loads(data_response.data)
		else:
			with open(args.source, encoding="utf-8") as f:
				data = json.load(f)

		for item in data:
			try:
				validator.validate(item)
				api_endpoint = args.type.removesuffix("Record").removesuffix("Response").removesuffix("Request")
				if api_endpoint.endswith("y"):
					api_endpoint = api_endpoint.removesuffix("y") + "ie"
				api_response = api.request("POST", f"/Api/{api_endpoint}s",
								json=item,
								headers={"Authorization": f"Bearer {args.token}"})
				if api_response.status != 200:
					logger.error(f"Failed to import {args.type} with id {item["Id"]}: [{api_response.status}] {api_response.data.decode('UTF-8')}")
					logger.debug(f"Problematic item: {json.dumps(item)}")
				else:
					logger.info(f"Successfully imported {args.type} with id {item["Id"]}: {api_response.data.decode('UTF-8')}.")
			except ValidationError:
					logger.error(f"Failed to import {args.type} with id {item["Id"]} due to failed validation.", exc_info=1)


if __name__ == "__main__":
	main()
