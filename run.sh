# ===================
# Davide Testoni 2021
# ===================

# Build the Device and the docker image
cd BlockchainAuthIoT.Device
dotnet publish -c Release
docker build -t davidetestoni/iotdevice:latest .
cd ..

# Build the Data Controller and the docker image
cd BlockchainAuthIoT.DataController
dotnet publish -c Release
docker build -t davidetestoni/iotdatacontroller:latest .
cd ..

# Build the Data Provider and the docker image
cd BlockchainAuthIoT.DataProvider
dotnet publish -c Release
docker build -t davidetestoni/iotdataprovider:latest .
cd ..

# Build the Client and the docker image
cd BlockchainAuthIoT.Client
dotnet publish -c Release
docker build -t davidetestoni/iotclient:latest .
cd ..

# Start compose
docker-compose up
