#!/bin/bash

cd ../Source
func deploy --platform kubernetes --name eventgriddemo --min 1 --max 2 --registry bjd145.azurecr.io
cd ../Deploy
