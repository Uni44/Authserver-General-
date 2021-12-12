using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Authserver
{
    static class ServiceData
    {

        public static List<Data_account> Accounts = new List<Data_account>();
        public static List<Data_banned> Accounts_banned = new List<Data_banned>();
        public static List<Data_banned> Ip_banned = new List<Data_banned>();
        public static List<Data_banned> Mac_banned = new List<Data_banned>();
        public static List<Data_access> Accounts_access = new List<Data_access>();
        public static string Version = "1.0.0";
        public static string sv_modo = "online";

        static MySqlDataReader dr;

        public static void LoadAllData()
        {
            try
            {
                Console.WriteLine("Cargando todos los datos al servidor...");
                LoadConfigs();
                LoadAccounts();
                LoadBans();
                LoadAccess();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ServiceData Error Load: " + ex);
            }
            dr.Close();
        }


        static void LoadConfigs()
        {
            if (File.Exists("config.cfg"))
            {
                string text = File.ReadAllText("config.cfg");
                char[] delimitador = { ';', ':' };
                string[] variables = text.Split(delimitador);
                Version = variables[1];
                sv_modo = variables[3];
            }
            Console.WriteLine("Configuración cargada: versión: " + Version + ", Modo del servidor: " + sv_modo);
        }

        static void SaveConfigs()
        {
            string[] lines = { "version:" + Version + ";" + "modo_server:" + sv_modo};
            System.IO.File.WriteAllLines("config.cfg", lines);
            Console.WriteLine("Configuración guardada");
        }

        public static void SaveAll()
        {
            try
            {
                save();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ServiceData Error Save: " + ex);
            }
            dr.Close();
        }

        static void save()
        {
            SaveConfigs();
			
            dr.Close();
			
            Console.WriteLine("Guardando todos los datos del servidor...");
            for (int i = 0; i < Accounts.Count; i++)
            {
                if (Accounts[i].id != 0)
                {
                    string query = "SELECT * FROM account WHERE id = '" + Accounts[i].id + "'";
                    MySqlCommand command = new MySqlCommand(query, MySQL.con);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    if (count > 0)
                    {
                        MySQL.Query("UPDATE account SET last_ip = '" + Accounts[i].last_ip + "', last_login = '" + Accounts[i].last_login.Year + "-" + Accounts[i].last_login.Month + "-" + Accounts[i].last_login.Day + " " + Accounts[i].last_login.Hour + ":" + Accounts[i].last_login.Minute + ":" + Accounts[i].last_login.Second + "' WHERE id = '" + Accounts[i].id + "'");
                    }
                    else
                    {
                        MySQL.Query("INSERT INTO account (id, username, pass, sessionkey, v, s, email, joindate, last_ip, last_mac, failed_logins, locked, last_login, online) VALUES ('" + Accounts[i].id + "', '" + Accounts[i].username + "', '" + Accounts[i].pass + "', '" + Accounts[i].sessionkey + "', '" + Accounts[i].v + "', '" + Accounts[i].s + "', '" + Accounts[i].email + "', '" + Accounts[i].joindate.Year + "-" + Accounts[i].joindate.Month + "-" + Accounts[i].joindate.Day + " " + Accounts[i].joindate.Hour + ":" + Accounts[i].joindate.Minute + ":" + Accounts[i].joindate.Second + "', '" + Accounts[i].last_ip + "', '" + Accounts[i].last_mac + "', '" + Accounts[i].failed_logins + "', '" + Accounts[i].locked + "', '" + Accounts[i].last_login.Year + "-" + Accounts[i].last_login.Month + "-" + Accounts[i].last_login.Day + " " + Accounts[i].last_login.Hour + ":" + Accounts[i].last_login.Minute + ":" + Accounts[i].last_login.Second + "', '" + Accounts[i].online + "')");
                    }
                    Console.WriteLine("Cuenta guardada: " + Accounts[i].id);
                }
            }

            //acc
            for (int i = 0; i < Accounts_banned.Count; i++)
            {
                if (Accounts_banned[i].id >= 0)
                {
                    string query = "SELECT * FROM account_banned WHERE id = '" + Accounts_banned[i].id + "' AND bandate = '" + Accounts_banned[i].bandate.Year + "-" + Accounts_banned[i].bandate.Month + "-" + Accounts_banned[i].bandate.Day + " " + Accounts_banned[i].bandate.Hour + ":" + Accounts_banned[i].bandate.Minute + ":" + Accounts_banned[i].bandate.Second + "'";
                    MySqlCommand command = new MySqlCommand(query, MySQL.con);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    if (count > 0)
                    {
                        MySQL.Query("UPDATE account_banned SET bandate = '" + Accounts_banned[i].bandate.Year + "-" + Accounts_banned[i].bandate.Month + "-" + Accounts_banned[i].bandate.Day + " " + Accounts_banned[i].bandate.Hour + ":" + Accounts_banned[i].bandate.Minute + ":" + Accounts_banned[i].bandate.Second + "', unbandate = '" + Accounts_banned[i].unbandate.Year + "-" + Accounts_banned[i].unbandate.Month + "-" + Accounts_banned[i].unbandate.Day + " " + Accounts_banned[i].unbandate.Hour + ":" + Accounts_banned[i].unbandate.Minute + ":" + Accounts_banned[i].unbandate.Second + "', active = '" + Accounts_banned[i].active + "' WHERE id = '" + Accounts_banned[i].id + "'");
                    }
                    else
                    {
                        MySQL.Query("INSERT INTO account_banned (id, bandate, unbandate, bannedby, banreason, active) VALUES ('" + Accounts_banned[i].id + "', '" + Accounts_banned[i].bandate.Year + "-" + Accounts_banned[i].bandate.Month + "-" + Accounts_banned[i].bandate.Day + " " + Accounts_banned[i].bandate.Hour + ":" + Accounts_banned[i].bandate.Minute + ":" + Accounts_banned[i].bandate.Second + "', '" + Accounts_banned[i].unbandate.Year + "-" + Accounts_banned[i].unbandate.Month + "-" + Accounts_banned[i].unbandate.Day + " " + Accounts_banned[i].unbandate.Hour + ":" + Accounts_banned[i].unbandate.Minute + ":" + Accounts_banned[i].unbandate.Second + "', '" + Accounts_banned[i].bannedby + "', '" + Accounts_banned[i].banreason + "', '" + Accounts_banned[i].active + "')");
                    }
                    Console.WriteLine("Baneo guardado: " + Accounts_banned[i].id);
                }
            }

            //ip
            for (int i = 0; i < Ip_banned.Count; i++)
            {
                if (Ip_banned[i].ip != "")
                {
                    string query = "SELECT * FROM ip_banned WHERE ip = '" + Ip_banned[i].ip + "' AND bandate = '" + Ip_banned[i].bandate.Year + "-" + Ip_banned[i].bandate.Month + "-" + Ip_banned[i].bandate.Day + " " + Ip_banned[i].bandate.Hour + ":" + Ip_banned[i].bandate.Minute + ":" + Ip_banned[i].bandate.Second + "'";
                    MySqlCommand command = new MySqlCommand(query, MySQL.con);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    if (count > 0)
                    {
                        MySQL.Query("UPDATE ip_banned SET bandate = '" + Ip_banned[i].bandate.Year + "-" + Ip_banned[i].bandate.Month + "-" + Ip_banned[i].bandate.Day + " " + Ip_banned[i].bandate.Hour + ":" + Ip_banned[i].bandate.Minute + ":" + Ip_banned[i].bandate.Second + "', unbandate = '" + Ip_banned[i].unbandate.Year + "-" + Ip_banned[i].unbandate.Month + "-" + Ip_banned[i].unbandate.Day + " " + Ip_banned[i].unbandate.Hour + ":" + Ip_banned[i].unbandate.Minute + ":" + Ip_banned[i].unbandate.Second + "', active = '" + Ip_banned[i].active + "' WHERE ip = '" + Ip_banned[i].ip + "'");
                    }
                    else
                    {
                        MySQL.Query("INSERT INTO ip_banned (ip, bandate, unbandate, bannedby, banreason, active) VALUES ('" + Ip_banned[i].ip + "', '" + Ip_banned[i].bandate.Year + "-" + Ip_banned[i].bandate.Month + "-" + Ip_banned[i].bandate.Day + " " + Ip_banned[i].bandate.Hour + ":" + Ip_banned[i].bandate.Minute + ":" + Ip_banned[i].bandate.Second + "', '" + Ip_banned[i].unbandate.Year + "-" + Ip_banned[i].unbandate.Month + "-" + Ip_banned[i].unbandate.Day + " " + Ip_banned[i].unbandate.Hour + ":" + Ip_banned[i].unbandate.Minute + ":" + Ip_banned[i].unbandate.Second + "', '" + Ip_banned[i].bannedby + "', '" + Ip_banned[i].banreason + "', '" + Ip_banned[i].active + "')");
                    }
                    Console.WriteLine("Baneo guardado: " + Ip_banned[i].ip);
                }
            }
            Console.WriteLine("Todos los baneos fueron guardados");

            //mac
            for (int i = 0; i < Mac_banned.Count; i++)
            {
                if (Mac_banned[i].mac != "")
                {
                    string query = "SELECT * FROM mac_banned WHERE mac = '" + Mac_banned[i].mac + "' AND bandate = '" + Mac_banned[i].bandate.Year + "-" + Mac_banned[i].bandate.Month + "-" + Mac_banned[i].bandate.Day + " " + Mac_banned[i].bandate.Hour + ":" + Mac_banned[i].bandate.Minute + ":" + Mac_banned[i].bandate.Second + "'";
                    MySqlCommand command = new MySqlCommand(query, MySQL.con);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    if (count > 0)
                    {
                        MySQL.Query("UPDATE mac_banned SET bandate = '" + Mac_banned[i].bandate.Year + "-" + Mac_banned[i].bandate.Month + "-" + Mac_banned[i].bandate.Day + " " + Mac_banned[i].bandate.Hour + ":" + Mac_banned[i].bandate.Minute + ":" + Mac_banned[i].bandate.Second + "', unbandate = '" + Mac_banned[i].unbandate.Year + "-" + Mac_banned[i].unbandate.Month + "-" + Mac_banned[i].unbandate.Day + " " + Mac_banned[i].unbandate.Hour + ":" + Mac_banned[i].unbandate.Minute + ":" + Mac_banned[i].unbandate.Second + "', active = '" + Mac_banned[i].active + "' WHERE mac = '" + Mac_banned[i].mac + "'");
                    }
                    else
                    {
                        MySQL.Query("INSERT INTO mac_banned (mac, bandate, unbandate, bannedby, banreason, active) VALUES ('" + Mac_banned[i].mac + "', '" + Mac_banned[i].bandate.Year + "-" + Mac_banned[i].bandate.Month + "-" + Mac_banned[i].bandate.Day + " " + Mac_banned[i].bandate.Hour + ":" + Mac_banned[i].bandate.Minute + ":" + Mac_banned[i].bandate.Second + "', '" + Mac_banned[i].unbandate.Year + "-" + Mac_banned[i].unbandate.Month + "-" + Mac_banned[i].unbandate.Day + " " + Mac_banned[i].unbandate.Hour + ":" + Mac_banned[i].unbandate.Minute + ":" + Mac_banned[i].unbandate.Second + "', '" + Mac_banned[i].bannedby + "', '" + Mac_banned[i].banreason + "', '" + Mac_banned[i].active + "')");
                    }
                    Console.WriteLine("Baneo guardado: " + Mac_banned[i].mac);
                }
            }
            Console.WriteLine("Todos los baneos fueron guardados");

            for (int i = 0; i < Accounts_access.Count; i++)
            {
                string query = "SELECT * FROM account_access WHERE id = '" + Accounts_access[i].id + "'";
                MySqlCommand command = new MySqlCommand(query, MySQL.con);
                int count = Convert.ToInt32(command.ExecuteScalar());
                if (count > 0)
                {
                    MySQL.Query("UPDATE account_access SET gmlevel = '" + Accounts_access[i].gmlevel + "', Comment = '" + Accounts_access[i].Comment + "' WHERE id = '" + Accounts_access[i].id + "'");
                }
                else
                {
                    MySQL.Query("INSERT INTO account_access (id, gmlevel, Comment) VALUES ('" + Accounts_access[i].id + "', '" + Accounts_access[i].gmlevel + "', '" + Accounts_access[i].Comment + "')");
                }
                Console.WriteLine("Acceso guardado: " + Accounts_access[i].id);
            }
            Console.WriteLine("Todos los accesos fueron guardados");
            //MySQL.Disconnect();
        }

        static void LoadAccounts()
        {
            Accounts.Add(new Data_account());

            MySqlCommand command = new MySqlCommand("SELECT * FROM account", MySQL.con);
            command.ExecuteNonQuery();
            dr = command.ExecuteReader();
            while (dr.Read())
            {
                Data_account a = new Data_account();
                a.id = dr.GetInt32("id");
                a.username = dr.GetString("username");
                a.pass = dr.GetString("pass");
                a.sessionkey = dr.GetString("sessionkey");
                a.v = dr.GetString("v");
                a.s = dr.GetString("s");
                a.email = dr.GetString("email");
                a.joindate = dr.GetDateTime("joindate");
                a.last_ip = dr.GetString("last_ip");
                a.last_mac = dr.GetString("last_mac");
                a.failed_logins = dr.GetInt32("failed_logins");
                a.locked = dr.GetInt32("locked");
                a.last_login = DateTime.Parse(dr.GetDateTime("last_login").Year + "-" + dr.GetDateTime("last_login").Month + "-" + dr.GetDateTime("last_login").Day + " " + dr.GetDateTime("last_login").Hour + ":" + dr.GetDateTime("last_login").Minute + ":" + dr.GetDateTime("last_login").Second);
                a.online = dr.GetInt32("online");
                Accounts.Add(a);
                Console.WriteLine("Cuenta cargada: " + a.id);
            }
            dr.Close();

            Console.WriteLine("Todas las cuentas fueron cargadas al servidor");
        }

        static void LoadBans()
        {
            //acc
            MySqlCommand command = new MySqlCommand("SELECT * FROM account_banned", MySQL.con);
            command.ExecuteNonQuery();
            dr = command.ExecuteReader();
            while (dr.Read())
            {
                if (System.DateTime.Now < dr.GetDateTime("unbandate"))
                {
                    Data_banned a = new Data_banned();
                    a.id = dr.GetInt32("id");
                    a.bandate = dr.GetDateTime("bandate");
                    a.unbandate = dr.GetDateTime("unbandate");
                    a.bannedby = dr.GetString("bannedby");
                    a.banreason = dr.GetString("banreason");
                    a.active = dr.GetInt32("active");
                    Accounts_banned.Add(a);
                    Console.WriteLine("Baneo cargado: " + a.id);
                }
            }
            dr.Close();

            //ip
            command = new MySqlCommand("SELECT * FROM ip_banned", MySQL.con);
            command.ExecuteNonQuery();
            dr = command.ExecuteReader();
            while (dr.Read())
            {
                if (System.DateTime.Now < dr.GetDateTime("unbandate"))
                {
                    Data_banned a = new Data_banned();
                    a.ip = dr.GetString("ip");
                    a.bandate = dr.GetDateTime("bandate");
                    a.unbandate = dr.GetDateTime("unbandate");
                    a.bannedby = dr.GetString("bannedby");
                    a.banreason = dr.GetString("banreason");
                    a.active = dr.GetInt32("active");
                    Ip_banned.Add(a);
                    Console.WriteLine("Baneo cargado: " + a.ip);
                }
            }
            dr.Close();

            //mac
            command = new MySqlCommand("SELECT * FROM mac_banned", MySQL.con);
            command.ExecuteNonQuery();
            dr = command.ExecuteReader();
            while (dr.Read())
            {
                if (System.DateTime.Now < dr.GetDateTime("unbandate"))
                {
                    Data_banned a = new Data_banned();
                    a.mac = dr.GetString("mac");
                    a.bandate = dr.GetDateTime("bandate");
                    a.unbandate = dr.GetDateTime("unbandate");
                    a.bannedby = dr.GetString("bannedby");
                    a.banreason = dr.GetString("banreason");
                    a.active = dr.GetInt32("active");
                    Mac_banned.Add(a);
                    Console.WriteLine("Baneo cargado: " + a.mac);
                }
            }
            dr.Close();

            Console.WriteLine("Todos los baneos fueron cargados al servidor");
        }

        static void LoadAccess()
        {
            MySqlCommand command = new MySqlCommand("SELECT * FROM account_access", MySQL.con);
            command.ExecuteNonQuery();
            dr = command.ExecuteReader();
            while (dr.Read())
            {
                Data_access a = new Data_access();
                a.id = dr.GetInt32("id");
                a.gmlevel = dr.GetInt32("gmlevel");
                a.Comment = dr.GetString("Comment");
                Accounts_access.Add(a);
                Console.WriteLine("Acceso cargado: " + a.id);
            }
            dr.Close();
            Console.WriteLine("Todos los accesos fueron cargados al servidor");
        }

        public static void ReloadAllData()
        {
            try
            {
                Accounts.Clear();
                Accounts_banned.Clear();
                Ip_banned.Clear();
                Mac_banned.Clear();
                Accounts_access.Clear();
                LoadAllData();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ServiceData Error ReloadAllData: " + ex);
            }
            dr.Close();
        }

        public static void ExeCommands()
        {
            try
            {
                LoadAllCommands();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ServiceData Error ExeCommands: " + ex);
            }

            if (Program.saveContinuo)
            {
                Program.saveContinuo = false;
                SaveAll();
            }
        }

        static void LoadAllCommands()
        {
            dr.Close();
            MySqlCommand command = new MySqlCommand("SELECT * FROM server_commands", MySQL.con);
            command.ExecuteNonQuery();
            dr = command.ExecuteReader();

            List<string> comandos = new List<string>();

            while (dr.Read())
            {
                comandos.Add(dr.GetString("string"));
            }
            dr.Close();
            MySQL.Query("DELETE FROM server_commands");

            for (int i = 0; i < comandos.Count; i++)
            {
                string text = comandos[i];
                char[] delimitador = { ';' };
                string[] variables = text.Split(delimitador);

                if (variables[0] == "changeonlinestate")
                {
                    ChangeOnlineState(int.Parse(variables[1]), 0);
                }

                if (variables[0] == "ban")
                {
                    Ban(variables[3], variables[4], variables[5], variables[6], variables[7]);
                }
            }
        }

        static public void ChangeOnlineState(int accID, int online)
        {
            Data_account a = Accounts.Find(x => accID == x.id);
            a.online = online;
        }

        static public void Ban(string tipo, string key, string fecha, string razon, string by)
        {
            Data_banned ban = new Data_banned();
            ban.active = 1;
            ban.bandate = DateTime.Now;
            DateTime.TryParse(fecha, out ban.unbandate);
            ban.banreason = razon;
            ban.bannedby = by;

            if (tipo == "account")
            {
                Data_account account = Accounts.Find(x => x.username == key);
                ban.id = account.id;
                Accounts_banned.Add(ban);
            }

            if (tipo == "ip")
            {
                ban.ip = key;
                Ip_banned.Add(ban);
            }

            if (tipo == "mac")
            {
                ban.mac = key;
                Mac_banned.Add(ban);
            }

            if (tipo == "superban")
            {
                Data_account account = Accounts.Find(x => x.username == key);
                Ban("account", account.username, fecha, razon, by);
                Ban("ip", ClearSring(account.last_ip), fecha, razon, by);
                Ban("mac", ClearSring(account.last_mac), fecha, razon, by);
            }
        }

        static public string ClearSring(string text)
        {
            string a = text.Replace(" ", string.Empty) + ";ccc";
            char[] delimitador = { ';' };
            string[] variables = text.Split(delimitador);
            return variables[0];
        }
    }
}
