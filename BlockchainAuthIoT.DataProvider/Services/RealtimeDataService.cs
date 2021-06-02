using BlockchainAuthIoT.DataProvider.Exceptions;
using BlockchainAuthIoT.DataProvider.Models.Policies.Rules;
using BlockchainAuthIoT.Models;
using LiteNetLib;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.DataProvider.Services
{
    public class RealtimeDataService
    {
        private readonly RealtimeServer _server;
        private readonly IPolicyVerificationService _policyVerification;
        private IConnection connection;
        private IModel channel;
        private readonly string[] queues = new[] { "temperatureRT", "humidityRT" };
        private readonly DeliveryMethod deliveryMethod;

        public RealtimeDataService(IConfiguration config, RealtimeServer server, IPolicyVerificationService policyVerification)
        {
            _server = server;
            _policyVerification = policyVerification;

            var realtimeOptions = config.GetSection("Realtime");

            var rabbitmqConnectionString = config.GetConnectionString("RabbitMQ");
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
                consumer.Received += async (sender, e) => await NotifyClients(e.Body.ToArray(), queue, deliveryMethod);

                channel.BasicConsume(queue, true, consumer);
            }
        }

        private async Task NotifyClients(byte[] data, string resource, DeliveryMethod deliveryMethod)
        {
            foreach (var client in _server.Peers)
            {
                // If the client didn't ask for this resource, continue
                if (client.Resource != resource)
                {
                    continue;
                }

                // Check if the client has access to this resource and to the devices
                try
                {
                    var reading = JsonConvert.DeserializeObject<Reading>(Encoding.UTF8.GetString(data));
                    await _policyVerification.VerifyPolicy(client.ContractAddress, resource, new List<PolicyRule>
                    {
                        new StringPolicyRule("devices", allowed =>
                        {
                            var allowedList = allowed.Split(',');
                            return allowedList.Contains(reading.Device);
                        })
                    });
                }
                catch (PolicyRuleVerificationException)
                {
                    // We violate a policy rule, so we just skip sending this piece of data
                }
                catch (Exception ex)
                {
                    // If it's any other exception, send the error to the client
                    SendError(client.NetPeer, ex);
                }

                SendData(client.NetPeer, data);
            }
        }

        private void SendData(NetPeer client, byte[] data)
        {
            try
            {
                client.Send(data, deliveryMethod);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error when sending data to {client.EndPoint}");
                Console.WriteLine(ex);
            }
        }

        private void SendError(NetPeer client, Exception ex)
            => SendData(client, Encoding.UTF8.GetBytes(ex.Message));

        public void Dispose()
        {
            channel?.Dispose();
            connection?.Dispose();
        }
    }
}
