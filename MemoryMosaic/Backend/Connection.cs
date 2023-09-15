namespace MemoryMosaic.Backend;

///Contains static methods for interacting with the MM PostgreSQL database.
public static class Connection
{
    #if DEBUG
    private const string CONNECTION_STRING = "Host=localhost; Port=5432; User Id=postgres; Password=Ph0t0s_Server; Database=MemoryMosaicTest";
    #else
    private const string CONNECTION_STRING = "Host=localhost; Port=5432; User Id=postgres; Password=Ph0t0s_Server; Database=MemoryMosaic";
    #endif
    
    public static readonly NpgsqlConnection connection = new(CONNECTION_STRING);

    ///Asynchronously creates, opens, and returns a new connection object.
    public static async Task<NpgsqlConnection> CreateLocalConnectionAsync()
    {
        NpgsqlConnection localConn = new(CONNECTION_STRING);
        await localConn.OpenAsync();
        return localConn;
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

    #region Media

    ///<summary>For inserting a photo or video into the media table (the main table). Will not insert duplicates.</summary>
    ///<param name="path">The short path that will be stored in media. Convention is to use '/' as the separator.</param>
    ///<param name="dateTaken">When this item was taken.</param>
    ///<param name="uuid">The uuid of this item.</param>
    ///<param name="thumbnail">A base64 string representing the thumbnail.</param>
    ///<param name="starred">Is this item starred or not?</param>
    public static async Task InsertMedia(string path, DateTime? dateTaken, Guid uuid, string thumbnail, bool starred = false)
    {
        NpgsqlConnection localConn = await CreateLocalConnectionAsync();

        try
        {
            await using NpgsqlCommand cmd = new("", localConn);
            cmd.Parameters.AddWithValue("@path", path);
            cmd.Parameters.AddWithValue("@uuid", uuid);
            cmd.Parameters.AddWithValue("@starred", starred);
            cmd.Parameters.AddWithValue("@thumbnail", thumbnail);
            if (dateTaken != null) cmd.Parameters.AddWithValue("@dateTaken", dateTaken);

            cmd.CommandText = $"INSERT INTO media (path, {(dateTaken == null ? "" : "date_taken,")} starred, uuid , thumbnail) VALUES (@path, {(dateTaken == null ? "" : "@dateTaken, ")} @starred, @uuid, @thumbnail) ON CONFLICT(path) DO NOTHING";

            await cmd.ExecuteNonQueryAsync();
        }
        catch (NpgsqlException e)
        {
            L.LogException(e);
        }
        finally
        {
            await localConn.CloseAsync();
        }
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
            D.UpdateDateTaken(newFullPath, newDateTaken);

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
            L.LogException(e);
            return false;
        }
        finally
        {
            Close();
        }
    }

    ///<summary>Gets an item's short path from its uuid.</summary>
    ///<returns>The short path of the item, if found. null if couldn't find short path.</returns>
    public static string? GetPathFromUuid(Guid uuid)
    {
        string? path = null;
        try
        {
            Open();
            using NpgsqlCommand cmd = new("SELECT path FROM media WHERE uuid=@uuid", connection);
            cmd.Parameters.AddWithValue("@uuid", uuid);
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
            L.LogException(e);
        }
        finally
        {
            Close();
        }
        return path;
    }
    
    ///<summary>Used in ViewItem for renaming the current item's file.</summary>
    ///<param name="oldShortPath">The original short path of the item.</param>
    ///<param name="newFilename">The new filename of the item.</param>
    ///<param name="ext">The file extension.</param>
    ///<param name="dateTaken">The date taken of the item.</param>
    ///<returns>The new short path (DB path) of this item. null if DB error occurred, which means there is already a file with the same name in that location.</returns>
    public static string? RenameFile(string oldShortPath, string newFilename, string ext, DateTime? dateTaken)
    {
        try
        {
            string originalFullPath = Path.Combine(S.libFolderPath, oldShortPath);
            string newShortPath = CreateShortPath(dateTaken, newFilename + ext);
            string newFullPath = Path.Combine(S.libFolderPath, newShortPath);
            File.Move(originalFullPath, newFullPath);
            
            Open();
            using NpgsqlCommand cmd = new("UPDATE media SET path = @newShortPath WHERE path = @oldShortPath", connection);
            cmd.Parameters.AddWithValue("@newShortPath", newShortPath);
            cmd.Parameters.AddWithValue("@oldShortPath", oldShortPath);
            cmd.ExecuteNonQuery();
            return newShortPath;
        }
        catch (Exception e)
        {
            L.LogException(e);
            return null;
        }
        finally
        {
            Close();
        }
    }

    ///<summary>Loads every row in the media table, even if has no DT, in a folder, in the trash, etc. Sorted by date_taken descending (NULL and newest DT first).</summary>
    ///<returns>List&lt;MediaRow&gt; of EVERY row in the media table.</returns>
    public static List<MediaRow> LoadEntireMediaTable()
    {
        List<MediaRow> media = new();
        try
        {
            Open();
            using NpgsqlCommand cmd = new("SELECT path, date_taken, date_added, starred, separate, uuid, thumbnail, description FROM media ORDER BY date_taken DESC", connection);
            using NpgsqlDataReader r = cmd.ExecuteReader();
            while (r.Read()) media.Add(new MediaRow(r.GetString(0), r.IsDBNull(1) ? null : r.GetDateTime(1), r.GetDateTime(2), r.GetBoolean(3), r.GetBoolean(4), r.GetGuid(5), r.GetString(6), r.IsDBNull(7) ? null : r.GetString(7)));
            r.Close();
        }
        catch (NpgsqlException e)
        {
            L.LogException(e);
        }
        finally
        {
            Close();
        }
        return media;
    }

    //TODO: possibly move me to memories
    // ///<summary>Loads every item in media that was taken on this month and day, sorted so newest items appear first.</summary>
    // ///<param name="monthName">The name of the month, automatically converted to a number by LoadMemories().</param>
    // ///<param name="day">The day of the month.</param>
    // ///<returns>List&lt;MediaRow&gt; of items taken on this month and day.</returns>
    // public static List<MediaRow> LoadMemories(string monthName, int day)
    // {
    //     List<MediaRow> memories = new();
    //     int month = DateTime.ParseExact(monthName, "MMMM", System.Globalization.CultureInfo.CurrentCulture).Month;
    //     string dd = day < 10 ? $"0{day}" : day.ToString();
    //
    //     try
    //     {
    //         Open();
    //         using NpgsqlCommand cmd = new($"SELECT path, date_taken, date_added, starred, uuid, thumbnail, description FROM media WHERE CAST(date_taken as TEXT) LIKE '%{month}-{dd}%' ORDER BY date_taken DESC", connection);
    //         using NpgsqlDataReader r = cmd.ExecuteReader();
    //         while (r.Read()) memories.Add(new MediaRow(r.GetString(0), r.IsDBNull(1) ? null : r.GetDateTime(1), r.GetDateTime(2), r.GetBoolean(3), r.GetGuid(4), r.GetString(5), r.IsDBNull(6) ? null : r.GetString(6)));
    //     }
    //     catch (NpgsqlException e)
    //     {
    //         L.LogException(e);
    //     }
    //     finally
    //     {
    //         Close();
    //     }
    //     return memories;
    // }
    
    /// <summary>Sets the description of an item.</summary>
    /// <param name="uuid">The uuid of the item.</param>
    /// <param name="newDescription">The new description of the item.</param>
    public static void UpdateDescription(Guid uuid, string? newDescription)
    {
        try
        {
            Open();
            using NpgsqlCommand cmd = new(null, connection);
            if (String.IsNullOrWhiteSpace(newDescription))
            {
                cmd.CommandText = "UPDATE media SET description = NULL WHERE uuid=@uuid";
            }
            else
            {
                cmd.CommandText = "UPDATE media SET description = @newDescription WHERE uuid=@uuid";
                cmd.Parameters.AddWithValue("@newDescription", newDescription);
            }
            cmd.Parameters.AddWithValue("@uuid", uuid);
            cmd.ExecuteNonQuery();
        }
        catch (NpgsqlException e)
        {
            L.LogException(e);
        }
        finally
        {
            Close();
        }
    }

    #region Trash

    ///Set this media item's date_deleted to the current date and time.
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
            L.LogException(e);
        }
        finally
        {
            Close();
        }
    }

    ///Set the date_deleted field of an IEnumerable&lt;Guid&gt; of items to the current date and time.
    public static void MoveToTrash(IEnumerable<Guid> uuids)
    {
        foreach (Guid uuid in uuids)
            MoveToTrash(uuid);
    }

    ///PERMANENTLY remove an item from the database and DELETES the file from disk.
    public static void RemoveFromTrash(Guid uuid)
    {
        try
        {
            FileSystem.DeleteFile(Path.Combine(S.libFolderPath, GetPathFromUuid(uuid) ?? throw new InvalidOperationException()), UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
        }
        catch (FileNotFoundException e)
        {
            L.LogException(e);
        }
        
        try
        {
            Open();

            using NpgsqlCommand cmd = new("DELETE FROM media WHERE uuid=@uuid AND date_deleted IS NOT NULL", connection);
            cmd.Parameters.AddWithValue("@uuid", uuid);
            cmd.ExecuteNonQuery();

            cmd.CommandText = "DELETE FROM collection_entries WHERE uuid=@uuid";
            cmd.ExecuteNonQuery();
        }
        catch (NpgsqlException e)
        {
            L.LogException(e);
        }
        finally
        {
            Close();
        }
    }

    ///PERMANENTLY removes an IEnumerable&lt;Guid&gt; of items from the database and DELETES the file from disk.
    public static void RemoveFromTrash(IEnumerable<Guid> uuids)
    {
        foreach (Guid uuid in uuids)
            MoveToTrash(uuid);
    }
    
    ///PERMANENTLY removes all items in Trash from server and database.
    public static void EmptyTrash()
    {
        try
        {
            Open();
            using NpgsqlCommand cmd = new("SELECT path FROM media WHERE date_deleted IS NOT NULL", connection);
            using NpgsqlDataReader r = cmd.ExecuteReader();

            while (r.Read())
            {
                try { FileSystem.DeleteFile(Path.Combine(S.libFolderPath, r.GetString(0)), UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin); }
                catch (IOException e) { L.LogException(e); }
            }

            r.Close();
            cmd.CommandText = "DELETE FROM media WHERE date_deleted IS NOT NULL";
            cmd.ExecuteNonQuery();
        }
        catch (NpgsqlException e) { L.LogException(e); }
        finally { Close(); }
    }

    ///Undoes a call to MoveToTrash(). Will restore collections it was in, as well as re-adding it to the media table.
    public static void RestoreItem(Guid uuid)
    {
        try
        {
            Open();

            using NpgsqlCommand cmd = new("UPDATE media SET date_deleted = NULL WHERE uuid = @uuid", connection);
            cmd.Parameters.AddWithValue("@uuid", uuid);
            cmd.ExecuteNonQuery();
        }
        catch (NpgsqlException e) { L.LogException(e); }
        finally { Close(); }
    }

    ///Restores EVERY item in the Trash back into library.
    public static void RestoreTrash()
    {
        try
        {
            Open();
            using NpgsqlCommand cmd = new("UPDATE media SET date_deleted = NULL WHERE date_deleted IS NOT NULL", connection);
            cmd.ExecuteNonQuery();
        }
        catch (NpgsqlException e) { L.LogException(e); }
        finally { Close(); }
    }
    
    #endregion

    #region Starred
    
    ///<summary>Change a single item from either starred (true) or not starred.</summary>
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
            L.LogException(e);
        }
        finally
        {
            Close();
        }
    }
    
    ///<summary>Change an IEnumerable of items from either starred (true) or not starred.</summary>
    public static void UpdateStarred(IEnumerable<Guid> uuids, bool starred)
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
            L.LogException(e);
        }
        finally
        {
            Close();
        }
    }
    #endregion
    
    #endregion

    #region Collections
    
    ///<summary>Create a new Collection and add it to the `collections` table.</summary>
    ///<param name="name">The name for the new collection.</param>
    ///<param name="isFolder">True if this collection should be a folder. False by default.</param>
    ///<returns>True if successfully created collection, false if collection couldn't be created (e.g., because duplicate name).</returns>
    ///<remarks>The name column in the collection table requires all values to be unique.</remarks>
    public static bool CreateCollection(string name, bool isFolder = false)
    {
        try
        {
            Open();
            using NpgsqlCommand cmd = new("INSERT INTO collections (name, last_updated, folder) VALUES (@name, now(), @isFolder)", connection);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@isFolder", isFolder);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (NpgsqlException e)
        {
            L.LogException(e);
            return false;
        }
        finally
        {
            Close();
        }
    }
    
    ///<param name="newName">The new name for the collection.</param>
    ///<param name="id">The id of the collection to rename.</param>
    public static void RenameCollection(string newName, int id)
    {
        try
        {
            Open();
            using NpgsqlCommand cmd = new("UPDATE collections SET name=@newName WHERE id=@id", connection);
            cmd.Parameters.AddWithValue("@newName", newName);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
        catch (NpgsqlException e)
        {
            L.LogException(e);
        }
        finally
        {
            Close();
        }
    }

    
    ///This has 3 different use cases: give a collection a cover if it doesn't have a cover, update an existing cover, or remove a collection cover (supply null as path).
    ///Collections aren't required to have a cover.
    public static void UpdateCollectionCover(int collectionID, string path)
    {
        try
        {
            Open();
            using NpgsqlCommand cmd = new("UPDATE collections SET cover=@path WHERE id=@collectionID", connection);
            cmd.Parameters.AddWithValue("@path", path);
            cmd.Parameters.AddWithValue("@collectionID", collectionID);
            cmd.ExecuteNonQuery();
        }
        catch (NpgsqlException e)
        {
            L.LogException(e);
        }
        finally
        {
            Close();
        }
    }

    ///<summary>Given the integer id of a collection represented as a string, return a new <see cref="Collection"/> with the extra details about a Collection, like name, last updated, folder, etc.</summary>
    /// <param name="collectionID"></param>
    public static async Task<Collection?> GetCollectionDetailsAsync(string collectionID)
    {
        NpgsqlConnection localConn = await CreateLocalConnectionAsync();
        
        try
        {
            await using NpgsqlCommand cmd = new($"SELECT name, last_updated, folder, readonly FROM collections WHERE id = {collectionID}", localConn);
            await using NpgsqlDataReader r = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
            await r.ReadAsync();
            return new Collection(Int32.Parse(collectionID), r.GetString(0), r.GetDateTime(1), r.GetBoolean(2), r.GetBoolean(3));
        }
        catch (NpgsqlException e)
        {
            L.LogException(e);
            return null;
        }
        finally
        {
            await localConn.CloseAsync();
        }
    }

    ///Toggles a Collection's readonly field in the collections table.
    public static async Task ToggleReadonlyAsync(Collection collection)
    {
        NpgsqlConnection localConn = await CreateLocalConnectionAsync();
        
        try
        {
            await using NpgsqlCommand cmd = new($"UPDATE collections SET readonly = {!collection.readOnly} WHERE id = {collection.id}", localConn);
            await cmd.ExecuteNonQueryAsync();
        }
        catch (NpgsqlException e)
        {
            L.LogException(e);
        }
        finally
        {
            await localConn.CloseAsync();
        }
    }

    ///<summary>Given an collection id, attempt to return its name.</summary>
    ///<param name="id">The id of the collection.</param>
    ///<returns>Collection name.</returns>
    public static string? GetCollectionName(int id)
    {
        string? returnVal = null;
        try
        {
            Open();

            using NpgsqlCommand selectCmd = new("SELECT name FROM collections WHERE id=@id", connection);
            selectCmd.Parameters.AddWithValue("@id", id);
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
            L.LogException(e);
        }
        finally
        {
            Close();
        }

        return returnVal;
    } 

    ///<summary>Deletes the collection with the given ID, and remove all items in this collection from collection_entries. THIS CANNOT BE UNDONE! This also does not delete the path from the media table, so you can safely delete a collection without losing the actual photos and videos.</summary>
    ///<param name="collectionID">The id of the collection to delete.</param>
    public static void DeleteCollection(int collectionID)
    {
        try
        {
            Open();
            
            //Set all items to no longer being separate (only matters if this was a folder). If don't do this they won't appear in main library.
            NpgsqlCommand cmd = new("UPDATE media SET separate=false FROM collection_entries WHERE collection_id=@collectionID AND collection_entries.uuid=media.uuid", connection);
            cmd.Parameters.AddWithValue("@collectionID", collectionID);
            cmd.ExecuteNonQuery();
            
            //Removing the row for this collection in collections table automatically removes any rows in collection_entries referencing this collection.
            cmd = new NpgsqlCommand("DELETE FROM collections WHERE id=@collectionID", connection);
            cmd.Parameters.AddWithValue("@collectionID", collectionID);
            cmd.ExecuteNonQuery();
        }
        catch (NpgsqlException e)
        {
            L.LogException(e);
        }
        finally
        {
            Close();
        }
    }

    ///<summary>Add a single item to a collection in collection_entries. If it's a folder it handles all that automatically.</summary>
    ///<param name="uuid">The uuid of the item.</param>
    ///<param name="collectionID">The ID of the collection to add the item to.</param>
    public static void AddToCollection(Guid uuid, int collectionID)
    {
        bool isFolder = IsFolder(collectionID);
        
        try
        {
            Open();
            using NpgsqlCommand cmd = new("", connection);
            cmd.Parameters.AddWithValue("@uuid", uuid);
            cmd.Parameters.AddWithValue("@collectionID", collectionID);

            if (isFolder)
            {
                //If an item is being added to a folder it can only be in 1 folder and 0 albums so remove from everywhere else first. Then, mark the item as in a folder (separate).
                cmd.CommandText = "DELETE FROM collection_entries WHERE uuid=@uuid; UPDATE media SET separate=true WHERE uuid=@uuid";
                cmd.ExecuteNonQuery();
            }

            //Actually add the item to the collection and set the collection's last updated to now.
            cmd.CommandText = "INSERT INTO collection_entries VALUES (@uuid, @collectionID) ON CONFLICT (uuid, collection_id) DO NOTHING; UPDATE collections SET last_updated = now() WHERE id=@collectionID";
            cmd.ExecuteNonQuery();
        }
        catch (NpgsqlException e)
        {
            L.LogException(e);
        }
        finally
        {
            Close();
        }
    }
    
    ///<summary>Add a single item to a collection in collection_entries. If it's a folder it handles all that automatically.</summary>
    ///<param name="uuid">The uuid of the item.</param>
    ///<param name="collectionID">The ID of the collection to add the item to.</param>
    public static async Task AddToCollectionAsync(Guid uuid, int collectionID)
    {
        NpgsqlConnection localConn = await CreateLocalConnectionAsync();
        bool isFolder = await IsFolderAsync(collectionID);
        
        try
        {
            await using NpgsqlCommand cmd = new("", localConn);
            cmd.Parameters.AddWithValue("@uuid", uuid);
            cmd.Parameters.AddWithValue("@collectionID", collectionID);

            if (isFolder)
            {
                //If an item is being added to a folder it can only be in 1 folder and 0 albums so remove from everywhere else first. Then, mark the item as in a folder (separate).
                cmd.CommandText = "DELETE FROM collection_entries WHERE uuid=@uuid; UPDATE media SET separate=true WHERE uuid=@uuid";
                await cmd.ExecuteNonQueryAsync();
            }

            //Actually add the item to the collection and set the collection's last updated to now.
            cmd.CommandText = "INSERT INTO collection_entries VALUES (@uuid, @collectionID) ON CONFLICT (uuid, collection_id) DO NOTHING; UPDATE collections SET last_updated = now() WHERE id=@collectionID";
            await cmd.ExecuteNonQueryAsync();
        }
        catch (NpgsqlException e)
        {
            L.LogException(e);
        }
        finally
        {
            await localConn.CloseAsync();
        }
    }

    ///<summary>Remove a single item from a collection.</summary>
    ///<param name="uuid">The uuid of the item to remove.</param>
    ///<param name="collectionID">ID of the collection to remove from.</param>
    public static void RemoveFromCollection(Guid uuid, int collectionID)
    {
        try
        {
            Open();
            using NpgsqlCommand cmd = new("DELETE FROM collection_entries WHERE collection_id=@collectionID AND uuid=@uuid", connection);
            cmd.Parameters.AddWithValue("@collectionID", collectionID);
            cmd.Parameters.AddWithValue("@uuid", uuid);
            cmd.ExecuteNonQuery();

            cmd.CommandText = "UPDATE collections SET last_updated = now() WHERE id=@collectionID";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "UPDATE media SET separate = false WHERE uuid=@uuid AND separate = true";
            cmd.ExecuteNonQuery();
        }
        catch (NpgsqlException e)
        {
            L.LogException(e);
        }
        finally
        {
            Close();
        }
    }

    ///<summary>Load all the albums and/or folders in the collections table.</summary>
    ///<param name="showAlbums">Should albums be selected?</param>
    ///<param name="showFolders">Should folders be selected?</param>
    ///<param name="showReadonly">Should readonly collections be selected?</param>
    ///<param name="mode">How should the data be sorted?</param>
    ///<returns>A List&lt;Collection&gt; of all the albums and/or folders.</returns>
    public static List<Collection> GetCollectionsTable(bool showAlbums, bool showFolders, bool showReadonly, CMSortMode mode = CMSortMode.Title)
    {
        List<Collection> collections = new();

        string orderBy = mode switch
        {
            CMSortMode.Title => "name ASC",
            CMSortMode.TitleReversed => "name DESC",
            CMSortMode.LastModified => "last_updated DESC",
            CMSortMode.LastModifiedReversed => "last_updated ASC",
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
        
        if (!String.IsNullOrEmpty(where) && !showReadonly) where += " AND readonly = false";

        try
        {
            Open();
            using NpgsqlCommand cmd = new($"SELECT id, name, cover, last_updated FROM collections {where} ORDER BY {orderBy}", connection);
            using NpgsqlDataReader r = cmd.ExecuteReader();
            while (r.Read()) collections.Add(new Collection(r.GetInt32(0), r.GetString(1), r.IsDBNull(2) ? String.Empty : r.GetString(2), r.GetDateTime(3))); //https://stackoverflow.com/a/38930847
            r.Close();
        }
        catch (NpgsqlException e)
        {
            L.LogException(e);
        }
        finally
        {
            Close();
        }

        return collections;
    }

    ///<summary>Returns a List&lt;Collection&gt; of all the Collections this uuid is in.</summary>
    ///<param name="uuid">Uuid of the item.</param>
    public static List<Collection> GetCollectionsContaining(Guid uuid)
    {
        List<Collection> collections = new();

        try
        {
            Open();
            using NpgsqlCommand cmd = new("SELECT id, name, cover FROM collections AS c INNER JOIN collection_entries AS e ON c.id=e.collection_id WHERE uuid=@uuid ORDER BY name ASC", connection);
            cmd.Parameters.AddWithValue("@uuid", uuid);
            using NpgsqlDataReader r = cmd.ExecuteReader();
            while (r.Read()) collections.Add(new Collection(r.GetInt32(0), r.GetString(1), r.IsDBNull(2) ? String.Empty : r.GetString(2)));
            r.Close();
        }
        catch (NpgsqlException e)
        {
            L.LogException(e);
        }
        finally
        {
            Close();
        }
        
        return collections;
    }

    //https://www.postgresqltutorial.com/postgresql-update-join/
    ///<summary>Change a collection to a folder or vice versa.</summary>
    ///<param name="collectionID">ID of album or folder to change into folder or album.</param>
    ///<param name="folder">Specify true if want to change album -> folder. False for folder -> album</param>
    public static void ChangeCollectionType(int collectionID, bool folder)
    {
        try
        {
            Open();
            using NpgsqlCommand cmd = new("UPDATE media SET separate=@folder FROM collection_entries WHERE collection_id=@collectionID AND collection_entries.uuid=media.uuid", connection);
            cmd.Parameters.AddWithValue("@folder", folder);
            cmd.Parameters.AddWithValue("@collectionID", collectionID);
            cmd.ExecuteNonQuery();

            cmd.CommandText = "UPDATE collections SET folder=@folder WHERE id=@collectionID";
            cmd.ExecuteNonQuery();
        }
        catch (NpgsqlException e)
        {
            L.LogException(e);
        }
        finally
        {
            Close();
        }
    }
    
    ///Returns true if a Collection is a folder, false otherwise.
    public static bool IsFolder(int collectionID)
    {
        bool isFolder = false;
        try
        {
            Open();
            using NpgsqlCommand cmd = new("SELECT folder FROM collections WHERE id=@collectionID", connection);
            cmd.Parameters.AddWithValue("@collectionID", collectionID);
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
            L.LogException(e);
        }
        finally
        {
            Close();
        }

        return isFolder;
    }
    
    ///Returns true if a Collection is a folder, false otherwise.
    public static async Task<bool> IsFolderAsync(int collectionID)
    {
        NpgsqlConnection localConn = await CreateLocalConnectionAsync();
        bool isFolder = false;
        
        try
        {
            await using NpgsqlCommand cmd = new("SELECT folder FROM collections WHERE id=@collectionID", localConn);
            cmd.Parameters.AddWithValue("@collectionID", collectionID);
            await using NpgsqlDataReader r = cmd.ExecuteReader();
            if (r.HasRows)
            {
                r.Read();
                isFolder = r.GetBoolean(0);
                await r.CloseAsync();
            }
        }
        catch (NpgsqlException e)
        {
            L.LogException(e);
        }
        finally
        {
            await localConn.CloseAsync();
        }

        return isFolder;
    }
    
    #endregion

    #region Utilities

    ///<summary>Generates a short path (DB path) given a Date Taken and filename. A DB path looks like this: 2022/5/filename.jpg.</summary>
    ///<param name="dateTaken">The date taken of the item.</param>
    ///<param name="filename">The filename and extension of the item.</param>
    ///<returns>A short/DB path for the item.</returns>
    ///<remarks>If date taken is null, the returned path's format is 'Unknown/filename.jpg'. If date taken is not null, the returned path's format is like 2022/5/filename.jpg.</remarks>
    public static string CreateShortPath(DateTime? dateTaken, string filename) => dateTaken == null ? $"Unknown/{filename}" : $"{dateTaken.Value.Year}/{dateTaken.Value.Month}/{filename}";

    ///<summary>Create the full folder path to where an item with this date taken would get moved to in the MM library.</summary>
    ///<param name="dateTaken">The date taken to use for creating the path.</param>
    ///<returns>The full date folder path.</returns>
    public static string CreateFullDateFolderPath(DateTime? dateTaken) => Path.Combine(S.libFolderPath, dateTaken == null ? "Unknown" : $"{dateTaken.Value.Year}/{dateTaken.Value.Month}");

    ///<summary>Given a Date Taken and a filename, create the full path to where the item would get moved to in the MM library.</summary>
    ///<param name="dateTaken">The date taken to use for creating the path.</param>
    ///<param name="filename">The filename and extension of the item.</param>
    ///<returns>The full path to where the item would get moved to in the MM library.</returns>
    public static string CreateFullPath(DateTime? dateTaken, string filename) => Path.Combine(S.libFolderPath, CreateShortPath(dateTaken, filename));

    #endregion
}