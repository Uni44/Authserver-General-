using System;
using System.Threading;
using System.Timers;

namespace Authserver
{
    static class Program
    {
        private static Thread threadConsole;
        private static bool consoleRunning;
        private static System.Timers.Timer aTimer;
        private static System.Timers.Timer aTimer2;
        private static bool setversion;
        private static bool setmodo;
        public static bool sitemaOnline = true;
        public static bool connectionEnabled = false;
        public static int limiteDeConectados = 1000;
        public static int OfficialServerConnecteds = 0;
        public static bool saveContinuo;
        private static bool setacceid;
        public static bool setacclevel;
        private static int setaccid_temp = 0;

        private static bool unban;
        private static string unban_type = "";
        private static bool unban_key;
        private static string unban_key_s = "";

        private static bool ban;
        private static string ban_type = "";
        private static bool ban_key;
        private static string ban_key_s = "";
        private static bool ban_time;

        static void Main(string[] args)
        {
            Console.WriteLine("Authserver 1.0.0");
            Console.WriteLine("Demoniaco Games 2019 (c) para Proyecto B");
            Console.WriteLine("");
            Console.WriteLine("");
            General.InitializateServer();
            Console.WriteLine("Conectando con el servidor MySQl...");
            MySQL.Connectar(true);
            SetTimer();
            SetTimer2();

            connectionEnabled = true;

            threadConsole = new Thread(new ThreadStart(ConsoleThread));
            threadConsole.Start();
        }

        private static void ConsoleThread ()
        {
            string line;
            consoleRunning = true;

            while (consoleRunning)
            {
                line = Console.ReadLine();
                if (String.IsNullOrWhiteSpace(line))
                {
                    //consoleRunning = false;
                    //return;
                }
                else
                {
                    if (setversion)
                    {
                        ServiceData.Version = line;
                        setversion = false;
                        Console.WriteLine("Versión cambiada, nueva versión: " + ServiceData.Version);
                    }

                    if (setmodo)
                    {
                        ServiceData.sv_modo = line;
                        setmodo = false;
                        Console.WriteLine("Modo cambiado, Modo actual: " + ServiceData.sv_modo);
                    }

                    if (setacclevel)
                    {
                        int level = int.Parse(line);
                        setacceid = false;
                        setacclevel = false;

                        if (level < 0 | level > 3)
                        {
                            Console.WriteLine("Nivel de permisos fuera de los limites. [0-3]");
                        }
                        else
                        {

                            Data_access acc = ServiceData.Accounts_access.Find(x => x.id == setaccid_temp);

                            if (acc == null)
                            {
                                acc = new Data_access();
                                acc.id = setaccid_temp;
                                acc.gmlevel = level;
                                ServiceData.Accounts_access.Add(acc);
                            }
                            else
                            {
                                acc.gmlevel = level;
                            }

                            Console.WriteLine("Permisos modificados correctamente.");
                        }
                    }

                    if (setacceid)
                    {
                        setaccid_temp = int.Parse(line);
                        Console.WriteLine("Escriba el nivel... [0-3]");
                        setacceid = false;
                        setacclevel = true;
                    }

                    if (unban_key)
                    {
                        unban_key_s = line;
                        unban = false;
                        unban_key = false;

                        if (unban_type == "account")
                        {
                            Data_account a = ServiceData.Accounts.Find(x => x.username == unban_key_s);

                            if (a != null)
                            {
                                Data_banned ban = ServiceData.Accounts_banned.Find(x => x.id == a.id);

                                if (ban != null)
                                {
                                    ban.active = 0;
                                    Console.WriteLine("Cuenta fue desbaneada con exito.");
                                }
                                else
                                {
                                    Console.WriteLine("La cuenta no posee bans activos.");
                                }
                            }
                            else
                            {
                                Console.WriteLine("La cuenta no fue encontrada.");
                            }
                        }

                        if (unban_type == "ip")
                        {
                            Data_banned ban = ServiceData.Ip_banned.Find(x => x.ip == unban_key_s);

                            if (ban != null)
                            {
                                ban.active = 0;
                                Console.WriteLine("La ip fue desbaneada con exito.");
                            }
                            else
                            {
                                Console.WriteLine("La ip no posee bans activos.");
                            }
                        }

                        if (unban_type == "mac")
                        {
                            Data_banned ban = ServiceData.Ip_banned.Find(x => x.mac == unban_key_s);

                            if (ban != null)
                            {
                                ban.active = 0;
                                Console.WriteLine("La mac fue desbaneada con exito.");
                            }
                            else
                            {
                                Console.WriteLine("La mac no posee bans activos.");
                            }
                        }

                        if (unban_type == "superban")
                        {
                            Data_account a = ServiceData.Accounts.Find(x => x.username == unban_key_s);

                            if (a != null)
                            {
                                Console.WriteLine("Comando ejecutado con exito.");

                                Data_banned ban = ServiceData.Accounts_banned.Find(x => x.id == a.id && x.active == 1);

                                if (ban != null)
                                {
                                    ban.active = 0;
                                    Console.WriteLine("Cuenta fue desbaneada con exito.");
                                }

                                Data_banned ban2 = ServiceData.Ip_banned.Find(x => x.ip == a.last_ip && x.active == 1);

                                if (ban2 != null)
                                {
                                    ban2.active = 0;
                                    Console.WriteLine("La ip fue desbaneada con exito.");
                                }

                                Data_banned ban3 = ServiceData.Mac_banned.Find(x => x.mac == a.last_mac && x.active == 1);

                                if (ban3 != null)
                                {
                                    ban3.active = 0;
                                    Console.WriteLine("La mac fue desbaneada con exito.");
                                }
                            }
                            else
                            {
                                Console.WriteLine("La cuenta no fue encontrada.");
                            }
                        }
                    }

                    if (unban)
                    {
                        unban_type = line;
                        Console.WriteLine("Escriba la llave...");
                        unban = false;
                        unban_key = true;
                    }

                    if (ban_time)
                    {
                        string ban_time_s = line;
                        ban = false;
                        ban_key = false;
                        ban_time = false;

                        if (ban_type == "account")
                        {
                            Data_account a = ServiceData.Accounts.Find(x => x.username == unban_key_s);

                            if (a != null)
                            {
                                Data_banned ban = new Data_banned();
                                ban.active = 1;
                                ban.bandate = DateTime.Now;
                                DateTime.TryParse(ban_time_s, out ban.unbandate);
                                ban.banreason = "";
                                ban.bannedby = "[Console] [Manual]";
                                ban.id = a.id;
                                ServiceData.Accounts_banned.Add(ban);
                                Console.WriteLine("Cuenta baneada. ID: " + a.id + "[" + a.username + "]. Hasta el " + ban.unbandate.ToString() + ".");
                            }
                            else
                            {
                                Console.WriteLine("La cuenta no fue encontrada.");
                            }
                        }

                        if (ban_type == "ip")
                        {
                            Data_banned ban = new Data_banned();
                            ban.active = 1;
                            ban.bandate = DateTime.Now;
                            DateTime.TryParse(ban_time_s, out ban.unbandate);
                            ban.banreason = "";
                            ban.bannedby = "[Console] [Manual]";
                            ban.ip = ban_key_s;
                            ServiceData.Ip_banned.Add(ban);
                            Console.WriteLine("IP baneada. IP: " + ban.ip + ". Hasta el " + ban.unbandate.ToString() + ".");
                        }

                        if (ban_type == "mac")
                        {
                            Data_banned ban = new Data_banned();
                            ban.active = 1;
                            ban.bandate = DateTime.Now;
                            DateTime.TryParse(ban_time_s, out ban.unbandate);
                            ban.banreason = "";
                            ban.bannedby = "[Console] [Manual]";
                            ban.mac = ban_key_s;
                            ServiceData.Mac_banned.Add(ban);
                            Console.WriteLine("MAC baneada. MAC: " + ban.ip + ". Hasta el " + ban.unbandate.ToString() + ".");
                        }

                        if (ban_type == "superban")
                        {
                            Data_account a = ServiceData.Accounts.Find(x => x.username == ban_key_s);

                            if (a != null)
                            {
                                Data_banned ban = new Data_banned();
                                ban.active = 1;
                                ban.bandate = DateTime.Now;
                                DateTime.TryParse(ban_time_s, out ban.unbandate);
                                ban.banreason = "";
                                ban.bannedby = "[Console] [Manual]";
                                ban.id = a.id;
                                ServiceData.Accounts_banned.Add(ban);
                                Console.WriteLine("Cuenta baneada. ID: " + a.id + "[" + a.username + "]. Hasta el " + ban.unbandate.ToString() + ".");

                                Data_banned ban2 = new Data_banned();
                                ban2.active = 1;
                                ban2.bandate = DateTime.Now;
                                DateTime.TryParse(ban_time_s, out ban2.unbandate);
                                ban2.banreason = "";
                                ban2.bannedby = "[Console] [Manual]";
                                ban2.ip = ServiceData.ClearSring(a.last_ip);
                                ServiceData.Ip_banned.Add(ban2);
                                Console.WriteLine("IP baneada. IP: " + ServiceData.ClearSring(ban2.ip) + ". Hasta el " + ban2.unbandate.ToString() + ".");

                                Data_banned ban3 = new Data_banned();
                                ban3.active = 1;
                                ban3.bandate = DateTime.Now;
                                DateTime.TryParse(ban_time_s, out ban3.unbandate);
                                ban3.banreason = "";
                                ban3.bannedby = "[Console] [Manual]";
                                ban3.mac = ServiceData.ClearSring(a.last_mac);
                                ServiceData.Mac_banned.Add(ban3);
                                Console.WriteLine("MAC baneada. MAC: " + ServiceData.ClearSring(ban3.mac) + ". Hasta el " + ban3.unbandate.ToString() + ".");
                            }
                            else
                            {
                                Console.WriteLine("La cuenta no fue encontrada.");
                            }
                        }
                    }

                    if (ban_key)
                    {
                        ban_key_s = line;
                        Console.WriteLine("Escriba la fecha de desbaneo...");
                        ban = false;
                        ban_key = false;
                        ban_time = true;
                    }

                    if (ban)
                    {
                        ban_type = line;
                        Console.WriteLine("Escriba la llave...");
                        ban = false;
                        ban_key = true;
                        ban_time = false;
                    }

                    //---

                    if (line == "help")
                    {
                        Console.WriteLine("");
                        Console.WriteLine("Ayuda del Authserver:");
                        Console.WriteLine("help -Propociona información sobre comandos disponibles.");
                        Console.WriteLine("exit/close -Cierra el Authserver.");
                        Console.WriteLine("--Configuración de datos del servidor--");
                        Console.WriteLine("datareload -Recarga todos los datos al servidor.");
                        Console.WriteLine("datasave -Inicia el guardado de todos los datos del servidor.");
                        Console.WriteLine("--Configuración de MySQL--");
                        Console.WriteLine("mysqlconnect -Establece la conexión a la base MySQL.");
                        Console.WriteLine("mysqldisconnect -Quita la conexión a la base MySQL.");
                        Console.WriteLine("--Configuración del servidor--");
                        Console.WriteLine("version -Muestra la versión actual admitida.");
                        Console.WriteLine("setversion -Cambia la versión admitida.");
                        Console.WriteLine("modo -Muestra el modo actual del servidor.");
                        Console.WriteLine("setmodo -Cambia el modo actual del servidor.");
                        Console.WriteLine("modolist -Lista de todos los modos disponibles.");
                        Console.WriteLine("accset -Añadir nivel de acceso a una cuenta.");
                        Console.WriteLine("ban -Añadir ban.");
                        Console.WriteLine("unban -Quitar ban.");
                        Console.WriteLine("--Información--");
                        Console.WriteLine("online -Numero de jugadores conectados actualmente.");
                        Console.WriteLine("");
                    }

                    if (line == "mysqlconnect")
                    {
                        MySQL.Connectar(false);
                    }

                    if (line == "mysqldisconnect")
                    {
                        MySQL.Disconnect();
                    }

                    if (line == "exit")
                    {
                        cerrar();
                    }

                    if (line == "close")
                    {
                        cerrar();
                    }

                    if (line == "datareload")
                    {
                        ServiceData.ReloadAllData();
                    }

                    if (line == "datasave")
                    {
                        ServiceData.SaveAll();
                    }

                    if (line == "version")
                    {
                        Console.WriteLine("Versión: " + ServiceData.Version);
                    }

                    if (line == "setversion")
                    {
                        Console.WriteLine("Escriba la nueva versión...");
                        setversion = true;
                    }

                    if (line == "modo")
                    {
                        Console.WriteLine("Modo del servidor: " + ServiceData.sv_modo);
                    }

                    if (line == "setmodo")
                    {
                        Console.WriteLine("Escriba el nuevo modo...");
                        setmodo = true;
                    }

                    if (line == "modolist")
                    {
                        Console.WriteLine("");
                        Console.WriteLine("Modos del servidor:");
                        Console.WriteLine("online -Acepta conexiones y permite jugar.");
                        Console.WriteLine("mantenimiento -No acepta conexiones y no permite jugar.");
                        Console.WriteLine("offline -No acepta conexiones y no permite jugar.");
                        Console.WriteLine("");
                    }

                    if (line == "online")
                    {
                        int onlines = 0;
                        for (int i = 0; i < ServiceData.Accounts.Count; i++)
                        {
                            if (ServiceData.Accounts[i].online == 1)
                            {
                                onlines++;
                            }
                        }
                        Console.WriteLine("Jugadores online actualmente: " + onlines);
                    }

                    if (line == "accset")
                    {
                        Console.WriteLine("Escriba el id de la cuenta...");
                        setacceid = true;
                        setacclevel = false;
                    }

                    if (line == "unban")
                    {
                        Console.WriteLine("Escriba el tipo de unban...");
                        unban = true;
                    }

                    if (line == "ban")
                    {
                        Console.WriteLine("Escriba el tipo de ban...");
                        ban = true;
                        ban_key = false;
                        ban_time = false;
                    }
                }
            }
        }

        private static void SetTimer()
        {
            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(1000 * 60 * 10);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private static void SetTimer2()
        {
            // Create a timer with a two second interval.
            aTimer2 = new System.Timers.Timer(1000 * 60 * 1);
            // Hook up the Elapsed event for the timer. 
            aTimer2.Elapsed += OnTimedEvent2;
            aTimer2.AutoReset = true;
            aTimer2.Enabled = true;
        }

        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            //ServiceData.SaveAll();
            saveContinuo = true;
            //SetTimer();
        }

        private static void OnTimedEvent2(Object source, ElapsedEventArgs e)
        {
            ServiceData.ExeCommands();
            //SetTimer2();
        }

        private static void cerrar ()
        {
            Console.WriteLine("Cerrando Authserver...");
            //ServerTCP.CloseNetwork();
            ServiceData.SaveAll();
            MySQL.Disconnect();
            Environment.Exit(0);
        }
    }
}
