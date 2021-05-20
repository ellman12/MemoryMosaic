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

        //Represents a row in the media and album_entries tables.
        //Used in SelectAlbum().
        public struct Media
        {
            public readonly string path;
            public readonly DateTime dateTaken;
            public readonly DateTime dateAdded;

            public Media(string p, DateTime dt, DateTime da)
            {
                path = p;
                dateTaken = dt;
                dateAdded = da;
            }
        }

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
                        throw new ApplicationException("Cannot connect to server.");

                    case 1045:
                        throw new ApplicationException("Invalid username/password, please try again");

                    default:
                        throw new ApplicationException("An unknown error occurred. Error code: " + e.Number + " Message: " + e.Message);
                }
            }
        }

        public void CloseConnection()
        {
            try
            {
                connection.Close();
            }
            catch (MySqlException e)
            {
                throw new ApplicationException("An error occurred when trying to close the connection. Error code: " + e.Number + " Message: " + e.Message);
            }
        }

        //For inserting a photo or video into the media table (the main table). Will not insert duplicates.
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

        //Create a new album and add it to the table of album names and IDs. ID is auto incrementing.
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

        //This has 3 different use cases: give an album a cover if it doesn't have a cover,
        //update an existing cover, or remove an album cover (supply 'null' as path).
        //Albums don't necessarily need to have an album cover.
        public void UpdateAlbumCover(string albumName, string path)
        {
            var ID = GetAlbumID(albumName); //Find ID of the album

            if (OpenConnection())
            {
                try
                {
                    MySqlCommand cmd = new("UPDATE albums SET album_cover=@path WHERE id=@ID", connection);
                    cmd.Parameters.AddWithValue("@path", path);
                    cmd.Parameters.AddWithValue("@ID", ID);
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

        //Given an album name, will find its ID in the albums table.
        //Returns 0 if not found or can't connect. IDs are greater than 0.
        public int GetAlbumID(string name)
        {
            int returnVal = 0;
            if (OpenConnection())
            {
                try
                {
                    //Find the album ID using the album name.
                    MySqlCommand selectCmd = new MySqlCommand("SELECT id FROM albums WHERE name=@name", connection);
                    selectCmd.Parameters.AddWithValue("@name", name);
                    selectCmd.ExecuteNonQuery();
                    MySqlDataReader reader = selectCmd.ExecuteReader();

                    if (reader.HasRows) //Check if there is actually a row to read. If reader.Read() is called and there isn't, a nasty exception is raised.
                    {
                        reader.Read(); //There should only be 1 line to read.
                        returnVal = reader.GetInt32(0); //First and only column.
                        reader.Close();
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

            return returnVal;
        }

        //Deletes items in the album from album_entries, then from the albums table.
        //THIS CANNOT BE UNDONE! This also does not delete the path from the media table, so you can safely delete an album without losing the actual photos.
        public void DeleteAlbum(string name)
        {
            if (OpenConnection())
            {
                try
                {
                    //Find the ID of the album to delete, then use that to delete every item in that album from album_entries.
                    MySqlCommand selectCmd = new MySqlCommand("SELECT id FROM albums WHERE name=@name", connection);
                    selectCmd.Parameters.AddWithValue("@name", name);
                    selectCmd.ExecuteNonQuery();
                    MySqlDataReader reader = selectCmd.ExecuteReader();

                    if (reader.HasRows) //Check if there is actually a row to read. If reader.Read() is called and there isn't, a nasty exception is raised.
                    {
                        reader.Read(); //There should only be 1 line to read.
                        int delID = reader.GetInt32(0); //First and only column
                        reader.Close();

                        //Remove all corresponding items from album_entries table.
                        MySqlCommand delCmd = new MySqlCommand("DELETE FROM album_entries WHERE album_id=@id", connection);
                        delCmd.Parameters.AddWithValue("@id", delID);
                        delCmd.ExecuteNonQuery();

                        //Finally, remove from albums table.
                        delCmd.CommandText = "DELETE FROM albums WHERE name=@name";
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

        //Add an item to media (main table) and an album.
        public void MediaAndAlbumInsert(string path, int albumID, DateTime dateTaken)
        {
            InsertMedia(path, dateTaken);
            AddToAlbum(path, albumID);
        }

        //Moves an item from media and album_entries (if applicable) into the 2 trash albums.
        public void DeleteItem(string path)
        {
            if (OpenConnection())
            {
                try
                {
                    //Copy item from media to trash
                    MySqlCommand mediaCopyCmd = new("INSERT INTO media_trash SELECT * FROM media WHERE path=@path", connection);
                    mediaCopyCmd.Parameters.AddWithValue("@path", path);
                    mediaCopyCmd.ExecuteNonQuery();

                    //Remove from media
                    MySqlCommand mediaDelCmd = new("DELETE FROM media WHERE path=@path", connection);
                    mediaDelCmd.Parameters.AddWithValue("@path", path);
                    mediaDelCmd.ExecuteNonQuery();

                    //Copy item(s) from album_entries to trash
                    MySqlCommand entriesCopyCmd = new("INSERT INTO album_entries_trash SELECT * FROM album_entries WHERE path=@path", connection);
                    entriesCopyCmd.Parameters.AddWithValue("@path", path);
                    entriesCopyCmd.ExecuteNonQuery();

                    //Remove from album_entries
                    MySqlCommand entriesDelCmd = new("DELETE FROM album_entries WHERE path=@path", connection);
                    entriesDelCmd.Parameters.AddWithValue("@path", path);
                    entriesDelCmd.ExecuteNonQuery();
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

        //Undoes a call to DeleteItem(). Will restore albums it was in, as well as re-adding it to the media table.
        public void RestoreItem(string path)
        {
            if (OpenConnection())
            {
                try
                {
                    //Copy item from media to trash
                    MySqlCommand mediaCopyCmd = new("INSERT INTO media SELECT * FROM media_trash WHERE path=@path", connection);
                    mediaCopyCmd.Parameters.AddWithValue("@path", path);
                    mediaCopyCmd.ExecuteNonQuery();

                    //Remove from media
                    MySqlCommand mediaDelCmd = new("DELETE FROM media_trash WHERE path=@path", connection);
                    mediaDelCmd.Parameters.AddWithValue("@path", path);
                    mediaDelCmd.ExecuteNonQuery();

                    //Copy item(s) from album_entries to trash
                    MySqlCommand entriesCopyCmd = new("INSERT INTO album_entries SELECT * FROM album_entries_trash WHERE path=@path", connection);
                    entriesCopyCmd.Parameters.AddWithValue("@path", path);
                    entriesCopyCmd.ExecuteNonQuery();

                    //Remove from album_entries
                    MySqlCommand entriesDelCmd = new("DELETE FROM album_entries_trash WHERE path=@path", connection);
                    entriesDelCmd.Parameters.AddWithValue("@path", path);
                    entriesDelCmd.ExecuteNonQuery();
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

        //Returns every path in an album
        public List<Media> SelectAlbum(string name)
        {
            List<Media> media = new(); //Stores every row retrieved; returned later.
            var ID = GetAlbumID(name); //Find the album to work with.

            if (OpenConnection())
            {
                try
                {
                    MySqlCommand cmd = new("SELECT a.path, m.date_taken, a.date_added_to_album FROM media AS m INNER JOIN album_entries AS a ON m.path=a.path WHERE album_id=@ID", connection);
                    cmd.Parameters.AddWithValue("@ID", ID);
                    cmd.ExecuteNonQuery();
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        //Add new row
                        media.Add(new(reader.GetString(0), reader.GetDateTime(1), reader.GetDateTime(2)));
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

            return media;
        }

        //Updates oldPath's DateTaken in the media and album_entries tables with newPath.
        public void UpdateDateTaken(string oldPath, string newPath)
        {
            if (OpenConnection())
            {
                try
                {
                    MySqlCommand updateMediaCmd = new("UPDATE media SET path=@newPath WHERE path=@oldPath", connection);
                    updateMediaCmd.Parameters.AddWithValue("@newPath", newPath);
                    updateMediaCmd.Parameters.AddWithValue("@oldPath", oldPath);
                    updateMediaCmd.ExecuteNonQuery();

                    MySqlCommand updateAlbumEntriesCmd = new("UPDATE album_entries SET path=@newPath WHERE path=@oldPath", connection);
                    updateAlbumEntriesCmd.Parameters.AddWithValue("@newPath", newPath);
                    updateAlbumEntriesCmd.Parameters.AddWithValue("@oldPath", oldPath);
                    updateAlbumEntriesCmd.ExecuteNonQuery();
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
    }
}