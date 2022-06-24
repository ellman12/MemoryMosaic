namespace PSS.Backend
{
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
                using NpgsqlCommand cmd = new("SELECT path FROM media WHERE date_deleted IS NULL", C.connection);
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
                using NpgsqlCommand cmd = new("SELECT path FROM media WHERE date_deleted IS NOT NULL", C.connection);
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
                C.Close();
            }
        }

        ///<summary>Returns how many albums you have.</summary>
        ///<returns>The number of albums you have. 0 if none. -1 if error occured.</returns>
        public static long CountAlbums()
        {
            long rows = 0;
            try
            {
                C.Open();
                using NpgsqlCommand cmd = new("SELECT id FROM albums", C.connection);
                cmd.ExecuteNonQuery();
                using NpgsqlDataReader r = cmd.ExecuteReader();
                while (r.Read()) rows++;
                return rows;
            }
            catch (Exception e)
            {
                Console.WriteLine("Counting albums rows error. " + e.Message);
                return -1;
            }
            finally
            {
                C.Close();
            }
        }

        ///<summary>Attempts to find the item with the oldest not-null date taken.</summary>
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine("Counting albums rows error. " + e.Message);
            }
            finally
            {
                C.Close();
            }

            return rows;
        }

        public static C.MediaRow FindOldestItem()
        {
            C.MediaRow oldestItem = new("", DateTime.Now, DateTime.Now, false, Guid.Empty);
            
            try
            {
                C.Open();
                NpgsqlCommand cmd = new("SELECT path, date_taken, date_added, starred, uuid FROM media ORDER BY DATE_TAKEN ASC", C.connection);
                NpgsqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read(); //There should only be 1 line to read.
                    oldestItem = new C.MediaRow(reader.GetString(0), reader.GetDateTime(1), reader.GetDateTime(2), reader.GetBoolean(3), reader.GetGuid(4));
                    reader.Close();
                }
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine("FindOldestItem error. " + e.Message);
            }
            finally
            {
                C.Close();
            }

            return oldestItem;
        }
        
        public static C.MediaRow FindNewestItem()
        {
            C.MediaRow oldestItem = new("", DateTime.Now, DateTime.Now, false, Guid.Empty);
            
            try
            {
                C.Open();
                NpgsqlCommand cmd = new("SELECT path, date_taken, date_added, starred, uuid FROM media ORDER BY DATE_TAKEN DESC", C.connection);
                NpgsqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read(); //There should only be 1 line to read.
                    oldestItem = new C.MediaRow(reader.GetString(0), reader.GetDateTime(1), reader.GetDateTime(2), reader.GetBoolean(3), reader.GetGuid(4));
                    reader.Close();
                }
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine("FindOldestItem error. " + e.Message);
            }
            finally
            {
                C.Close();
            }

            return oldestItem;
        }
    }
}