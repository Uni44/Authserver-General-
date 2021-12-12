using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Authserver
{
    public enum ClientPackets
    {
        ChelloServer = 1,
        CRegistroServer,
        CLoginServer,
        COfficialAuthServer,
        CChangeOnline,
        CBan,
        CLogoutServer = 101,
    }

    class DataReceiver
    {

        public static void HandleHelloServer(int connectionID, byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            int packetsID = buffer.ReadInteger();
            string msg = buffer.ReadString();
            buffer.Dispose();

            DataSender.SendWelcomeMessage(connectionID);
        }

        public static void HandleLogoutServer(int connectionID, byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            int packetsID = buffer.ReadInteger();
            string msg = buffer.ReadString();
            buffer.Dispose();

            if (msg != "0")
            {
                ServiceData.Accounts.Find(x => int.Parse(msg) == x.id).online = 0;
            }
        }


        public static void HandleRegistroServer(int connectionID, byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            int packetsID = buffer.ReadInteger();
            string msg = buffer.ReadString();
            buffer.Dispose();

            string text = msg;
            char[] delimitador = { ';' };
            string[] variables = text.Split(delimitador);

            Data_account account = ServiceData.Accounts.Find(x => x.username == variables[0]);

            if (account != null)
            {
                DataSender.SendRegistroResult(connectionID, "error_user");
            }
            else
            {
                Data_account a = new Data_account();
                a.id = ServiceData.Accounts[ServiceData.Accounts.Count - 1].id + 1;
                a.username = variables[0];
                a.pass = Security.Encriptar(variables[1]);
                a.sessionkey = variables[3];
                a.v = variables[4];
                a.s = variables[5];
                a.email = variables[2];
                a.joindate = System.DateTime.Now;
                ServiceData.Accounts.Add(a);
                DataSender.SendRegistroResult(connectionID, "ok");
            }

            //old

            return;

            for (int i = 0; i < ServiceData.Accounts.Count; i++)
            {
                if (ServiceData.Accounts[i].username == variables[0])
                {
                    DataSender.SendRegistroResult(connectionID, "error_user");
                    break;
                }
                else
                {
                    if (i + 1 >= ServiceData.Accounts.Count)
                    {
                        Data_account a = new Data_account();
                        a.id = ServiceData.Accounts[ServiceData.Accounts.Count - 1].id + 1;
                        a.username = variables[0];
                        a.pass = Security.Encriptar(variables[1]);
                        a.sessionkey = variables[3];
                        a.v = variables[4];
                        a.s = variables[5];
                        a.email = variables[2];
                        a.joindate = System.DateTime.Now;
                        ServiceData.Accounts.Add(a);
                        DataSender.SendRegistroResult(connectionID, "ok");
                        break;
                    }
                }
            }
        }

        public static void HandleLoginServer(int connectionID, byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            int packetsID = buffer.ReadInteger();
            string msg = buffer.ReadString();
            buffer.Dispose();

            string text = msg;
            char[] delimitador = { ';' };
            string[] variables = text.Split(delimitador);

            Data_account account = ServiceData.Accounts.Find(x => (x.username == variables[0]));

            if (account != null)
            {
                Data_account account2 = ServiceData.Accounts.Find(x => (x.username == variables[0]) && (x.pass == Security.Encriptar(variables[1])));

                if (account2 != null)
                {
                    if (account.locked == 0)
                    {
                        if (account.online == 0 && Program.sitemaOnline | !Program.sitemaOnline)
                        {
                            Data_banned acc = ServiceData.Accounts_banned.Find(x => (x.id == account.id) && (x.active == 1));

                            if (acc == null)
                            {
                                Data_banned ip = ServiceData.Ip_banned.Find(x => (x.ip == variables[2]) && (x.active == 1));

                                if (ip == null)
                                {
                                    Data_banned mac = ServiceData.Mac_banned.Find(x => (x.mac == variables[3]) && (x.active == 1));

                                    if (mac == null)
                                    {
                                        account.last_ip = variables[2];
                                        account.last_login = System.DateTime.Now;

                                        if (Program.sitemaOnline)
                                        {
                                            account.online = 1;
                                        }

                                        foreach (var item in ClientManager.client)
                                        {
                                            if (item.Value.connectionID == connectionID)
                                            {
                                                item.Value.accID = account.id;
                                                break;
                                            }
                                        }

                                        int gmlevel = 0;
                                        Data_access access = ServiceData.Accounts_access.Find(x => x.id == account.id);
                                        if (access != null)
                                        {
                                            gmlevel = access.gmlevel;
                                        }

                                        string superstring = "loged;" + account.id.ToString() + ";" + account.username + ";" + account.sessionkey + ";" + account.v + ";" + account.s + ";" + gmlevel + ";" + connectionID;
                                        DataSender.SendLoginResult(connectionID, superstring);
                                    }
                                    else
                                    {
                                        if (System.DateTime.Now > mac.unbandate)
                                        {
                                            mac.active = 0;
                                            HandleLoginServer(connectionID, data);
                                        }
                                        else
                                        {
                                            string superstring2 = "banned;" + mac.bandate.ToString() + ";" + mac.unbandate.ToString() + ";" + mac.banreason;
                                            DataSender.SendLoginResult(connectionID, superstring2);
                                        }
                                    }
                                }
                                else
                                {
                                    if (System.DateTime.Now > ip.unbandate)
                                    {
                                        ip.active = 0;
                                        HandleLoginServer(connectionID, data);
                                    }
                                    else
                                    {
                                        string superstring2 = "banned;" + ip.bandate.ToString() + ";" + ip.unbandate.ToString() + ";" + ip.banreason;
                                        DataSender.SendLoginResult(connectionID, superstring2);
                                    }
                                }
                            }
                            else
                            {
                                if (System.DateTime.Now > acc.unbandate)
                                {
                                    acc.active = 0;
                                    HandleLoginServer(connectionID, data);
                                }
                                else
                                {
                                    string superstring2 = "banned;" + acc.bandate.ToString() + ";" + acc.unbandate.ToString() + ";" + acc.banreason;
                                    DataSender.SendLoginResult(connectionID, superstring2);
                                }
                            }
                        }
                        else
                        {
                            string superstring = "ready_connected;no";
                            DataSender.SendLoginResult(connectionID, superstring);

                        }
                    }
                    else
                    {
                        if (account.time_locked < System.DateTime.Now)
                        {
                            account.failed_logins = 0;
                            account.locked = 0;
                            HandleLoginServer(connectionID, data);
                        }
                        else
                        {
                            string superstring = "temp_block;no";
                            DataSender.SendLoginResult(connectionID, superstring);
                        }
                    }
                }
                else
                {
                    account.failed_logins++;
                    if (account.failed_logins >= 6)
                    {
                        account.locked = 1;
                        account.time_locked = System.DateTime.Now.AddMinutes(15);
                        string superstring = "temp_block;no";
                        DataSender.SendLoginResult(connectionID, superstring);
                    }
                    else
                    {
                        string superstring = "incorrecto;no";
                        DataSender.SendLoginResult(connectionID, superstring);
                    }
                }
            }
            else
            {
                string superstring = "incorrecto;no";
                DataSender.SendLoginResult(connectionID, superstring);
            }

            return;

            //old

            for (int i = 0; i < ServiceData.Accounts.Count; i++)
            {
                if (ServiceData.Accounts[i].username == variables[0])
                {
                    if (ServiceData.Accounts[i].pass == Security.Encriptar(variables[1]))
                    {
                        if (ServiceData.Accounts[i].online == 0 && Program.sitemaOnline | !Program.sitemaOnline)
                        {
                            if (ServiceData.Accounts[i].locked == 0)
                            {
                                for (int j = 0; j < ServiceData.Accounts_banned.Count; j++)
                                {
                                    if (ServiceData.Accounts_banned[j].id == ServiceData.Accounts[i].id && ServiceData.Accounts_banned[j].active == 1)
                                    {
                                        if (System.DateTime.Now > ServiceData.Accounts_banned[j].unbandate)
                                        {
                                            ServiceData.Accounts_banned[j].active = 0;
                                            HandleLoginServer(connectionID, data);
                                            break;
                                        }
                                        else
                                        {
                                            string superstring2 = "banned;" + ServiceData.Accounts_banned[j].bandate.ToString() + ";" + ServiceData.Accounts_banned[j].unbandate.ToString() + ";" + ServiceData.Accounts_banned[j].banreason;
                                            DataSender.SendLoginResult(connectionID, superstring2);
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (j + 1 >= ServiceData.Accounts_banned.Count)
                                        {
                                            ServiceData.Accounts[i].last_ip = variables[2];
                                            ServiceData.Accounts[i].last_login = System.DateTime.Now;
                                            if (Program.sitemaOnline)
                                            {
                                                ServiceData.Accounts[i].online = 1;
                                            }
                                            foreach (var item in ClientManager.client)
                                            {
                                                if (item.Value.connectionID == connectionID)
                                                {
                                                    item.Value.accID = ServiceData.Accounts[i].id;
                                                    break;
                                                }
                                            }
                                            int gmlevel = 0;
                                            for (int k = 0; k < ServiceData.Accounts_access.Count; k++)
                                            {
                                                if (ServiceData.Accounts_access[k].id == ServiceData.Accounts[i].id)
                                                {
                                                    gmlevel = ServiceData.Accounts_access[k].gmlevel;
                                                    break;
                                                }
                                            }
                                            string superstring = "loged;" + ServiceData.Accounts[i].id.ToString() + ";" + ServiceData.Accounts[i].username + ";" + ServiceData.Accounts[i].sessionkey + ";" + ServiceData.Accounts[i].v + ";" + ServiceData.Accounts[i].s + ";" + gmlevel + ";" + connectionID;
                                            DataSender.SendLoginResult(connectionID, superstring);
                                            break;
                                        }
                                    }
                                }
                                break;
                            }
                            else
                            {
                                if (ServiceData.Accounts[i].time_locked < System.DateTime.Now)
                                {
                                    ServiceData.Accounts[i].failed_logins = 0;
                                    ServiceData.Accounts[i].locked = 0;
                                    HandleLoginServer(connectionID, data);
                                    break;
                                }
                                else
                                {
                                    string superstring = "temp_block;no";
                                    DataSender.SendLoginResult(connectionID, superstring);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            string superstring = "ready_connected;no";
                            DataSender.SendLoginResult(connectionID, superstring);
                            break;
                        }
                    }
                    else
                    {
                        ServiceData.Accounts[i].failed_logins++;
                        if (ServiceData.Accounts[i].failed_logins >= 6)
                        {
                            ServiceData.Accounts[i].locked = 1;
                            ServiceData.Accounts[i].time_locked = System.DateTime.Now.AddMinutes(15);
                            string superstring = "temp_block;no";
                            DataSender.SendLoginResult(connectionID, superstring);
                            break;
                        }
                        else
                        {
                            string superstring = "incorrecto;no";
                            DataSender.SendLoginResult(connectionID, superstring);
                            break;
                        }
                    }
                }
                else
                {
                    if (i + 1 >= ServiceData.Accounts.Count)
                    {
                        string superstring = "incorrecto;no";
                        DataSender.SendLoginResult(connectionID, superstring);
                        break;
                    }
                }
            }
        }

        public static void HandleAuthOfficialServer(int connectionID, byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            int packetsID = buffer.ReadInteger();
            string msg = buffer.ReadString();
            buffer.Dispose();

            if (msg == "95ghw598gffw9jre89fh4er908jcerc094")
            {
                foreach (var item in ClientManager.client)
                {
                    if (item.Value.connectionID == connectionID)
                    {
                        item.Value.OfficialServer = true;
                        Program.OfficialServerConnecteds++;
                        Console.WriteLine("Conexión establecida con exito con un servidor de juego oficial.");
                        break;
                    }
                }
            }
            else
            {
                Console.WriteLine("Autenticación fallida de parte de un servidor no oficial");
            }
        }

        public static void HandleChangeOnline(int connectionID, byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            int packetsID = buffer.ReadInteger();
            string msg = buffer.ReadString();
            buffer.Dispose();

            string text = msg;
            char[] delimitador = { ';' };
            string[] variables = text.Split(delimitador);

            ServiceData.ChangeOnlineState(int.Parse(variables[0]), int.Parse(variables[1]));
        }

        public static void HandleCBan(int connectionID, byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            int packetsID = buffer.ReadInteger();
            string msg = buffer.ReadString();
            buffer.Dispose();

            string text = msg;
            char[] delimitador = { ';' };
            string[] variables = text.Split(delimitador);

            if (variables[0] == "autoban")
            {
                Data_account ban = ServiceData.Accounts.Find(x => x.id == int.Parse(variables[1]) && x.v == variables[8] && x.s == variables[9]);

                if (ban != null)
                {
                    ServiceData.Ban(variables[3], variables[4], variables[5], variables[6], variables[7]);
                }
            }
            else
            {
                Data_account Admin = ServiceData.Accounts.Find(x => x.id == int.Parse(variables[1]) && x.v == variables[8] && x.s == variables[9]);

                if (Admin != null)
                {

                    Data_access acceso = ServiceData.Accounts_access.Find(x => x.id == Admin.id);

                    if (acceso.gmlevel == int.Parse(variables[2]))
                        ServiceData.Ban(variables[3], variables[4], variables[5], variables[6], variables[7]);
                }
            }
        }
    }
}
