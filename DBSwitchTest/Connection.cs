using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Npgsql;

namespace DBSwitchTest
{
    class Connection
    {
        public static readonly NpgsqlConnection connection = new("Host=localhost; Port=5432; User Id=postgres; Password=Ph0t0s_Server; Database=PSS");

        //Represents a row in the media table.
        public readonly struct MediaRow
        {
            public readonly string path;
            public readonly DateTime dateTaken;
            public readonly DateTime dateAdded;
            public readonly Guid uuid;

            public MediaRow(string p, DateTime dt, DateTime da, Guid uuid)
            {
                path = p;
                dateTaken = dt;
                dateAdded = da;
                this.uuid = uuid;
            }
        }

        public static void Open()
        {
            if (connection.State == ConnectionState.Closed)
                connection.Open();
        }

        public static void Close()
        {
            if (connection.State == ConnectionState.Open)
                connection.Close();
        }

        //For inserting a photo or video into the media table (the main table). Will not insert duplicates.
        public static void InsertMedia(string path, DateTime dateTaken)
        {
            try
            {
                Open();
                NpgsqlCommand cmd = new("INSERT INTO media VALUES (@path, @dateAdded, @dateTaken) ON CONFLICT (path) DO NOTHING", connection);
                cmd.Parameters.AddWithValue("@path", path);
                cmd.Parameters.AddWithValue("@dateAdded", DateTime.Now);
                cmd.Parameters.AddWithValue("@dateTaken", dateTaken);
                cmd.ExecuteNonQuery();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine("An unknown error occurred. Error code: " + e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }
        }

        //Add an item to media (main table) and an album.
        public static void MediaAndAlbumInsert(string path, int albumID, DateTime dateTaken)
        {
            InsertMedia(path, dateTaken);
            AddToAlbum(path, albumID);
        }

        //Create a new album and add it to the table of album names and IDs. ID is auto incrementing.
        public static void CreateAlbum(string name)
        {
            try
            {
                Open();
                NpgsqlCommand cmd = new("INSERT INTO albums (name) VALUES (@name)", connection);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.ExecuteNonQuery();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine("An unknown error occurred. Error code: " + e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }
        }

        //This has 3 different use cases: give an album a cover if it doesn't have a cover,
        //update an existing cover, or remove an album cover (supply literal 'null' as path).
        //Albums aren't required to have an album cover.
        public static void UpdateAlbumCover(string albumName, string path)
        {
            try
            {
                Open();
                NpgsqlCommand cmd = new("UPDATE albums SET album_cover=@path WHERE name=@albumName", connection);
                cmd.Parameters.AddWithValue("@path", path);
                cmd.Parameters.AddWithValue("@albumName", albumName);
                cmd.ExecuteNonQuery();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine("An unknown error occurred. Error code: " + e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }
        }

        public static void UpdateAlbumCover(int albumID, string path)
        {
            try
            {
                Open();
                NpgsqlCommand cmd = new("UPDATE albums SET album_cover=@path WHERE id=@albumID", connection);
                cmd.Parameters.AddWithValue("@path", path);
                cmd.Parameters.AddWithValue("@albumID", albumID);
                cmd.ExecuteNonQuery();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine("An unknown error occurred. Error code: " + e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }
        }

        //Given an album name, will find its ID in the albums table.
        //Returns 0 if not found or can't connect. IDs are greater than 0.
        public static int GetAlbumID(string name)
        {
            int returnVal = 0;
            try
            {
                Open();
                //Find the album ID using the album name.
                NpgsqlCommand selectCmd = new("SELECT id FROM albums WHERE name=@name", connection);
                selectCmd.Parameters.AddWithValue("@name", name);
                selectCmd.ExecuteNonQuery();
                NpgsqlDataReader reader = selectCmd.ExecuteReader();

                if (reader.HasRows) //Check if there is actually a row to read. If reader.Read() is called and there isn't, a nasty exception is raised.
                {
                    reader.Read(); //There should only be 1 line to read.
                    returnVal = reader.GetInt32(0); //First and only column.
                    reader.Close();
                }
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine("An unknown error occurred. Error code: " + e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }

            return returnVal;
        }

        //Deletes items in the album from album_entries, then remove the album from the albums table.
        //THIS CANNOT BE UNDONE! This also does not delete the path from the media table, so you can safely delete an album without losing the actual photos.
        public static void DeleteAlbum(string name)
        {
            try
            {
                int delID = GetAlbumID(name);
                Open();

                //Remove all corresponding items from album_entries table.
                NpgsqlCommand delCmd = new("DELETE FROM album_entries WHERE album_id=@id", connection);
                delCmd.Parameters.AddWithValue("@id", delID);
                delCmd.ExecuteNonQuery();

                //Finally, remove from albums table.
                delCmd.CommandText = "DELETE FROM albums WHERE name=@name";
                delCmd.Parameters.AddWithValue("@name", name);
                delCmd.ExecuteNonQuery();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine("An unknown error occurred. Error code: " + e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }
        }

        public static void DeleteAlbum(int albumID)
        {
            try
            {
                Open();

                //Remove all corresponding items from album_entries table.
                NpgsqlCommand delCmd = new("DELETE FROM album_entries WHERE album_id=@id", connection);
                delCmd.Parameters.AddWithValue("@id", albumID);
                delCmd.ExecuteNonQuery();

                //Finally, remove from albums table.
                delCmd.CommandText = "DELETE FROM albums WHERE id=@id";
                delCmd.Parameters.AddWithValue("@ID", albumID);
                delCmd.ExecuteNonQuery();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine("An unknown error occurred. Error code: " + e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }
        }

        //Add a single path to an album in album_entries.
        public static void AddToAlbum(string path, int albumID)
        {
            try
            {
                Open();

                NpgsqlCommand cmd = new("INSERT INTO album_entries VALUES (@path, @albumID, @date_added_to_album) ON CONFLICT (path, album_id) DO NOTHING", connection);
                cmd.Parameters.AddWithValue("@path", path);
                cmd.Parameters.AddWithValue("@albumID", albumID);
                cmd.Parameters.AddWithValue("@date_added_to_album", DateTime.Now);
                cmd.ExecuteNonQuery();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine("An unknown error occurred. Error code: " + e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }
        }

        //Remove a single path from an album.
        public static void RemoveFromAlbum(string path, int albumID)
        {
            try
            {
                Open();
                NpgsqlCommand cmd = new("DELETE FROM album_entries WHERE album_id=@albumID AND path=@path", connection);
                cmd.Parameters.AddWithValue("@albumID", albumID);
                cmd.Parameters.AddWithValue("@path", path);
                cmd.ExecuteNonQuery();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine("An unknown error occurred. Error code: " + e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }
        }

        //Moves an item from media and album_entries (if applicable) into the 2 trash albums.
        public static void DeleteItem(string path)
        {
            try
            {
                Open();

                //Copy item from media to trash
                NpgsqlCommand cmd = new("INSERT INTO media_trash SELECT * FROM media WHERE path=@path", connection);
                cmd.Parameters.AddWithValue("@path", path);
                cmd.ExecuteNonQuery();

                //Remove from media
                cmd.CommandText = "DELETE FROM media WHERE path=@path";
                cmd.ExecuteNonQuery();

                //Copy item(s) from album_entries to trash
                cmd.CommandText = "INSERT INTO album_entries_trash SELECT * FROM album_entries WHERE path=@path";
                cmd.ExecuteNonQuery();

                //Remove from album_entries
                cmd.CommandText = "DELETE FROM album_entries WHERE path=@path";
                cmd.ExecuteNonQuery();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine("An unknown error occurred. Error code: " + e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }
        }

        //Undoes a call to DeleteItem(). Will restore albums it was in, as well as re-adding it to the media table.
        public static void RestoreItem(string path)
        {
            try
            {
                Open();

                //Copy item from media to trash
                NpgsqlCommand cmd = new("INSERT INTO media SELECT * FROM media_trash WHERE path=@path", connection);
                cmd.Parameters.AddWithValue("@path", path);
                cmd.ExecuteNonQuery();

                //Remove from media
                cmd.CommandText = "DELETE FROM media_trash WHERE path=@path";
                cmd.ExecuteNonQuery();

                //Copy item(s) from album_entries to trash
                cmd.CommandText = "INSERT INTO album_entries SELECT * FROM album_entries_trash WHERE path=@path";
                cmd.ExecuteNonQuery();

                //Remove from album_entries
                cmd.CommandText = "DELETE FROM album_entries_trash WHERE path=@path";
                cmd.ExecuteNonQuery();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine("An unknown error occurred. Error code: " + e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }
        }

        //Loads everything in the media table in descending order.
        public static List<MediaRow> LoadMediaTable()
        {
            List<MediaRow> media = new(); //Stores every row retrieved; returned later.
            try
            {
                Open();
                NpgsqlCommand cmd = new("SELECT * FROM media ORDER BY date_taken DESC", connection);
                cmd.ExecuteNonQuery();
                NpgsqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                    media.Add(new MediaRow(reader.GetString(0), reader.GetDateTime(1), reader.GetDateTime(2), reader.GetGuid(3)));
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine("An unknown error occurred. Error code: " + e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }
            return media;
        }

        public static List<MediaRow> LoadAlbum(string name)
        {
            List<MediaRow> media = new(); //Stores every row retrieved; returned later.
            int ID = GetAlbumID(name); //Find the album to work with.

            try
            {
                Open();
                NpgsqlCommand cmd = new("SELECT a.path, m.date_taken, a.date_added_to_album, m.uuid FROM media AS m INNER JOIN album_entries AS a ON m.path=a.path WHERE album_id=@ID", connection);
                cmd.Parameters.AddWithValue("@ID", ID);
                cmd.ExecuteNonQuery();
                NpgsqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                    media.Add(new MediaRow(reader.GetString(0), reader.GetDateTime(1), reader.GetDateTime(2), reader.GetGuid(3)));
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine("An unknown error occurred. Error code: " + e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }
            return media;
        }

        public static List<MediaRow> LoadAlbum(int albumID)
        {
            List<MediaRow> media = new(); //Stores every row retrieved; returned later.

            try
            {
                Open();
                NpgsqlCommand cmd = new("SELECT a.path, m.date_taken, a.date_added_to_album, m.uuid FROM media AS m INNER JOIN album_entries AS a ON m.path=a.path WHERE album_id=@albumID", connection);
                cmd.Parameters.AddWithValue("@albumID", albumID);
                cmd.ExecuteNonQuery();
                NpgsqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                    media.Add(new MediaRow(reader.GetString(0), reader.GetDateTime(1), reader.GetDateTime(2), reader.GetGuid(3)));
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine("An unknown error occurred. Error code: " + e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }
            return media;
        }

        //Update oldPath in the media and album_entries tables with newPath.
        public static void UpdatePath(string oldPath, string newPath)
        {
            try
            {
                Open();
                NpgsqlCommand updateMediaCmd = new("UPDATE media SET path=@newPath WHERE path=@oldPath", connection);
                updateMediaCmd.Parameters.AddWithValue("@newPath", newPath);
                updateMediaCmd.Parameters.AddWithValue("@oldPath", oldPath);
                updateMediaCmd.ExecuteNonQuery();

                NpgsqlCommand updateAlbumEntriesCmd = new("UPDATE album_entries SET path=@newPath WHERE path=@oldPath", connection);
                updateAlbumEntriesCmd.Parameters.AddWithValue("@newPath", newPath);
                updateAlbumEntriesCmd.Parameters.AddWithValue("@oldPath", oldPath);
                updateAlbumEntriesCmd.ExecuteNonQuery();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine("An unknown error occurred. Error code: " + e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }
        }

        //Update path's DateTaken in media and album_entries tables.
        public static void UpdateDateTaken(string path, DateTime newDateTaken)
        {
            try
            {
                Open();

                //Update media
                NpgsqlCommand cmd = new("UPDATE media SET date_taken=@newDateTaken WHERE path=@path", connection);
                cmd.Parameters.AddWithValue("@path", path);
                cmd.Parameters.AddWithValue("@newDateTaken", newDateTaken);
                cmd.ExecuteNonQuery();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine("An unknown error occurred. Error code: " + e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }
        }

        //Gets an item's path from its uuid.
        public static string GetPathFromUuid(string uuid)
        {
            string path = "";

            try
            {
                Open();
                NpgsqlCommand cmd = new("SELECT path FROM media WHERE uuid=@uuid", connection);
                cmd.Parameters.AddWithValue("@uuid", Guid.Parse(uuid));
                cmd.ExecuteNonQuery();
                NpgsqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read(); //There should only be 1 line to read.
                    path = reader.GetString(0); //First and only column.
                    reader.Close();
                }
                else
                    path = "null";
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine("An unknown error occurred. Error code: " + e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }

            return path;
        }

        public static string GetPathFromUuid(Guid uuid)
        {
            string path = "";

            try
            {
                Open();
                NpgsqlCommand cmd = new("SELECT path FROM media WHERE uuid=@uuid", connection);
                cmd.Parameters.AddWithValue("@uuid", uuid);
                cmd.ExecuteNonQuery();
                NpgsqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read(); //There should only be 1 line to read.
                    path = reader.GetString(0); //First and only column.
                    reader.Close();
                }
                else
                    path = "null";
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine("An unknown error occurred. Error code: " + e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }

            return path;
        }

        public static DateTime GetDateTaken(Guid uuid)
        {
            DateTime dateTaken = new();

            try
            {
                Open();
                NpgsqlCommand cmd = new("SELECT date_taken FROM media WHERE uuid=@uuid", connection);
                cmd.Parameters.AddWithValue("@uuid", uuid);
                cmd.ExecuteNonQuery();
                NpgsqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();
                    dateTaken = reader.GetDateTime(0);
                    reader.Close();
                }
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine("An unknown error occurred. Error code: " + e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }

            return dateTaken;
        }

        public static DateTime GetDateTaken(string path)
        {
            DateTime dateTaken = new();

            try
            {
                Open();
                NpgsqlCommand cmd = new("SELECT date_taken FROM media WHERE path=@path", connection);
                cmd.Parameters.AddWithValue("@path", path);
                cmd.ExecuteNonQuery();
                NpgsqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();
                    dateTaken = reader.GetDateTime(0);
                    reader.Close();
                }
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine("An unknown error occurred. Error code: " + e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }

            return dateTaken;
        }

        //Get whether an item was taken in the morning or afternoon: returns "AM" or "PM". //https://stackoverflow.com/a/7875351
        public static string GetPeriod(Guid uuid)
        {
            return GetDateTaken(uuid).ToString("tt", CultureInfo.InvariantCulture);
        }

        public static string GetPeriod(string path)
        {
            return GetDateTaken(path).ToString("tt", CultureInfo.InvariantCulture);
        }
    }
}