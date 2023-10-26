namespace MemoryMosaic.Backend;

///Functions for server/library maintenance.
public static class Maintenance
{
    ///<summary>
    ///<para>Return if a folder is empty (has 0 files in folder or subfolders). Subfolders inside path aren't counted unless they have stuff inside.</para>
    ///<para>If root/ has 2 subfolders and 3 files it would only find 3 files and thus it wouldn't be counted as empty.</para>
    ///<para>If root/ has 2 subfolders and 0 files, it would be considered empty.</para>
    ///</summary>
    ///<returns>True if empty, false otherwise.</returns>
    public static bool IsFolderEmpty(string path) => Directory.GetFiles(path, "*", SearchOption.AllDirectories).Length == 0;

    ///Search mm_library and if an item is not in the library table, add it to the List of full paths that is returned. 
    public static List<string> GetUntrackedFiles()
    {
        List<string> untrackedPaths = new(); //Tracks items in mm_library but not in database
        HashSet<string> libraryPaths = C.LoadEntireLibraryTable().Select(item => item.Path).ToHashSet();

        foreach (string fullPath in Directory.GetFiles(S.libFolderPath, "*", SearchOption.AllDirectories))
        {
            string shortPath = fullPath.Replace(S.libFolderPath, null).Replace('\\', '/');
            if (shortPath.StartsWith('/')) shortPath = shortPath[1..]; //Database short paths don't ever start with '/'.
                
            if (!libraryPaths.Contains(shortPath))
                untrackedPaths.Add(fullPath);
        }
        return untrackedPaths;
    }

    ///Returns a List&lt;LibraryItem&gt; of all rows and columns from the library table that don't have existing files in mm_library.
    public static List<LibraryItem> GetMissingFiles()
    {
        List<LibraryItem> missingFiles = new();
        try
        {
            C.Open();
            using NpgsqlCommand cmd = new("SELECT path, date_taken, date_added, starred, uuid, thumbnail, description, date_deleted FROM library", C.connection);
            using NpgsqlDataReader r = cmd.ExecuteReader();

            while (r.Read())
            {
                string shortPath = r.GetString(0);
                string fullPath = Path.Combine(S.libFolderPath, shortPath);
                if (!File.Exists(fullPath))
                    missingFiles.Add(new LibraryItem(shortPath, r.IsDBNull(1) ? null : r.GetDateTime(1), r.GetDateTime(2), r.GetBoolean(3), r.GetGuid(4), r.GetString(5), r.IsDBNull(6) ? null : r.GetString(6), r.IsDBNull(7) ? null : r.GetDateTime(7)));
            }
            r.Close();
        }
        catch (NpgsqlException e)
        {
            L.LogException(e);
        }
        finally
        {
            C.Close();
        }

        return missingFiles;
    }

    ///<summary>Delete these items FROM library table that are in the DB but don't exist as files.</summary>
    ///<param name="rows">List&lt;LibraryItem&gt; retrieved with GetMissingFiles()</param>
    public static void RemoveMissingFiles(List<LibraryItem> rows)
    {
        try
        {
            C.Open();
            foreach (LibraryItem row in rows)
            {
                using NpgsqlCommand cmd = new("DELETE FROM library WHERE path=@path", C.connection);
                cmd.Parameters.AddWithValue("@path", row.Path);
                cmd.ExecuteNonQuery();
            }
        }
        catch (NpgsqlException e)
        {
            L.LogException(e);
        }
        finally
        {
            C.Close();
        }
    }
}