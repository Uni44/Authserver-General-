using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public enum AuthServerPackets
    {
        SWelComeMessage = 1,
        SRegistroResult,
        SLoginResult,
    }

    public enum GameServerPackets
    {
        SWelComeMessage = 1,
        GameSRegistroResult = 100,
        SSincroResult,
        SaveResult,
        SGrupoSincroResult,
        SAdministration,
    }

    static class DataReceiver
    {

        /// <summary>
        /// auth server
        /// </summary>

        public static void HandleWelcomeMsg(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            int packetID = buffer.ReadInteger();
            string msg = buffer.ReadString();
            buffer.Dispose();

            //Debug.Log(msg);
            NetworkManager.instance.ServerState(msg);
        }

        public static void HandleRegistroMsg(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            int packetID = buffer.ReadInteger();
            string msg = buffer.ReadString();
            buffer.Dispose();

            //Debug.Log(msg);
            NetworkManager.instance.Registromsg(msg);
        }

        public static void HandleLoginMsg(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            int packetID = buffer.ReadInteger();
            string msg = buffer.ReadString();
            buffer.Dispose();

            //Debug.Log(msg);
            NetworkManager.instance.loginmsg(msg);
        }

        /// <summary>
        /// game server
        /// </summary>

        public static void HandleGameSRegistroResult(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            int packetID = buffer.ReadInteger();
            string msg = buffer.ReadString();
            buffer.Dispose();

            //Debug.Log(msg);
            NetworkManager.instance.GameSRegistroResult(msg);
        }

        public static void HandleGameSSincroResult(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            int packetID = buffer.ReadInteger();
            string msg = buffer.ReadString();
            buffer.Dispose();

            //Debug.Log(msg);
            NetworkManager.instance.GameSSincro(msg);
        }

        public static void HandleGameSGrupoSincroResult(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            int packetID = buffer.ReadInteger();
            string msg = buffer.ReadString();
            buffer.Dispose();

            //Debug.Log(msg);
            NetworkManager.instance.GameSGrupoSincro(msg);
        }

        public static void HandleGameSAdministration(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            int packetID = buffer.ReadInteger();
            string msg = buffer.ReadString();
            buffer.Dispose();

            //Debug.Log(msg);
            NetworkManager.instance.ReferenceAdmin(msg);
        }
    }
}
