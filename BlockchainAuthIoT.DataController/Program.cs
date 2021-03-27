using RabbitMQ.Client;
using System;

namespace BlockchainAuthIoT.DataController
{
    class Program
    {
        static void Main(string[] args)
        {
            ConnectionFactory factory = new ConnectionFactory();

            factory.UserName = "guest";
            factory.Password = "guest";
            factory.VirtualHost = "/";
            factory.HostName = "localhost";

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            // TODO: Implement logic to subscribe to new messages and store the valid ones into the database

            channel.Close();
            connection.Close();

            channel.Dispose();
            connection.Dispose();
        }
    }
}
