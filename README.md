# Meraki vMX High Availability Solution
Architecture Overview
---------------------
![Image of vMXHA](https://storagegomez.blob.core.windows.net/public/images/vmx-ha-az.png)

This solution is the Azure Function implementation that will:
-------------------------------------------------------------
1. Get the current Active and StandBy vMX information (Resource Group, VM Name, Network Id, Device Serial and Private IP) from Blob Storage
2. Probe the Active vMX using the Meraki API to get Loss and Latency. \
    https://dashboard.meraki.com/api_docs/v0#get-the-uplink-loss-percentage-and-latency-in-milliseconds-for-a-wired-network-device
3. If loss is over the configured percentage level, update an array of Route Tables with next hop = the private IP of the StandBy vMX and update the state in Blob Storage
4. Log activity to Log Analytics (optional)

Pre-requisites
--------------
* Meraki dashboard account with licenses for 2 vMX appliances, API Key and a configured testing destination for the MX network (for example 1.1.1.1).
* 2 Meraki vMX appliances deployed in Azure
* Azure Active Directory App Registration (Service Principal) with RBAC rights to update the Route Tables
* Optional: Log Analytics Workspace where the Azure Function will log results.
* At least one defined Route Table with a route that has nexthop (Virtual Appliance) as the designated active vMX
* A Storage Account, Blob Container, and Blob with the vMX Pair information in the following json format:
 ```json
      {
	"activeNode":{ 
		"resourceGroup":"vMX resource group where the device is deployed",
		"vmName":"VM Name",
		"networkId":"from Meraki dashboard",
		"deviceSerial":"from Meraki dashboard",
		"privateIp":"private IP of the vMX"
	},
	"standByNode":{
		"resourceGroup":"vMX resource group where the device is deployed",
		"vmName":"VM Name",
		"networkId":"from Meraki dashboard",
		"deviceSerial":"from Meraki dashboard",
		"privateIp":"private IP of the vMX"
	}
}
```
