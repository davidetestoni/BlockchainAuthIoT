rem ===================
rem Davide Testoni 2021
rem ===================

rem Build the Device and the docker image
cd BlockchainAuthIoT.Device
dotnet publish -c Release
docker build -t davidetestoni/iotdevice:latest .
cd ..

rem Start compose
docker-compose up
