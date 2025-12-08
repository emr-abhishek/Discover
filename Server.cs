using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AB_EIP.Model
{
    public class Server
    {
        private static Server _instance;
        public static Server Instance           // Public static property to access the single instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Server();
                }
                return _instance;
            }
        }

        private TcpListener _listener;
        private CancellationTokenSource _cts;
        private Task _serverTask;

        public bool EIPServerRunning { get; set; }
        private readonly CommandHandler _commandHandler = new CommandHandler();    // Command handler for managing command

        public Server()
        {
            ConnectStatus.ConnStatus += ConnectionState;
        }

        private void ConnectionState(string connectState)
        {
            
        }

        public async Task StartServer()
        {
            if (EIPServerRunning) return;

            try
            {
                string ipAddress = TCPLayer.Instance.Ip;
                int port=TCPLayer.Instance.Port;

                _cts = new CancellationTokenSource();
                _listener = new TcpListener(IPAddress.Parse(ipAddress), port);
                _listener.Start();
                EIPServerRunning = true;
                LogHandler.LogMessage($"Listening on: {ipAddress}:{port}", null, 0);
                ConnectStatus.NotifyConnStatus("3"); // listening

                _serverTask = Task.Run(async () =>
                {
                    while (!_cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            var client = await _listener.AcceptTcpClientAsync();
                            string clientIp = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                            LogHandler.LogMessage($"Accepted connection from: {clientIp}", null, 0);
                            ConnectStatus.NotifyConnStatus("1"); // client connected
                            _ = HandleClient(client, _cts.Token);
                        }
                        catch (ObjectDisposedException)
                        {
                            break; // listener stopped
                        }
                    }
                }, _cts.Token);
            }
            catch (Exception ex)
            {
                ConnectStatus.NotifyConnStatus("2");
                MessageBox.Show($"Error starting server: {ex.Message}");
                throw;
            }
        }

        private async Task HandleClient(TcpClient client, CancellationToken token)
        {
            using (client)
            using (var stream = client.GetStream())
            {
                byte[] buffer = new byte[1024];
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);
                        if (bytesRead == 0) break; // disconnected

                        LogHandler.LogMessage("Received", buffer, bytesRead);
                        byte[] response = _commandHandler.DataProcess(buffer, bytesRead);
                        await stream.WriteAsync(response, 0, response.Length, token);

                        if (response.Length > 0)
                        {
                            LogHandler.LogMessage("Sent", response, response.Length);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Client error: {ex.Message}");
                }
            }
        }


        public async Task StopServer()
        {
            try
            {
                await Task.Run(() =>
                {
                    _listener.Stop();
                    ConnectStatus.NotifyConnStatus("0");
                    EIPServerRunning = false;
                    LogHandler.LogMessage("Server Stopped", null, 0);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }


    }
}
