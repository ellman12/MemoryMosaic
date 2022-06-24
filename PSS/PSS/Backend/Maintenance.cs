using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualBasic.FileIO;
using SearchOption = System.IO.SearchOption;

namespace PSS.Backend
{
    ///Functions for server/library maintenance.
    public static class Maintenance
    {
        ///Backup library and database on server.
        public static void BackupServer()
        {
            //Clear backup folder to remove old backup: https://stackoverflow.com/a/12297082
            DirectoryInfo di = new(S.backupFolderPath);
            di.Delete(true);
            Directory.CreateDirectory(S.backupFolderPath); 
            
            FileSystem.CopyDirectory(S.libFolderPath, Path.Combine(S.backupFolderPath, "PSS Media Backup"));
            File.WriteAllText(Path.Combine(S.backupFolderPath, "Backed up on.txt"), DateTime.Now.ToString("M-d-yyyy h:mm:ss tt"));
            
            Process process = new(); //Backup entire database to a file
            ProcessStartInfo startInfo = new()
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = "C:/Program Files/PostgreSQL/14/bin/",
                FileName = "cmd.exe",
                Arguments = $"/C {S.databaseBackupCommand}"
            };
            process.StartInfo = startInfo;
            process.Start(); //How to run this cmd without a password prompt: https://stackoverflow.com/a/62417775
            process.WaitForExit();
        }

        public static void RestoreBackup()
        {
            //Kept getting a stupid error like OP did when trying to do it the normal way but luckily SO comes in to save the day: https://serverfault.com/a/260610
            FileSystem.CopyDirectory(Path.Combine(S.backupFolderPath, "PSS Media Backup"), S.libFolderPath);

            Process process = new(); //Backup entire database to a file
            ProcessStartInfo startInfo = new()
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = S.backupFolderPath,
                FileName = "cmd.exe",
                Arguments = $"/C {S.databaseRestoreCommand}"
            };
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
        }
        
        ///<summary>
        ///<para>Return if a folder is empty (has 0 files in folder or subfolders). Subfolders inside path aren't counted unless they have stuff inside.</para>
        ///<para>If root/ has 2 subfolders and 3 files it would only find 3 files and thus it wouldn't be counted as empty.</para>
        ///<para>If root/ has 2 subfolders and 0 files, it would be considered empty.</para>
        ///</summary>
        ///<returns>True if empty, false otherwise.</returns>
        public static bool IsFolderEmpty(string path) => Directory.GetFiles(path, "*", SearchOption.AllDirectories).Length == 0;

        ///Search pss_library and if an item is not in the media table, add it to the List of full paths that is returned. 
        public static List<string> GetUntrackedLibFiles()
        {
            List<string> untrackedPaths = new(); //Tracks items in pss_library but not in database
            HashSet<string> mediaPaths = C.LoadEntireMediaTable().Select(media => media.path).ToHashSet();

            foreach (string fullPath in Directory.GetFiles(S.libFolderPath, "*", SearchOption.AllDirectories))
            {
                string shortPath = fullPath.Replace(S.libFolderPath, null).Replace('\\', '/');
                if (shortPath.StartsWith('/')) shortPath = shortPath[1..]; //Database short paths don't ever start with '/'.
                
                if (!mediaPaths.Contains(shortPath))
                    untrackedPaths.Add(fullPath);
            }
            return untrackedPaths;
        }

        ///The table to search for missing files.        
        public enum MissingFilesTable
        {
            media,
            media_trash,
            album_entries,
            album_entries_trash
        }

        ///<summary>
        ///Return string List of all shortPaths from the table specified that don't have existing files in the photo library.
        ///</summary>
        public static List<string> GetMissingFiles(MissingFilesTable table)
        {
            string tableStr = table.ToString();
            List<string> missingFiles = new();

            try
            {
                C.Open();
                NpgsqlCommand cmd = new("SELECT path FROM " + tableStr, C.connection);
                cmd.ExecuteNonQuery();
                NpgsqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string shortPath = reader.GetString(0);
                    string fullPath = Path.Combine(S.libFolderPath, shortPath);
                    if (!File.Exists(fullPath))
                        missingFiles.Add(shortPath);
                }
                reader.Close();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine("An unknown error occurred. Error code: " + e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                C.Close();
            }

            return missingFiles;
        }

        ///<summary>
        ///Loop through List and delete these paths from table that are in the DB but don't exist as files.
        ///</summary>
        ///<param name="paths">List of shortPaths retrieved with GetMissingFiles()</param>
        ///<param name="table">The table to delete from</param>
        public static void RemoveMissingFiles(List<string> paths, MissingFilesTable table)
        {
            try
            {
                C.Open();
                foreach (string path in paths)
                {
                    NpgsqlCommand cmd = new("DELETE FROM " + table + " WHERE path=@path", C.connection);
                    cmd.Parameters.AddWithValue("@path", path);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine("An unknown error occurred. Error code: " + e.ErrorCode + " Message: " + e.Message);
            }
            finally
            {
                C.Close();
            }
        }
    }
}