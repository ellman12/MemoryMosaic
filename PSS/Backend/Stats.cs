using System;
using Npgsql;

namespace PSS.Backend
{
    public static class Stats
    {
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