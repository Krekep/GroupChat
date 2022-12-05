using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO.Pipes;
using System.Threading;

namespace MyServer
{
    public class Server
    {
        private Socket serverSocket;
        private HashSet<Socket> connectedClients;
        private List<Socket> diedClients;
        public Server(string ipPort)
        {
            IPEndPoint ipPoint = IPEndPoint.Parse(ipPort);
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(ipPoint);

            diedClients = new List<Socket>();
            connectedClients = new HashSet<Socket>();
        }
        
        public void Run()
        {
            serverSocket.Listen(10);
            while (true)
            {
                var newClient = serverSocket.Accept();
                Console.WriteLine($"Client {newClient.RemoteEndPoint} connected");

                Thread clientThread = new Thread(HandleClient);
                 
                connectedClients.Add(newClient);
                clientThread.Start(newClient);
            }
        }

        private void HandleClient(object client)
        {
            HandleClient((Socket)client);
        }

        private void HandleClient(Socket client)
        {
            while (true)
            {
                byte[] buffer = new byte[1024];
                int messageSize = 0;
                try
                {
                    messageSize = client.Receive(buffer);
                }
                catch
                {
                    client.Close();
                    Monitor.Enter(connectedClients);
                    connectedClients.Remove(client);
                    Monitor.Exit(connectedClients);
                    break;
                }
                var message = Encoding.UTF8.GetString(buffer, 0, messageSize);

                Console.WriteLine($"Client[{client.LocalEndPoint}]: {message}");

                foreach (var otherClient in connectedClients)
                {
                    if (otherClient.Equals(client))
                        continue;
                    try
                    {
                        otherClient.Send(buffer, messageSize, SocketFlags.None);
                    }
                    catch
                    {
                        Monitor.Enter(diedClients);
                        diedClients.Add(otherClient);
                        Monitor.Exit(diedClients);
                    }
                }

                ClearDiedClientList();
                }
        }

        private void ClearDiedClientList()
        {
            Monitor.Enter(diedClients);
            foreach (var client in diedClients)
            {
                if (diedClients.Contains(client))
                {
                    client.Close();
                    diedClients.Remove(client);
                }
            }
            diedClients.Clear();
            Monitor.Exit(diedClients);
        }
    }
}
