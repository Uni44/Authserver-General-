using System.Collections.Generic;
using UnityEngine;
using System.Net;
using DarkTreeFPS;
using UnityStandardAssets.CrossPlatformInput;
using System.Net.NetworkInformation;
using CommandTerminal;
using System;

namespace Assets.Scripts
{
    [System.Serializable]
    public class Servidor
    {
        public string nombre;
        public string ip;
        public int puerto;
    }

    public class NetworkManager : MonoBehaviour
    {
        protected string clientVersion = "0.0.1";

        public static NetworkManager instance;

        [Header("Cuenta variables")]
        public int acc_id;
        public string username;
        public string sessionkey;
        private string v;
        private string s;
        private int gmlevel;

        private string finalString1;
        private string finalString2;
        private string finalString3;

        [Header("Authserver variables")]
        public string auth_ip;
        public int auth_puerto;

        [Header("Servidores de juego variables")]
        public List<Servidor> servidoresDeJuego = new List<Servidor>();

        [Header("Otras variables")]
        public float gameserverPing;
        public float maxPing = 1.000f;
        public GameObject player;
        public GameObject cameras;
        public GameObject cameraYrecoil;
        public Inventory inventory;
        public WeaponManager weaponManager;
        public EquipManager equipManager;
        public PlayerStats playerStats;
        public FPSController fPSController;
        public Recoil recoil;
        public Sway sway;

        [Header("Tiempo variables")]
        public float timePing = 5;
        private float cargar_game = 5;
        private bool cargar_game_bool;
        private float registro_time;
        private bool registro_bool;

        //sincro
        private Vector3 pos_temp;
        private Vector3 rot_temp;
        private string anim;
        private int lean_temp;
        private string weapon_temp;

        private float timer_ping;
        private bool pingged;

        private void Awake()
        {
            instance = this;
        }

        void Start()
        {
            cargar_game = 5 + (float)MenuManager.instance.logoVideo.clip.length;
            cargar_game_bool = true;

            timer_ping = timePing;

            if (Application.version != clientVersion)
            {
                Debug.LogError("Version del cliente no correspodiente");
                Application.Quit();
            }

            DontDestroyOnLoad(this);
            UnityThread.initUnityThread();

            ClientHandleData.InitializePackets();
            ConnectToAuthServer();
            PopulateListServer();
            MenuManager.instance.ChangeMenu("Cargando");
        }

        public void Reset()
        {
            acc_id = 0;
            username = "";
            sessionkey = "";
            v = "";
            s = "";
            gmlevel = 0;
            timer_ping = timePing;
            ResetPlayer();
        }

        void PopulateListServer()
        {
            List<string> names = new List<string>();
            foreach (var server in servidoresDeJuego)
            {
                names.Add(server.nombre);
            }

            MenuManager.instance.server_dropdown.AddOptions(names);
        }

        void ConnectToAuthServer()
        {
            ConnectToServer(auth_ip, auth_puerto);
        }

        void ConnectToServer(string ip, int puerto)
        {
            if (ClientTCP.clientSocket != null)
            {
                if (ClientTCP.clientSocket.Connected)
                {
                    ClientTCP.Disconnect();
                    Debug.Log("Disconnected");
                }
            }
            ClientTCP.InitializingNetworking(ip, puerto);
        }

        public void ServerState(string msg)
        {
            string text = msg;
            char[] delimitador = { ';' };
            string[] variables = text.Split(delimitador);

            if (variables[0] == "online")
            {
                //MenuManager.instance.ChangeMenu("Login");
                cargar_game = 5f + (float)MenuManager.instance.logoVideo.clip.length;
            }

            if (variables[0] == "mantenimiento")
            {
                //MenuManager.instance.ChangeMenu("Login");
                DataSender.SendLogout();
                cargar_game = 5f + (float)MenuManager.instance.logoVideo.clip.length;
                MenuManager.instance.Mensaje("En mantenimiento", "El servidor actualmente se encuentra en mantenimiento");
                ClientTCP.Disconnect();
                Debug.Log("Disconnected");
                return;
            }

            if (variables[0] == "offline")
            {
                //MenuManager.instance.ChangeMenu("Login");
                DataSender.SendLogout();
                cargar_game = 5f + (float)MenuManager.instance.logoVideo.clip.length;
                MenuManager.instance.Mensaje("Desconectado", "El servidor actualmente se encuentra desconectado");
                ClientTCP.Disconnect();
                Debug.Log("Disconnected");
                return;
            }

            if (variables[2] == "gameserver")
            {
                ResetPlayer();
                if (Application.version != variables[1])
                {
                    DataSender.SendLogout();
                    MenuManager.instance.Mensaje("Desactualizado", "Su cliente se encuentra desactualizado, por favor actualicé de la versión " + Application.version + " a la nueva versión " + variables[1]);
                    ClientTCP.Disconnect();
                    Debug.Log("Disconnected");
                    MenuManager.instance.ChangeMenu("Login");
                    return;
                }
                Debug.Log("gameserver connected");
                MenuManager.instance.ChangeMenu("Cargando");
                registro_time = 4;
                registro_bool = true;
            }
        }

        private void Update()
        {
            ComprobarSincro();

            //timers

            if (cargar_game_bool == true)
            {
                MenuManager.escribiendo = true;
                cargar_game -= Time.deltaTime;

                if (cargar_game <= 0)
                {
                    cargar_game_bool = false;
                    SettingManager.instance.LoadPersonalizar();
                    MenuManager.instance.EnterMenu();
                    MenuManager.instance.ChangeMenu("Login");
                    MusicManager.instance.MusicOn();
                    MenuManager.instance.FirstTime();
                }

                if (cargar_game - 3 <= 0)
                {
                    MenuManager.instance.logoVideoBack.SetActive(false);
                    MenuManager.instance.logoVideo.gameObject.SetActive(false);
                    MenuManager.instance.fondoCamera.enabled = true;

                    ModoManager.instance.ConvertRecursos();

                }
            }

            if (registro_bool)
            {
                registro_time -= Time.deltaTime;
                if (registro_time <= 0)
                {
                    registro_bool = false;
                    string superstring = acc_id + ";" + username + ";" + sessionkey + ";" + v + ";" + s + ";" + gmlevel + ";" + SettingManager.instance.sexo + ";" + SettingManager.instance.pelo;
                    DataSender.SendCRegistroServer(superstring);
                }
            }

            if (ModoManager.instance.enPartida)
            {
                if (!pingged)
                {
                    timer_ping -= Time.deltaTime;

                    if (timer_ping <= 0)
                    {
                        timer_ping = timePing;
                        pingged = true;
                        gameserverPing = 0;
                        DataSender.SendSincroServer("ping;no");
                    }
                }
                else
                {
                    gameserverPing += Time.deltaTime;

                    if (gameserverPing >= maxPing)
                    {
                        ModoManager.instance.Desconectar();
                        MenuManager.instance.Mensaje("Atención", "Fuiste expulsado por tener un tiempo de respuesta alto con el servidor");
                        timer_ping = timePing;
                        pingged = false;
                    }
                }
            }
        }

        void ComprobarSincro()
        {
            if (ModoManager.instance.enPartida)
            {
                if (pos_temp != player.transform.position | rot_temp != cameras.transform.eulerAngles | anim != GetAnim() | lean_temp != Lean.leanGlobalValue | weapon_temp != weaponManager.GetCurrentWeapongID())
                {
                    if (!playerStats.InCar)
                        SincroMyPlayer();
                }
            }
        }

        public void SincroMyPlayer()
        {
            if (!PlayerStats.isPlayerDead)
            {
                pos_temp = player.transform.position;
                rot_temp = cameras.transform.eulerAngles;
                anim = GetAnim();
                lean_temp = Lean.leanGlobalValue;
                weapon_temp = weaponManager.GetCurrentWeapongID();
                string pos = player.transform.position.x.ToString("f4", System.Globalization.CultureInfo.InvariantCulture) + ":" + player.transform.position.y.ToString("f4", System.Globalization.CultureInfo.InvariantCulture) + ":" + player.transform.position.z.ToString("f4", System.Globalization.CultureInfo.InvariantCulture);
                float angle = cameras.transform.eulerAngles.x;
                angle = (angle > 180) ? angle - 360 : angle;
                string rot = angle.ToString("f4", System.Globalization.CultureInfo.InvariantCulture) + ":" + cameras.transform.eulerAngles.y.ToString("f4", System.Globalization.CultureInfo.InvariantCulture) + ":" + cameras.transform.eulerAngles.z.ToString("f4", System.Globalization.CultureInfo.InvariantCulture);
                string weaponid = weaponManager.GetCurrentWeapongID();
                string equipamiento = equipManager.GetAllIds();
                DataSender.SendSincroServer("sincro_my_player;" + acc_id + ";" + playerStats.health + ";" + pos + ";" + rot + ";" + weaponid + ";" + equipamiento + ";" + anim + ";" + Lean.leanGlobalValue);
            }
        }

        string GetAnim()
        {
            string agachado = "0";
            string corriendo = "0";

            if (fPSController.crouch)
            {
                agachado = "1";
            }

            if (fPSController.corriendo)
            {
                corriendo = "1";
            }

            string axis = CrossPlatformInputManager.GetAxis("Vertical").ToString("f4", System.Globalization.CultureInfo.InvariantCulture) + ";" + CrossPlatformInputManager.GetAxis("Horizontal").ToString("f4", System.Globalization.CultureInfo.InvariantCulture);

            if (MenuManager.escribiendo)
            {
                axis = "0;0";
            }

            return axis + ";" + agachado + ";" + corriendo;
        }

        public void SendPlayerDead(string id)
        {
            DataSender.SendSincroServer("dead_my_player;" + acc_id + ";" + id);
        }

        private void OnApplicationQuit()
        {
            if (ModoManager.instance.enPartida)
                GuardarDatos();
        }

        public void Disconnect(bool onlyDisconnect)
        {
            ClientTCP.Disconnect();
            Debug.Log("Disconnected");
        }

        public void GuardarDatos()
        {
            //salir del auto
            if (!playerStats.CarExit(false))
            {
                playerStats.ApplyDamage(9999, 0);
            }

            //guardar inventario
            string superstring = acc_id + ";" + "inventory_save;" + inventory.monedas + ";f";
            foreach (var item in inventory.inventoryUI.UIItems)
            {
                string id = "0";
                if (item.item != null)
                    id = item.item.id.ToString();
                superstring = superstring + ":" + id;
            }
            superstring = superstring + ":g";
            foreach (var item in inventory.inventoryUI.UIItems)
            {
                string ammo = "0";
                if (item.item != null)
                    ammo = item.item.ammo.ToString();
                superstring = superstring + ":" + ammo;
            }
            //guardar armas
            string nameweapon1 = "0";
            string nameweapon2 = "0";
            string ammoweapon1 = "0";
            string ammoweapon2 = "0";
            if (weaponManager.slots[1].storedDropObject != null)
            {
                nameweapon1 = weaponManager.slots[1].storedDropObject.GetComponent<Item>().id.ToString();
                ammoweapon1 = weaponManager.slots[1].storedWeapon.currentAmmo.ToString();
            }
            if (weaponManager.slots[2].storedDropObject != null)
            {
                nameweapon2 = weaponManager.slots[2].storedDropObject.GetComponent<Item>().id.ToString();
                ammoweapon2 = weaponManager.slots[2].storedWeapon.currentAmmo.ToString();
            }
            //guardar equipamiento
            string equipamiento = ":i:" + equipManager.GetAllIds();
            superstring = superstring + ":h:" + nameweapon1 + ":" + nameweapon2 + ":" + ammoweapon1 + ":" + ammoweapon2 + equipamiento;

            //guardar pos y rot
            string superstring2 = ":j:" + player.transform.position.x.ToString("f4", System.Globalization.CultureInfo.InvariantCulture) + ":" + player.transform.position.y.ToString("f4", System.Globalization.CultureInfo.InvariantCulture) + ":" + player.transform.position.z.ToString("f4", System.Globalization.CultureInfo.InvariantCulture) + ":" + cameras.transform.eulerAngles.x.ToString("f4", System.Globalization.CultureInfo.InvariantCulture) + ":" + cameras.transform.eulerAngles.y.ToString("f4", System.Globalization.CultureInfo.InvariantCulture) + ":" + cameras.transform.eulerAngles.z.ToString("f4", System.Globalization.CultureInfo.InvariantCulture);

            //guardar estados
            string playerdead = "0";
            if (PlayerStats.isPlayerDead)
            {
                playerdead = "1";
            }

            string playersangrado = "0";
            if (playerStats.sangrado)
            {
                playersangrado = "1";
            }

            string playerquebrado = "0";
            if (playerStats.quebradura)
            {
                playerquebrado = "1";
            }

            string superstring3 = ":k:" + playerdead.ToString() + ":" + playerStats.health.ToString() + ":" + playerStats.hydratation.ToString() + ":" + playerStats.satiety.ToString() + ":" + playersangrado.ToString() + ":" + playerquebrado;

            //guardar terminado
            DataSender.SendCSaveDatosServer(superstring + superstring2 + superstring3);
        }

        public void GuardarPlanos ()
        {
            string superplanos1 = "pb";

            List<int> pb = new List<int>(BuildingSystem.instance.planosSabidos);
            List<int> pc = new List<int>(CraftSystem.instance.planosSabidos);

            for (int i = 0; i < BuildingSystem.instance.planosDefault.Count; i++)
            {
                pb.Remove(BuildingSystem.instance.planosDefault[i]);
            }

            for (int i = 0; i < CraftSystem.instance.planosDefault.Count; i++)
            {
                pc.Remove(CraftSystem.instance.planosDefault[i]);
            }

            for (int i = 0; i < pb.Count; i++)
            {
                superplanos1 = superplanos1 + ":" + pb[i];
            }

            superplanos1 = superplanos1 + ":pc";

            for (int i = 0; i < pc.Count; i++)
            {
                superplanos1 = superplanos1 + ":" + pc[i];
            }

            DataSender.SendCSaveDatosServer(acc_id + ";planos_save;0;" + superplanos1);
        }

        public void LogOut()
        {
            ConnectToAuthServer();
        }

        public void CerrarSesion()
        {
            Reset();
            MenuManager.instance.ChangeMenu("Login");
            ConnectToAuthServer();
        }

        public void registrarse()
        {
            if (MenuManager.instance.user_reg_text.text.Length < 3)
            {
                MenuManager.instance.Mensaje("Corregir", "El nombre de usuario es demasiado corto");
                return;
            }

            if (MenuManager.instance.pass_reg_text.text.Length < 3)
            {
                MenuManager.instance.Mensaje("Corregir", "La contraseña es demasiada corta");
                return;
            }

            if (MenuManager.instance.pass_reg_text.text != MenuManager.instance.pass2_reg_text.text)
            {
                MenuManager.instance.Mensaje("Corregir", "Las contraseñas no coinciden");
                return;
            }

            if (MenuManager.instance.acept_reg_togle.isOn != true)
            {
                MenuManager.instance.Mensaje("Corregir", "Debes aceptar los términos y condiciones");
                return;
            }

            if (MenuManager.instance.pass_reg_text.text.Contains(";"))
            {
                MenuManager.instance.Mensaje("Corregir", "La contraseña no debe contener ';'");
                return;
            }

            if (MenuManager.instance.user_reg_text.text.Contains(";"))
            {
                MenuManager.instance.Mensaje("Corregir", "El nombre de usuario no debe contener ';'");
                return;
            }

            if (MenuManager.instance.email_reg_text.text == "")
            {
                MenuManager.instance.Mensaje("Corregir", "Introduce un email");
                return;
            }

            if (MenuManager.instance.email_reg_text.text.Contains(";"))
            {
                MenuManager.instance.Mensaje("Corregir", "El email no debe contener ';'");
                return;
            }

            RandomString(80);
            string datos = MenuManager.instance.user_reg_text.text + ";" + MenuManager.instance.pass_reg_text.text + ";" + MenuManager.instance.email_reg_text.text + ";" + finalString1 + ";" + finalString2 + ";" + finalString3;
            DataSender.SendRegistro(datos);
            Debug.Log("Registro Enviado: " + datos);
            MenuManager.instance.ChangeMenu("Cargando");
        }

        public void RandomString(int numero)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[numero];
            var random = new System.Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            finalString1 = new string(stringChars);

            var stringChars2 = new char[numero];

            for (int i = 0; i < stringChars2.Length; i++)
            {
                stringChars2[i] = chars[random.Next(chars.Length)];
            }

            finalString2 = new string(stringChars2);

            var stringChars3 = new char[numero];

            for (int i = 0; i < stringChars3.Length; i++)
            {
                stringChars3[i] = chars[random.Next(chars.Length)];
            }

            finalString3 = new string(stringChars3);
        }

        public void Registromsg(string msg)
        {
            if (msg == "error_user")
            {
                MenuManager.instance.ChangeMenu("Registro");
                MenuManager.instance.Mensaje("Error", "El nombre de usuario ya esta usado");
            }
            if (msg == "ok")
            {
                MenuManager.instance.ChangeMenu("Login");
                MenuManager.instance.user_reg_text.text = "";
                MenuManager.instance.pass_reg_text.text = "";
                MenuManager.instance.pass2_reg_text.text = "";
                MenuManager.instance.email_reg_text.text = "";
                MenuManager.instance.acept_reg_togle.isOn = false;
            }
        }

        public void ConnectToGameServer()
        {
            if (acc_id != 0 && username != "")
            {
                MenuManager.instance.ChangeMenu("Cargando");
                ConnectToServer(servidoresDeJuego[MenuManager.instance.server_dropdown.value].ip, servidoresDeJuego[MenuManager.instance.server_dropdown.value].puerto);
            }
        }

        public void loginmsg(string msg)
        {
            string text = msg;
            char[] delimitador = { ';' };
            string[] variables = text.Split(delimitador);
            if (variables[0] == "loged")
            {
                acc_id = int.Parse(variables[1]);
                username = variables[2];
                sessionkey = variables[3];
                v = variables[4];
                s = variables[5];
                gmlevel = int.Parse(variables[6]);
                MenuManager.instance.pass_login_text.text = "";
                MenuManager.instance.ChangeMenu("Lobby");
            }
            if (variables[0] == "incorrecto")
            {
                MenuManager.instance.ChangeMenu("Login");
                MenuManager.instance.Mensaje("Error", "Usuario o contraseña incorrectos");
            }
            if (variables[0] == "ready_connected")
            {
                MenuManager.instance.ChangeMenu("Login");
                MenuManager.instance.Mensaje("Error", "La cuenta se encuentra conectada ahora mismo");
            }
            if (variables[0] == "temp_block")
            {
                MenuManager.instance.ChangeMenu("Login");
                MenuManager.instance.Mensaje("Error", "La cuenta fue bloqueada temporalmente por intentar iniciar muchas veces");
            }
            if (variables[0] == "banned")
            {
                MenuManager.instance.ChangeMenu("Login");
                MenuManager.instance.Mensaje("Error", "La cuenta esta suspendida hasta el " + variables[2]);
            }
        }

        public void login()
        {
            if (ClientTCP.clientSocket == null | !ClientTCP.clientSocket.Connected)
            {
                ConnectToAuthServer();
                return;
            }

            if (MenuManager.instance.user_login_text.text.Contains(";"))
            {
                MenuManager.instance.Mensaje("Corregir", "El nombre de usuario no debe contener ';'");
                return;
            }
            if (MenuManager.instance.pass_login_text.text.Contains(";"))
            {
                MenuManager.instance.Mensaje("Corregir", "La contraseña no debe contener ';'");
                return;
            }

            if (MenuManager.instance.user_login_text.text != "")
            {
                if (MenuManager.instance.pass_login_text.text != "")
                {
                    string ip = PublicIP();
                    DataSender.SendLogin(MenuManager.instance.user_login_text.text + ";" + MenuManager.instance.pass_login_text.text + ";" + ip + ";" + GetMacAddress());
                    MenuManager.instance.ChangeMenu("Cargando");
                    //if (MenuManager.instance.user_save_login_toggle.isOn)
                    //{
                    //PlayerPrefs.SetString("usuario", MenuManager.instance.user_login_text.text);
                    //PlayerPrefs.Save();
                    //}
                }
            }
        }

        string PublicIP()
        {
            string externalip = new WebClient().DownloadString("http://icanhazip.com");
            return externalip;
        }

        string GetMacAddress()
        {
            var macAdress = "";
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var adapter in nics)
            {
                var address = adapter.GetPhysicalAddress();
                if (address.ToString() != "")
                {
                    macAdress = address.ToString();
                    return macAdress;
                }
            }
            return "Error";
        }

        public void GameSRegistroResult(string msg)
        {
            string text = msg;
            char[] delimitador = { ';' };
            string[] variables = text.Split(delimitador);

            if (variables[0] == "error")
            {
                if (variables[1] == "max_players")
                {
                    MenuManager.instance.ChangeMenu("Login");
                    MenuManager.instance.Mensaje("Error", "Servidor lleno");
                    ClientTCP.Disconnect();
                    Debug.Log("Disconnected");
                }

                if (variables[1] == "max_players")
                {
                    MenuManager.instance.ChangeMenu("Login");
                    MenuManager.instance.Mensaje("Error", "La cuenta se encuentra conectada ahora mismo");
                    ClientTCP.Disconnect();
                    Debug.Log("Disconnected");
                }
            }

            if (variables[0] == "ok")
            {
                MenuManager.instance.ChangeMenu("Cargando");
            }
        }

        public void ResetPlayer()
        {
            //fPSController.controllerRigidbody.isKinematic = true;
            fPSController.enabled = false;
            player.transform.position = new Vector3(0f, -15f, 0f);
            fPSController.ApplyNormal();
            inventory.monedas = 533;
            inventory.ChangeMonedas(0);

            for (int i = 0; i < inventory.characterItems.Count; i++)
            {
                if (inventory.characterItems[i] != null)
                {
                    GameObject temp = inventory.characterItems[i].gameObject;
                    inventory.characterItems.Remove(inventory.characterItems[i]);
                    Destroy(temp);
                }
            }
            inventory.characterItems.Clear();

            foreach (var item in inventory.inventoryUI.UIItems)
            {
                item.UpdateItem(null);
            }

            InventoryManager.showInventory = false;

            weaponManager.Reset();
            equipManager.Reset();
            playerStats.Reset();
            inventory.Actaul_peso = 444;
            inventory.Max_peso = 444 + 100;
            inventory.ChangePeso(0);
            Destroy(ModoManager.instance.temp_camerahead);
            cameraYrecoil.GetComponent<AudioListener>().enabled = true;
            weaponManager.handsAvatar.transform.SetParent(cameraYrecoil.transform.Find("WeaponCamera/Weapon holder/Sway"));
            weaponManager.handsAvatar.transform.localPosition = new Vector3(0f, -20, 0);
            FindObjectOfType<EnviroInterior>().Exit();
        }

        public void GameSSincro(string msg)
        {
            string text = msg;
            char[] delimitador = { ';' };
            string[] variables = text.Split(delimitador);

            if (variables[0] == "my_player")
            {
                MenuManager.instance.cargandoBarra.value = 0;
                MenuManager.instance.ChangeMenu("Cargando");
                ModoManager.instance.ChangeMap(true);
                MenuManager.instance.cargandoBarra.value = 20;

                //sincro mi inventario
                if (variables[1] == "sincro_inventory")
                {
                    if (variables[2] == "no_inventory")
                    {
                        ModoManager.instance.ReSpawn(player);
                        return;
                    }

                    char[] delimitador2 = { ':' };

                    string[] variables_inventario = variables[3].Split(delimitador2);

                    inventory.ChangeMonedas(int.Parse(variables[2]) - 533);
                    for (int i = 0; i < variables_inventario.Length; i++)
                    {
                        if (variables_inventario[i] != "0" && variables_inventario[i] != "f" && variables_inventario[i] != "g")
                        {
                            Item a = inventory.itemManager.FindItemByID(int.Parse(variables_inventario[i]));
                            inventory.characterItems.Add(a);
                            inventory.inventoryUI.UIItems[i - 1].UpdateItem(a);
                            inventory.ChangePeso(a.peso);

                            if (a.brujula)
                                inventory.brujula.SetActive(true);

                            if (a.radio)
                                playerStats.tieneRadio = true;
                        }

                        if (variables_inventario[i] != "0" && variables_inventario[i] == "g")
                        {
                            for (int j = 0 + i; j < variables_inventario.Length; j++)
                            {
                                if (variables_inventario[j] != "0" && variables_inventario[j] != "g" && variables_inventario[j] != "h" && variables_inventario[j] != "i")
                                {
                                    inventory.inventoryUI.UIItems[j - i - 1].item.ammo = int.Parse(variables_inventario[j]);
                                }

                                if (variables_inventario[j] == "h")
                                {
                                    weaponManager.Reset();

                                    if (variables_inventario[j + 1] != "0")
                                    {
                                        Item wea1 = inventory.itemManager.FindItemByID(int.Parse(variables_inventario[j + 1]));
                                        weaponManager.EquipWeapon(wea1.title, wea1.gameObject);
                                        weaponManager.slots[1].storedWeapon.ChangeAmmo(int.Parse(variables_inventario[j + 3]));
                                        //weaponManager.slots[1].storedWeapon.currentAmmo = int.Parse(variables_inventario[j + 3]);
                                    }
                                    if (variables_inventario[j + 2] != "0")
                                    {
                                        Item wea2 = inventory.itemManager.FindItemByID(int.Parse(variables_inventario[j + 2]));
                                        weaponManager.EquipWeapon(wea2.title, wea2.gameObject);
                                        weaponManager.slots[1].storedWeapon.ChangeAmmo(int.Parse(variables_inventario[j + 4]));
                                        //weaponManager.slots[2].storedWeapon.currentAmmo = int.Parse(variables_inventario[j + 4]);
                                    }

                                    if (variables_inventario[j + 5] == "i")
                                    {
                                        equipManager.Reset();
                                        if (variables_inventario[j + 1 + 5] != "0")
                                        {
                                            equipManager.Equipar(equipManager.SlotCasco, inventory.itemManager.FindItemByID(int.Parse(variables_inventario[j + 1 + 5])));
                                        }

                                        if (variables_inventario[j + 2 + 5] != "0")
                                        {
                                            equipManager.Equipar(equipManager.SlotCara, inventory.itemManager.FindItemByID(int.Parse(variables_inventario[j + 2 + 5])));
                                        }

                                        if (variables_inventario[j + 3 + 5] != "0")
                                        {
                                            equipManager.Equipar(equipManager.SlotPecho, inventory.itemManager.FindItemByID(int.Parse(variables_inventario[j + 3 + 5])));
                                        }

                                        if (variables_inventario[j + 4 + 5] != "0")// si alguien quiere entender este desastre llamame. Uni44.
                                        {
                                            equipManager.Equipar(equipManager.SlotChaleco, inventory.itemManager.FindItemByID(int.Parse(variables_inventario[j + 4 + 5])));
                                        }

                                        if (variables_inventario[j + 5 + 5] != "0")
                                        {
                                            equipManager.Equipar(equipManager.SlotMochila, inventory.itemManager.FindItemByID(int.Parse(variables_inventario[j + 5 + 5])));
                                        }

                                        if (variables_inventario[j + 6 + 5] != "0")
                                        {
                                            equipManager.Equipar(equipManager.SlotGuantes, inventory.itemManager.FindItemByID(int.Parse(variables_inventario[j + 6 + 5])));
                                        }

                                        if (variables_inventario[j + 7 + 5] != "0")
                                        {
                                            equipManager.Equipar(equipManager.SlotPantalones, inventory.itemManager.FindItemByID(int.Parse(variables_inventario[j + 7 + 5])));
                                        }

                                        if (variables_inventario[j + 8 + 5] != "0")
                                        {
                                            equipManager.Equipar(equipManager.SlotBotas, inventory.itemManager.FindItemByID(int.Parse(variables_inventario[j + 8 + 5])));
                                        }

                                        if (variables_inventario[j + 11 + 3] == "j")
                                        {
                                            player.transform.position = new Vector3(float.Parse(variables_inventario[j + 12 + 3], System.Globalization.CultureInfo.InvariantCulture), float.Parse(variables_inventario[j + 13 + 3], System.Globalization.CultureInfo.InvariantCulture), float.Parse(variables_inventario[j + 14 + 3], System.Globalization.CultureInfo.InvariantCulture));
                                            cameras.transform.localEulerAngles = new Vector3(float.Parse(variables_inventario[j + 15 + 3], System.Globalization.CultureInfo.InvariantCulture), float.Parse(variables_inventario[j + 16 + 3], System.Globalization.CultureInfo.InvariantCulture), float.Parse(variables_inventario[j + 17 + 3], System.Globalization.CultureInfo.InvariantCulture));
                                            fPSController.ApplyNormal();
                                        }

                                        if (variables_inventario[j + 18 + 3] == "k")
                                        {
                                            if (variables_inventario[j + 19 + 3] == "1")
                                            {
                                                ModoManager.instance.ReSpawn(player);
                                                return;
                                            }

                                            playerStats.health = int.Parse(variables_inventario[j + 20 + 3]);
                                            playerStats.hydratation = int.Parse(variables_inventario[j + 21 + 3]);
                                            playerStats.satiety = int.Parse(variables_inventario[j + 22 + 3]);

                                            if (variables_inventario[j + 22 + 4] == "1")
                                            {
                                                playerStats.sangrado = true;
                                            }

                                            if (variables_inventario[j + 22 + 5] == "1")
                                            {
                                                playerStats.quebradura = true;
                                            }
                                        }
                                    }
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }

                if (variables[1] == "sincro_planos")
                {
                    char[] delimitador2 = { ':' };

                    string[] variables_planos = variables[3].Split(delimitador2);

                    for (int i = 0; i < variables_planos.Length; i++)
                    {
                        if (variables_planos[i] != "pb" && variables_planos[i] != "pc")
                        {
                            BuildingSystem.instance.planosSabidos.Add(int.Parse(variables_planos[i]));
                        }
                        else
                        {
                            if (variables_planos[i] == "pc")
                            {
                                for (int j = 0 + i; j < variables_planos.Length; j++)
                                {
                                    if (variables_planos[j] != "pc")
                                    {
                                        CraftSystem.instance.planosSabidos.Add(int.Parse(variables_planos[j]));
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }

            if (variables[0] == "sincro_corpose")
            {
                ModoManager.instance.CreateCorpose(msg);
            }

            if (variables[0] == "sincro_del_corpose")
            {
                ModoManager.instance.DeleteCorpose(msg);
            }

            if (variables[0] == "add_player")
            {
                if (int.Parse(variables[2]) != acc_id)
                    ModoManager.instance.CreateNetworkPlayer(msg);
            }

            if (variables[0] == "remove_player")
            {
                if (int.Parse(variables[2]) != acc_id)
                    ModoManager.instance.RemoveNetworkPlayer(msg);
            }

            if (variables[0] == "semi_ok")
            {
                MenuManager.instance.ChangeMenu("Cargando");
                MenuManager.instance.cargandoBarra.value = 40;
                DataSender.SendSincroServer("send_my_player;" + acc_id);
            }

            if (variables[0] == "casi_ok")
            {
                MenuManager.instance.ChangeMenu("Cargando");
                MenuManager.instance.cargandoBarra.value = 60;
                ModoManager.instance.CorposeComprobe();
            }

            if (variables[0] == "sincro_recurso_reset")
            {
                MenuManager.instance.ChangeMenu("Cargando");
                MenuManager.instance.cargandoBarra.value = 80;
                ModoManager.instance.ResetRecursos(false);
            }

            if (variables[0] == "sincro_recurso")
            {
                ModoManager.instance.SincroRecurso(int.Parse(variables[1]), int.Parse(variables[2]));
            }

            if (variables[0] == "add_building")
            {
                ModoManager.instance.AddBuilding(msg);
            }

            if (variables[0] == "sincro_all_car")
            {
                ModoManager.instance.SincroCar(msg);
            }

            if (variables[0] == "sincro_airdrop")
            {
                ModoManager.instance.CreateSupply(int.Parse(variables[1]), int.Parse(variables[2]));
            }

            if (variables[0] == "sincro_weather")
            {
                ModoManager.instance.ActiveSky(DateTime.Parse(variables[1]), int.Parse(variables[2]), variables[3]);
            }

            if (variables[0] == "ok")
            {
                MenuManager.instance.cargandoBarra.value = 100;
                playerStats.gameObject.GetComponent<PlayerAvatar>().HidePartsAvatar();
                ModoManager.instance.ResetTimerSaver();
                ModoManager.instance.enPartida = true;
                //fPSController.controllerRigidbody.isKinematic = false;
                fPSController.ApplyNormal();
                fPSController.enabled = true;
                fPSController.ApplyNormal();
                MenuManager.instance.ExitMenu();
                MenuManager.instance.ChangeMenu("Game");
                BlumpAntiCheatManager.instance.ProgramsComprobe();
            }

            //ingame

            if (variables[0] == "sincro_player")
            {
                if (int.Parse(variables[2]) != acc_id)
                {
                    ModoManager.instance.UpdatePlayer(msg);
                }
            }

            if (variables[0] == "ping")
            {
                pingged = false;
            }

            if (variables[0] == "container_close")
            {
                if (ModoManager.instance.containerManager.open)
                {
                    if (ModoManager.instance.containerManager.tempContainer.id == int.Parse(variables[1]))
                        ModoManager.instance.containerManager.Close();
                }
            }

            if (variables[0] == "horno_close")
            {
                if (HornosManager.instance.open)
                {
                    if (HornosManager.instance.tempHorno.instanceId == int.Parse(variables[1]))
                        HornosManager.instance.Close();
                }
            }

            if (variables[0] == "dead_player")
            {
                ModoManager.instance.DeadPlayer(msg);
            }

            if (variables[0] == "sincro_lootbox")
            {
                ModoManager.instance.UpdateLootBox(msg);
            }

            if (variables[0] == "sincro_container_content")
            {
                ModoManager.instance.UpdateContainerContent(msg, false, null);
            }

            if (variables[0] == "sincro_pasos")
            {
                if (int.Parse(variables[2]) != acc_id)
                {
                    ModoManager.instance.PlayerPlayFootstep(msg);
                }
            }

            if (variables[0] == "add_item")
            {
                ModoManager.instance.AddItem(msg);
            }

            if (variables[0] == "remove_item")
            {
                ModoManager.instance.RemoveItem(msg);
            }

            if (variables[0] == "fire")
            {
                ModoManager.instance.Fire(int.Parse(variables[1]));
            }

            if (variables[0] == "reload")
            {
                ModoManager.instance.Reload(int.Parse(variables[1]));
            }

            if (variables[0] == "hit")
            {
                if (int.Parse(variables[2]) == acc_id)
                {
                    ModoManager.instance.Hit(msg);
                }
            }

            if (variables[0] == "recurso_farm")
            {
                ModoManager.instance.FarmRecurso(int.Parse(variables[1]), int.Parse(variables[2]));
            }

            if (variables[0] == "recurso_reset")
            {
                ModoManager.instance.ResetRecurso(int.Parse(variables[1]));
            }

            if (variables[0] == "build_hit")
            {
                ModoManager.instance.HitBuilding(int.Parse(variables[1]), int.Parse(variables[2]));
            }

            if (variables[0] == "sincro_horno_content")
            {
                ModoManager.instance.UpdateHornoContent(msg);
            }

            if (variables[0] == "sincro_partes_car")
            {
                ModoManager.instance.UpdateCarPartes(msg);
            }

            if (variables[0] == "sincro_car")
            {
                ModoManager.instance.UpdateCar(msg);
            }

            if (variables[0] == "sincro_player_car")
            {
                ModoManager.instance.UpdatePlayerCar(int.Parse(variables[1]), int.Parse(variables[4]), variables[3], variables[2]);
            }

            if (variables[0] == "sincro_ligth_car")
            {
                ModoManager.instance.UpdateLigthCar(int.Parse(variables[1]), bool.Parse(variables[2]));
            }

            if (variables[0] == "car_desactive")
            {
                ModoManager.instance.DesactiveCar(int.Parse(variables[1]));
            }

            if (variables[0] == "car_respawn")
            {
                ModoManager.instance.RespawnCar(int.Parse(variables[1]));
            }

            if (variables[0] == "change_door")
            {
                ModoManager.instance.DoorChange(int.Parse(variables[1]), bool.Parse(variables[2]));
            }

            if (variables[0] == "create_granada")
            {
                ModoManager.instance.InstantiateGranada(msg);
            }

            if (variables[0] == "new_supply")
            {
                ModoManager.instance.CreateAvionSupply(int.Parse(variables[1]), int.Parse(variables[2]));
            }

            if (variables[0] == "ricochet")
            {
                Vector3 hit = new Vector3(float.Parse(variables[1], System.Globalization.CultureInfo.InvariantCulture), float.Parse(variables[2], System.Globalization.CultureInfo.InvariantCulture), float.Parse(variables[3], System.Globalization.CultureInfo.InvariantCulture));

                if (Vector3.Distance(hit, player.transform.position) <= 100)
                    weaponManager.RicochetSFX(hit);
            }

            if (variables[0] == "send_chat")
            {
                ChatManager.instance.AddMSG(variables[1], variables[2]);
            }

            if (variables[0] == "radio")
            {
                ModoManager.instance.RadioSound(int.Parse(variables[2]), variables[3], variables[4]);
            }

            if (variables[0] == "change_cartel")
            {
                ModoManager.instance.UpdateCartel(int.Parse(variables[1]), variables[2]);
            }

            if (variables[0] == "change_parlante")
            {
                ModoManager.instance.UpdateParlante(int.Parse(variables[1]), variables[2], int.Parse(variables[3]));
            }

            if (variables[0] == "update_weather")
            {
                ModoManager.instance.UpdateSky(DateTime.Parse(variables[1]), int.Parse(variables[2]));
            }
        }

        public void GameSGrupoSincro(string msg)
        {
            string text = msg;
            char[] delimitador = { ';' };
            string[] variables = text.Split(delimitador);

            if (variables[0] == "error")
            {
                if (variables[1] == "nombreocupado")
                {
                    MenuManager.instance.MensajeGame("Error", "El nombre ya esta en uso");
                }

                if (variables[1] == "limitedeguerras")
                {
                    MenuManager.instance.MensajeGame("Error", "Para iniciar una guerra nueva necesitas 10 miembros mínimos por cada guerra en progreso del grupo");
                }
            }

            if (variables[0] == "sincro_grupo")
            {
                GrupoManager.instance.UpdateGrupo(msg);
            }

            if (variables[0] == "sincro_permisos")
            {
                GrupoManager.instance.UpdatePermisos(msg);
            }

            if (variables[0] == "sincro_permisos")
            {
                GrupoManager.instance.UpdatePermisos(msg);
            }

            if (variables[0] == "sincro_miembro_clear")
            {
                GrupoManager.instance.miembros.Clear();
            }

            if (variables[0] == "sincro_miembro_add")
            {
                GrupoManager.instance.AddMiembro(msg);
            }

            if (variables[0] == "sincro_subgrupo_clear")
            {
                GrupoManager.instance.Grupos.Clear();
            }

            if (variables[0] == "sincro_subgrupo_add")
            {
                GrupoManager.instance.AddGrupo(msg);
            }

            if (variables[0] == "invitar")
            {
                GrupoManager.instance.AddInvitacion(msg);
            }

            if (variables[0] == "reset_grupo")
            {
                GrupoManager.instance.ResetGrupo();
            }

            if (variables[0] == "sincro_miembro_remove")
            {
                GrupoManager.instance.RemoveMiembro(int.Parse(variables[1]));
            }

            if (variables[0] == "sincro_miembro_change_grupo")
            {
                GrupoManager.instance.CambienGrupoMiembro(int.Parse(variables[1]), int.Parse(variables[2]));
            }

            if (variables[0] == "sincro_guerra_todas_clear")
            {
                GrupoManager.instance.todasLasGuerras.Clear();
            }

            if (variables[0] == "sincro_guerra_todas_add")
            {
                GrupoManager.instance.AddGuerra(GrupoManager.instance.todasLasGuerras, "", variables[1], variables[2], variables[3], variables[4], variables[5], int.Parse(variables[6]));
            }

            if (variables[0] == "sincro_guerra_grupo_clear")
            {
                GrupoManager.instance.grupoGuerras.Clear();
            }

            if (variables[0] == "sincro_guerra_grupo_add")
            {
                GrupoManager.instance.AddGuerra(GrupoManager.instance.grupoGuerras, variables[1], variables[2], variables[3], variables[4], variables[5], variables[6], int.Parse(variables[7]));
            }

            if (variables[0] == "sincro_guerra_rendido")
            {
                GrupoManager.instance.GuerraRendida(int.Parse(variables[1]), variables[2]);
            }
        }

        public void ReferenceAdmin(string msg)
        {
            BlumpAntiCheatManager.instance.ProgramsComprobe();
            GameSAdministration(msg);
        }

        private void GameSAdministration(string msg)
        {
            string text = msg;
            char[] delimitador = { ';' };
            string[] variables = text.Split(delimitador);

            if (variables[0] == "kicked")
            {
                ModoManager.instance.Desconectar();
                MenuManager.instance.Mensaje("Atención", "Fuiste expulsado del servidor");
            }
        }

        public void PlayerList ()
        {
            if (NetworkManager.instance.gmlevel > 0)
            {

                if (Terminal.IssuedError) return; // Error will be handled by Terminal

                Terminal.Log("List of players:");

                for (int i = 0; i < ModoManager.instance.NetworkPlayers.Count; i++)
                {
                    Terminal.Log("Player {0} - {1} - ID: {2}", i, ModoManager.instance.NetworkPlayers[i].username, ModoManager.instance.NetworkPlayers[i].id);
                }
            }
        }

        public void PlayerBan(string tipo, string key, string tiempo, string razon)
        {
            if (NetworkManager.instance.gmlevel > 0)
            {
                DateTime fecha;

                if (Terminal.IssuedError) return; // Error will be handled by Terminal

                if (!DateTime.TryParse(tiempo, out fecha))
                {
                    Terminal.Log("Date and time not recognized in argument 3", TerminalLogType.Error);
                    return;
                }

                Terminal.Log("Baning player with key: {0}", key);
                DataSender.SendAdministration("ban;" + NetworkManager.instance.acc_id + ";" + NetworkManager.instance.gmlevel + ";" + tipo + ";" + key + ";" + tiempo + ";" + razon + ";" + NetworkManager.instance.username + ";" + NetworkManager.instance.v + ";" + NetworkManager.instance.s);
            }
        }

        public void PlayerCut (string name)
        {
            if (NetworkManager.instance.gmlevel > 0)
            {
                string a = name;

                if (Terminal.IssuedError) return; // Error will be handled by Terminal

                Terminal.Log("Cuting player with name: {0}", a);
                DataSender.SendAdministration("cut;" + NetworkManager.instance.acc_id + ";" + NetworkManager.instance.gmlevel + ";" + a);
            }
        }

        public void PlayerKick(string name)
        {
            if (NetworkManager.instance.gmlevel > 0)
            {
                string a = name;

                if (Terminal.IssuedError) return; // Error will be handled by Terminal

                Terminal.Log("Kiking player with name: {0}", a);
                DataSender.SendAdministration("kick;" + NetworkManager.instance.acc_id + ";" + NetworkManager.instance.gmlevel + ";" + a);
            }
        }

        public void IAmStupid()
        {
            AutoBan();
        }

        private void AutoBan()
        {
            DataSender.SendAdministration("iamstupid;" + username + ";" + sessionkey + ";" + v + ";" + s);
        }
    }
}
