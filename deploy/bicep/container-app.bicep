param location string = resourceGroup().location

param appName string = 'srewithazure-containerapp'
param environmentName string = 'srewithazure-environment'
param logAnalyticsWorkspaceName string = '${appName}-logs'
param image string = 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'
// Azure Container registry
@description('ACR password')
@secure()
param acrPassword string

@description('ACR server')
param acrServer string = 'srewithazure.azurecr.io'

@description('ACR username')
param acrUsername string = 'srewithazure'

@description('existing revision name')
param existingRevisionName string

@description('New revision number')
param newRevisionNumber string

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2020-03-01-preview' = {
  name: logAnalyticsWorkspaceName
  location: location
  properties: any({
    retentionInDays: 30
    features: {
      searchVersion: 1
    }
    sku: {
      name: 'PerGB2018'
    }
  })
}

resource environment 'Microsoft.App/managedEnvironments@2022-01-01-preview' = {
  name: environmentName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsWorkspace.properties.customerId
        sharedKey: logAnalyticsWorkspace.listKeys().primarySharedKey
      }
    }
  }
}

resource containerApp 'Microsoft.App/containerApps@2022-01-01-preview' = {
  name: appName
  location: location
  properties: {
    managedEnvironmentId: environment.id
    configuration: {
      secrets: [
        {
            name: 'acr-password'
            value: acrPassword
        }
      ]
      registries: [
        {
          server: acrServer
          username: acrUsername
          passwordSecretRef: 'acr-password'
        } 
      ] 
      ingress: {
        external: true
        targetPort: 5000
        traffic:[
          {
            latestRevision: true
            weight: 0
          }
          {
            latestRevision: false
            revisionName: existingRevisionName
            weight: 100
          }
        ]
      }
      activeRevisionsMode:'multiple'
      
    }
    template: {
      revisionSuffix: newRevisionNumber
      containers: [
        {
          image: image
          name: appName
        }
      ]
    }
  }
}

output fqdn string = containerApp.properties.configuration.ingress.fqdn
output newRevisionName string = containerApp.properties.configuration.ingress.traffic[1].revisionName
