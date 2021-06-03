using BlockchainAuthIoT.Shared.Messages;
using LiteNetLib;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.Client.Services
{
    public class RealtimeClient
    {
        private readonly EventBasedNetListener listener;
        private readonly NetManager client;

        public event EventHandler<string> MessageReceived;
        public bool Connected => client.IsRunning && client.ConnectedPeersCount > 0;

        public RealtimeClient()
        {
            listener = new EventBasedNetListener();
            client = new NetManager(listener);

            listener.PeerConnectedEvent += peer =>
            {
                Console.WriteLine($"Connected to {peer.EndPoint}");
            };

            listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
            {
                var message = dataReader.GetString();
                Console.WriteLine($"Got message: {message}");
                MessageReceived?.Invoke(this, message);
                dataReader.Recycle();
            };

            _ = Task.Run(async () =>
            {
                while (true)
                {
                    if (client.IsRunning)
                    {
                        try
                        {
                            client.PollEvents();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }

                    await Task.Delay(150);
                }
            });
        }

        public void Connect(string host, int port, string resource, string token)
        {
            // If a client was already running, stop it
            Disconnect();

            var authMessage = new RealtimeAuthMessage
            {
                Resource = resource,
                Token = token
            };

            client.Start();
            client.Connect(host, port, JsonConvert.SerializeObject(authMessage));
        }

        public void Disconnect()
        {
            if (client.IsRunning)
            {
                client.DisconnectAll();
                client.Stop();
            }
        }
    }
}
