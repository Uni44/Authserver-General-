using System;
using System.Collections.Generic;
using System.Text;

namespace Authserver
{
    public enum ServerPackets
    {
        SwelComeMessage = 1,
        SRegistroResult,
        SLoginResult,
    }
    static class DataSender
    {
        public static void SendWelcomeMessage(int connectionID)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)ServerPackets.SwelComeMessage);
            buffer.WriteString(ServiceData.sv_modo + ";" + ServiceData.Version + ";authserver");
            ClientManager.SendDataTo(connectionID, buffer.ToArray());
            buffer.Dispose();
        }

        public static void SendRegistroResult(int connectionID, string result)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)ServerPackets.SRegistroResult);
            buffer.WriteString(result);
            ClientManager.SendDataTo(connectionID, buffer.ToArray());
            buffer.Dispose();
        }

        public static void SendLoginResult(int connectionID, string result)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)ServerPackets.SLoginResult);
            buffer.WriteString(result);
            ClientManager.SendDataTo(connectionID, buffer.ToArray());
            buffer.Dispose();
        }
    }
}
