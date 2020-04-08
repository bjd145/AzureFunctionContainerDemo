# Introduction

This is a simple repository to show how to use Keda with Azure Functions and HTTP Trigger with Auth level set to Function

# Challenge

Currently the Azure Function runtime does not expose host/function keys. How to generate and configure Functions to use Host Keys.
## Details
Azure Functions can store its host keys in a file if the environmental variable AzureWebJobsSecretStorageType is equal to files. The runtime will look for a host.json or httpTrigger.json in the Secrets directory under /azure-functions-host.  You can manually create keys and update the deployment to mount the files

# Steps
1. Deploy Keda to Kubernetes Cluster: func kubernetes install --namespace keda
2. Initialize Azure Function:  func init --worker-runtime dotnet --docker 
3. Do a dry run deployment to Kubernetes: func kubernetes deploy --name simple --registry bjd145.azurecr.io  --dry-run > deploy.yaml   
    * The func command can build and deploy containers to Kubernetes for you but we need to manipulate the artifacts so we have to do it manually today 
4. Build and Push Container: docker build -t bjd145.azurecr.io/simple:1.0 .  
5. Generate two keys: head -c 4096 /dev/urandom | sha256sum | cut -b1-32
    * One will be the Host Key and one will be the Function Key.
    * You can have more if you wish 
6. Add the following to your deploy.yaml file:
   ``` 
    ---
    apiVersion: v1
    kind: Secret
    metadata:
        name: azure-functions-secrets
    type: Opaque
    stringData:
        host.json: |-
        {
            "masterKey": {
                "name": "master",
                "value": "{REPLACEME}",
                "encrypted": false
            },
            "functionKeys": [ ]
        }
        httpTrigger.json: |-
        {
            "keys": [
                {
                    "name": "default",
                    "value": "{REPLACEME}",
                    "encrypted": false
                }
            ]
        }
    ```
7. Update the Container Spec to match your docker image and add the following:
    ```
        env:
        - name: AzureWebJobsSecretStorageType
          value: files
        volumeMounts:
        - name: secrets
          mountPath: "/azure-functions-host/Secrets"
          readOnly: true
      volumes:
      - name: secrets
        secret:
          secretName: azure-functions-secrets
    ```
8. Deploy to Kuberentes: kubectl apply -f ./deploy.yaml
9. Test with Curl: curl 'http://52.230.224.71/api/keda?name=brian&code={REPLACEME}'

# Reference
* https://github.com/Azure/azure-functions-host/issues/4147#issuecomment-477442831