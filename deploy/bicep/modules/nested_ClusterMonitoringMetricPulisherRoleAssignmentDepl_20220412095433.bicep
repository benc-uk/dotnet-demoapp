param reference_parameters_resourceName_addonProfiles_omsAgent_identity_objectId object

resource srewithazureunai_aks_Microsoft_Authorization_f4340940_1c51_4c3b_8591_afb6af835fbd 'Microsoft.ContainerService/managedClusters/providers/roleAssignments@2018-01-01-preview' = {
  name: 'srewithazureunai-aks/Microsoft.Authorization/f4340940-1c51-4c3b-8591-afb6af835fbd'
  properties: {
    roleDefinitionId: '/subscriptions/${subscription().subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/3913510d-42f4-4e42-8a64-420c390055eb'
    principalId: reference_parameters_resourceName_addonProfiles_omsAgent_identity_objectId.addonProfiles.omsAgent.identity.objectId
    scope: '/subscriptions/dfd808dc-8a3f-4019-9c4d-9901ee75eca1/resourceGroups/srewithazureunai/providers/Microsoft.ContainerService/managedClusters/srewithazureunai-aks'
  }
}
