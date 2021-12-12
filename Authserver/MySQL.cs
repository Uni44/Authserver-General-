using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Authserver
{
    static class MySQL
    {

        public static MySqlConnection con = null;

        public static void Connectar(bool cargar_datos)
        {
            MySqlConnectionStringBuilder conexion = new MySqlConnectionStringBuilder();
            conexion.Server = "localhost";
            conexion.UserID = "root";
            conexion.Password = "ascent";
            conexion.Database = "authB";
            conexion.SslMode = MySqlSslMode.None;
            con = new MySqlConnection(conexion.ToString());
            try
            {
                con.Open();
                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("Conexion exitosa con el servidor MySQL");
                if (cargar_datos)
                {
                    string query = "SELECT * FROM account";
                    MySqlCommand command = new MySqlCommand(query, MySQL.con);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    if (count > 0)
                    {
                        MySQL.Query("UPDATE account SET online = '0'");
                    }
                    ServiceData.LoadAllData();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al conectar con el servidor MySQL: " + ex.Message);
            }
        }

        public static void Disconnect()
        {
            if (con.State == System.Data.ConnectionState.Open)
            {
                Console.WriteLine("Cerrando Conexion con el servidor MySQL");
                Console.WriteLine("");
                Console.WriteLine("");
                con.Close();
            }
        }

        public static void Query(string query)
        {
            try
            {
                MySqlCommand commandss = new MySqlCommand(query, con);
                commandss.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("MySQL Query Error: " + ex);
            }
        }
    }
}
