namespace MemoryMosaic.Backend;

///Static functions for getting statistical data about the library.
public static class Stats
{
    ///<summary>Returns how many items are in your library (not including the trash).</summary>
    ///<returns>The number of items in your library NOT in the trash. 0 if no items in library. -1 if an error occurred.</returns>
    public static int GetNumItemsInLibrary()
    {
        int rows = 0;
        try
        {
            D.Open();
            using NpgsqlCommand cmd = new("SELECT path FROM library WHERE date_deleted IS NULL", D.connection);
            using NpgsqlDataReader r = cmd.ExecuteReader();
            while (r.Read())
                rows++;
            return rows;
        }
        catch (Exception e)
        {
            L.LogException(e);
            return -1;
        }
        finally
        {
            D.Close();
        }
    }

    ///<summary>Returns how many items are in the trash.</summary>
    ///<returns>The number of items in the trash. 0 if no items in trash. -1 if an error occurred.</returns>
    public static int GetNumItemsInTrash()
    {
        int rows = 0;
        try
        {
            D.Open();
            using NpgsqlCommand cmd = new("SELECT path FROM library WHERE date_deleted IS NOT NULL", D.connection);
            using NpgsqlDataReader r = cmd.ExecuteReader();
            while (r.Read())
                rows++;
            return rows;
        }
        catch (Exception e)
        {
            L.LogException(e);
            return -1;
        }
        finally
        {
            D.Close();
        }
    }

    ///<summary>Returns how many collections you have.</summary>
    ///<returns>The number of collections you have. 0 if none. -1 if error occured.</returns>
    public static int CountCollections()
    {
        int rows = 0;
        try
        {
            D.Open();
            using NpgsqlCommand cmd = new("SELECT id FROM collections", D.connection);
            using NpgsqlDataReader r = cmd.ExecuteReader();
            while (r.Read())
                rows++;
            return rows;
        }
        catch (Exception e)
        {
            L.LogException(e);
            return -1;
        }
        finally
        {
            D.Close();
        }
    }

    private static async Task<LibraryItem?> FindItemAsync(string filter)
    {
        LibraryItem? item = null;
        
        try
        {
            await using NpgsqlConnection conn = await D.CreateLocalConnectionAsync();
            NpgsqlCommand cmd = new($"SELECT path, id, date_taken, date_added, starred, description, date_deleted, thumbnail thumbnail FROM library {filter}", conn);
            NpgsqlDataReader r = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);

            if (r.HasRows)
            {
                await r.ReadAsync();
                item = new LibraryItem(r);
                await r.CloseAsync();
            }
        }
        catch (Exception e)
        {
            L.LogException(e);
        }

        return item;
    }

    public static async Task<LibraryItem?> FindItemWithOldestDateTakenAsync() => await FindItemAsync("WHERE date_taken IS NOT NULL ORDER BY date_taken ASC");
    public static async Task<LibraryItem?> FindItemWithNewestDateTakenAsync() => await FindItemAsync("WHERE date_taken IS NOT NULL ORDER BY date_taken DESC");
    public static async Task<LibraryItem?> FindItemWithOldestDateAddedAsync() => await FindItemAsync("ORDER BY date_added ASC");
    public static async Task<LibraryItem?> FindItemWithNewestDateAddedAsync() => await FindItemAsync("ORDER BY date_added DESC");
}