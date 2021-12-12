using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public enum AuthClientPackets
    {
        CHelloServer = 1,
        CRegistroServer,
        CLoginServer,
    }

    public enum GameClientPackets
    {
        CHelloServer = 1,
        CRegistroServer = 100,
        CLogoutServer,
        CSincroResult,
        CSaveDatos,
        CSincroGrupo,
        CAdministration,
    }

    static class DataSender
    {

        /// <summary>
        /// authserver
        /// </summary>

        public static void SendHelloServer()
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)AuthClientPackets.CHelloServer);
            buffer.WriteString("Gracias por dejarme conectar");
            ClientTCP.SendData(buffer.ToArray());
            buffer.Dispose();
        }

        public static void SendRegistro(string datos)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)AuthClientPackets.CRegistroServer);
            buffer.WriteString(datos);
            ClientTCP.SendData(buffer.ToArray());
            buffer.Dispose();
        }

        public static void SendLogin(string datos)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)AuthClientPackets.CLoginServer);
            buffer.WriteString(datos);
            ClientTCP.SendData(buffer.ToArray());
            buffer.Dispose();
        }

        /// <summary>
        /// game server
        /// </summary>

        public static void SendLogout()
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)GameClientPackets.CLogoutServer);
            buffer.WriteString(NetworkManager.instance.acc_id.ToString());
            ClientTCP.SendData(buffer.ToArray());
            buffer.Dispose();
        }

        public static void SendCRegistroServer(string datos)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)GameClientPackets.CRegistroServer);
            buffer.WriteString(datos);
            ClientTCP.SendData(buffer.ToArray());
            buffer.Dispose();
        }

        public static void SendSincroServer(string datos)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)GameClientPackets.CSincroResult);
            buffer.WriteString(datos);
            ClientTCP.SendData(buffer.ToArray());
            buffer.Dispose();
        }

        public static void SendCSaveDatosServer(string datos)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)GameClientPackets.CSaveDatos);
            buffer.WriteString(datos);
            ClientTCP.SendData(buffer.ToArray());
            buffer.Dispose();
        }

        public static void SendSincroGrupoServer(string datos)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)GameClientPackets.CSincroGrupo);
            buffer.WriteString(datos);
            ClientTCP.SendData(buffer.ToArray());
            buffer.Dispose();
        }

        public static void SendAdministration(string datos)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)GameClientPackets.CAdministration);
            buffer.WriteString(datos);
            ClientTCP.SendData(buffer.ToArray());
            buffer.Dispose();
        }
    }
}
