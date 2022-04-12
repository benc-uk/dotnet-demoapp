@description('Specify the region for your OMS workspace.')
param workspaceRegion string

@description('Specify the resource id of the OMS workspace.')
param omsWorkspaceId string

resource ContainerInsights_omsWorkspaceId_8 'Microsoft.OperationsManagement/solutions@2015-11-01-preview' = {
  location: workspaceRegion
  name: 'ContainerInsights(${split(omsWorkspaceId, '/')[8]})'
  properties: {
    workspaceResourceId: omsWorkspaceId
  }
  plan: {
    name: 'ContainerInsights(${split(omsWorkspaceId, '/')[8]})'
    product: 'OMSGallery/ContainerInsights'
    promotionCode: ''
    publisher: 'Microsoft'
  }
}