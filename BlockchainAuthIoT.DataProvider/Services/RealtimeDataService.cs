using LiteNetLib;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.DataProvider.Services
{
    public class RealtimeDataService
    {
        private readonly RealtimeServer _server;
        private readonly IPolicyVerificationService _policyVerification;
        private IConnection connection;
        private IModel channel;
        private readonly string[] queues;
        private readonly DeliveryMethod deliveryMethod;

        public RealtimeDataService(IConfiguration config, RealtimeServer server, IPolicyVerificationService policyVerification)
        {
            _server = server;
            _policyVerification = policyVerification;

            var realtimeOptions = config.GetSection("Realtime");

            var rabbitmqConnectionString = config.GetConnectionString("RabbitMQ");
            queues = realtimeOptions["Queues"].Split(',');
            deliveryMethod = (DeliveryMethod)Enum.Parse(typeof(DeliveryMethod), realtimeOptions["DeliveryMethod"]);

            var factory = new ConnectionFactory
            {
                Uri = new Uri(rabbitmqConnectionString)
            };

            // Try to asynchronously connect
            _ = Task.Run(async () =>
            {
                while (connection == null)
                {
                    try
                    {
                        connection = factory.CreateConnection();
                        Console.WriteLine("Connected to RabbitMQ");
                        SetupConsumers();
                    }
                    catch
                    {
                        Console.WriteLine("Cannot connect to RabbitMQ, trying again in 5 seconds...");
                        await Task.Delay(5000);
                    }
                }
            });
        }

        private void SetupConsumers()
        {
            channel = connection.CreateModel();
            foreach (var queue in queues)
            {
                channel.QueueDeclare(queue, true, false, false, null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, e) => NotifyClients(e.Body.ToArray(), queue, deliveryMethod);

                channel.BasicConsume(queue, true, consumer);
            }
        }

        private void NotifyClients(byte[] data, string resource, DeliveryMethod deliveryMethod)
        {
            foreach (var client in _server.Peers)
            {
                // TODO: Check policies

                try
                {
                    client.NetPeer.Send(data, deliveryMethod);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error when sending data to {client.NetPeer.EndPoint}");
                    Console.WriteLine(ex);
                }
            }
        }

        public void Dispose()
        {
            channel?.Dispose();
            connection?.Dispose();
        }
    }
}
