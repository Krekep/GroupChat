using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;

namespace MyClient
{
    public class Client
    {
        private Socket serverSocket;
        private IPEndPoint serverIP;
        private string name;
        public Client(string ipPort, string name)
        {
            serverIP = IPEndPoint.Parse(ipPort);
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.name = name;
        }

        public void Run()
        {
            serverSocket.Connect(serverIP);
            Thread serverListener = new Thread(HandleServer);
            serverListener.Start();

            HandleClient();
            serverSocket.Close();
        }

        private void HandleServer()
        {
            while (true)
            {
                byte[] buffer = new byte[1024];
                int messageSize = 0;
                try
                {
                    messageSize = serverSocket.Receive(buffer);
                }
                catch
                {
                    serverSocket.Close();
                    break;
                }
                var message = Encoding.UTF8.GetString(buffer, 0, messageSize);

                Console.WriteLine(message);
            }
        }

        private void HandleClient()
        {
            string message;
            message = $"[{name} joined to chat]\n";
            var messageBytes = Encoding.UTF8.GetBytes(message);
            serverSocket.Send(messageBytes, SocketFlags.None);
            while (true)
            {
                message = Console.ReadLine();
                if (message == "exit")
                    break;
                message = $"[{name}]: {message}";
                messageBytes = Encoding.UTF8.GetBytes(message);
                try
                {
                    serverSocket.Send(messageBytes, SocketFlags.None);
                }
                catch
                {
                    serverSocket.Close();
                    break;
                }
            }
        }
    }
}
