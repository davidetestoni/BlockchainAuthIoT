using BlockchainAuthIoT.Models;
using MySqlConnector;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Globalization;
using System.Text;
using System.Threading;

namespace BlockchainAuthIoT.DataController
{
    static class Program
    {
        static void Main(string[] args)
        {
            var mysqlConnectionString = Environment.GetEnvironmentVariable("MYSQL_CONN") ?? "server=mysql;user=root;password=admin;database=iot";
            var db = new MySqlConnection(mysqlConnectionString);
            var dbOpen = false;

            while (!dbOpen)
            {
                try
                {
                    db.Open();
                    dbOpen = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Cannot connect to the MySQL database, trying again in 5 seconds...");
                    Thread.Sleep(5000);
                }
            }

            var rabbitConnectionString = Environment.GetEnvironmentVariable("RABBITMQ_CONN") ?? "amqp://guest:guest@rabbitmq:5672";
            var factory = new ConnectionFactory
            {
                Uri = new Uri(rabbitConnectionString)
            };

            IConnection connection = null;

            while (connection == null)
            {
                try
                {
                    connection = factory.CreateConnection();
                }
                catch
                {
                    Console.WriteLine("Cannot connect to RabbitMQ, trying again in 5 seconds...");
                    Thread.Sleep(5000);
                }
            }

            var channel = connection.CreateModel();

            // TEMPERATURE
            channel.QueueDeclare("temperature", true, false, false, null);
            channel.QueueDeclare("humidity", true, false, false, null);

            var temperatureConsumer = new EventingBasicConsumer(channel);
            temperatureConsumer.Received += (sender, e) =>
            {
                var body = e.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Consumed message: {json}");

                var msg = JsonConvert.DeserializeObject<TemperatureReading>(json);
                var date = msg.Date.ToString("yyyy-MM-dd HH:mm:ss");
                var value = msg.Value;
                var device = msg.Device.Replace("'", "''");
                var query = $"INSERT INTO `iot`.`Temperature` (`Date`, `Device`, `Value`) VALUES ('{date}', '{device}', {value.ToString(CultureInfo.InvariantCulture)});";

                Console.WriteLine($"Executing query: {query}");
                using var command = new MySqlCommand(query, db);
                command.ExecuteNonQuery();
            };

            channel.BasicConsume("temperature", true, temperatureConsumer);

            // HUMIDITY
            var humidityConsumer = new EventingBasicConsumer(channel);
            humidityConsumer.Received += (sender, e) =>
            {
                var body = e.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Consumed message: {json}");

                var msg = JsonConvert.DeserializeObject<HumidityReading>(json);
                var date = msg.Date.ToString("yyyy-MM-dd HH:mm:ss");
                var value = msg.Value;
                var device = msg.Device.Replace("'", "''");
                var query = $"INSERT INTO `iot`.`Humidity` (`Date`, `Device`, `Value`) VALUES ('{date}', '{device}', {value.ToString(CultureInfo.InvariantCulture)});";

                Console.WriteLine($"Executing query: {query}");
                using var command = new MySqlCommand(query, db);
                command.ExecuteNonQuery();
            };

            channel.BasicConsume("humidity", true, humidityConsumer);

            while (true)
            {

            }
        }
    }
}
