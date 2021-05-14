using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Actual_DB_Test
{
    public class PSSDBConnection
    {
        private readonly MySqlConnection connection;
        private readonly string server;
        private readonly string database;
        private readonly string username;
        private readonly string password;

        //Constructor
        public PSSDBConnection()
        {
            server = "localhost";
            database = "photos_storage_server";
            username = "root";
            password = "Ph0t0s_Server";
            connection = new MySqlConnection("SERVER=" + server + ";" + "DATABASE=" + database + ";" + "username=" + username + ";" + "PASSWORD=" + password + ";");
        }

        public bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException e)
            {
                switch (e.Number)
                {
                    case 0:
                        //TODO: make these throw exceptions and catch these when trying to connect
                        Console.WriteLine("Cannot connect to server.  Contact administrator");
                        break;

                    case 1045:
                        Console.WriteLine("Invalid username/password, please try again");
                        break;

                    default:
                        Console.WriteLine("Something happened. Error code: " + e.Number);
                        break;
                }
                return false;
            }
            catch (Exception)
            {
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
            catch (MySqlException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        //For inserting a photo or video into the media table (the main table).
        public void InsertMedia(string path, DateTime dateTaken)
        {
            if (OpenConnection())
            {
                MySqlCommand cmd = new MySqlCommand("INSERT INTO media VALUES (@path, @dateAdded, @dateTaken)", connection);
                cmd.Parameters.AddWithValue("@path", path);
                cmd.Parameters.AddWithValue("@dateAdded", DateTime.Now);
                cmd.Parameters.AddWithValue("@dateTaken", dateTaken);

                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (MySqlException e)
                {
                    switch (e.Number)
                    {
                        case 1062:
                            Console.WriteLine('"' + path + "\" is already in the database. Error code: " + e.Number);
                            break;

                        default:
                            Console.WriteLine("Something happened. Error code: " + e.Number);
                            break;
                    }
                }
                finally
                {
                    CloseConnection();
                }
            }
        }

        //Add a new album to the table of Album names and IDs
        public void CreateAlbum(string name)
        {
            if (OpenConnection())
            {
                try
                {
                    //TODO: INJECTION!!!!!!!!!!!!!
                    MySqlCommand cmd = new MySqlCommand("INSERT INTO albums (Name) VALUES (\"" + name + "\")", connection);
                    cmd.ExecuteNonQuery();
                }
                catch (MySqlException e)
                {
                    Console.WriteLine("Something happened. Error code: " + e.Number);
                }
                finally
                {
                    CloseConnection();
                }
            }
        }

        public void AddToAlbum(string path, int albumID)
        {
            if (OpenConnection())
            {
                try
                {
                    //Will IGNORE (not throw error) if there is a duplicate.
                    MySqlCommand cmd = new MySqlCommand("INSERT IGNORE INTO album_entries VALUES (@path, @albumID, @date_added_to_album)", connection);
                    cmd.Parameters.AddWithValue("@path", path);
                    cmd.Parameters.AddWithValue("@albumID", albumID);
                    cmd.Parameters.AddWithValue("@date_added_to_album", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
                catch (MySqlException e)
                {
                    Console.WriteLine("Something happened. Error code: " + e.Number);
                }
                finally
                {
                    CloseConnection();
                }
            }
        }

        //public void DeletePhoto(string path)
        //{
        //    if (OpenConnection())
        //    {
        //        MySqlCommand cmd = new MySqlCommand("DELETE FROM photos WHERE Directory = @path", connection);
        //        cmd.Parameters.AddWithValue("@path", path);
        //        cmd.ExecuteNonQuery();
        //        CloseConnection();
        //    }
        //}
    }
}