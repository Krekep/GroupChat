using MyClient;
using MyServer;
using System;

namespace GroupChat
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var mode = Console.ReadLine();
            var ipPort = Console.ReadLine();
            if (mode == "server")
            {
                var server = new Server(ipPort);
                server.Run();
            }
            else
            {
                var name = Console.ReadLine();
                var client = new Client(ipPort, name);
                client.Run();
            }
        }
    }
}
/*
client
127.1:8888

*/
