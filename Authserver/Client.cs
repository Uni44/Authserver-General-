using System;
using System.Net.Sockets;

namespace Authserver
{
    class Client
    {
        public int connectionID;
        public TcpClient Socket;
        public NetworkStream stream;
        private byte[] readBuff;
        public ByteBuffer buffer;
        public int accID;
        public bool OfficialServer = false;

        public void Start ()
        {
            Socket.SendBufferSize = 4096;
            Socket.ReceiveBufferSize = 4096;
            stream = Socket.GetStream();
            readBuff = new byte[4096];
            stream.BeginRead(readBuff, 0, Socket.ReceiveBufferSize, OnReceiveData, null);
        }

        public void CloseConnection ()
        {
            Socket.Close();
            if (!OfficialServer)
            {
                ChangeOnlineAccount(accID);
            }
            else
            {
                Program.OfficialServerConnecteds--;
            }

            Socket = null;
            accID = 0;
            ClientManager.client.Remove(connectionID);
        }

        void ChangeOnlineAccount (int accID)
        {
            if (Program.sitemaOnline)
            {
                ServiceData.Accounts.Find(x => x.id == accID).online = 0;
            }
        }

        void OnReceiveData (IAsyncResult result)
        {
            try
            {
                int readBytes = stream.EndRead(result);
                if (readBytes <= 0)
                {
                    CloseConnection();
                    return;
                }
                byte[] newBytes = null;
                Array.Resize(ref newBytes, readBytes);
                Buffer.BlockCopy(readBuff, 0, newBytes, 0, readBytes);
                ServerHandleData.HandleData(connectionID, newBytes);
                stream.BeginRead(readBuff, 0, Socket.ReceiveBufferSize, OnReceiveData, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error OnReceiveData Client: " + connectionID + ". Ex: " + ex);
                CloseConnection();
                return;
            }
        }
    }
}
