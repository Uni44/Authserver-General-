using System;
using System.Collections.Generic;
using System.Text;

namespace Authserver
{
    class ServerHandleData
    {
        public delegate void Packet(int connectionID, byte[] data);
        public static Dictionary<int, Packet> packets = new Dictionary<int, Packet>();

        public static void InitializePackets()
        {
            packets.Add((int)ClientPackets.ChelloServer, DataReceiver.HandleHelloServer);
            packets.Add((int)ClientPackets.CRegistroServer, DataReceiver.HandleRegistroServer);
            packets.Add((int)ClientPackets.CLoginServer, DataReceiver.HandleLoginServer);
            packets.Add((int)ClientPackets.COfficialAuthServer, DataReceiver.HandleAuthOfficialServer);
            packets.Add((int)ClientPackets.CChangeOnline, DataReceiver.HandleChangeOnline);
            packets.Add((int)ClientPackets.CBan, DataReceiver.HandleCBan);
            packets.Add((int)ClientPackets.CLogoutServer, DataReceiver.HandleLogoutServer);
        }

        public static void HandleData(int connectionID, byte[] data)
        {
            byte[] buffer = (byte[])data.Clone();
            int pLength = 0;

            if (ClientManager.client[connectionID].buffer == null) ClientManager.client[connectionID].buffer = new ByteBuffer();

            ClientManager.client[connectionID].buffer.WriteBytes(buffer);
            if (ClientManager.client[connectionID].buffer.Count() == 0)
            {
                ClientManager.client[connectionID].buffer.Clear();
                return;
            }

            if (ClientManager.client[connectionID].buffer.Length() >= 4)
            {
                pLength = ClientManager.client[connectionID].buffer.ReadInteger(false);
                if (pLength <= 0)
                {
                    ClientManager.client[connectionID].buffer.Clear();
                    return;
                }
            }

            while (pLength > 0 & pLength <= ClientManager.client[connectionID].buffer.Length() - 4)
            {
                if (pLength <= ClientManager.client[connectionID].buffer.Length() - 4)
                {
                    ClientManager.client[connectionID].buffer.ReadInteger();
                    data = ClientManager.client[connectionID].buffer.ReadBytes(pLength);
                    HandleDataPackets(connectionID, data);
                }

                pLength = 0;
                if (ClientManager.client[connectionID].buffer.Length() >= 4)
                {
                    pLength = ClientManager.client[connectionID].buffer.ReadInteger(false);
                    if (pLength <= 0)
                    {
                        ClientManager.client[connectionID].buffer.Clear();
                        return;
                    }
                }
            }

            if (pLength <= 1)
            {
                ClientManager.client[connectionID].buffer.Clear();
            }
        }

        private static void HandleDataPackets(int connectionID, byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            int packetID = buffer.ReadInteger();
            buffer.Dispose();
            if (packets.TryGetValue(packetID, out Packet packet))
            {
                packet.Invoke(connectionID, data);
            }
        }
    }
}
