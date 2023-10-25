namespace MemoryMosaic.Backend;

///Static functions for getting statistical data about the library.
public static class Stats
{
    ///<summary>Returns how many items are in your library (not including the trash).</summary>
    ///<returns>The number of items in your library NOT in the trash. 0 if no items in library. -1 if an error occurred.</returns>
    public static long GetNumItemsInLibrary()
    {
        long rows = 0;
        try
        {
            C.Open();
            using NpgsqlCommand cmd = new("SELECT path FROM library WHERE date_deleted IS NULL", C.connection);
            using NpgsqlDataReader r = cmd.ExecuteReader();
            while (r.Read()) rows++;
            return rows;
        }
        catch (Exception e)
        {
            Console.WriteLine("Counting library rows error. " + e.Message);
            return -1;
        }
        finally
        {
            C.Close();
        }
    }

    ///<summary>Returns how many items are in the trash.</summary>
    ///<returns>The number of items in the trash. 0 if no items in trash. -1 if an error occurred.</returns>
    public static long GetNumItemsInTrash()
    {
        long rows = 0;
        try
        {
            C.Open();
            using NpgsqlCommand cmd = new("SELECT path FROM library WHERE date_deleted IS NOT NULL", C.connection);
            using NpgsqlDataReader r = cmd.ExecuteReader();
            while (r.Read()) rows++;
            return rows;
        }
        catch (Exception e)
        {
            Console.WriteLine("Counting trash rows error. " + e.Message);
            return -1;
        }
        finally
        {
            C.Close();
        }
    }

    ///<summary>Returns how many collections you have.</summary>
    ///<returns>The number of collections you have. 0 if none. -1 if error occured.</returns>
    public static long CountCollections()
    {
        long rows = 0;
        try
        {
            C.Open();
            using NpgsqlCommand cmd = new("SELECT id FROM collections", C.connection);
            using NpgsqlDataReader r = cmd.ExecuteReader();
            while (r.Read()) rows++;
            return rows;
        }
        catch (Exception e)
        {
            Console.WriteLine("Counting collections rows error. " + e.Message);
            return -1;
        }
        finally
        {
            C.Close();
        }
    }

    private static async Task<LibraryItem?> FindItemAsync(string filter)
    {
        LibraryItem? item = null;
        
        try
        {
            await using NpgsqlConnection conn = await C.CreateLocalConnectionAsync();
            NpgsqlCommand cmd = new($"SELECT path, date_taken, date_added, uuid, thumbnail FROM library {filter}", conn);
            NpgsqlDataReader r = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);

            if (r.HasRows)
            {
                await r.ReadAsync();
                item = new LibraryItem(r.GetString(0), r.GetDateTime(1), r.GetDateTime(2), false, r.GetGuid(3), r.GetString(4), null);
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