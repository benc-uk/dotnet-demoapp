@description('The name of the Managed Cluster resource.')
param resourceName string

@description('The location of AKS resource.')
param location string

@description('Optional DNS prefix to use with hosted Kubernetes API server FQDN.')
param dnsPrefix string

@description('Disk size (in GiB) to provision for each of the agent pool nodes. This value ranges from 0 to 1023. Specifying 0 will apply the default disk size for that agentVMSize.')
@minValue(0)
@maxValue(1023)
param osDiskSizeGB int = 0

@description('The version of Kubernetes.')
param kubernetesVersion string = '1.21.9'

@description('Network plugin used for building Kubernetes network.')
@allowed([
  'azure'
  'kubenet'
])
param networkPlugin string = 'azure'

@description('Boolean flag to turn on and off of RBAC.')
param enableRBAC bool = true

@description('Boolean flag to turn on and off of virtual machine scale sets')
param vmssNodePool bool = false

@description('Boolean flag to turn on and off of virtual machine scale sets')
param windowsProfile bool = false

@description('Enable private network access to the Kubernetes cluster.')
param enablePrivateCluster bool = false

@description('Boolean flag to turn on and off http application routing.')
param enableHttpApplicationRouting bool = true

@description('Boolean flag to turn on and off Azure Policy addon.')
param enableAzurePolicy bool = false

@description('Boolean flag to turn on and off secret store CSI driver.')
param enableSecretStoreCSIDriver bool = false

@description('Boolean flag to turn on and off omsagent addon.')
param enableOmsAgent bool = true

@description('Specify the region for your OMS workspace.')
param workspaceRegion string = 'West Europe'

@description('Specify the name of the OMS workspace.')
param workspaceName string

// @description('Specify the resource id of the OMS workspace.')
// param omsWorkspaceId string

@description('Select the SKU for your workspace.')
@allowed([
  'free'
  'standalone'
  'pernode'
])
param omsSku string = 'standalone'

@description('Specify the name of the Azure Container Registry.')
param acrName string

@description('The name of the resource group the container registry is associated with.')
param acrResourceGroup string

@description('The unique id used in the role assignment of the kubernetes service to the container registry service. It is recommended to use the default value.')
param guidValue string = newGuid()

module SolutionDeployment_20220412095433 'modules/nested_SolutionDeployment_20220412095433.bicep' = {
  name: 'SolutionDeployment-20220412095433'
  params: {
    workspaceRegion: workspaceRegion
    omsWorkspaceId: WorkspaceDeployment_20220412095433.outputs.workspaceId
  }
  dependsOn: [
    WorkspaceDeployment_20220412095433
  ]
}

module WorkspaceDeployment_20220412095433 'modules/nested_WorkspaceDeployment_20220412095433.bicep' = {
  name: 'WorkspaceDeployment-20220412095433'

  params: {
    workspaceRegion: workspaceRegion
    workspaceName: workspaceName
    omsSku: omsSku
  }
}

module ConnectAKStoACR_9fd845cd_6e76_40aa_99a0_6f0085d73073 'modules/nested_ConnectAKStoACR_9fd845cd_6e76_40aa_99a0_6f0085d73073.bicep' = {
  name: 'ConnectAKStoACR-9fd845cd-6e76-40aa-99a0-6f0085d73073'
  scope: resourceGroup(acrResourceGroup)
  params: {
    reference_parameters_resourceName_2021_07_01_identityProfile_kubeletidentity_objectId: reference(resourceName, '2021-07-01')
    resourceId_parameters_acrResourceGroup_Microsoft_ContainerRegistry_registries_parameters_acrName: resourceId(acrResourceGroup, 'Microsoft.ContainerRegistry/registries/', acrName)
    acrName: acrName
    guidValue: guidValue
  }
  dependsOn: [
    resourceName_resource
  ]
}

resource resourceName_resource 'Microsoft.ContainerService/managedClusters@2021-07-01' = {
  location: location
  name: resourceName
  properties: {
    kubernetesVersion: kubernetesVersion
    enableRBAC: enableRBAC
    dnsPrefix: dnsPrefix
    agentPoolProfiles: [
      {
        name: 'agentpool'
        osDiskSizeGB: osDiskSizeGB
        count: 3
        enableAutoScaling: true
        minCount: 1
        maxCount: 5
        vmSize: 'Standard_DS2_v2'
        osType: 'Linux'
        storageProfile: 'ManagedDisks'
        type: 'VirtualMachineScaleSets'
        mode: 'System'
        maxPods: 110
        availabilityZones: [
          '1'
          '2'
          '3'
        ]
        enableNodePublicIP: false
        tags: {}
      }
    ]
    networkProfile: {
      loadBalancerSku: 'standard'
      networkPlugin: networkPlugin
    }
    apiServerAccessProfile: {
      enablePrivateCluster: enablePrivateCluster
    }
    addonProfiles: {
      httpApplicationRouting: {
        enabled: enableHttpApplicationRouting
      }
      azurepolicy: {
        enabled: enableAzurePolicy
      }
      azureKeyvaultSecretsProvider: {
        enabled: enableSecretStoreCSIDriver
      }
      omsAgent: {
        enabled: enableOmsAgent
        config: {
          logAnalyticsWorkspaceResourceID: WorkspaceDeployment_20220412095433.outputs.workspaceId
        }
      }
    }
  }
  tags: {}
  sku: {
    name: 'Basic'
    tier: 'Free'
  }
  identity: {
    type: 'SystemAssigned'
  }
  dependsOn: [
    WorkspaceDeployment_20220412095433
  ]
}

module ClusterMonitoringMetricPulisherRoleAssignmentDepl_20220412095433 'modules/nested_ClusterMonitoringMetricPulisherRoleAssignmentDepl_20220412095433.bicep' = {
  name: 'ClusterMonitoringMetricPulisherRoleAssignmentDepl-20220412095433'
  scope: resourceGroup('dfd808dc-8a3f-4019-9c4d-9901ee75eca1', 'SREwithAzure')
  params: {
    reference_parameters_resourceName_addonProfiles_omsAgent_identity_objectId: resourceName_resource.properties
  }
  // dependsOn: [
  //   '/subscriptions/dfd808dc-8a3f-4019-9c4d-9901ee75eca1/resourceGroups/SREwithAzure/providers/Microsoft.ContainerService/managedClusters/srewithazure-aks'
  // ]
}

output controlPlaneFQDN string = resourceName_resource.properties.fqdn
