param location string = resourceGroup().location

param planName string = 'app-plan-linux'
param planTier string = 'P1v2'

param webappName string = 'dotnet-demoapp'
param webappImage string = 'ghcr.io/benc-uk/dotnet-demoapp:latest'
param weatherKey string = ''
param releaseInfo string = 'Released on ${utcNow('f')}'

// These can be left blank and AAD auth will be disbaled by the app at runtime
@secure()
param adClientSecret string = ''
param adClientId string = ''

resource appServicePlan 'Microsoft.Web/serverfarms@2020-10-01' = {
  name: planName
  location: location
  kind: 'linux'
  sku: {
    name: planTier
  }
  properties: {
    reserved: true
  }
}

resource webApp 'Microsoft.Web/sites@2020-10-01' = {
  name: webappName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      appSettings:[
        {
          name: 'Weather__ApiKey'
          value: weatherKey
        }
        {
          name: 'AzureAd__ClientSecret'
          value: adClientSecret
        }
        {
          name: 'AzureAd__ClientId'
          value: adClientId
        }
        {
          name: 'AzureAd__Instance'
          value: 'https://login.microsoftonline.com/'
        }
        {
          name: 'AzureAd__TenantId'
          value: 'common'
        }
        {
          name: 'AzureAd__CallbackPath'
          value: '/signin-oidc'
        }
        {
          name: 'RELEASE_INFO'
          value: releaseInfo
        }                                      
      ]
      linuxFxVersion: 'DOCKER|${webappImage}'
    }
  }
}
