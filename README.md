# Blockchain Auth IoT
Master's thesis project on the use of an **Ethereum Blockchain** to manage **Authentication** and **Access Control** when querying data from an **Edge IoT Network**.

### Local setup
First of all create a network called `iot` for our containers
```bash
docker network create iot
```
Then start the `mysql` container
```bash
docker run --name mysql --network iot -p 3306:3306 -e MYSQL_ROOT_PASSWORD=admin -d mysql:latest
```
Now connect to the database (e.g. using MySQL Workbench) and execute this query to recreate the schema for the test environment
```sql
CREATE SCHEMA `iot`;

CREATE TABLE `iot`.`Temperature` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `Date` DATETIME NOT NULL,
  `Device` VARCHAR(45) NOT NULL,
  `Value` DOUBLE NOT NULL,
  PRIMARY KEY (`Id`));

CREATE TABLE `iot`.`Humidity` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `Date` DATETIME NOT NULL,
  `Device` VARCHAR(45) NOT NULL,
  `Value` DOUBLE NOT NULL,
  PRIMARY KEY (`Id`));
```
Now switch into the `geth-clique-linux` folder and start the testchain
```bash
docker build -t geth .
docker run --name chain --network iot -p 30303:30303 -p 8545:8545 -it -d geth
```
Finally run the build script
```bash
./run.bat
```

### Setting up the policies
Now access `http://localhost:4000` and you will see the web-based client. After deploying a contract, you can add some policies, for example
```
Resource: temperature/latest
Location: https://raw.githubusercontent.com/davidetestoni/BlockchainAuthIoT.Policies/master/temperature.json

Resource: humidity/latest
Location: https://raw.githubusercontent.com/davidetestoni/BlockchainAuthIoT.Policies/master/humidity.json
```

### Querying the data
After the contract has been initialized and signed, the signer can send a query to one of the test endpoints to see the data.
```
http://dataprovider:3000/temperature/latest?count=10&deviceNames=Sensor_1,Sensor_2
http://dataprovider:3000/humidity/latest?count=10&deviceNames=Sensor_1
```
Here is a list of the private keys of the unlocked accounts in the testchain. In the future, these will be automatically obtained from the keystore file after typing the correct password.
```
PUB|PRIV
0x12890d2cce102216644c59dae5baed380d84830c|0xb5b1870957d373ef0eeffecc6e4812c0fd08f554b37b233526acc331bf1544f7
0x13f022d72158410433cbd66f5dd8bf6d2d129924|0xe8f16cd59a70e7fa6efd9320aa9d40d598ecdee6ee9cb6f0f3bf65047b68fd99
0x20cbd8342f212af402bbd3385a94e9a7f96d2604|0xa21d2e24645bc5a2953891cc19c2be410831abbc5dd3a73d1b7f97ddca77219f
0xa28a48ed350e7cdd58f14276fc32645610a0c703|0xd04b113487a9190b17ab0b540ed49cb556430f93f45be2574df69936a28cee77
0x07c5c99b45548ef33cd5a6618cc121fde7337cf6|0x6bcc86014fb81679c1bbc6bc9175b2da1c39519d96f5b3372f368ab56ff42237
```

### Clearing the redis cache
If you need to clear the redis cache for any reason, you can `sh` into the container and then type
```bash
redis-cli
flushall
```
CTRL+D twice to exit the `redis-cli` program and the `sh` shell.
