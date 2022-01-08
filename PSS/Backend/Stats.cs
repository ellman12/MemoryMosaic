using System;
using Npgsql;

namespace PSS.Backend
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
                Connection.Open();
                NpgsqlCommand cmd = new("SELECT path FROM media", Connection.connection);
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
                Connection.Close();
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
                Connection.Open();
                NpgsqlCommand cmd = new("SELECT id FROM albums", Connection.connection);
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
                Connection.Close();
            }

            return rows;
        }
    }
}