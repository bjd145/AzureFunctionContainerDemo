#!/bin/bash
kubectl apply -f <(istioctl kube-inject -f ./deployment.yaml)
