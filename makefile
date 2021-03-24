# Used by `image`, `push` & `deploy` targets, override as required
IMAGE_REG ?= ghcr.io
IMAGE_REPO ?= benc-uk/dotnet-demoapp
IMAGE_TAG ?= latest

# Used by `deploy` target, sets Azure webap defaults, override as required
AZURE_RES_GROUP ?= temp-demoapps
AZURE_REGION ?= uksouth
AZURE_SITE_NAME ?= dotnet-$(shell git rev-parse --short HEAD)

# Used by `test-api` target
TEST_HOST ?= localhost:5000

# Don't change
SRC_DIR := src
TEST_DIR := tests

.PHONY: help lint lint-fix image push run deploy undeploy .EXPORT_ALL_VARIABLES
.DEFAULT_GOAL := help

help:  ## 💬 This help message
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-20s\033[0m %s\n", $$1, $$2}'

lint:  ## 🔎 Lint & format, will not fix but sets exit code on error 
	@dotnet format --help > /dev/null 2> /dev/null || dotnet tool install --global dotnet-format
	dotnet format --check ./src

lint-fix:  ## 📜 Lint & format, will try to fix errors and modify code 
	@dotnet format --help > /dev/null 2> /dev/null || dotnet tool install --global dotnet-format
	dotnet format ./src

image:  ## 🔨 Build container image from Dockerfile 
	docker build . --file build/Dockerfile \
	--tag $(IMAGE_REG)/$(IMAGE_REPO):$(IMAGE_TAG)

push:  ## 📤 Push container image to registry 
	docker push $(IMAGE_REG)/$(IMAGE_REPO):$(IMAGE_TAG)

run:  ## 🏃‍ Run locally using Dotnet CLI
	dotnet run dotnet run -p $(SRC_DIR)/dotnet-demoapp.csproj

deploy:  ## 🚀 Deploy to Azure Web App 
	az group create --resource-group $(AZURE_RES_GROUP) --location $(AZURE_REGION) -o table
	az deployment group create --template-file deploy/webapp.bicep \
		--resource-group $(AZURE_RES_GROUP) \
		--parameters webappName=$(AZURE_SITE_NAME) \
		--parameters webappImage=$(IMAGE_REG)/$(IMAGE_REPO):$(IMAGE_TAG) -o table 
	@echo "### 🚀 Web app deployed to https://$(AZURE_SITE_NAME).azurewebsites.net/"

undeploy:  ## 💀 Remove from Azure 
	@echo "### WARNING! Going to delete $(DEPLOY_RES_GROUP) 😲"
	az group delete -n $(DEPLOY_RES_GROUP) -o table --no-wait

test:  ## 🎯 Unit tests with xUnit
	dotnet test tests/tests.csproj 

test-report:  ## 🤡 Unit tests with xUnit & output report
	rm -rf $(TEST_DIR)/TestResults
	dotnet test $(TEST_DIR)/tests.csproj --logger html

test-api:  ## 🚦 Run integration API tests, server must be running!
	node node_modules/.bin/newman -v > /dev/null 2>&1 || npm install newman
	node_modules/.bin/newman run $(TEST_DIR)/postman_collection.json --env-var apphost=$(TEST_HOST)

clean:  ## 🧹 Clean up project
	rm -rf node_modules
	rm -rf package*.json
	rm -rf $(TEST_DIR)/TestResults
	rm -rf $(TEST_DIR)/bin
	rm -rf $(TEST_DIR)/obj
	rm -rf $(SRC_DIR)/bin
	rm -rf $(SRC_DIR)/obj
