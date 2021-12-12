using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    class ClientHandleData
    {

        private static ByteBuffer playerBuffer;
        public delegate void Packet(byte[] data);
        public static Dictionary<int, Packet> packets = new Dictionary<int, Packet>();

        public static void InitializePackets()
        {
            //authserver
            packets.Add((int)AuthServerPackets.SWelComeMessage, DataReceiver.HandleWelcomeMsg);
            packets.Add((int)AuthServerPackets.SRegistroResult, DataReceiver.HandleRegistroMsg);
            packets.Add((int)AuthServerPackets.SLoginResult, DataReceiver.HandleLoginMsg);
            //gameserver
            packets.Add((int)GameServerPackets.GameSRegistroResult, DataReceiver.HandleGameSRegistroResult);
            packets.Add((int)GameServerPackets.SSincroResult, DataReceiver.HandleGameSSincroResult);
            packets.Add((int)GameServerPackets.SGrupoSincroResult, DataReceiver.HandleGameSGrupoSincroResult);
            packets.Add((int)GameServerPackets.SAdministration, DataReceiver.HandleGameSAdministration);
        }

        public static void HandleData(byte[] data)
        {
            byte[] buffer = (byte[])data.Clone();
            int pLength = 0;

            if (playerBuffer == null) playerBuffer = new ByteBuffer();

            playerBuffer.WriteBytes(buffer);
            if (playerBuffer.Count() == 0)
            {
                playerBuffer.Clear();
                return;
            }

            if (playerBuffer.Length() >= 4)
            {
                pLength = playerBuffer.ReadInteger(false);
                if (pLength <= 0)
                {
                    playerBuffer.Clear();
                    return;
                }
            }

            while (pLength > 0 & pLength <= playerBuffer.Length() - 4)
            {
                if (pLength <= playerBuffer.Length() - 4)
                {
                    playerBuffer.ReadInteger();
                    data = playerBuffer.ReadBytes(pLength);
                    HandleDataPackets(data);
                }

                pLength = 0;
                if (playerBuffer.Length() >= 4)
                {
                    pLength = playerBuffer.ReadInteger(false);
                    if (pLength <= 0)
                    {
                        playerBuffer.Clear();
                        return;
                    }
                }
            }

            if (pLength <= 1)
            {
                playerBuffer.Clear();
            }
        }

        private static void HandleDataPackets(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            int packetID = buffer.ReadInteger();
            buffer.Dispose();
            if (packets.TryGetValue(packetID, out Packet packet))
            {
                packet.Invoke(data);
            }
        }
    }
}