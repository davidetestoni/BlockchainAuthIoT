using BlockchainAuthIoT.Models;
using MySqlConnector;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace BlockchainAuthIoT.DataController
{
    static class Program
    {
        static readonly string queueName = "iot";

        static void Main(string[] args)
        {
            using var db = new MySqlConnection("server=localhost;user=root;password=admin;database=iot");
            db.Open();

            var factory = new ConnectionFactory
            {
                Uri = new Uri("amqp://guest:guest@localhost:5672")
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

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
