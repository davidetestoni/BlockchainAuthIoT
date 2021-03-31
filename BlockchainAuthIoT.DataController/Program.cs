using BlockchainAuthIoT.Models;
using MySqlConnector;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

namespace BlockchainAuthIoT.DataController
{
    static class Program
    {
        static readonly string queueName = "iot";

        static void Main(string[] args)
        {
            var db = new MySqlConnection("server=mysql;user=root;password=admin;database=iot");
            var dbOpen = false;

            while (!dbOpen)
            {
                try
                {
                    db.Open();
                    dbOpen = true;
                }
                catch
                {
                    Console.WriteLine("Cannot connect to the MySQL database, trying again in 5 seconds...");
                    Thread.Sleep(5000);
                }
            }

            var factory = new ConnectionFactory
            {
                Uri = new Uri("amqp://guest:guest@rabbitmq:5672")
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

            channel.QueueDeclare(queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (sender, e) =>
            {
                var body = e.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                Console.WriteLine(json);

                var msg = JsonConvert.DeserializeObject<SampleData>(json);
                var date = msg.Date.ToString("yyyy-MM-dd HH:mm:ss");
                var data = Encoding.UTF8.GetString(msg.Data).Replace("'", "''");
                var name = msg.Name.Replace("'", "''");
                var device = msg.Device.Replace("'", "''");
                var query = $"INSERT INTO `iot`.`Data` (`Date`, `Name`, `Device`, `Data`) VALUES ('{date}', '{name}', '{device}', '{data}');";

                Console.WriteLine($"Executing query: {query}");
                using var command = new MySqlCommand(query, db);
                command.ExecuteNonQuery();
            };

            channel.BasicConsume(queueName, true, consumer);
            
            while (true)
            {

            }
        }
    }
}
