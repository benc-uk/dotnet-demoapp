@description('Specify the region for your OMS workspace.')
param workspaceRegion string

@description('Specify the name of the OMS workspace.')
param workspaceName string

@description('Select the SKU for your workspace.')
@allowed([
  'free'
  'standalone'
  'pernode'
])
param omsSku string

resource workspaceName_resource 'Microsoft.OperationalInsights/workspaces@2015-11-01-preview' = {
  location: workspaceRegion
  name: workspaceName
  properties: {
    sku: {
      name: omsSku
    }
  }
}