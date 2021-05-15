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
                        Console.WriteLine("Cannot connect to server.");
                        break;

                    case 1045:
                        Console.WriteLine("Invalid username/password, please try again");
                        break;

                    default:
                        Console.WriteLine("An unknown error occurred. Error code: " + e.Number + " Message: " + e.Message);
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
                MySqlCommand cmd = new MySqlCommand("INSERT IGNORE INTO media VALUES (@path, @dateAdded, @dateTaken, @separate)", connection); //Ignores duplicates
                cmd.Parameters.AddWithValue("@path", path);
                cmd.Parameters.AddWithValue("@dateAdded", DateTime.Now);
                cmd.Parameters.AddWithValue("@dateTaken", dateTaken);
                cmd.Parameters.AddWithValue("@separate", false);

                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (MySqlException e)
                {
                    switch (e.Number)
                    {
                        default:
                            Console.WriteLine("An unknown error occurred. Error code: " + e.Number + " Message: " + e.Message);
                            break;
                    }
                }
                finally
                {
                    CloseConnection();
                }
            }
        }

        //Add a new album to the table of Album names and IDs. ID is auto incrementing.
        public void CreateAlbum(string name)
        {
            if (OpenConnection())
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand("INSERT INTO albums (Name) VALUES (@name)", connection);
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.ExecuteNonQuery();
                }
                catch (MySqlException e)
                {
                    switch (e.Number)
                    {
                        case 1062:
                            Console.WriteLine("Album \"" + name + "\" already exists. Error code: " + e.Number);
                            break;

                        default:
                            Console.WriteLine("An unknown error occurred. Error code: " + e.Number + " Message: " + e.Message);
                            break;
                    }
                }
                finally
                {
                    CloseConnection();
                }
            }
        }

        //Deletes items in the album from album_entries, then from the albums table.
        public void DeleteAlbum(string name)
        {
            if (OpenConnection())
            {
                try
                {
                    //Find the ID of the album to delete, then use that to delete every item in that album from album_entries.
                    MySqlCommand selectCmd = new MySqlCommand("SELECT id FROM albums WHERE name = @name", connection);
                    selectCmd.Parameters.AddWithValue("@name", name);
                    selectCmd.ExecuteNonQuery();
                    MySqlDataReader reader = selectCmd.ExecuteReader();

                    if (reader.HasRows) //Check if there is actually a row to read. If reader.Read() is called and there isn't, a nasty exception is raised.
                    {
                        reader.Read(); //There should only be 1 line to read.
                        int delID = reader.GetInt32(0); //First and only column
                        reader.Close();

                        //Remove all corresponding items from album_entries table.
                        MySqlCommand delCmd = new MySqlCommand("DELETE FROM album_entries WHERE album_id = @id", connection);
                        delCmd.Parameters.AddWithValue("@id", delID);
                        delCmd.ExecuteNonQuery();

                        //Finally, remove from albums table.
                        delCmd.CommandText = "DELETE FROM albums WHERE name = @name";
                        delCmd.Parameters.AddWithValue("@name", name);
                        delCmd.ExecuteNonQuery();
                    }
                    else
                    {
                        throw new ArgumentException("The album " + name + " does not exist!");
                    }
                }
                catch (MySqlException e)
                {
                    Console.WriteLine("An unknown error occurred. Error code: " + e.Number + " Message: " + e.Message);
                }
                finally
                {
                    CloseConnection();
                }
            }
        }

        //Add a single path to an album in album_entries.
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
                    Console.WriteLine("An unknown error occurred. Error code: " + e.Number + " Message: " + e.Message);
                }
                finally
                {
                    CloseConnection();
                }
            }
        }

        //Add an item to media and an album.
        public void MediaAndAlbumInsert(string path, int albumID, DateTime dateTaken)
        {
            InsertMedia(path, dateTaken);
            AddToAlbum(path, albumID);
        }

        // public void DeletePhonto(string path)
        // {
        //    if (OpenConnection())
        //    {
        //        MySqlCommand cmd = new MySqlCommand("DELETE FROM media WHERE path = @path", connection);
        //        cmd.Parameters.AddWithValue("@path", path);
        //        cmd.ExecuteNonQuery();
        //        cmd.CommandText = "DELETE from album_entries"
        //        CloseConnection();
        //    }
        // }
    }
}