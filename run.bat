@echo off
echo ===================
echo Davide Testoni 2021
echo ===================

echo Building the Device and the docker image
@echo on
cd BlockchainAuthIoT.Device
dotnet publish -c Release
docker build -t davidetestoni/iotdevice:latest .
cd ..

@echo off
echo Building the Data Controller and the docker image
@echo on
cd BlockchainAuthIoT.DataController
dotnet publish -c Release
docker build -t davidetestoni/iotdatacontroller:latest .
cd ..

@echo off
echo Building the Data Provider and the docker image
@echo on
cd BlockchainAuthIoT.DataProvider
dotnet publish -c Release
docker build -t davidetestoni/iotdataprovider:latest .
cd ..

@echo off
echo Building the Client and the docker image
@echo on
cd BlockchainAuthIoT.Client
dotnet publish -c Release
docker build -t davidetestoni/iotclient:latest .
cd ..

@echo off
echo Starting compose
@echo on
docker-compose up
