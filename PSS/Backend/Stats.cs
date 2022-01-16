﻿namespace PSS.Backend
{
    /// <summary>
    /// Static functions for getting statistical data about photo library.
    /// </summary>
    public static class Stats
    {
        /// <summary>
        /// Returns how many rows in media table (and thus how many items in library). 
        /// </summary>
        public static long CountMediaRows()
        {
            long rows = 0;
            try
            {
                C.Open();
                NpgsqlCommand cmd = new("SELECT path FROM media", C.connection);
                cmd.ExecuteNonQuery();
                NpgsqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read()) rows++;
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine("Counting media rows error. " + e.Message);
            }
            finally
            {
                C.Close();
            }

            return rows;
        }

        /// <summary>
        /// Returns number of rows in albums table and thus how many albums there are. 
        /// </summary>
        public static long CountAlbums()
        {
            long rows = 0;
            try
            {
                C.Open();
                NpgsqlCommand cmd = new("SELECT id FROM albums", C.connection);
                cmd.ExecuteNonQuery();
                NpgsqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read()) rows++;
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