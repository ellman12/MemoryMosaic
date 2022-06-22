using System.Collections.Generic;
using System.Data;

namespace PSS.Backend
{
    ///Contains static methods for interacting with the PSS PostgreSQL database.
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
            NewestDateDeleted, //Default
            NewestDateTaken,
            OldestDateDeleted,
            OldestDateTaken
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
            public readonly DateTime? dateTaken;
            public readonly DateTime dateAdded;
            public readonly bool starred;
            public readonly Guid uuid;
            public readonly string thumbnail;

            public MediaRow(string p, DateTime? dt, bool starred, Guid uuid, string thumbnail)
            {
                path = p;
                dateTaken = dt;
                this.starred = starred;
                this.uuid = uuid;
            }

            public MediaRow(string p, DateTime? dt, DateTime da, bool starred, Guid uuid)
            {
                path = p;
                dateTaken = dt;
                dateAdded = da;
                this.starred = starred;
                this.uuid = uuid;
            }
            
            public MediaRow(string p, DateTime? dt, DateTime da, Guid uuid, string thumbnail)
            {
                path = p;
                dateTaken = dt;
                dateAdded = da;
                this.uuid = uuid;
                this.thumbnail = thumbnail;
            }

            public MediaRow(string p, DateTime? dt, DateTime da, bool starred, Guid uuid, string thumbnail)
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

            ///The uuid of the item, which will be added to the database upon upload.
            public Guid uuid;
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
        ///<param name="uuid">The uuid of this item.</param>
        ///<param name="thumbnail">ONLY FOR VIDEOS. A base64 string for the video thumbnail. Use null or "" for pictures.</param>
        ///<param name="starred">Is this item starred or not?</param>
        ///<param name="separate">Is this item separate from main library (i.e., is it in a folder)?</param>
        ///<returns>Int saying how many rows were affected.</returns>
        public static int InsertMedia(string path, DateTime? dateTaken, Guid uuid, string thumbnail, bool starred = false, bool separate = false)
        {
            int rowsAffected = 0;
            try
            {
                Open();
                using NpgsqlCommand cmd = new("", connection);
                cmd.Parameters.AddWithValue("@path", path);
                cmd.Parameters.AddWithValue("@uuid", uuid);
                cmd.Parameters.AddWithValue("@starred", starred);
                cmd.Parameters.AddWithValue("@separate", separate);
                if (dateTaken != null) cmd.Parameters.AddWithValue("@dateTaken", dateTaken);
                
                if (String.IsNullOrWhiteSpace(thumbnail))
                    cmd.CommandText = dateTaken == null ? "INSERT INTO media (path, date_added, starred, separate, uuid) VALUES (@path, now(), @starred, @separate, @uuid)" : "INSERT INTO media (path, date_taken, date_added, starred, separate, uuid) VALUES (@path, @dateTaken, now(), @starred, @separate, @uuid)";
                else
                {
                    cmd.Parameters.AddWithValue("@thumbnail", thumbnail);
                    cmd.CommandText = dateTaken == null ? "INSERT INTO media (path, date_added, starred, separate, uuid, thumbnail) VALUES (@path, now(), @starred, @separate, @uuid, @thumbnail)" : "INSERT INTO media (path, date_taken, date_added, starred, separate, uuid, thumbnail) VALUES (@path, @dateTaken, now(), @starred, @separate, @uuid, @thumbnail)";
                }

                cmd.CommandText += " ON CONFLICT(path) DO NOTHING";
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

        ///<summary>Create a new album and add it to the albums table.</summary>
        ///<param name="name">The name for the new album.</param>
        ///<param name="folder">True if this album should be a folder. False by default.</param>
        ///<returns>True if successfully created album/folder, false if album/folder couldn't be created (e.g., because duplicate album name).</returns>
        ///<remarks>The name column in the albums table requires all values to be unique.</remarks>
        public static bool CreateAlbum(string name, bool folder = false)
        {
            try
            {
                Open();
                using NpgsqlCommand cmd = new("INSERT INTO albums (name, last_updated, folder) VALUES (@name, now(), @folder)", connection);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@folder", folder);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e.ErrorCode + " Message: " + e.Message);
                return false;
            }
            finally
            {
                Close();
            }
        }

        
        ///<summary>Give an album a new name.</summary>
        ///<param name="newName">The new name for the album.</param>
        ///<param name="id">The id of the album to rename.</param>
        public static void RenameAlbum(string newName, int id)
        {
            try
            {
                Open();
                using NpgsqlCommand cmd = new("UPDATE albums SET name=@newName WHERE id=@id", connection);
                cmd.Parameters.AddWithValue("@newName", newName);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }
        }

        ///This has 3 different use cases: give an album a cover if it doesn't have a cover,
        ///update an existing cover, or remove an album cover (supply null as path).
        ///Albums aren't required to have an album cover.
        public static void UpdateAlbumCover(string albumName, string path)
        {
            try
            {
                Open();
                using NpgsqlCommand cmd = new("UPDATE albums SET album_cover=@path WHERE name=@albumName", connection);
                cmd.Parameters.AddWithValue("@path", path);
                cmd.Parameters.AddWithValue("@albumName", albumName);
                cmd.ExecuteNonQuery();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }
        }

        ///This has 3 different use cases: give an album a cover if it doesn't have a cover,
        ///update an existing cover, or remove an album cover (supply null as path).
        ///Albums aren't required to have an album cover.
        public static void UpdateAlbumCover(int albumID, string path)
        {
            try
            {
                Open();
                using NpgsqlCommand cmd = new("UPDATE albums SET album_cover=@path WHERE id=@albumID", connection);
                cmd.Parameters.AddWithValue("@path", path);
                cmd.Parameters.AddWithValue("@albumID", albumID);
                cmd.ExecuteNonQuery();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }
        }

        ///Given an album name, will find its ID in the albums table.
        ///Returns 0 if not found or can't connect. IDs are greater than 0.
        public static int GetAlbumID(string albumName)
        {
            int returnVal = 0;
            try
            {
                Open();
                using NpgsqlCommand selectCmd = new("SELECT id FROM albums WHERE name=@albumName", connection);
                selectCmd.Parameters.AddWithValue("@albumName", albumName);
                selectCmd.ExecuteNonQuery();

                using NpgsqlDataReader r = selectCmd.ExecuteReader();
                if (r.HasRows)
                {
                    r.Read(); //There should only be 1 column in 1 row to read.
                    returnVal = r.GetInt32(0);
                    r.Close();
                }
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }

            return returnVal;
        }

        ///<summary>Given an album id, attempt to return its album name.</summary>
        ///<param name="id">The id of the album.</param>
        ///<returns>Album name.</returns>
        public static string GetAlbumName(int id)
        {
            string returnVal = "";
            try
            {
                Open();

                //Find the album ID using the album name.
                using NpgsqlCommand selectCmd = new("SELECT name FROM albums WHERE id=@id", connection);
                selectCmd.Parameters.AddWithValue("@id", id);
                selectCmd.ExecuteNonQuery();

                using NpgsqlDataReader r = selectCmd.ExecuteReader();
                if (r.HasRows)
                {
                    r.Read(); //There should only be 1 line to read.
                    returnVal = r.GetString(0); //First and only column.
                    r.Close();
                }
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }

            return returnVal;
        } 

        ///<summary>Deletes the album with the given name, and remove all items in this album from album_entries. THIS CANNOT BE UNDONE! This also does not delete the path from the media table, so you can safely delete an album without losing the actual photos and videos.</summary>
        ///<param name="albumName">The name of the album to delete.</param>
        public static void DeleteAlbum(string albumName) => DeleteAlbum(GetAlbumID(albumName));

        ///<summary>Deletes the album with the given ID, and remove all items in this album from album_entries. THIS CANNOT BE UNDONE! This also does not delete the path from the media table, so you can safely delete an album without losing the actual photos and videos.</summary>
        ///<param name="albumID">The id of the album to delete.</param>
        public static void DeleteAlbum(int albumID)
        {
            try
            {
                Open();
                
                //Set all items to no longer being separate (only matters if this was a folder). If don't do this they won't appear in main library.
                NpgsqlCommand cmd = new("UPDATE media SET separate=false FROM album_entries WHERE album_id=@albumID AND album_entries.uuid=media.uuid", connection);
                cmd.Parameters.AddWithValue("@albumID", albumID);
                cmd.ExecuteNonQuery();
                
                //Removing the row for this album in albums table automatically removes any rows in album_entries referencing this album.
                cmd = new NpgsqlCommand("DELETE FROM albums WHERE id=@albumID", connection);
                cmd.Parameters.AddWithValue("@albumID", albumID);
                cmd.ExecuteNonQuery();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }
        }

        ///<summary>Add a single item to an album in album_entries. If it's a folder it handles all that automatically.</summary>
        ///<param name="uuid">The uuid of the item.</param>
        ///<param name="albumID">The ID of the album to add the item to.</param>
        public static void AddToAlbum(Guid uuid, int albumID)
        {
            bool isFolder = IsFolder(albumID);
            
            try
            {
                Open();
                using NpgsqlCommand cmd = new("", connection);
                cmd.Parameters.AddWithValue("@uuid", uuid);
                cmd.Parameters.AddWithValue("@albumID", albumID);

                if (isFolder)
                {
                    //If an item is being added to a folder it can only be in 1 folder and 0 albums so remove from everywhere else first. Then, mark the item as in a folder (separate).
                    cmd.CommandText = "DELETE FROM album_entries WHERE uuid=@uuid; UPDATE media SET separate=true WHERE uuid=@uuid";
                    cmd.ExecuteNonQuery();
                }

                //Actually add the item to the folder/album and set the album's last updated to now.
                cmd.CommandText = "INSERT INTO album_entries VALUES (@uuid, @albumID) ON CONFLICT (uuid, album_id) DO NOTHING; UPDATE albums SET last_updated = now() WHERE id=@albumID";
                cmd.ExecuteNonQuery();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }
        }

        ///<summary>Remove a single item from an album.</summary>
        ///<param name="uuid">The uuid of the item to remove.</param>
        ///<param name="albumID">ID of the album to remove from.</param>
        public static void RemoveFromAlbum(Guid uuid, int albumID)
        {
            try
            {
                Open();
                using NpgsqlCommand cmd = new("DELETE FROM album_entries WHERE album_id=@albumID AND uuid=@uuid", connection);
                cmd.Parameters.AddWithValue("@albumID", albumID);
                cmd.Parameters.AddWithValue("@uuid", uuid);
                cmd.ExecuteNonQuery();

                cmd.CommandText = "UPDATE albums SET last_updated = now() WHERE id=@albumID";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "UPDATE media SET separate = false WHERE uuid=@uuid AND separate = true";
                cmd.ExecuteNonQuery();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }
        }

        ///<summary>Load all the albums and/or folders in the albums table.</summary>
        ///<param name="showAlbums">Should albums be selected?</param>
        ///<param name="showFolders">Should folders be selected?</param>
        ///<param name="mode">How should the data be sorted?</param>
        ///<returns>A List&lt;Album&gt; of all the albums and/or folders.</returns>
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
                using NpgsqlCommand cmd = new($"SELECT id, name, album_cover, last_updated FROM albums {where} ORDER BY {orderBy}", connection);
                cmd.ExecuteNonQuery();
                using NpgsqlDataReader r = cmd.ExecuteReader();
                while (r.Read()) albums.Add(new Album(r.GetInt32(0), r.GetString(1), r.IsDBNull(2) ? String.Empty : r.GetString(2), r.GetDateTime(3))); //https://stackoverflow.com/a/38930847
                r.Close();
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

        ///<summary>Returns a List&lt;Album&gt; of all the items an album is in.</summary>
        ///<param name="uuid">Uuid of the item.</param>
        ///<returns>List of the albums the item is in, if any</returns>
        public static List<Album> GetAlbumsItemIn(Guid uuid)
        {
            List<Album> albums = new();

            try
            {
                Open();
                using NpgsqlCommand cmd = new("SELECT album_id, name, album_cover FROM album_entries AS e INNER JOIN albums AS a ON e.album_id=a.id WHERE uuid=@uuid ORDER BY name ASC", connection);
                cmd.Parameters.AddWithValue("@uuid", uuid);
                cmd.ExecuteNonQuery();
                using NpgsqlDataReader r = cmd.ExecuteReader();
                while (r.Read()) albums.Add(new Album(r.GetInt32(0), r.GetString(1), r.IsDBNull(2) ? String.Empty : r.GetString(2)));
                r.Close();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }
            
            return albums;
        }

        //https://www.postgresqltutorial.com/postgresql-update-join/
        ///<summary>Change an album to a folder or vice versa.</summary>
        ///<param name="albumID">ID of album or folder to change into folder or album.</param>
        ///<param name="folder">Specify true if want to change album -> folder. False for folder -> album</param>
        public static void ChangeAlbumType(int albumID, bool folder)
        {
            try
            {
                Open();
                using NpgsqlCommand cmd = new("UPDATE media SET separate=@folder FROM album_entries WHERE album_id=@albumID AND album_entries.uuid=media.uuid", connection);
                cmd.Parameters.AddWithValue("@folder", folder);
                cmd.Parameters.AddWithValue("@albumID", albumID);
                cmd.ExecuteNonQuery();

                cmd.CommandText = "UPDATE albums SET folder=@folder WHERE id=@albumID";
                cmd.ExecuteNonQuery();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }
        }

        ///<summary>Mark an item in the media table as in the Trash.</summary>
        ///<param name="uuid">The uuid of the item to move to Trash.</param>
        public static void MoveToTrash(Guid uuid)
        {
            try
            {
                Open();
                using NpgsqlCommand cmd = new("UPDATE media SET date_deleted = now() WHERE uuid=@uuid", connection);
                cmd.Parameters.AddWithValue("uuid", uuid);
                cmd.ExecuteNonQuery();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }
        }

        ///PERMANENTLY remove an item from the database and DELETES the file from server.
        public static void PermDeleteItem(Guid uuid)
        {
            File.Delete(Path.Join(S.libFolderPath, GetPathFromUuid(uuid)));

            try
            {
                Open();

                //Copy item from media to trash
                using NpgsqlCommand cmd = new("DELETE FROM media WHERE uuid=@uuid AND date_deleted IS NOT NULL", connection);
                cmd.Parameters.AddWithValue("@uuid", uuid);
                cmd.ExecuteNonQuery();

                cmd.CommandText = "DELETE FROM album_entries WHERE uuid=@uuid AND deleted = TRUE";
                cmd.ExecuteNonQuery();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }
        }

        ///Undoes a call to MoveToTrash(). Will restore albums it was in, as well as re-adding it to the media table.
        public static void RestoreItem(Guid uuid)
        {
            try
            {
                Open();

                using NpgsqlCommand cmd = new("UPDATE media SET date_deleted = NULL WHERE uuid = @uuid", connection);
                cmd.Parameters.AddWithValue("@uuid", uuid);
                cmd.ExecuteNonQuery();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }
        }

        ///<summary>Loads all rows and columns in the media table not in a folder (separate==false), and that have a date taken, into a List&lt;MediaRow&gt;.</summary>
        ///<returns>List&lt;MediaRow&gt; of items in media table not in a folder, sorted by date taken descending (newest first).</returns>
        public static List<MediaRow> LoadMediaTable()
        {
            List<MediaRow> media = new();
            try
            {
                Open();
                using NpgsqlCommand cmd = new("SELECT path, date_taken, date_added, starred, uuid, thumbnail FROM media WHERE date_taken IS NOT NULL AND date_deleted IS NULL AND separate=false ORDER BY date_taken DESC", connection);
                cmd.ExecuteNonQuery();
                using NpgsqlDataReader r = cmd.ExecuteReader();
                while (r.Read()) media.Add(new MediaRow(r.GetString(0), r.GetDateTime(1), r.GetDateTime(2), r.GetBoolean(3), r.GetGuid(4), r.IsDBNull(5) ? null : r.GetString(5)));
                r.Close();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }

            return media;
        }

        ///<summary>Loads all rows and columns in the media table not in a folder (separate==false) and that DON'T have a date taken.</summary>
        ///<returns>A List&lt;MediaRow&gt; containing only items without a date taken (NULL DT).</returns>
        public static List<MediaRow> LoadMediaNoDT()
        {
            List<MediaRow> media = new();
            try
            {
                Open();
                using NpgsqlCommand cmd = new("SELECT path, date_added, starred, uuid, thumbnail FROM media WHERE date_taken IS NULL AND date_deleted IS NULL AND separate=false ORDER BY date_added DESC", connection);
                cmd.ExecuteNonQuery();
                using NpgsqlDataReader r = cmd.ExecuteReader();
                while (r.Read()) media.Add(new MediaRow(r.GetString(0), null, r.GetDateTime(1), r.GetBoolean(2), r.GetGuid(3), r.IsDBNull(4) ? null : r.GetString(4)));
                r.Close();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }

            return media;
        }
        
        ///<summary>Like LoadMediaTable() but only loads items where separate==false AND starred==true.</summary>
        ///<returns>List&lt;MediaRow&gt; of items in media table not in a folder AND starred, sorted by date taken descending (newest first).</returns>
        public static List<MediaRow> LoadStarred()
        {
            List<MediaRow> media = new();
            try
            {
                Open();
                using NpgsqlCommand cmd = new("SELECT path, date_taken, date_added, uuid, thumbnail FROM media WHERE date_taken IS NOT NULL AND separate=FALSE AND starred=TRUE ORDER BY date_taken DESC", connection);
                cmd.ExecuteNonQuery();
                using NpgsqlDataReader r = cmd.ExecuteReader();
                while (r.Read()) media.Add(new MediaRow(r.GetString(0), r.GetDateTime(1), r.GetDateTime(2), r.GetGuid(3), r.IsDBNull(4) ? null : r.GetString(4)));
                r.Close();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }

            return media;
        }

        ///<summary>Get if an item is starred or not.</summary>
        ///<returns>True if this item is starred, false if not.</returns>
        public static bool IsStarred(Guid uuid)
        {
            bool starred = false;
            
            try
            {
                Open();
                using NpgsqlCommand cmd = new("SELECT starred FROM media WHERE uuid=@uuid", connection);
                cmd.Parameters.AddWithValue("@uuid", uuid);
                cmd.ExecuteNonQuery();

                using NpgsqlDataReader r = cmd.ExecuteReader();
                if (r.HasRows)
                {
                    r.Read();
                    starred = r.GetBoolean(0);
                    r.Close();
                }
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }

            return starred;
        }

        ///<summary>Change a single item from either starred or not starred.</summary>
        public static void UpdateStarred(Guid uuid, bool starred)
        {
            try
            {
                Open();
                using NpgsqlCommand cmd = new("UPDATE media SET starred=@starred WHERE uuid=@uuid", connection);
                cmd.Parameters.AddWithValue("@starred", starred);
                cmd.Parameters.AddWithValue("@uuid", uuid);
                cmd.ExecuteNonQuery();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }
        }
        
        ///<summary>Change a List of items from either starred (true) or not starred.</summary>
        public static void UpdateStarred(List<Guid> uuids, bool starred)
        {
            try
            {
                Open();
                foreach(Guid uuid in uuids)
                {
                    using NpgsqlCommand cmd = new("UPDATE media SET starred=@starred WHERE uuid=@uuid", connection);
                    cmd.Parameters.AddWithValue("@starred", starred);
                    cmd.Parameters.AddWithValue("@uuid", uuid);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }
        }
        
        ///<summary>Loads the contents of an album/folder into a List&lt;MediaRow&gt;, optionally including items without a Date Taken (set in Settings).</summary>        
        ///<param name="albumID">The id of the album/folder to load.</param>
        ///<param name="mode">How the items should be sorted.</param>
        ///<returns>List&lt;MediaRow&gt; of the album/folder contents.</returns>
        public static List<MediaRow> LoadAlbum(int albumID, AVSortMode mode = AVSortMode.NewestDateTaken)
        {
            bool isFolder = IsFolder(albumID);
            List<MediaRow> media = new();
            
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
                using NpgsqlCommand cmd = new($"SELECT m.path, m.date_taken, m.starred, m.uuid, m.thumbnail FROM media AS m INNER JOIN album_entries AS a ON m.uuid=a.uuid WHERE album_id=@albumID AND {(S.displayNoDTInAV ? "" : "date_taken IS NOT NULL AND")} date_deleted IS NULL AND separate={isFolder} ORDER BY {orderBy}", connection);
                cmd.Parameters.AddWithValue("@albumID", albumID);
                cmd.ExecuteNonQuery();
                using NpgsqlDataReader r = cmd.ExecuteReader();
                while (r.Read()) media.Add(new MediaRow(r.GetString(0), r.IsDBNull(1) ? null : r.GetDateTime(1), r.GetBoolean(2), r.GetGuid(3), r.IsDBNull(4) ? null : r.GetString(4)));
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }

            return media;
        }

        ///<summary>Loads everything in the media table that is in the Trash.</summary>
        ///<param name="mode">An enum controlling how the items in Trash are sorted.</param>
        ///<returns>A List&lt;MediaRow&gt; containing everything in the Trash.</returns>
        public static List<MediaRow> LoadMediaTrash(TrashSortMode mode = TrashSortMode.NewestDateDeleted)
        {
            List<MediaRow> media = new(); //Stores every row retrieved; returned later.

            string orderBy = mode switch
            {
                TrashSortMode.NewestDateDeleted => "date_deleted DESC",
                TrashSortMode.NewestDateTaken => "date_taken DESC",
                TrashSortMode.OldestDateDeleted => "date_deleted ASC",
                TrashSortMode.OldestDateTaken => "date_taken ASC",
                _ => "date_deleted DESC"
            };
            
            try
            {
                Open();
                using NpgsqlCommand cmd = new($"SELECT path, date_taken, date_added, starred, uuid, thumbnail FROM media WHERE date_deleted IS NOT NULL ORDER BY {orderBy}", connection);
                cmd.ExecuteNonQuery();
                using NpgsqlDataReader r = cmd.ExecuteReader();
                while (r.Read()) media.Add(new MediaRow(r.GetString(0), r.GetDateTime(1), r.GetDateTime(2), r.GetBoolean(3), r.GetGuid(4), r.IsDBNull(5) ? null : r.GetString(5)));
                r.Close();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }

            return media;
        }

        ///<summary>Update when an item was taken, update its short path, and move it to the new path on the server.</summary>
        ///<param name="shortPath">The path to the item that is stored in the database</param>
        ///<param name="newDateTaken">The new date taken for this item</param>
        ///<returns>True if completed successfully, false otherwise. If it failed, it's probably because an item with the same filename and Date Taken already exists there.</returns>
        public static bool UpdateDateTaken(string shortPath, DateTime? newDateTaken)
        {
            try
            {
                string filename = Path.GetFileName(shortPath);
                string originalFullPath = Path.Combine(S.libFolderPath, shortPath);
                
                string newShortPath = CreateShortPath(newDateTaken, filename);
                string newDTFolderPath = CreateFullDateFolderPath(newDateTaken);
                string newFullPath = Path.Combine(newDTFolderPath, filename);
                
                //Move item to new path on server. This is how the user input is validated. If this fails, user needs to pick a new DT and/or filename.
                Directory.CreateDirectory(newDTFolderPath); //Create in case it doesn't exist.
                File.Move(originalFullPath, newFullPath);

                Open();
                using NpgsqlCommand cmd = new("", connection);
                if (newDateTaken == null)
                {
                    cmd.CommandText = "UPDATE media SET path = @newPath, date_taken = NULL WHERE path = @shortPath";
                }
                else
                {
                    cmd.CommandText = "UPDATE media SET path = @newPath, date_taken = @newDateTaken WHERE path = @shortPath";
                    cmd.Parameters.AddWithValue("@newDateTaken", newDateTaken);
                }

                //Update date taken and short path in media.
                cmd.Parameters.AddWithValue("@newPath", newShortPath);
                cmd.Parameters.AddWithValue("@shortPath", shortPath);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            finally
            {
                Close();
            }
        }

        ///<summary>Gets an item's path from its uuid.</summary>
        ///<returns>The short path of the item, if found. null if couldn't find short path.</returns>
        public static string GetPathFromUuid(string uuid) => GetPathFromUuid(new Guid(uuid));

        ///<summary>Gets an item's path from its uuid.</summary>
        ///<returns>The short path of the item, if found. null if couldn't find short path.</returns>
        public static string GetPathFromUuid(Guid uuid)
        {
            string path = null;
            try
            {
                Open();
                using NpgsqlCommand cmd = new("SELECT path FROM media WHERE uuid=@uuid", connection);
                cmd.Parameters.AddWithValue("@uuid", uuid);
                cmd.ExecuteNonQuery();

                using NpgsqlDataReader r = cmd.ExecuteReader();
                if (r.HasRows)
                {
                    r.Read(); //There should only be 1 line to read.
                    path = r.GetString(0); //First and only column.
                    r.Close();
                }
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }
            return path;
        }

        ///<summary>Get the UUID of the item with this short path.</summary>
        ///<param name="shortPath">The short path of the item.</param>
        ///<returns>The uuid of the item.</returns>
        public static Guid GetUuidFromPath(string shortPath)
        {
            Guid uuid = new();

            try
            {
                Open();
                using NpgsqlCommand cmd = new("SELECT uuid FROM media WHERE shortPath=@shortPath", connection);
                cmd.Parameters.AddWithValue("@shortPath", shortPath);
                cmd.ExecuteNonQuery();

                using NpgsqlDataReader r = cmd.ExecuteReader();
                if (r.HasRows)
                {
                    r.Read(); //There should only be 1 column in 1 row to read.
                    uuid = r.GetGuid(0);
                    r.Close();
                }
                else uuid = Guid.Empty;
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }
            return uuid;
        }

        ///<summary>Returns the date taken of the item with this uuid.</summary>
        ///<param name="uuid">The uuid of the item.</param>
        ///<returns>The DateTime? date taken of the item.</returns>
        public static DateTime? GetDateTaken(Guid uuid)
        {
            DateTime? dateTaken = null;

            try
            {
                Open();
                using NpgsqlCommand cmd = new("SELECT date_taken FROM media WHERE uuid=@uuid", connection);
                cmd.Parameters.AddWithValue("@uuid", uuid);
                cmd.ExecuteNonQuery();
                
                using NpgsqlDataReader r = cmd.ExecuteReader();
                if (r.HasRows)
                {
                    r.Read();
                    dateTaken = r.IsDBNull(0) ? null : r.GetDateTime(0);
                    r.Close();
                }
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }

            return dateTaken;
        }

        ///<summary>Get the DateTime of when this item was added to the library.</summary>
        ///<param name="uuid">The uuid of the item.</param>
        ///<returns>DateTime representing its date_added value.</returns>
        public static DateTime GetDateAdded(Guid uuid)
        {
            DateTime dateTaken = new();
            try
            {
                Open();
                using NpgsqlCommand cmd = new("SELECT date_added FROM media WHERE uuid=@uuid", connection);
                cmd.Parameters.AddWithValue("@uuid", uuid);
                cmd.ExecuteNonQuery();

                using NpgsqlDataReader r = cmd.ExecuteReader();
                if (r.HasRows)
                {
                    r.Read();
                    dateTaken = r.GetDateTime(0);
                    r.Close();
                }
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }

            return dateTaken;
        }
        
        ///<summary>Get the DateTime a uuid was added to an album.</summary>
        ///<param name="uuid">The uuid of the item.</param>
        ///<param name="albumID">The id of the album the item is in.</param>
        ///<returns>DateTime the uuid was added to the album.</returns>
        public static DateTime GetDateAddedToAlbum(Guid uuid, int albumID)
        {
            DateTime dateTaken = new();
            try
            {
                Open();
                using NpgsqlCommand cmd = new("SELECT date_added_to_album FROM album_entries WHERE uuid=@uuid AND album_id=@albumID", connection);
                cmd.Parameters.AddWithValue("@uuid", uuid);
                cmd.Parameters.AddWithValue("@albumID", albumID);
                cmd.ExecuteNonQuery();

                using NpgsqlDataReader r = cmd.ExecuteReader();
                if (r.HasRows)
                {
                    r.Read();
                    dateTaken = r.GetDateTime(0);
                    r.Close();
                }
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }

            return dateTaken;
        }

        ///Returns true if an album is a folder, false otherwise.
        public static bool IsFolder(int albumID)
        {
            bool isFolder = false;
            try
            {
                Open();
                using NpgsqlCommand cmd = new("SELECT folder FROM albums WHERE id=@albumID", connection);
                cmd.Parameters.AddWithValue("@albumID", albumID);
                cmd.ExecuteNonQuery();

                using NpgsqlDataReader r = cmd.ExecuteReader();
                if (r.HasRows)
                {
                    r.Read();
                    isFolder = r.GetBoolean(0);
                    r.Close();
                }
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                Close();
            }

            return isFolder;
        }

        //TODO: use this guy in UA, GPO import, etc. Forgot about it lmao.
        ///<summary>Generates a short path (DB path) given a Date Taken and filename. A DB path looks like this: 2022/5/filename.jpg.</summary>
        ///<param name="dateTaken">The date taken of the item.</param>
        ///<param name="filename">The filename and extension of the item.</param>
        ///<returns>A short/DB path for the item.</returns>
        ///<remarks>If date taken is null, the returned path's format is 'Unknown/filename.jpg'. If date taken is not null, the returned path's format is like 2022/5/filename.jpg.</remarks>
        public static string CreateShortPath(DateTime? dateTaken, string filename) => dateTaken == null ? $"Unknown/{filename}" : $"{dateTaken.Value.Year}/{dateTaken.Value.Month}/{filename}";

        ///<summary>Create the full folder path to where an item with this date taken would get moved to in the PSS library.</summary>
        ///<param name="dateTaken">The date taken to use for creating the path.</param>
        ///<returns>The full date folder path.</returns>
        public static string CreateFullDateFolderPath(DateTime? dateTaken) => Path.Combine(S.libFolderPath, dateTaken == null ? "Unknown" : $"{dateTaken.Value.Year}/{dateTaken.Value.Month}");

        ///<summary>Given a Date Taken and a filename, create the full path to where the item would get moved to in the PSS library.</summary>
        ///<param name="dateTaken">The date taken to use for creating the path.</param>
        ///<param name="filename">The filename and extension of the item.</param>
        ///<returns>The full path to where the item would get moved to in the PSS library.</returns>
        public static string CreateFullPath(DateTime? dateTaken, string filename) => Path.Combine(S.libFolderPath, CreateShortPath(dateTaken, filename));

        ///<summary>Used in ViewItem for renaming the current item's file.</summary>
        ///<param name="oldShortPath">The original short path of the item.</param>
        ///<param name="newFilename">The new filename of the item.</param>
        ///<param name="ext">The file extension.</param>
        ///<param name="dateTaken">The date taken of the item.</param>
        ///<returns>The new short path (DB path) of this item. null if DB error occurred, which means there is already a file with the same name in that location.</returns>
        public static string RenameFile(string oldShortPath, string newFilename, string ext, DateTime? dateTaken)
        {
            string newShortPath = CreateShortPath(dateTaken, newFilename + ext);
            
            //Rename the file in the DB path.
            try
            {
                Open();
                using NpgsqlCommand cmd = new("UPDATE media SET path=@newShortPath WHERE path=@oldShortPath", connection);
                cmd.Parameters.AddWithValue("@newShortPath", newShortPath);
                cmd.Parameters.AddWithValue("@oldShortPath", oldShortPath);
                cmd.ExecuteNonQuery();
            }
            catch (NpgsqlException e)
            {
                if (e.ErrorCode != -2147467259) //Duplicate key value error. No need to print this out since the error is caught.
                    Console.WriteLine(e.ErrorCode + " Message: " + e.Message);
                return null;
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