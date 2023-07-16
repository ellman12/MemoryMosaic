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
            Connection.Open();
            using NpgsqlCommand cmd = new("SELECT path FROM media WHERE date_deleted IS NULL", Connection.connection);
            cmd.ExecuteNonQuery();
            using NpgsqlDataReader r = cmd.ExecuteReader();
            while (r.Read()) rows++;
            return rows;
        }
        catch (Exception e)
        {
            Console.WriteLine("Counting media rows error. " + e.Message);
            return -1;
        }
        finally
        {
            Connection.Close();
        }
    }

    ///<summary>Returns how many items are in the trash.</summary>
    ///<returns>The number of items in the trash. 0 if no items in trash. -1 if an error occurred.</returns>
    public static long GetNumItemsInTrash()
    {
        long rows = 0;
        try
        {
            Connection.Open();
            using NpgsqlCommand cmd = new("SELECT path FROM media WHERE date_deleted IS NOT NULL", Connection.connection);
            cmd.ExecuteNonQuery();
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
            Connection.Close();
        }
    }

    ///<summary>Returns how many collections you have.</summary>
    ///<returns>The number of collections you have. 0 if none. -1 if error occured.</returns>
    public static long CountCollections()
    {
        long rows = 0;
        try
        {
            Connection.Open();
            using NpgsqlCommand cmd = new("SELECT id FROM collections", Connection.connection);
            cmd.ExecuteNonQuery();
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
            Connection.Close();
        }
    }

    ///<summary>Attempts to find the item with the oldest not-null date taken.</summary>
    ///<returns>The row with the oldest date taken, otherwise a MediaRow with Guid.Empty, and null path/DT.</returns>
    public static MediaRow FindItemWithOldestDateTaken()
    {
        MediaRow oldestItem = new(null, null, DateTime.MinValue, false, Guid.Empty);
            
        try
        {
            Connection.Open();
            NpgsqlCommand cmd = new("SELECT path, date_taken, date_added, starred, uuid FROM media WHERE date_taken IS NOT NULL ORDER BY date_taken ASC", Connection.connection);
            cmd.ExecuteNonQuery();
            NpgsqlDataReader r = cmd.ExecuteReader();

            if (r.HasRows)
            {
                r.Read(); //There should only be 1 row to read.
                oldestItem = new MediaRow(r.GetString(0), r.GetDateTime(1), r.GetDateTime(2), r.GetBoolean(3), r.GetGuid(4));
                r.Close();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("FindItemWithOldestDateTaken error. " + e.Message);
        }
        finally
        {
            Connection.Close();
        }

        return oldestItem;
    }
        
    ///<summary>Attempts to find the item with the newest not-null date taken.</summary>
    ///<returns>The row with the newest date taken, otherwise a MediaRow with Guid.Empty, and null path/DT.</returns>
    public static MediaRow FindItemWithNewestDateTaken()
    {
        MediaRow newestItem = new(null, null, DateTime.MinValue, false, Guid.Empty);
            
        try
        {
            Connection.Open();
            using NpgsqlCommand cmd = new("SELECT path, date_taken, date_added, starred, uuid FROM media WHERE date_taken IS NOT NULL ORDER BY date_taken DESC", Connection.connection);
            cmd.ExecuteNonQuery();
            using NpgsqlDataReader r = cmd.ExecuteReader();

            if (r.HasRows)
            {
                r.Read(); //There should only be 1 row to read.
                newestItem = new MediaRow(r.GetString(0), r.GetDateTime(1), r.GetDateTime(2), r.GetBoolean(3), r.GetGuid(4));
                r.Close();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("FindItemWithNewestDateTaken error. " + e.Message);
        }
        finally
        {
            Connection.Close();
        }

        return newestItem;
    }

    ///<summary>Attempts to find the item with the oldest date added, which may or may not have a date taken.</summary>
    ///<returns>The row with the oldest date added, otherwise a MediaRow with Guid.Empty, and null path/DT.</returns>
    public static MediaRow FindItemWithOldestDateAdded()
    {
        MediaRow newestItem = new(null, null, DateTime.MinValue, false, Guid.Empty);
            
        try
        {
            Connection.Open();
            using NpgsqlCommand cmd = new("SELECT path, date_taken, date_added, starred, uuid FROM media ORDER BY date_added ASC", Connection.connection);
            cmd.ExecuteNonQuery();
            using NpgsqlDataReader r = cmd.ExecuteReader();

            if (r.HasRows)
            {
                r.Read(); //There should only be 1 row to read.
                newestItem = new MediaRow(r.GetString(0), r.IsDBNull(1) ? null : r.GetDateTime(1), r.GetDateTime(2), r.GetBoolean(3), r.GetGuid(4));
                r.Close();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("FindItemWithOldestDateAdded error. " + e.Message);
        }
        finally
        {
            Connection.Close();
        }

        return newestItem;
    }
        
    ///<summary>Attempts to find the item with the newest date added, which may or may not have a date taken.</summary>
    ///<returns>The row with the newest date added, otherwise a MediaRow with Guid.Empty, and null path/DT.</returns>
    public static MediaRow FindItemWithNewestDateAdded()
    {
        MediaRow newestItem = new(null, null, DateTime.MinValue, false, Guid.Empty);
            
        try
        {
            Connection.Open();
            using NpgsqlCommand cmd = new("SELECT path, date_taken, date_added, starred, uuid FROM media ORDER BY date_added DESC", Connection.connection);
            cmd.ExecuteNonQuery();
            using NpgsqlDataReader r = cmd.ExecuteReader();

            if (r.HasRows)
            {
                r.Read(); //There should only be 1 row to read.
                newestItem = new MediaRow(r.GetString(0), r.IsDBNull(1) ? null : r.GetDateTime(1), r.GetDateTime(2), r.GetBoolean(3), r.GetGuid(4));
                r.Close();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("FindItemWithNewestDateAdded error. " + e.Message);
        }
        finally
        {
            Connection.Close();
        }

        return newestItem;
    }
}