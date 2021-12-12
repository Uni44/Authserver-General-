using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Authserver
{
    class Test
    {

        public MySqlConnection con;

        public void hacer()
        {
            Query("INSERT INTO account (username) VALUES ('putooo')", 1, 1);
        }

        public void Connectar()
        {
            MySqlConnectionStringBuilder conexion = new MySqlConnectionStringBuilder();
            conexion.Server = "localhost";
            conexion.UserID = "root";
            conexion.Password = "ascent";
            conexion.Database = "auth";
            conexion.SslMode = MySqlSslMode.None;
            con = new MySqlConnection(conexion.ToString());
            try
            {
                con.Open();
                Console.WriteLine("Conexion exitosa con el servidor MySQL");
                Console.WriteLine("");
                Console.WriteLine("");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al conectar con el servidor MySQL: " + ex.Message);
            }
        }

        public void Query(string query33, int resultado, int connectionID)
        {
            if (con == null)
            {
                Connectar();
            }
            string text = "personaje_comprado;197699;1;o";
            char[] delimitador = { ';' };
            string[] variables = text.Split(delimitador);
            MySQL.Query("INSERT INTO datos_saved (id, key_id, value_int, value_string) VALUES ('" + variables[1] + "', '" + Security.Encriptar(variables[0]) + "', '" + variables[2] + "', '" + variables[3] + "')");
        }
    }
}
