@description('Specify the region for your OMS workspace.')
param workspaceRegion string

@description('Specify the name of the OMS workspace.')
param workspaceName string

@description('Select the SKU for your workspace.')
param omsSku string = 'PerGB2018'

resource workspaceName_resource 'Microsoft.OperationalInsights/workspaces@2015-11-01-preview' = {
  location: workspaceRegion
  name: workspaceName
  properties: {
    sku: {
      name: omsSku
    }
  }
}

output workspaceId string = workspaceName_resource.id
