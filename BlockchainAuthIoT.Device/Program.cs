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

            channel.QueueDeclare("temperature",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            while (true)
            {
                var temperature = RandomDouble(rand, 20, 30);

                Reading message = new TemperatureReading
                {
                    Date = DateTime.Now,
                    Device = DEVICE_NAME,
                    Value = temperature
                };

                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);

                channel.BasicPublish("", "temperature", null, body);

                var humidity = RandomDouble(rand, 50, 60);

                message = new HumidityReading
                {
                    Date = DateTime.Now,
                    Device = DEVICE_NAME,
                    Value = humidity
                };

                json = JsonConvert.SerializeObject(message);
                body = Encoding.UTF8.GetBytes(json);

                channel.BasicPublish("", "humidity", null, body);
                Console.WriteLine($"Published temperature ({temperature} °C) and humidity ({humidity}%)");

                Thread.Sleep(sleepTime);
            }
        }

        private static double RandomDouble(Random random, double minimum, double maximum)
            => random.NextDouble() * (maximum - minimum) + minimum;
    }
}
