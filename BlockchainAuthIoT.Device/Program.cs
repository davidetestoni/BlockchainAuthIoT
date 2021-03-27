using RabbitMQ.Client;

namespace BlockchainAuthIoT.Device
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

            // TODO: Implement logic to send messages

            channel.Close();
            connection.Close();

            channel.Dispose();
            connection.Dispose();
        }
    }
}
