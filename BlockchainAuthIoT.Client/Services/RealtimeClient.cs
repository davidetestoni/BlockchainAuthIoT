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
            
            listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
            {
                MessageReceived?.Invoke(this, dataReader.GetString());
                dataReader.Recycle();
            };

            _ = Task.Run(async () =>
            {
                while (true)
                {
                    if (client.IsRunning)
                    {
                        client.PollEvents();
                        await Task.Delay(15);
                    }
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
