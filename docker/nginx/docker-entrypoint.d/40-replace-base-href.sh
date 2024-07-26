#!/bin/bash

sed -i -e "s~base href=\".*\"~base href=\"${BACKOFFICE_BASE_PATH}/\"~g" /usr/share/nginx/html/index.html
