# Notes from Unai - SRE Book
The following repo is used as a demo in the book **The Art of Site Reliability Engineering (SRE) with Azure**

<img src="https://user-images.githubusercontent.com/64772417/204004573-e4793fe1-1e45-46ad-8e2f-96514ffb2429.png" width="250"/>

## Demo Guide

1. Create a Resource Group under your Azure subscription for your demo.
  
    `az group create --name myResourceGroup --location eastus`
    
1. Create an Azure Container Registry (ACR) to keep your .net container (later build and pushed with GitHub Actions. From Cloud Shell:

    `az acr create --resource-group myResourceGroup --name yourcontainerregistry --sku Basic`

1. Create an Azure Key Vaul resource. It will later keep secrets like the Weather API key. From Cloud Shell:

    `az keyvault create --name your-unique-keyvault-name --resource-group myResourceGroup --location EastUS`

1. To Test the Weather API calls in the App, you need to create an account in [Openweathermap](https://home.openweathermap.org/users/sign_up) and get an API Key

    ![image](https://user-images.githubusercontent.com/64772417/204014383-2b2000fd-1a28-476b-b9db-02b8c336ae82.png)
    
1. Keep your **Openweathermap** API key as a secret in Azure Key Vault. 
    ![image](https://user-images.githubusercontent.com/64772417/204014872-997e06d0-ec6f-4e6b-a1e8-c66c5b6adb4b.png)
    
1. Create a Service Principal that will be used as:
    1. Identity used by the GH Action to deploy/update resources
    1. Identity used by the .NET container to connect to Azure Key Vault and Azure App Configur



# .NET - Demo Web Application

This is a simple .NET web app using the new minimal hosting model, and Razor pages. It was created from the `dotnet new webapp` template and modified adding custom APIs, Bootstrap v5, Microsoft Identity and other packages/features.

The app has been designed with cloud native demos & containers in mind, in order to provide a real working application for deployment, something more than "hello-world" but with the minimum of pre-reqs. It is not intended as a complete example of a fully functioning architecture or complex software design.

Typical uses would be deployment to Kubernetes, demos of Docker, CI/CD (build pipelines are provided), deployment to cloud (Azure) monitoring, auto-scaling

The app has several basic pages accessed from the top navigation menu, some of which are only lit up when certain configuration variables are set (see 'Optional Features' below):

- **Info** - Will show system & runtime information, and will also display if the app is running from within a Docker container and Kubernetes.
- **Tools** - Some tools useful in demos, such a forcing CPU load (for autoscale demos), and error/exception pages for use with App Insights or other monitoring tool.
- **Monitoring** - Displays realtime CPU load and memory working set charts, fetched from an REST API and displayed using chart.js
- **Weather** - (Optional) Gets the location of the client page (with HTML5 Geolocation). The resulting location is used to fetch a weather forecast from the [OpenWeather API](https://openweathermap.org/)
- **User Account** - (Optional) When configured with Azure AD (application client id and secret) user login button will be enabled, and an user-account details page enabled, which calls the Microsoft Graph API

![screen](https://user-images.githubusercontent.com/14982936/71717446-0bc47400-2e10-11ea-8db2-1db5b991d566.png)
![screen](https://user-images.githubusercontent.com/14982936/71717448-0bc47400-2e10-11ea-8bf0-5115d4c8c4a4.png)
![screen](https://user-images.githubusercontent.com/14982936/71717426-fea78500-2e0f-11ea-881f-ad9bd8adbfae.png)

# Status

![](https://img.shields.io/github/last-commit/benc-uk/dotnet-demoapp) ![](https://img.shields.io/github/release-date/benc-uk/dotnet-demoapp) ![](https://img.shields.io/github/v/release/benc-uk/dotnet-demoapp) ![](https://img.shields.io/github/commit-activity/y/benc-uk/dotnet-demoapp)

Live instances:

[![](https://img.shields.io/website?label=Hosted%3A%20Kubernetes&up_message=online&url=https%3A%2F%2Fdotnet-demoapp.kube.benco.io%2F)](https://dotnet-demoapp.kube.benco.io/)

# Running and Testing Locally

### Pre-reqs

- Be using Linux, WSL or MacOS, with bash, make etc
- [.NET 6](https://docs.microsoft.com/en-us/dotnet/core/install/linux) - for running locally, linting, running tests etc
- [Docker](https://docs.docker.com/get-docker/) - for running as a container, or image build and push
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli-linux) - for deployment to Azure

Clone the project to any directory where you do development work

```
git clone https://github.com/benc-uk/dotnet-demoapp.git
```

### Makefile

A standard GNU Make file is provided to help with running and building locally.

```txt
$ make

help                 💬 This help message
lint                 🔎 Lint & format, will not fix but sets exit code on error
image                🔨 Build container image from Dockerfile
push                 📤 Push container image to registry
run                  🏃‍ Run locally using Dotnet CLI
deploy               🚀 Deploy to Azure Container App
undeploy             💀 Remove from Azure
test                 🎯 Unit tests with xUnit
test-report          🤡 Unit tests with xUnit & output report
clean                🧹 Clean up project
```

Make file variables and default values, pass these in when calling `make`, e.g. `make image IMAGE_REPO=blah/foo`

| Makefile Variable | Default                |
| ----------------- | ---------------------- |
| IMAGE_REG         | ghcr<span>.</span>io   |
| IMAGE_REPO        | benc-uk/dotnet-demoapp |
| IMAGE_TAG         | latest                 |
| AZURE_RES_GROUP   | demoapps               |
| AZURE_REGION      | northeurope            |
| AZURE_APP_NAME    | dotnet-demoapp         |

Web app will listen on the usual Kestrel port of 5000, but this can be changed by setting the `ASPNETCORE_URLS` environmental variable or with the `--urls` parameter ([see docs](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel?view=aspnetcore-6.0)).

# Containers

Public container image is [available on GitHub Container Registry](https://github.com/users/benc-uk/packages/container/package/dotnet-demoapp).

Run in a container with:

```bash
docker run --rm -it -p 5000:5000 ghcr.io/benc-uk/dotnet-demoapp:latest
```

Should you want to build your own container, use `make image` and the above variables to customise the name & tag.

## Kubernetes

The app can easily be deployed to Kubernetes using Helm, see [deploy/kubernetes/readme.md](deploy/kubernetes/readme.md) for details

# GitHub Actions CI/CD

A set of GitHub Actions workflows are included for CI / CD. Automated builds for PRs are run in GitHub hosted runners validating the code (linting and tests) and building dev images. When code is merged into master, then automated deployment to AKS is done using Helm.

[![](https://img.shields.io/github/workflow/status/benc-uk/dotnet-demoapp/CI%20Build%20App)](https://github.com/benc-uk/dotnet-demoapp/actions?query=workflow%3A%22CI+Build+App%22) [![](https://img.shields.io/github/workflow/status/benc-uk/dotnet-demoapp/CD%20Release%20-%20AKS?label=release-kubernetes)](https://github.com/benc-uk/dotnet-demoapp/actions?query=workflow%3A%22CD+Release+-+AKS%22)

# Optional Features

The app will start up and run with zero configuration, however the only features that will be available will be the *Info*, *Tools* & *Monitoring* views. The following optional features can be enabled:

### Application Insights

Enable this by setting `ApplicationInsights__InstrumentationKey`

The app has been instrumented with the Application Insights SDK, it will however need to be configured to point to your App Insights instance/workspace. All requests will be tracked, as well as dependant calls to other APIs, exceptions & errors will also be logged

[This article](https://docs.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core) has more information on monitoring .NET with App Insights

If running locally, and using appsettings.Development.json, this can be configured as follows

```json
"ApplicationInsights": {
  "InstrumentationKey": "<key value here>"
}
```

### Weather Details

Enable this by setting `Weather__ApiKey`

This will require a API key from OpenWeather, you can [sign up for free and get one here](https://openweathermap.org/). The page uses a browser API for geolocation to fetch the user's location.
However, the `geolocation.getCurrentPosition()` browser API will only work when the site is served via HTTPS or from localhost. As a fallback, weather for London, UK will be show if the current position can not be obtained

If running locally, and using appsettings.Development.json, this can be configured as follows

```json
"Weather": {
  "ApiKey": "<key value here>"
}
```

### User Authentication with Azure AD and Microsoft Graph

Enable this feature by setting several 'AzureAd' environmental variables, once enabled, a 'Login' button will be displayed on the main top nav bar.

This uses [Microsoft.Identity.Web](https://github.com/AzureAD/microsoft-identity-web) which is a library allowing .NET web apps to use the Microsoft Identity Platform (i.e. Azure AD v2.0 endpoint)

In addition the user account page shows details & photo retrieved from the Microsoft Graph API

You will need to register an app in your Azure AD tenant. [See this guide](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app). Make sure you enable the options

- Enable the app for _"Accounts in any organizational directory and personal Microsoft accounts"_
- Add _'Web platform'_ for authentication
- Ensure your Redirect URI ends with `/signin-oidc`
- Enable _"Access Tokens"_ and _"ID Tokens"_ in the authentication settings.
- Add a new client secret, and make a note of it's value

Environmental Variables:

- `AzureAd__ClientId`: You app's client id
- `AzureAd__ClientSecret`: You app's client secret
- `AzureAd__Instance`: Set to `https://login.microsoftonline.com/`
- `AzureAd__TenantId`: Set to `common`

If running locally, and using appsettings.Development.json, this can be configured as follows

```json
"AzureAd": {
  "Instance": "https://login.microsoftonline.com/",
  "ClientId": "<change me>",
  "ClientSecret": "<change me>",
  "TenantId": "common",
}
```

## Running in Azure - Container App

If you want to deploy to an Azure Container App, a Bicep template is provided in the [deploy](deploy/) directory

For a quick deployment, use `make deploy` which will create a resource group, the Azure Container App instance (with supporting resources) and deploy the latest image to it

```bash
make deploy
```

> Note. Azure Container App doesn't currently support HTTP header forwarding, so Azure AD sign-in will not work as it mis-redirects to the wrong URL

# Updates

- Nov 2021 - Large scale rewrite to .NET 6
- Mar 2021 - Update to deployment, added dummy unit tests and makefile
- Nov 2020 - Updated to .NET 5
- Nov 2020 - New GitHub pipelines & container registry
- Jun 2020 - Moved to NuGet for the Microsoft.Identity.Web
- Jan 2020 - Rewritten from scratch
