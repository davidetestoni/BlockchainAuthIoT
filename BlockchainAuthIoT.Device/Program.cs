using BlockchainAuthIoT.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading;

namespace BlockchainAuthIoT.Device
{
    static class Program
    {
        private static readonly string queueName = "iot";
        private static readonly Random rand = new();

        static void Main(string[] args)
        {
            var DEVICE_NAME = Environment.GetEnvironmentVariable("DEVICE_NAME") ?? "Sensor_1";
            var DEVICE_SLEEP = Environment.GetEnvironmentVariable("DEVICE_SLEEP") ?? "5000";
            
            if (!int.TryParse(DEVICE_SLEEP, out int sleepTime))
            {
                Console.WriteLine("DEVICE_SLEEP must contain an integer value! Using default (5000 ms)");
                sleepTime = 5000;
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

            using var channel = connection.CreateModel();

            channel.QueueDeclare(queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            while (true)
            {
                var temperature = rand.Next(20, 30);

                var message = new SampleData
                {
                    Date = DateTime.Now,
                    Name = "Temperature",
                    Device = DEVICE_NAME,
                    Data = Encoding.UTF8.GetBytes(temperature.ToString())
                };

                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);

                channel.BasicPublish("", queueName, null, body);
                Console.WriteLine($"Published {json}");

                Thread.Sleep(sleepTime);
            }
        }
    }
}
