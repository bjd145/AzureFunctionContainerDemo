# AzureFunctionContainerDemo

* This repository shows how to use Azure Functions in a Container.
* The Function is triggered by Azure Event Grid and writes a Blob Storage Event to Azure Table Storage 
* The Function runs in Kubernetes and uses Istio/Envoy for ingress and egress control. Uses SSL to integrate with Event Grid