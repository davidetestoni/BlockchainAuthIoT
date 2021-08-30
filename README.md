# Blockchain Auth IoT
Master's thesis project on the use of an **Ethereum Smart Contract** to manage **Authentication** and **Access Control** when querying data from an IoT network at the **Edge**.

## Local setup with docker-compose
First of all create a network called `iot` for your containers
```text
docker network create iot
```
Then create a `mysql` container with this command (it will create a database called `iot`). The iotdataprovider container will take care of applying migrations when it starts.
```text
docker run --name mysql --network iot -p 3306:3306 -e MYSQL_ROOT_PASSWORD=admin -e MYSQL_DATABASE=iot -d mysql:latest
```
Configure access to a web3 provider (for example through infura, or a local web3 provider like `geth` or `ganache`).
This can be done by editing all instances of the `ConnectionStrings__Chain` environment variable in the `docker-compose.yml` file.

Run the build script
```text
./run.bat
```
or on Linux
```text
chmod +x run.sh
./run.sh
```

## Deployment on a kubernetes cluster
First of all, edit the configuration files in the `k8s` folder according to your needs and then apply them
```text
cd k8s
kubectl apply -f . --namespace=blockchain
```
Then deploy as many sample IoT devices as you need (outside the cluster) by running this command and editing the environmental variables and replacing `<ip>` with the IP of the cluster. By default, the RabbitMQ service is exposed on port 30002 of the cluster and the default access credentials are `guest:guest`. Change these according to your specific configuration.
```text
docker run -it -e DEVICE_NAME=Sensor_1 -e DEVICE_SLEEP=5000 -e RABBITMQ_CONN="amqp://guest:guest@<ip>:30002" davidetestoni/iotdevice:latest
docker run -it -e DEVICE_NAME=Sensor_2 -e DEVICE_SLEEP=6000 -e RABBITMQ_CONN="amqp://guest:guest@<ip>:30002" davidetestoni/iotdevice:latest
```
Finally, deploy two instances of the client application. Configure the connection string to the web3 provider you intend to use.
```text
docker run -it -e ASPNETCORE_ENVIRONMENT=Release -e ConnectionStrings__Chain="https://kovan.infura.io/v3/2c64819f193e4fdca3ca3520ab1a2b1b" -p 4000:4000 davidetestoni/iotclient:latest
docker run -it -e ASPNETCORE_ENVIRONMENT=Release -e ConnectionStrings__Chain="https://kovan.infura.io/v3/2c64819f193e4fdca3ca3520ab1a2b1b" -p 4000:4001 davidetestoni/iotclient:latest
```

## Contract setup
Now access `http://localhost:4000` and `http://localhost:4001` and you will see two instances of the web-based client that you can use to impersonate the owner and the signer/user.
You can use the premade wallets `admin.json` and `signer.json` on the Kovan testchain (the password is `password`) that are provided inside the client's container. To refill them you can use this [free faucet](https://faucet.kovan.network/).

After deploying a contract, you can add some policies, for example
```text
Resource: temperature/latest
Location: https://raw.githubusercontent.com/davidetestoni/BlockchainAuthIoT.Policies/688ac97c92aa749205f13d0c8ed4924e1c07a05f/temperature.json

Resource: humidity/latest
Location: https://raw.githubusercontent.com/davidetestoni/BlockchainAuthIoT.Policies/688ac97c92aa749205f13d0c8ed4924e1c07a05f/humidity.json

Resource: temperatureRT
Location: https://raw.githubusercontent.com/davidetestoni/BlockchainAuthIoT.Policies/aebc7f8957606fd26a6ffdf4e75054e1b623587c/temperatureRT.json

Resource: humidityRT
Location: https://raw.githubusercontent.com/davidetestoni/BlockchainAuthIoT.Policies/aebc7f8957606fd26a6ffdf4e75054e1b623587c/humidityRT.json
```
Alternatively, just load the premade contract on the Kovan testchain at `0x66c9886b18fe944078fe3eb3c60315a4474796f1`.

## Querying the data
After the contract has been initialized and signed, the user appointed by the signer (for the premade contract it's the signer itself) can send a query to one of the test endpoints to see the data. On a local deployment via docker-compose you can use these URLs
```text
http://dataprovider:3000/temperature/latest?count=10&deviceNames=Sensor_1,Sensor_2
http://dataprovider:3000/humidity/latest?count=10&deviceNames=Sensor_1
```
On a kubernetes deployment you can use these URLs (replace `<ip>` with the IP of the cluster).
```text
http://<ip>:30000/temperature/latest?count=10&deviceNames=Sensor_1,Sensor_2
http://<ip>:30000/humidity/latest?count=10&deviceNames=Sensor_1
```

### Realtime data
In the realtime tab of the client, the user can require connection to a realtime resource. On a local deployment via docker-compose, the server will be running on the host `dataprovider` on port 6390 (UDP). On a kubernetes deployment, it will be exposed on port 30001 (UDP) reachable via the IP of the cluster. In addition, the client must provide the name of the desired resource, for example `temperatureRT` or `humidityRT` as configured in the Smart Contract.

### Clearing the redis cache
If you need to clear the redis cache for any reason, you can `sh` into the container and then type
```text
redis-cli
flushall
```
CTRL+D twice to exit the `redis-cli` program and the `sh` shell.
