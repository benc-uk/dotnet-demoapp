# .NET Core - Demo Web Application
This is a simple .NET Core web app using Razor pages and MVC. It was created from the `dotnet new webapp` template and modified adding Bootstrap v4, Fontawesome and other packages/features.

The app has been designed with cloud native demos & containers in mind, in order to provide a real working application for deployment, something more than "hello-world" but with the minimum of pre-reqs. It is not intended as a complete example of a fully functioning architecture or complex software design.

Typical uses would be deployment to Kubernetes, demos of Docker, CI/CD (build pipelines are provided), deployment to cloud (Azure) monitoring, auto-scaling

The app has several basic pages accessed from the top navigation menu, some of which are only lit up when certain configuration variables are set (see 'Optional Features' below):

- **'Info'** - Will show system & runtime information, and will also display if the app is running from within a Docker container and Kubernetes.  
- **'Tools'** - Some tools useful in demos, such a forcing CPU load (for autoscale demos), and error/exception pages for use with App Insights or other monitoring tool.
- **'Monitoring'** - Displays realtime CPU load and memory working set charts, fetched from an REST API (/api/monitoringdata) and displayed using chart.js
- **'Weather'** - (Optional) Gets the location of the client page (with HTML5 Geolocation). The resulting location is used to fetch a weather forecast from the [Dark Sky](http://darksky.net) weather API
- **'User Account'** - (Optional) When configured with Azure AD (application client id and secret) user login button will be enabled, and an user-account details page enabled, which calls the Microsoft Graph API


![screen](https://user-images.githubusercontent.com/14982936/71717446-0bc47400-2e10-11ea-8db2-1db5b991d566.png)
![screen](https://user-images.githubusercontent.com/14982936/71717448-0bc47400-2e10-11ea-8bf0-5115d4c8c4a4.png)
![screen](https://user-images.githubusercontent.com/14982936/71717426-fea78500-2e0f-11ea-881f-ad9bd8adbfae.png)


# Running Locally 
```
cd src
dotnet restore
dotnet run
```

Web app will listen on the usual Kestrel port of 5000, but this can be changed by setting the `ASPNETCORE_URLS` environmental variable or with the `--urls` parameter ([see docs](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel?view=aspnetcore-3.1)).  
Tested with Dotnet Core v3.0 and 3.1


# Docker 
Public Docker image is [available on Dockerhub](https://hub.docker.com/r/bencuk/dotnet-demoapp/).  

Run in a container with:
```
docker run -d -p 5000:5000 bencuk/dotnet-demoapp
```

Should you want to build your own container, use the `Dockerfile` found the in the 'docker' directory of the project

# GitHub Actions CI/CD 
A working CI and release GitHub Actions workflow is provided `.github/workflows/build-deploy-aks.yml`, automated builds are run in GitHub hosted runners

### [GitHub Actions](https://github.com/benc-uk/dotnet-demoapp/actions)

![](https://img.shields.io/github/workflow/status/benc-uk/dotnet-demoapp/Build%20%26%20Deploy%20AKS)  
![](https://img.shields.io/github/last-commit/benc-uk/dotnet-demoapp)  


# Optional Features
The app will start up and run with zero configuration, however the only features that will be available will be the Info, Tools & Monitoring views. The following optional features can be enabled:

### Application Insights 
Enable this by setting `ApplicationInsights__InstrumentationKey` 

The app has been instrumented with the Application Insights SDK, it will however need to be configured to point to your App Insights instance/workspace. All requests will be tracked, as well as dependant calls to other APIs, exceptions & errors will also be logged

[This article](https://docs.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core) has more information on monitoring .NET Core with App Insights

If running locally, and using appsettings.json, this can be configured as follows
```
  "Weather": {
    "ApiKey": "{{ key value here }}"
  },
```

### Weather Details
Enable this by setting `Weather__ApiKey`

This will require a API key from Dark Sky, you can [sign up for free and get one here](https://darksky.net/dev). The page uses a browser API for geolocation to fetch the user's location.  
However, the `geolocation.getCurrentPosition()` browser API will only work when the site is served via HTTPS or from localhost. As a fallback, weather for London, UK will be show if the current position can not be obtained

If running locally, and using appsettings.json, this can be configured as follows
```
  "Weather": {
    "ApiKey": "{{ key value here }}"
  },
```

### User Authentication with Azure AD and Microsoft Graph 
Enable this feature by setting several 'AzureAd' environmental variables, once enabled, a 'Login' button will be displayed on the main top nav bar.

This uses [Microsoft.Identity.Web](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/Microsoft.Identity.Web) which is a library allowing .NET Core web apps to use the Microsoft Identity Platform (i.e. Azure AD v2.0 endpoint). As this library is not available as NuGet package yet, it is included in this repo in the 'external' directory.  

In addition the user account page shows details & photo retrieved from the Microsoft Graph API

You will need to register an app in your Azure AD tennant. [See this guide](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app). Make sure you enable the options 
- Enable the app for *"Accounts in any organizational directory and personal Microsoft accounts"*
- Add *'Web platform'* for authentication
- Ensure your Redirect URI ends with `/signin-oidc`
- Enable *"Access Tokens"* and *"ID Tokens"* in the authentication settings. 
- Add a new client secret, and make a note of it's value

Environmental Variables:
- `AzureAd__ClientId`: You app's client id
- `AzureAd__ClientSecret`: You app's client secret
- `AzureAd__Instance`: Set to `https://login.microsoftonline.com/`
- `AzureAd__TenantId`: Set to `common`
- `AzureAd__CallbackPath`: Set to `/signin-oidc`

If running locally, and using appsettings.json, this can be configured as follows
```
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "ClientId": "9c8ff43e-73da-4db3-815a-22f457cec7f0",
    "ClientSecret": "{{ secret value here }}",
    "TenantId": "common",
    "CallbackPath": "/signin-oidc"
  },
```


# Updates
* Jun 2020 - Moved to NuGet for the Microsoft.Identity.Web
* Jan 2020 - Rewritten from scratch
