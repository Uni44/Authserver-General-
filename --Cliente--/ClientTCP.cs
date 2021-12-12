using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;

namespace Assets.Scripts
{
    static class ClientTCP
    {
        public static TcpClient clientSocket;
        private static NetworkStream myStream;
        private static byte[] recBuffer;

        public static void InitializingNetworking(string ip, int puerto)
        {
            clientSocket = new TcpClient();
            clientSocket.ReceiveBufferSize = 4096;
            clientSocket.SendBufferSize = 4096;
            recBuffer = new byte[4096 * 2];
            clientSocket.BeginConnect(ip, puerto, new System.AsyncCallback(ClientConnectCallback), clientSocket);
        }

        private static void ClientConnectCallback(System.IAsyncResult result)
        {
            clientSocket.EndConnect(result);
            if (clientSocket.Connected == false)
            {
                return;
            }
            else
            {
                clientSocket.NoDelay = true;
                myStream = clientSocket.GetStream();
                myStream.BeginRead(recBuffer, 0, 4096 * 2, ReceiveCallback, null);
            }
        }

        private static void ReceiveCallback(System.IAsyncResult result)
        {
            try
            {
                int length = myStream.EndRead(result);
                if (length <= 0)
                {
                    return;
                }

                byte[] newBytes = new byte[length];
                System.Array.Copy(recBuffer, newBytes, length);
                UnityThread.executeInFixedUpdate(() =>
                {
                    ClientHandleData.HandleData(newBytes);
                });
                myStream.BeginRead(recBuffer, 0, 4096 * 2, ReceiveCallback, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void SendData(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((data.GetUpperBound(0) - data.GetLowerBound(0) + 1));
            buffer.WriteBytes(data);
            if (myStream == null)
            {
                Debug.LogError("No hay conexión");
                return;
            }

            try
            {
                myStream.BeginWrite(buffer.ToArray(), 0, buffer.ToArray().Length, null, null);
            }
            catch (Exception ex)
            {
                Debug.LogError("Error de conexión: " + ex);

                if (!ModoManager.instance.enPartida)
                {
                    MenuManager.instance.timeOutLoad();
                }
            }

            buffer.Dispose();
        }

        public static void Disconnect()
        {
            clientSocket.Close();
        }
    }
}