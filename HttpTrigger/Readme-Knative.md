# Introduction

This is a simple repository to show how to use Knative with Azure Functions and HTTP Trigger with Auth level set to Function

# Challenge

Currently the Azure Function runtime does not expose host/function keys. How to generate and configure Functions to use Host Keys.
## Details
Azure Functions can store its host keys in a file if the environmental variable AzureWebJobsSecretStorageType is equal to files. The runtime will look for a host.json or httpTrigger.json in the Secrets directory under /azure-functions-host.  You can manually create keys and update the deployment to mount the files

# Steps
0. Create an Azure Storage Account and copy the Acces Key
1. Deploy Knative Servicing and it's required dependencies - https://knative.dev/docs/install/any-kubernetes-cluster/#installing-the-serving-component
    ## Core Components 
    * kubectl apply --filename https://github.com/knative/serving/releases/download/v0.13.0/serving-crds.yaml
    * kubectl apply --filename https://github.com/knative/serving/releases/download/v0.13.0/serving-core.yaml

    ## Istio
    * curl -L https://git.io/getLatestIstio | sh -
    * cd istio-${ISTIO_VERSION}
    * for i in install/kubernetes/helm/istio-init/files/crd*yaml; do kubectl apply -f $i; done
    * kubectl create ns istio-system
    * kubectl apply --filename ./deploy/istio-lean.yaml
    * kubectl apply --filename https://github.com/knative/serving/releases/download/v0.13.0/serving-istio.yaml
    * kubectl --namespace istio-system get service istio-ingressgateway
        * Copy External IP Address and Create DNS A Record pointting *.knative.bjdcsa.cloud
    * kubectl patch configmap/config-domain \
        --namespace knative-serving \
        --type merge \
        --patch '{"data":{"knative.bjdcsa.cloud":""}}'
2. cd src
3. docker build -t {docker-repo}/knative:1.4 . 
4. docker push {docker-repo}/knative:1.4 
    * If you want to use a private repo, please follow - https://knative.dev/docs/serving/deploying/private-registry/
5. Generate two keys: head -c 4096 /dev/urandom | sha256sum | cut -b1-32
    * One will be the Host Key and one will be the Function Key.
    * You can have more if you wish 
6. Update the ../deploy/deploy-knative.yaml
    * Update AzureWebJobsStorage with the correct Access Keys for your Storage Account
    * Update image with the proper docker repository
    * Replace the HTTP and Master Keys with output from step 5
8. Deploy to Kuberentes: kubectl apply -f ./deploy-knative.yaml
9. kubectl get ksvc 
    * This will get you the external URL to test aganist 
9. Test with hey (https://github.com/rakyll/hey):  
    * Example: hey -c 50 -z 30s "http://azure-function-test.default.knative.bjdcsa.cloud/api/keda?name=brian&code=abcxyz"
