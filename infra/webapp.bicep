param location string = resourceGroup().location

param planName string = 'app-plan-linux'
param planTier string = 'P1v2'

param webappName string = 'nodejs-demoapp'
param webappImage string = 'ghcr.io/benc-uk/nodejs-demoapp:latest'
param weatherKey string = ''
param adClientSecret string = ''

resource appServicePlan 'Microsoft.Web/serverFarms@2020-06-01' = {
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

resource webApp 'Microsoft.Web/sites@2018-11-01' = {
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
          value: '3584ac39-1ab1-4fe6-a3dd-4b2fbedc9d7d'
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
      ]
      linuxFxVersion: 'DOCKER|${webappImage}'
    }
  }
}