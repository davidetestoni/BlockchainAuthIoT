# Blockchain Auth IoT
Master's thesis project on the use of an **Ethereum Blockchain** to manage **Authentication** and **Access Control** when querying data from an **Edge IoT Network**.

## Local setup with docker-compose
First of all create a network called `iot` for our containers
```bash
docker network create iot
```
Then start the `mysql` container with this command (it will create a database called `iot`). The iotdataprovider container will take care of applying migrations when it starts.
```bash
docker run --name mysql --network iot -p 3306:3306 -e MYSQL_ROOT_PASSWORD=admin MYSQL_DATABASE=iot -d mysql:latest
```
You will need to configure access to an ethereum blockchain (for example through infura, or a local chain like `geth` or `ganache`).
You can do so by editing all instances of the `ConnectionStrings__Chain` environment variable in the `docker-compose.yml` file.

Finally run the build script
```bash
./run.bat
```
or on Linux
```bash
chmod +x run.sh
./run.sh
```

### Setting up the policies
Now access `http://localhost:4000` and `http://localhost:4001` and you will see the web-based clients for the admin and the signer.
You can use the premade wallets `admin.json` and `signer.json` on the Kovan testchain (the password is `password`). To refill them you can use this [free faucet](https://faucet.kovan.network/).

After deploying a contract, you can add some policies, for example
```
Resource: temperature/latest
Location: https://raw.githubusercontent.com/davidetestoni/BlockchainAuthIoT.Policies/688ac97c92aa749205f13d0c8ed4924e1c07a05f/temperature.json

Resource: humidity/latest
Location: https://raw.githubusercontent.com/davidetestoni/BlockchainAuthIoT.Policies/688ac97c92aa749205f13d0c8ed4924e1c07a05f/humidity.json

Resource: temperatureRT
Location: https://raw.githubusercontent.com/davidetestoni/BlockchainAuthIoT.Policies/aebc7f8957606fd26a6ffdf4e75054e1b623587c/temperatureRT.json

Resource: humidityRT
Location: https://raw.githubusercontent.com/davidetestoni/BlockchainAuthIoT.Policies/aebc7f8957606fd26a6ffdf4e75054e1b623587c/humidityRT.json
```
Alternatively, just load the premade contract on the Kovan testchain at `0x254ccedc328705d258661c3d1cb852a4f43763e5`.

### Querying the data
After the contract has been initialized and signed, the signer can send a query to one of the test endpoints to see the data.
```
http://dataprovider:3000/temperature/latest?count=10&deviceNames=Sensor_1,Sensor_2
http://dataprovider:3000/humidity/latest?count=10&deviceNames=Sensor_1
```

### Realtime data
In the realtime tab of the client, the signer can require connection to a realtime resource. By default, the server will be running on the host `dataprovider` on port 6390 (UDP). In addition, the client must provide the name of the desired resource, for example `temperatureRT` or `humidityRT` as configured above.

### Clearing the redis cache
If you need to clear the redis cache for any reason, you can `sh` into the container and then type
```bash
redis-cli
flushall
```
CTRL+D twice to exit the `redis-cli` program and the `sh` shell.
