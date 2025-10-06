#!/bin/bash
# Export OpenAPI specification from Swagger endpoint

set -e

API_URL="${API_URL:-http://localhost:5000}"
OUTPUT_FILE="${OUTPUT_FILE:-docs/api/openapi.yaml}"

echo "Exporting OpenAPI spec from $API_URL/swagger/v1/swagger.json"
echo "Output: $OUTPUT_FILE"

# Check if API is running
if ! curl -f -s "$API_URL/swagger/v1/swagger.json" > /dev/null; then
    echo "Error: API is not running at $API_URL"
    echo "Please start the API first: dotnet run --project src/Ledgerly.Api"
    exit 1
fi

# Fetch and save OpenAPI spec
curl -s "$API_URL/swagger/v1/swagger.json" | \
    python3 -c "import sys, json, yaml; yaml.safe_dump(json.load(sys.stdin), sys.stdout, default_flow_style=False)" \
    > "$OUTPUT_FILE" 2>/dev/null || \
    curl -s "$API_URL/swagger/v1/swagger.json" > "${OUTPUT_FILE%.yaml}.json"

if [ -f "$OUTPUT_FILE" ]; then
    echo "✓ OpenAPI spec exported successfully to $OUTPUT_FILE"
elif [ -f "${OUTPUT_FILE%.yaml}.json" ]; then
    echo "✓ OpenAPI spec exported as JSON to ${OUTPUT_FILE%.yaml}.json"
    echo "  (Install PyYAML for YAML output: pip install pyyaml)"
else
    echo "✗ Export failed"
    exit 1
fi
