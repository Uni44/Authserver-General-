using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Authserver
{
    static class ClientManager
    {
        public static Dictionary<int, Client> client = new Dictionary<int, Client>();

        public static void CreateNewConnection(TcpClient tempClient)
        {
            if (Program.connectionEnabled)
            {
                if (client.Count < Program.limiteDeConectados - Program.OfficialServerConnecteds)
                {
                    Client newClient = new Client();
                    newClient.Socket = tempClient;
                    newClient.connectionID = ((IPEndPoint)tempClient.Client.RemoteEndPoint).Port;
                    newClient.Start();
                    client.Add(newClient.connectionID, newClient);

                    DataSender.SendWelcomeMessage(newClient.connectionID);
                }
                else
                {
                    tempClient.Close();
                }
            }
            else
            {
                tempClient.Close();
            }
        }

        public static void SendDataTo(int connectionID, byte[] data)
        {
            try
            {
                ByteBuffer buffer = new ByteBuffer();
                buffer.WriteInteger((data.GetUpperBound(0) - data.GetLowerBound(0) + 1));
                buffer.WriteBytes(data);
                client[connectionID].stream.BeginWrite(buffer.ToArray(), 0, buffer.ToArray().Length, null, null);
                buffer.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error SendDataTo: " + ex);
            }
        }
    }
}
