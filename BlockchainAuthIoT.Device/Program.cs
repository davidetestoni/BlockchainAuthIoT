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
        static readonly string queueName = "iot";
        static readonly Random rand = new();

        static void Main(string[] args)
        {
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

            while (true)
            {
                var temperature = rand.Next(20, 30);

                var message = new SampleData
                {
                    Date = DateTime.Now,
                    Name = "Temperature",
                    Device = "Sensor_1",
                    Data = Encoding.UTF8.GetBytes(temperature.ToString())
                };

                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                channel.BasicPublish("", queueName, null, body);

                Thread.Sleep(5000);
            }
        }
    }
}
