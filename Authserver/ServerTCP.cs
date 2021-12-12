using System;
using System.Net;
using System.Net.Sockets;

namespace Authserver
{
    class ServerTCP
    {
        static TcpListener serverSocket = new TcpListener(GetLocalIPAddress(), 5558);

        public static void intializeNetwork()
        {
            Console.WriteLine("Iniciando servidor en: " + GetLocalIPAddress().ToString());
            Console.WriteLine("Inicializando Paquetes...");
            ServerHandleData.InitializePackets();
            serverSocket.Start();
            serverSocket.BeginAcceptTcpClient(new AsyncCallback(onClientConnect), null);
        }

        private static void onClientConnect (IAsyncResult result)
        {
            TcpClient client = serverSocket.EndAcceptTcpClient(result);
            serverSocket.BeginAcceptTcpClient(new AsyncCallback(onClientConnect), null);
            ClientManager.CreateNewConnection(client);
        }

        public static void CloseNetwork()
        {
            serverSocket.Server.Close();
            serverSocket.Stop();
        }

        public static IPAddress GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
