param reference_parameters_resourceName_2021_07_01_identityProfile_kubeletidentity_objectId object
param resourceId_parameters_acrResourceGroup_Microsoft_ContainerRegistry_registries_parameters_acrName string

@description('Specify the name of the Azure Container Registry.')
param acrName string

@description('The unique id used in the role assignment of the kubernetes service to the container registry service. It is recommended to use the default value.')
param guidValue string

resource acrName_Microsoft_Authorization_guidValue 'Microsoft.ContainerRegistry/registries/providers/roleAssignments@2018-09-01-preview' = {
  name: '${acrName}/Microsoft.Authorization/${guidValue}'
  properties: {
    principalId: reference_parameters_resourceName_2021_07_01_identityProfile_kubeletidentity_objectId.identityProfile.kubeletidentity.objectId
    principalType: 'ServicePrincipal'
    roleDefinitionId: '/subscriptions/${subscription().subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/b24988ac-6180-42a0-ab88-20f7382dd24c'
    scope: resourceId_parameters_acrResourceGroup_Microsoft_ContainerRegistry_registries_parameters_acrName
  }
}