namespace MemoryMosaic.Backend;

///Contains static methods for interacting with the MM PostgreSQL database.
public static class Database
{
#if DEBUG
	private const string CONNECTION_STRING = "Host=localhost; Port=5432; User Id=postgres; Password=Ph0t0s_Server; Database=MemoryMosaicDebug";
#else
    private const string CONNECTION_STRING = "Host=localhost; Port=5432; User Id=postgres; Password=Ph0t0s_Server; Database=MemoryMosaic";
#endif

	public static readonly NpgsqlConnection connection = new(CONNECTION_STRING);

	///Creates, opens, and returns a new connection object.
	public static NpgsqlConnection CreateLocalConnection()
	{
		NpgsqlConnection localConn = new(CONNECTION_STRING);
		localConn.Open();
		return localConn;
	}

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

	#region Library

	///<summary>For inserting an item into the library table.</summary>
	///<param name="item">The item to insert into the library.</param>
	public static async Task InsertItem(ImportItem item)
	{
		NpgsqlConnection localConn = await CreateLocalConnectionAsync();

		try
		{
			await using NpgsqlCommand cmd = new("", localConn);

			string columns = "path, id, separate, starred, thumbnail ";
			string values = "@path, @id, @separate, @starred, @thumbnail ";

			if (item.SelectedDateTaken != null)
			{
				columns += ", date_taken";
				values += ", @dateTaken";
				cmd.Parameters.AddWithValue("@dateTaken", item.SelectedDateTaken);
			}

			if (!String.IsNullOrWhiteSpace(item.Description))
			{
				columns += ", description";
				values += ", @description";
				cmd.Parameters.AddWithValue("@description", item.Description);
			}

			cmd.Parameters.AddWithValue("@path", item.DestinationPath);
			cmd.Parameters.AddWithValue("@id", item.Id);
			cmd.Parameters.AddWithValue("@separate", item.Collections?.All(collection => collection.Folder) ?? false);
			cmd.Parameters.AddWithValue("@starred", item.Starred);
			cmd.Parameters.AddWithValue("@thumbnail", item.Thumbnail);

			cmd.CommandText = $"INSERT INTO library ({columns}) VALUES ({values})";
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
			string filename = P.GetFileName(shortPath);
			string originalFullPath = P.Combine(S.LibFolderPath, shortPath);

			string newShortPath = CreateShortPath(newDateTaken, filename);
			string newDTFolderPath = CreateFullDateFolderPath(newDateTaken);
			string newFullPath = P.Combine(newDTFolderPath, filename);

			GC.Collect();
			GC.WaitForPendingFinalizers();
			
			Directory.CreateDirectory(newDTFolderPath);
			File.Move(originalFullPath, newFullPath);
			DTE.UpdateDateTaken(newFullPath, newDateTaken);

			Open();
			using NpgsqlCommand cmd = new("", connection);
			if (newDateTaken == null)
			{
				cmd.CommandText = "UPDATE library SET path = @newPath, date_taken = NULL WHERE path = @shortPath";
			}
			else
			{
				cmd.CommandText = "UPDATE library SET path = @newPath, date_taken = @newDateTaken WHERE path = @shortPath";
				cmd.Parameters.AddWithValue("@newDateTaken", newDateTaken);
			}

			//Update date taken and short path in library.
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

	///<summary>Used in FullscreenViewer for renaming the current item's file.</summary>
	///<param name="oldShortPath">The original short path of the item.</param>
	///<param name="newFilename">The new filename of the item.</param>
	///<param name="ext">The file extension.</param>
	///<param name="dateTaken">The date taken of the item.</param>
	///<returns>The new short path (DB path) of this item. null if DB error occurred, which means there is already a file with the same name in that location.</returns>
	public static string? RenameFile(string oldShortPath, string newFilename, string ext, DateTime? dateTaken)
	{
		try
		{
			string originalFullPath = P.Combine(S.LibFolderPath, oldShortPath);
			string newShortPath = CreateShortPath(dateTaken, newFilename + ext);
			string newFullPath = P.Combine(S.LibFolderPath, newShortPath);

			if (File.Exists(newFullPath))
				return null;
			else
				File.Move(originalFullPath, newFullPath);

			Open();
			using NpgsqlCommand cmd = new("UPDATE library SET path = @newShortPath WHERE path = @oldShortPath", connection);
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

	///<summary>Loads every row in the library table, even if has no DT, in a folder, in the trash, etc. Sorted by date_taken descending (NULL and newest DT first).</summary>
	///<returns>IEnumerable&lt;LibraryItem&gt; of EVERY row in the library table.</returns>
	public static IEnumerable<LibraryItem> GetEntireLibrary()
	{
		Open();
		using NpgsqlCommand cmd = new("SELECT path, id, date_taken, date_added, starred, description, date_deleted, thumbnail FROM library ORDER BY date_taken DESC", connection);
		using NpgsqlDataReader r = cmd.ExecuteReader();

		while (r.Read())
			yield return new LibraryItem(r);

		r.Close();
		Close();
	}

	///<summary>Sets the description of an item.</summary>
	///<param name="id">The id of the item.</param>
	///<param name="newDescription">The new description of the item.</param>
	public static void UpdateDescription(Guid id, string? newDescription)
	{
		try
		{
			Open();
			using NpgsqlCommand cmd = new(null, connection);
			if (String.IsNullOrWhiteSpace(newDescription))
			{
				cmd.CommandText = "UPDATE library SET description = NULL WHERE id = @id";
			}
			else
			{
				cmd.CommandText = "UPDATE library SET description = @newDescription WHERE id = @id";
				cmd.Parameters.AddWithValue("@newDescription", newDescription);
			}
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

	#region Trash

	///Set this library item's date_deleted to the current date and time.
	public static void MoveToTrash(Guid id)
	{
		try
		{
			Open();
			using NpgsqlCommand cmd = new("UPDATE library SET date_deleted = now() WHERE id = @id", connection);
			cmd.Parameters.AddWithValue("id", id);
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
	public static void MoveToTrash(IEnumerable<Guid> ids)
	{
		foreach (Guid id in ids)
			MoveToTrash(id);
	}

	///PERMANENTLY remove an item from the database and DELETES the file from disk.
	public static void RemoveFromTrash(LibraryItem item)
	{
		try
		{
			FileSystem.DeleteFile(item.FullPath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
		}
		catch (FileNotFoundException e)
		{
			L.LogException(e);
		}

		try
		{
			Open();

			using NpgsqlCommand cmd = new("DELETE FROM library WHERE id = @id AND date_deleted IS NOT NULL", connection);
			cmd.Parameters.AddWithValue("@id", item.Id);
			cmd.ExecuteNonQuery();

			cmd.CommandText = "DELETE FROM collection_entries WHERE item_id = @id";
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
	public static void RemoveFromTrash(IEnumerable<LibraryItem> items)
	{
		foreach (LibraryItem item in items)
			RemoveFromTrash(item);
	}

	///PERMANENTLY removes all items in Trash from server and database.
	public static void EmptyTrash()
	{
		try
		{
			Open();
			using NpgsqlCommand cmd = new("SELECT path FROM library WHERE date_deleted IS NOT NULL", connection);
			using NpgsqlDataReader r = cmd.ExecuteReader();

			while (r.Read())
			{
				try { FileSystem.DeleteFile(P.Combine(S.LibFolderPath, r.GetString(0)), UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin); }
				catch (IOException e) { L.LogException(e); }
			}

			r.Close();
			cmd.CommandText = "DELETE FROM library WHERE date_deleted IS NOT NULL";
			cmd.ExecuteNonQuery();
		}
		catch (NpgsqlException e) { L.LogException(e); }
		finally { Close(); }
	}

	///Clears an item's date_deleted field, removing it from the Trash and restoring it back into the library. Also restores the collections it was previously in.
	public static void RestoreItem(Guid id)
	{
		try
		{
			Open();

			using NpgsqlCommand cmd = new("UPDATE library SET date_deleted = NULL WHERE id = @id", connection);
			cmd.Parameters.AddWithValue("@id", id);
			cmd.ExecuteNonQuery();
		}
		catch (NpgsqlException e) { L.LogException(e); }
		finally { Close(); }
	}

	///Clears the date_deleted field for each item in the IEnumerable&lt;Guid&gt;, restoring it back into the library.
	public static void RestoreItems(IEnumerable<Guid> ids)
	{
		foreach (Guid id in ids)
			RestoreItem(id);
	}

	///Restores EVERY item in the Trash back into library.
	public static void RestoreTrash()
	{
		try
		{
			Open();
			using NpgsqlCommand cmd = new("UPDATE library SET date_deleted = NULL WHERE date_deleted IS NOT NULL", connection);
			cmd.ExecuteNonQuery();
		}
		catch (NpgsqlException e) { L.LogException(e); }
		finally { Close(); }
	}

	#endregion

	#region Starred

	///<summary>Change a single item from either starred (true) or not starred.</summary>
	public static void UpdateStarred(Guid id, bool starred)
	{
		try
		{
			Open();
			using NpgsqlCommand cmd = new("UPDATE library SET starred = @starred WHERE id = @id", connection);
			cmd.Parameters.AddWithValue("@starred", starred);
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

	///<summary>Change an IEnumerable of items from either starred (true) or not starred.</summary>
	public static void UpdateStarred(IEnumerable<Guid> ids, bool starred)
	{
		foreach (Guid id in ids)
			UpdateStarred(id, starred);
	}

	#endregion

	#endregion

	#region Collections

	///<summary>Create a new Collection and add it to the `collections` table.</summary>
	///<param name="name">The name for the new collection.</param>
	///<param name="isFolder">True if this collection should be a folder. False by default.</param>
	///<returns>True if successfully created collection, false if collection couldn't be created (e.g., because duplicate name).</returns>
	///<remarks>The name column in the collection table requires all values to be unique.</remarks>
	public static bool CreateCollection(string name, bool isFolder)
	{
		try
		{
			Open();
			using NpgsqlCommand cmd = new("INSERT INTO collections (name, folder) VALUES (@name, @isFolder)", connection);
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
			using NpgsqlCommand cmd = new("UPDATE collections SET name = @newName WHERE id = @id", connection);
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
			using NpgsqlCommand cmd = new("UPDATE collections SET cover = @path WHERE id = @collectionID; UPDATE collections SET last_modified = now() WHERE id = @collectionID", connection);
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

	public static void UpdateCollectionDescription(Collection collection)
	{
		bool clear = String.IsNullOrWhiteSpace(collection.Description);

		try
		{
			Open();
			using NpgsqlCommand cmd = new($"UPDATE collections SET description = {(clear ? "NULL" : "@description")} WHERE id = @collectionID", connection);

			if (!clear)
				cmd.Parameters.AddWithValue("@description", collection.Description!);

			cmd.Parameters.AddWithValue("@collectionID", collection.Id);
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
	///<param name="collectionID"></param>
	public static async Task<Collection?> GetCollectionDetailsAsync(string collectionID)
	{
		NpgsqlConnection localConn = await CreateLocalConnectionAsync();

		try
		{
			await using NpgsqlCommand cmd = new($"SELECT name, description, folder, readonly, last_modified FROM collections WHERE id = {collectionID}", localConn);
			await using NpgsqlDataReader r = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);

			if (!r.HasRows) return null;

			await r.ReadAsync();
			return new Collection(Int32.Parse(collectionID), r.GetString(0), r.TryGetString(1), r.GetBoolean(2), r.GetBoolean(3), r.GetDateTime(4));
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
			await using NpgsqlCommand cmd = new($"UPDATE collections SET readonly = {!collection.ReadOnly} WHERE id = {collection.Id}", localConn);
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

	///<summary>Deletes the collection with the given ID, and remove all items in this collection from collection_entries. THIS CANNOT BE UNDONE! This also does not delete the path from the library table, so you can safely delete a collection without losing the actual photos and videos.</summary>
	///<param name="collectionID">The id of the collection to delete.</param>
	public static void DeleteCollection(int collectionID)
	{
		try
		{
			Open();

			//Set all items to no longer being separate (only matters if this was a folder). If don't do this they won't appear in main library.
			NpgsqlCommand cmd = new("UPDATE library SET separate = false FROM collection_entries WHERE collection_id = @collectionID AND collection_entries.item_id = library.id", connection);
			cmd.Parameters.AddWithValue("@collectionID", collectionID);
			cmd.ExecuteNonQuery();

			//Removing the row for this collection in collections table automatically removes any rows in collection_entries referencing this collection.
			cmd = new NpgsqlCommand("DELETE FROM collections WHERE id = @collectionID", connection);
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
	///<param name="collectionID">The ID of the collection to add the item to.</param>
	///<param name="itemId">The id of the item.</param>
	public static async Task AddToCollectionAsync(int collectionID, Guid itemId)
	{
		NpgsqlConnection localConn = await CreateLocalConnectionAsync();
		bool isFolder = await IsFolderAsync(collectionID);

		try
		{
			await using NpgsqlCommand cmd = new("", localConn);
			cmd.Parameters.AddWithValue("@collectionID", collectionID);
			cmd.Parameters.AddWithValue("@itemId", itemId);

			if (isFolder)
			{
				//If an item is being added to a folder it can only be in 1 folder and 0 albums so remove from everywhere else first. Then, mark the item as in a folder (separate).
				cmd.CommandText = "DELETE FROM collection_entries WHERE item_id = @itemId; UPDATE library SET separate = true WHERE id = @itemId";
				await cmd.ExecuteNonQueryAsync();
			}

			//Actually add the item to the collection and set the collection's last updated to now.
			cmd.CommandText = "INSERT INTO collection_entries VALUES (@collectionID, @itemId) ON CONFLICT (collection_id, item_id) DO NOTHING; UPDATE collections SET last_modified = now() WHERE id = @collectionID";
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
	///<param name="id">The id of the item to remove.</param>
	///<param name="collectionID">ID of the collection to remove from.</param>
	public static void RemoveFromCollection(Guid id, int collectionID)
	{
		try
		{
			Open();
			using NpgsqlCommand cmd = new("DELETE FROM collection_entries WHERE collection_id = @collectionID AND item_id = @id", connection);
			cmd.Parameters.AddWithValue("@collectionID", collectionID);
			cmd.Parameters.AddWithValue("@id", id);
			cmd.ExecuteNonQuery();

			cmd.CommandText = "UPDATE collections SET last_modified = now() WHERE id = @collectionID";
			cmd.ExecuteNonQuery();

			cmd.CommandText = "UPDATE library SET separate = false WHERE id = @id AND separate = true";
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
	public static List<Collection> GetCollections(bool showAlbums, bool showFolders, bool showReadonly, CollectionsSortMode mode = CollectionsSortMode.Title)
	{
		List<Collection> collections = new();

		string orderBy = mode switch
		{
			CollectionsSortMode.Title => "name ASC",
			CollectionsSortMode.TitleReversed => "name DESC",
			CollectionsSortMode.LastModified => "last_modified DESC",
			CollectionsSortMode.LastModifiedReversed => "last_modified ASC",
			CollectionsSortMode.HighestCount => "count DESC",
			CollectionsSortMode.LowestCount => "count ASC",
			CollectionsSortMode.NewestItemFirst => "max_date_taken DESC NULLS LAST",
			CollectionsSortMode.OldestItemFirst => "min_date_taken ASC NULLS LAST",
			_ => "name ASC"
		};

		string where = (showAlbums, showFolders) switch
		{
			(false, false) => "WHERE folder is null",
			(true, false) => "WHERE folder = false",
			(false, true) => "WHERE folder = true",
			_ => ""
		};

		if (where == "")
			where = "WHERE ";
		else
			where += " AND ";

		if (showReadonly)
			where += "readonly is not null";
		else
			where += "readonly = false";

		//LEFT JOIN includes empty collections.
		string query = $"""
						    SELECT c.id, c.name, c.cover, c.description, c.folder, c.readonly, c.last_modified, COUNT(ce.item_id) AS count, MIN(l.date_taken) AS min_date_taken, MAX(l.date_taken) AS max_date_taken
						    FROM collections c
						    LEFT JOIN collection_entries ce ON c.id = ce.collection_id
						    LEFT JOIN library l ON ce.item_id = l.id
						    {where}
						    GROUP BY c.id, c.name, c.cover, c.last_modified
						    ORDER BY {orderBy};
						""";

		try
		{
			Open();
			using NpgsqlCommand cmd = new(query, connection);
			using NpgsqlDataReader r = cmd.ExecuteReader();

			while (r.Read())
				collections.Add(new Collection(r.GetInt32(0), r.GetString(1), r.TryGetString(2), r.TryGetString(3), r.GetBoolean(4), r.GetBoolean(5), r.GetDateTime(6), r.GetInt32(7), r.TryGetDateTime(8), r.TryGetDateTime(9)));

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

	///<summary>Returns a HashSet&lt;Collection&gt; of all the Collections this id is in.</summary>
	///<param name="id">ID of the item.</param>
	public static HashSet<Collection> GetCollectionsContaining(Guid id)
	{
		HashSet<Collection> collections = new();

		try
		{
			Open();
			using NpgsqlCommand cmd = new("SELECT id, name, cover FROM collections AS c INNER JOIN collection_entries AS e ON c.id = e.collection_id WHERE item_id = @id ORDER BY name ASC", connection);
			cmd.Parameters.AddWithValue("@id", id);
			using NpgsqlDataReader r = cmd.ExecuteReader();

			while (r.Read())
				collections.Add(new Collection(r.GetInt32(0), r.GetString(1), r.TryGetString(2)));

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
			using NpgsqlCommand cmd = new("UPDATE library SET separate=@folder FROM collection_entries WHERE collection_id = @collectionID AND collection_entries.item_id = library.id", connection);
			cmd.Parameters.AddWithValue("@folder", folder);
			cmd.Parameters.AddWithValue("@collectionID", collectionID);
			cmd.ExecuteNonQuery();

			cmd.CommandText = "UPDATE collections SET folder = @folder WHERE id = @collectionID";
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
	public static async Task<bool> IsFolderAsync(int collectionID)
	{
		NpgsqlConnection localConn = await CreateLocalConnectionAsync();
		bool isFolder = false;

		try
		{
			await using NpgsqlCommand cmd = new("SELECT folder FROM collections WHERE id = @collectionID", localConn);
			cmd.Parameters.AddWithValue("@collectionID", collectionID);
			await using NpgsqlDataReader r = cmd.ExecuteReader(CommandBehavior.SingleRow);
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
	public static string CreateFullDateFolderPath(DateTime? dateTaken) => P.Combine(S.LibFolderPath, dateTaken == null ? "Unknown" : $"{dateTaken.Value.Year}/{dateTaken.Value.Month}");

	#endregion
}