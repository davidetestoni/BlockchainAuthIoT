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
Now access `http://localhost:4000` and you will see the web-based client. After deploying a contract, you can send a query to an endpoint like `http://dataprovider:3000/data/getall` in order to see the data.
