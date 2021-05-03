using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Actual_DB_Test
{
    public class PSSDBConnection
    {
        private MySqlConnection connection;
        private string server;
        private string database;
        private string username;
        private string password;

        //Constructor
        public PSSDBConnection()
        {
            server = "localhost";
            database = "photos_storage_server";
            username = "root";
            password = "Ph0t0s_Server";
            string connectionString = "SERVER=" + server + ";" + "DATABASE=" + database + ";" + "username=" + username + ";" + "PASSWORD=" + password + ";";
            connection = new MySqlConnection(connectionString);
        }

        public bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                //When handling errors, you can your application's response based 
                //on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                switch (ex.Number)
                {
                    case 0:
                        Console.WriteLine("Cannot connect to server.  Contact administrator");
                        break;

                    case 1045:
                        Console.WriteLine("Invalid username/password, please try again");
                        break;
                }
                return false;
            }
        }

        public bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public void InsertPhoto(string dir, DateTime dateTaken, string albumsList)
        {
            if (OpenConnection())
            {
                MySqlCommand cmd = new MySqlCommand("INSERT INTO photos VALUES (@dir, @dateAdded, @dateTaken)", connection);
                cmd.Parameters.AddWithValue("@dir", dir);
                cmd.Parameters.AddWithValue("@dateAdded", DateTime.Now);
                cmd.Parameters.AddWithValue("@dateTaken", dateTaken);
                cmd.ExecuteNonQuery();
                CloseConnection();
            }
        }

        //public void UpdatePhoto(string dir, DateTime dateTaken, string albumsList)
        //{This needs some work. https://stackoverflow.com/questions/7505808/why-do-we-always-prefer-using-parameters-in-sql-statements/7505842???
        //    if (OpenConnection())
        //    {
        //        string query = "UPDATE photos SET Directory = dir, Date_Added = @dateAdded, Date_Taken = @dateTaken";
        //        MySqlCommand cmd = new MySqlCommand(query, connection);
        //        cmd.Parameters.AddWithValue("@dir", dir);
        //        cmd.Parameters.AddWithValue("@dateAdded", DateTime.Now);
        //        cmd.Parameters.AddWithValue("@dateTaken", dateTaken);
        //        cmd.ExecuteNonQuery();
        //        CloseConnection();
        //    }
        //}

        public void DeletePhoto(string dir)
        {
            if (OpenConnection())
            {
                MySqlCommand cmd = new MySqlCommand("DELETE FROM photos WHERE Directory = @dir", connection);
                cmd.Parameters.AddWithValue("@dir", dir);
                cmd.ExecuteNonQuery();
                CloseConnection();
            }
        }

        //public List<string>[] Select()
        //{
        //    string query = "SELECT Directory, Date_Added, Date_Taken, FROM photos";

        //    //Create a list to store the result
        //    List<string>[] list = new List<string>[3];
        //    list[0] = new List<string>();
        //    list[1] = new List<string>();
        //    list[2] = new List<string>();

        //    if (OpenConnection())
        //    {
        //        MySqlCommand cmd = new MySqlCommand(query, connection);
        //        MySqlDataReader dataReader = cmd.ExecuteReader();

        //        //Read the data and store them in the list
        //        while (dataReader.Read())
        //        {
        //            list[0].Add(dataReader["Directory"] + "");
        //            list[1].Add(dataReader["Date_Added"] + "");
        //            list[2].Add(dataReader["Date_Taken"] + "");
        //        }

        //        dataReader.Close();
        //        CloseConnection();
        //        return list;
        //    }
        //    else
        //        return list;
        //}
    }
}