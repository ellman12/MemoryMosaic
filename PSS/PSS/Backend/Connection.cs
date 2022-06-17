using System.Collections.Generic;
using System.Data;

namespace PSS.Backend
{
    ///<summary>
    ///Backend database stuff.
    ///</summary>
    public static class Connection
    {
        public static readonly NpgsqlConnection connection = new("Host=localhost; Port=5432; User Id=postgres; Password=Ph0t0s_Server; Database=PSS");

        //AM = AlbumsMain
        public enum AMSortMode
        {
            Title,
            TitleReversed,
            LastModified,
            LastModifiedReversed
        }
        
        //AV = AlbumView
        public enum AVSortMode
        {
            OldestDateTaken,
            NewestDateTaken,
            OldestAdded,
            NewestAdded
        }

        public enum TrashSortMode
        {
            DateDeleted, //Default
            DateTaken,
            DateDeletedReversed,
            DateTakenReversed
        }

        //Represents a record from the albums table.
        public record Album
        {
            public readonly int id;
            public readonly string name;
            public readonly string albumCover;
            public readonly DateTime dateUpdated;

            public Album(int id, string name, string albumCover)
            {
                this.id = id;
                this.name = name;
                this.albumCover = albumCover;
            }
            
            public Album(int id, string name, string albumCover, DateTime dateUpdated)
            {
                this.id = id;
                this.name = name;
                this.albumCover = albumCover;
                this.dateUpdated = dateUpdated;
            }
        }

        //Represents a row in the media table.
        public record MediaRow
        {
            public readonly string path;
            public readonly DateTime dateTaken;
            public readonly DateTime dateAdded;
            public readonly bool starred;
            public readonly Guid uuid;
            public readonly string thumbnail;

            public MediaRow(string p, DateTime dt, DateTime da, bool starred, Guid uuid)
            {
                path = p;
                dateTaken = dt;
                dateAdded = da;
                this.starred = starred;
                this.uuid = uuid;
            }
            
            public MediaRow(string p, DateTime dt, DateTime da, Guid uuid, string thumbnail)
            {
                path = p;
                dateTaken = dt;
                dateAdded = da;
                this.uuid = uuid;
                this.thumbnail = thumbnail;
            }

            public MediaRow(string p, DateTime dt, DateTime da, bool starred, Guid uuid, string thumbnail)
            {
                path = p;
                dateTaken = dt;
                dateAdded = da;
                this.starred = starred;
                this.uuid = uuid;
                this.thumbnail = thumbnail;
            }
        }

        ///Represents an item that is being uploaded.
        public class UploadFile
        {
            ///The absolute path to the file to upload.
            public string fullPath;
            
            ///null for images, otherwise a base64 string for video files.
            public string thumbnail;
            
            ///If this item is already in pss_library.
            public bool alreadyInLib;
            
            ///The date and time this image or video was captured. null if this item doesnt have any.
            public DateTime? dateTaken;
            
            ///Where the date taken data came from (Filename, Metadata, or None).
            public D.DateTakenSrc dateTakenSrc;
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

        ///<summary>For inserting a photo or video into the media table (the main table). Will not insert duplicates.</summary>
        ///<param name="path">The short path that will be stored in media. Convention is to use '/' as the separator.</param>
        ///<param name="dateTaken">When this item was taken.</param>
        ///<param name="thumbnail">ONLY FOR VIDEOS. A base64 string for the video thumbnail. Use null for pictures.</param>
        ///<param name="starred">Is this item starred or not?</param>
        ///<param name="separate">Is this item separate from main library (i.e., is it in a folder)?</param>
        ///<returns>Int saying how many rows were affected.</returns>
        public static int InsertMedia(string path, DateTime? dateTaken, string thumbnail, bool starred = false, bool separate = false)
        {
            int rowsAffected = 0;
            try
            {
                Open();
                NpgsqlCommand cmd = new("", connection);
                cmd.Parameters.AddWithValue("@path", path);
                cmd.Parameters.AddWithValue("@starred", starred);
                cmd.Parameters.AddWithValue("@separate", separate);
                
                if (thumbnail == null)
                {
                    if (dateTaken == null)
                    {
                        cmd.CommandText = "INSERT INTO media (path, date_added, starred, separate, uuid) VALUES (@path, now(), @starred, @separate, uuid_generate_v1())";
                    }
                    else
                    {
                        cmd.CommandText = "INSERT INTO media (path, date_taken, date_added, starred, separate, uuid) VALUES (@path, @dateTaken, now(), @starred, @separate, uuid_generate_v1())";
                    }
                }
                else
                {
                    if (dateTaken == null)
                    {
                        cmd.CommandText = "INSERT INTO media (path, date_added, starred, separate, uuid, thumbnail) VALUES (@path, now(), @starred, @separate, uuid_generate_v1(), @thumbnail)";
                    }
                    else
                    {
                        cmd.CommandText = "INSERT INTO media (path, date_taken, date_added, starred, separate, uuid, thumbnail) VALUES (@path, @dateTaken, now(), @starred, @separate, uuid_generate_v1(), @thumbnail)";
                    }
                }
                
                rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine("An unknown error occurred. Error code: " + e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }

            return rowsAffected;
        }

        //Create a new album and add it to the table of album names and IDs. ID is auto incrementing.
        public static void CreateAlbum(string name, bool folder = false)
        {
            try
            {
                Open();
                NpgsqlCommand cmd = new("INSERT INTO albums (name, last_updated, folder) VALUES (@name, now(), @folder)", connection);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@folder", folder);
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

        public static void RenameAlbum(string newName, int id)
        {
            try
            {
                Open();
                NpgsqlCommand cmd = new("UPDATE albums SET name=@newName WHERE id=@id", connection);
                cmd.Parameters.AddWithValue("@newName", newName);
                cmd.Parameters.AddWithValue("@id", id);
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
        //update an existing cover, or remove an album cover (supply literal null as path).
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

        public static string GetAlbumName(int id)
        {
            string returnVal = "";
            try
            {
                Open();

                //Find the album ID using the album name.
                NpgsqlCommand selectCmd = new("SELECT name FROM albums WHERE id=@id", connection);
                selectCmd.Parameters.AddWithValue("@id", id);
                selectCmd.ExecuteNonQuery();
                NpgsqlDataReader reader = selectCmd.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read(); //There should only be 1 line to read.
                    returnVal = reader.GetString(0); //First and only column.
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
                
                //Set all items to no longer being separate (only matters if this was a folder).
                NpgsqlCommand cmd = new("UPDATE media SET separate=false FROM album_entries WHERE album_id=@albumID AND album_entries.path=media.path", connection);
                cmd.Parameters.AddWithValue("@albumID", albumID);
                cmd.ExecuteNonQuery();
                
                //Remove all corresponding items from album_entries table.
                NpgsqlCommand delCmd = new("DELETE FROM album_entries WHERE album_id=@id", connection);
                delCmd.Parameters.AddWithValue("@id", albumID);
                delCmd.ExecuteNonQuery();
                
                //Remove all items from the trash table too.
                delCmd.CommandText = "DELETE FROM album_entries_trash WHERE album_id=@id";
                delCmd.ExecuteNonQuery();

                //Finally, remove from albums table.
                delCmd.CommandText = "DELETE FROM albums WHERE id=@id";
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

        ///<summary>
        ///Add a single path to an album in album_entries. If it's a folder it handles all that automatically.
        ///</summary>
        public static void AddToAlbum(string path, int albumID)
        {
            bool isFolder = IsFolder(albumID);
            
            try
            {
                Open();

                if (isFolder)
                {
                    //If an item is being added to a folder it can only be in 1 folder and 0 albums so remove from everywhere else first.
                    NpgsqlCommand delCmd = new("DELETE FROM album_entries WHERE path=@path", connection);
                    delCmd.Parameters.AddWithValue("@path", path);
                    delCmd.ExecuteNonQuery();

                    //Mark this item as in a folder (separate).
                    NpgsqlCommand separateCmd = new("UPDATE media SET separate=true WHERE path=@path", connection);
                    separateCmd.Parameters.AddWithValue("@path", path);
                    separateCmd.ExecuteNonQuery();
                }

                NpgsqlCommand cmd = new("INSERT INTO album_entries VALUES (@path, @albumID, @date_added_to_album) ON CONFLICT (path, album_id) DO NOTHING", connection);
                cmd.Parameters.AddWithValue("@path", path);
                cmd.Parameters.AddWithValue("@albumID", albumID);
                cmd.Parameters.AddWithValue("@date_added_to_album", DateTime.Now);
                cmd.ExecuteNonQuery();

                cmd.CommandText = "UPDATE albums SET last_updated = now() WHERE id=@albumID";
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

                cmd.CommandText = "UPDATE albums SET last_updated = now() WHERE id=@albumID";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "UPDATE media SET separate = false WHERE path=@path AND separate = true";
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

        public static List<Album> GetAlbumsTable(bool showAlbums, bool showFolders, AMSortMode mode = AMSortMode.Title)
        {
            List<Album> albums = new();

            string orderBy = mode switch
            {
                AMSortMode.Title => "name ASC",
                AMSortMode.TitleReversed => "name DESC",
                AMSortMode.LastModified => "last_updated ASC",
                AMSortMode.LastModifiedReversed => "last_updated DESC",
                _ => "name ASC"
            };

            //If both true, show albums and folders and thus no need for a where clause.
            string where;
            if (showAlbums && showFolders)
                where = "";
            else if (showAlbums)
                where = "WHERE folder = false";
            else if (showFolders)
                where = "WHERE folder = true";
            else
                where = "WHERE folder = true and folder = false";

            try
            {
                Open();
                NpgsqlCommand cmd = new("SELECT id, name, album_cover, last_updated FROM albums " + where + " ORDER BY " + orderBy, connection);
                //cmd.Parameters.AddWithValue("@orderBy", orderBy); //NOTE: I'd love to use this line that's commented out instead of a '+', but for some reason, it doesn't work and the '+' does. No idea why.
                cmd.ExecuteNonQuery();
                NpgsqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read()) albums.Add(new Album(reader.GetInt32(0), reader.GetString(1), reader.IsDBNull(2) ? String.Empty : reader.GetString(2), reader.GetDateTime(3))); //https://stackoverflow.com/a/38930847
                reader.Close();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine("An unknown error occurred in GetAlbumsTable. Error code: " + e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }

            return albums;
        }

        ///<summary>
        ///Returns a List of all the items an album is in.
        ///</summary>
        ///<param name="path">path to item</param>
        ///<returns>List of the albums the item is in, if any</returns>
        public static List<Album> GetAlbumsItemIn(string path)
        {
            List<Album> albums = new();

            try
            {
                Open();
                NpgsqlCommand cmd = new("SELECT album_id, name, album_cover FROM album_entries AS e INNER JOIN albums AS a ON e.album_id=a.id WHERE path=@path ORDER BY name ASC", connection);
                cmd.Parameters.AddWithValue("@path", path);
                cmd.ExecuteNonQuery();
                NpgsqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read()) albums.Add(new Album(reader.GetInt32(0), reader.GetString(1), reader.IsDBNull(2) ? String.Empty : reader.GetString(2)));
                reader.Close();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine("An unknown error occurred in GetAlbumsTable. Error code: " + e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }
            
            return albums;
        }

        //https://www.postgresqltutorial.com/postgresql-update-join/
        ///<summary>
        ///Used for changing an album to a folder or vice versa.
        ///</summary>
        ///<param name="albumID">ID of album or folder to change modes</param>
        ///<param name="folder">Specify true if want to change album -> folder. False for folder -> album</param>
        public static void ChangeAlbumType(int albumID, bool folder)
        {
            try
            {
                Open();
                NpgsqlCommand cmd = new("UPDATE media SET separate=@folder FROM album_entries WHERE album_id=@albumID AND album_entries.path=media.path", connection);
                cmd.Parameters.AddWithValue("@folder", folder);
                cmd.Parameters.AddWithValue("@albumID", albumID);
                cmd.ExecuteNonQuery();

                cmd.CommandText = "UPDATE albums SET folder=@folder WHERE id=@albumID";
                cmd.ExecuteNonQuery();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine("An unknown error occurred in GetAlbumsTable. Error code: " + e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }
        }

        //Moves an item from media and album_entries (if applicable) into the 2 trash albums.
        public static void MoveToTrash(string path)
        {
            try
            {
                Open();

                NpgsqlCommand cmd = new("INSERT INTO media_trash SELECT * FROM media WHERE path=@path", connection);
                cmd.Parameters.AddWithValue("@path", path);
                cmd.ExecuteNonQuery();

                cmd.CommandText = "DELETE FROM media WHERE path=@path";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "INSERT INTO album_entries_trash SELECT * FROM album_entries WHERE path=@path";
                cmd.ExecuteNonQuery();

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

        ///<summary>
        ///PERMANENTLY remove an item from database and server.
        ///</summary>
        public static void PermDeleteItem(string path)
        {
            File.Delete(Path.Join(S.libFolderPath, path));

            try
            {
                Open();

                //Copy item from media to trash
                NpgsqlCommand cmd = new("DELETE FROM media_trash WHERE path=@path", connection);
                cmd.Parameters.AddWithValue("@path", path);
                cmd.ExecuteNonQuery();

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

        //Undoes a call to MoveToTrash(). Will restore albums it was in, as well as re-adding it to the media table.
        public static void RestoreItem(string path)
        {
            try
            {
                Open();

                //Copy item from media to trash
                NpgsqlCommand cmd = new("INSERT INTO media SELECT path, date_taken, date_added, starred, separate, uuid, thumbnail FROM media_trash WHERE path=@path", connection);
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

        ///<summary>
        ///Loads everything in the media table into a List of the rows. Does not store separate column. Also only selects ones where separate==false
        ///</summary>
        ///<returns>List of MediaRow records</returns>
        public static List<MediaRow> LoadMediaTable()
        {
            List<MediaRow> media = new(); //Stores every row retrieved; returned later.
            try
            {
                Open();
                NpgsqlCommand cmd = new("SELECT path, date_taken, date_added, starred, uuid, thumbnail FROM media WHERE separate=false ORDER BY date_taken DESC", connection);
                cmd.ExecuteNonQuery();
                NpgsqlDataReader r = cmd.ExecuteReader();

                while (r.Read()) media.Add(new MediaRow(r.GetString(0), r.GetDateTime(1), r.GetDateTime(2), r.GetBoolean(3), r.GetGuid(4), r.IsDBNull(5) ? null : r.GetString(5)));
                r.Close();
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
        
        ///<summary>
        ///Load only starred items from the media table. Doesn't bother selecting the starred column because it does SELECT WHERE starred == true.
        ///</summary>
        ///<returns>Row(s) retrieved in a List&lt;MediaRow&gt;</returns>
        public static List<MediaRow> LoadStarred()
        {
            List<MediaRow> media = new();
            try
            {
                Open();
                NpgsqlCommand cmd = new("SELECT path, date_taken, date_added, uuid, thumbnail FROM media WHERE starred=true ORDER BY date_taken DESC", connection);
                cmd.ExecuteNonQuery();
                NpgsqlDataReader r = cmd.ExecuteReader();

                while (r.Read())
                    media.Add(new MediaRow(r.GetString(0), r.GetDateTime(1), r.GetDateTime(2), r.GetGuid(3), r.IsDBNull(4) ? null : r.GetString(4)));

                r.Close();
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

        ///<summary>
        ///Get if a path is starred or not.
        ///</summary>
        public static bool GetStarred(string path)
        {
            bool starred = false;
            
            try
            {
                Open();
                NpgsqlCommand cmd = new("SELECT starred FROM media WHERE path=@path", connection);
                cmd.Parameters.AddWithValue("@path", path);
                cmd.ExecuteNonQuery();
                NpgsqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();
                    starred = reader.GetBoolean(0);
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

            return starred;
        }

        ///<summary>Change a single item from either starred (true) or not starred.</summary>
        public static void UpdateStarred(string path, bool starred)
        {
            try
            {
                Open();
                NpgsqlCommand cmd = new("UPDATE media SET starred=@starred WHERE path=@path;", connection);
                cmd.Parameters.AddWithValue("@starred", starred);
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
        
        ///<summary>Change a List of paths (strings) from either starred (true) or not starred.</summary>
        public static void UpdateStarred(List<string> paths, bool starred)
        {
            try
            {
                Open();
                foreach(string path in paths)
                {
                    NpgsqlCommand cmd = new("UPDATE media SET starred=@starred WHERE path=@path", connection);
                    cmd.Parameters.AddWithValue("@starred", starred);
                    cmd.Parameters.AddWithValue("@path", path);
                    cmd.ExecuteNonQuery();
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
        }
        
        ///<summary>
        ///Loads the contents of an album (or folder) into a List of MediaRows
        ///</summary>
        public static List<MediaRow> LoadAlbum(int albumID, AVSortMode mode = AVSortMode.NewestDateTaken)
        {
            bool isFolder = IsFolder(albumID);
            List<MediaRow> media = new(); //Stores every row retrieved; returned later.
            
            string orderBy = mode switch
            {
                AVSortMode.OldestDateTaken => "date_taken ASC",
                AVSortMode.NewestDateTaken => "date_taken DESC",
                AVSortMode.OldestAdded => "date_added_to_album ASC",
                AVSortMode.NewestAdded => "date_added_to_album DESC",
                _ => "date_taken DESC"
            };

            try
            {
                Open();
                NpgsqlCommand cmd = new("SELECT a.path, m.date_taken, a.date_added_to_album, m.starred, m.uuid, m.thumbnail FROM media AS m INNER JOIN album_entries AS a ON m.path=a.path WHERE album_id=@albumID AND separate=" + isFolder + " ORDER BY " + orderBy, connection);
                cmd.Parameters.AddWithValue("@albumID", albumID);
                cmd.ExecuteNonQuery();
                NpgsqlDataReader r = cmd.ExecuteReader();

                while (r.Read())
                    media.Add(new MediaRow(r.GetString(0), r.GetDateTime(1), r.GetDateTime(2), r.GetBoolean(3), r.GetGuid(4), r.IsDBNull(5) ? null : r.GetString(5)));
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

        ///<summary>
        ///Load everything in media_trash into a List of MediaRows.
        ///</summary>
        public static List<MediaRow> LoadMediaTrashTable(TrashSortMode mode = TrashSortMode.DateDeleted)
        {
            List<MediaRow> media = new(); //Stores every row retrieved; returned later.

            string orderBy = mode switch
            {
                TrashSortMode.DateDeleted => "date_deleted ASC",
                TrashSortMode.DateTaken => "date_taken ASC",
                TrashSortMode.DateDeletedReversed => "date_deleted DESC",
                TrashSortMode.DateTakenReversed => "date_taken DESC",
                _ => "date_deleted ASC"
            };
            
            try
            {
                Open();
                NpgsqlCommand cmd = new("SELECT path, date_taken, date_added, starred, uuid, thumbnail FROM media_trash ORDER BY " + orderBy, connection);
                cmd.ExecuteNonQuery();
                NpgsqlDataReader r = cmd.ExecuteReader();

                while (r.Read())
                    media.Add(new MediaRow(r.GetString(0), r.GetDateTime(1), r.GetDateTime(2), r.GetBoolean(3), r.GetGuid(4), r.IsDBNull(5) ? null : r.GetString(5)));

                r.Close();
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

        ///<summary>
        ///Update when an item was taken and also update its path and move it to the new path.
        ///</summary>
        ///<param name="shortPath">The path to the item that is stored in the database</param>
        ///<param name="newDateTaken">The new date taken for this item</param>
        public static void UpdateDateTaken(string shortPath, DateTime newDateTaken)
        {
            try
            {
                Open();

                //1. Update date taken in media.
                NpgsqlCommand cmd = new("UPDATE media SET date_taken=@newDateTaken WHERE path=@shortPath", connection);
                cmd.Parameters.AddWithValue("@shortPath", shortPath);
                cmd.Parameters.AddWithValue("@newDateTaken", newDateTaken);
                cmd.ExecuteNonQuery();

                //2. Update shortPath in media.
                string filename = Path.GetFileName(shortPath);
                string newPath = Path.Combine(Pages.UploadApply.GenerateDatePath(newDateTaken), filename); //Don't need full path, just the short path (/2021/10 October/...).
                cmd.CommandText = "UPDATE media SET path=@newPath WHERE path=@shortPath";
                cmd.Parameters.AddWithValue("@newPath", newPath);
                cmd.Parameters.AddWithValue("@shortPath", shortPath);
                cmd.ExecuteNonQuery();
                
                //3. Update path(s) in Album_Entries table.
                cmd.CommandText = "UPDATE album_entries SET path=@newPath WHERE path=@shortPath";
                cmd.ExecuteNonQuery();
                
                //4. Update album cover(s).
                cmd.CommandText = "UPDATE albums SET album_cover=@newPath WHERE album_cover=@shortPath";
                cmd.ExecuteNonQuery();
                
                //5. Move item to new path on server.
                string originalFullPath = Path.Combine(S.libFolderPath, shortPath);
                string newFullDir = Pages.UploadApply.GenerateSortedDir(newDateTaken);
                string newFullPath = Path.Combine(newFullDir, filename);
                Directory.CreateDirectory(newFullDir); //Create in case it doesn't exist.
                File.Move(originalFullPath, newFullPath);
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

        ///<summary>
        ///Gets an item's path from its (string) uuid.
        ///</summary>
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

        ///<summary>
        ///Gets an item's path from its Guid uuid.
        ///</summary>
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

        public static DateTime GetDateAdded(string path)
        {
            DateTime dateTaken = new();

            try
            {
                Open();
                NpgsqlCommand cmd = new("SELECT date_added FROM media WHERE path=@path", connection);
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

        public static DateTime GetDateAdded(Guid uuid)
        {
            DateTime dateTaken = new();

            try
            {
                Open();
                NpgsqlCommand cmd = new("SELECT date_added FROM media WHERE uuid=@uuid", connection);
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
        
        public static DateTime GetDateAddedToAlbum(string path, int albumID)
        {
            DateTime dateTaken = new();

            try
            {
                Open();
                NpgsqlCommand cmd = new("SELECT date_added_to_album FROM album_entries WHERE path=@path AND album_id=@albumID", connection);
                cmd.Parameters.AddWithValue("@path", path);
                cmd.Parameters.AddWithValue("@albumID", albumID);
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

        ///<summary>
        ///Return if an album is a folder.
        ///</summary>
        public static bool IsFolder(int albumID)
        {
            bool isFolder = false;

            try
            {
                Open();
                NpgsqlCommand cmd = new("SELECT folder FROM albums WHERE id=@albumID", connection);
                cmd.Parameters.AddWithValue("@albumID", albumID);
                cmd.ExecuteNonQuery();
                NpgsqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();
                    isFolder = reader.GetBoolean(0);
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

            return isFolder;
        }

        ///Generate a short path (DB path) given a DateTaken and filename. A DB path looks like this: 2022/5 May/yes.png
        public static string CreateShortPath(DateTime dateTaken, string filename) => Path.Combine(dateTaken.Year.ToString(), $"{dateTaken.Month} {System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(dateTaken.Month)}", filename).Replace('\\', '/');

        ///<summary>Used in ViewItem for renaming the current item's file.</summary>
        ///<returns>The new short path (DB path) of this item. Blank string if DB error occurred.</returns>
        public static string RenameFile(string oldShortPath, string newFilename, string ext, DateTime dateTaken)
        {
            string newShortPath = CreateShortPath(dateTaken, newFilename + ext);
            
            //Rename the file in the DB path.
            try
            {
                Open();
                NpgsqlCommand cmd = new("UPDATE media SET path=@newShortPath WHERE path=@oldShortPath", connection);
                cmd.Parameters.AddWithValue("@newShortPath", newShortPath);
                cmd.Parameters.AddWithValue("@oldShortPath", oldShortPath);
                cmd.ExecuteNonQuery();
            }
            catch (NpgsqlException e)
            {
                if (e.ErrorCode != -2147467259) //Duplicate key value error. No need to print this out since the error is caught.
                    Console.WriteLine("An unknown error occurred. Error code: " + e.ErrorCode + " Message: " + e.Message);
                return "";
            }
            finally
            {
                Close();
            }

            //Rename the actual file
            string fullOldPath = Path.Combine(S.libFolderPath, oldShortPath);
            string fullNewPath = Path.Combine(S.libFolderPath, newShortPath);
            File.Move(fullOldPath, fullNewPath);
            return newShortPath;
        }
    }
}