using BlockchainAuthIoT.DataProvider.Models.Realtime;
using BlockchainAuthIoT.Shared.Messages;
using LiteNetLib;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.DataProvider.Services
{
    public class RealtimeServer : IDisposable
    {
        private readonly ITokenVerificationService _tokenVerification;
        private readonly CancellationTokenSource cancellationTokenSource = new();
        private readonly EventBasedNetListener eventListener;
        private readonly NetManager server;
        private readonly Dictionary<(string ip, int port), RealtimePeer> peers = new();

        public IEnumerable<RealtimePeer> Peers => peers.Values;

        public RealtimeServer(IConfiguration config, ITokenVerificationService tokenVerification)
        {
            _tokenVerification = tokenVerification;

            var port = int.Parse(config.GetSection("Realtime")["Port"]);

            // Start the server on the port written in the configuration
            eventListener = new EventBasedNetListener();
            server = new NetManager(eventListener);
            server.Start(IPAddress.Any, IPAddress.IPv6Any, port);
            Console.WriteLine($"Realtime server listening on UDP port {port}");

            // On connection request, verify the token and accept
            eventListener.ConnectionRequestEvent += async request =>
            {
                Console.WriteLine($"{request.RemoteEndPoint} requested a connection");
                var message = JsonConvert.DeserializeObject<RealtimeAuthMessage>(request.Data.GetString());

                try
                {
                    var ip = request.RemoteEndPoint.Address.ToString();
                    var port = request.RemoteEndPoint.Port;
                    var contractAddress = await _tokenVerification.VerifyToken(message.Token);
                    var peer = request.Accept();
                    peers[(ip, port)] = new(peer, contractAddress, message.Resource);
                    Console.WriteLine($"{peer.EndPoint} connected");
                }
                catch
                {
                    Console.WriteLine($"{request.RemoteEndPoint} failed to connect. Invalid token.");
                    request.Reject();
                }
            };

            // Periodically refresh
            _ = Task.Run(async () =>
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    try
                    {
                        server.PollEvents();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to poll events from the realtime server: {ex.Message}");
                    }
                    
                    await Task.Delay(15);
                }
            });
        }

        public void Dispose()
        {
            cancellationTokenSource.Cancel();
            server.Stop();
            Console.WriteLine("Realtime server stopped");
        }
    }
}
