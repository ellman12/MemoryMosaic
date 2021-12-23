using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PSS.Backend
{
    /// <summary>
    /// Functions for server maintenance.
    /// </summary>
    public static class Maintenance
    {
        /// <summary>
        /// Return if a folder is empty (has 0 files in folder or subfolders). Subfolders inside path aren't counted unless they have stuff inside.
        ///  If root/ has 2 subfolders and 3 files it would only find 3 files and thus it wouldn't be counted as empty.
        /// If root/ has 2 subfolders and 0 files, it would be considered empty.
        /// </summary>
        /// <returns>True if empty.</returns>
        public static bool IsFolderEmpty(string path) => Directory.GetFiles(path, "*", SearchOption.AllDirectories).Length == 0;

        /// <summary>
        /// Search library folder and if an item is not in the media or media_trash tables, add it to the List of full paths that is returned. 
        /// </summary>
        public static List<string> GetUntrackedLibFiles()
        {
            List<string> untrackedPaths = new(); //Items in lib folder but not in database
            string[] paths = Directory.GetFiles(Settings.libFolderFullPath, "*", SearchOption.AllDirectories);
            List<string> mediaPaths = Connection.LoadMediaTable().Select(media => media.path).ToList(); //Get just paths
            List<string> mediaTrashPaths = Connection.LoadMediaTrashTable().Select(media => media.path).ToList();

            foreach (string fullPath in paths)
            {
                string shortPath = fullPath.Replace(Settings.libFolderFullPath, "");
                if (shortPath.StartsWith('\\') || shortPath.StartsWith('/')) shortPath = shortPath[1..];
                
                if (!mediaPaths.Contains(shortPath) && !mediaTrashPaths.Contains(shortPath)) //If find an item not in library add to list
                    untrackedPaths.Add(fullPath);
            }
            return untrackedPaths;
        }
    }
}