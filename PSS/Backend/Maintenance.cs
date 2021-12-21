using System.IO;

namespace PSS.Backend
{
    public static class Maintenance
    {
        /// <summary>
        /// Return if a folder is empty (has 0 files in folder or subfolders). Subfolders inside path aren't counted unless they have stuff inside.
        ///  If root/ has 2 subfolders and 3 files it would only find 3 files and thus it wouldn't be counted as empty.
        /// If root/ has 2 subfolders and 0 files, it would be considered empty.
        /// </summary>
        /// <returns>True if empty.</returns>
        public static bool IsFolderEmpty(string path) => Directory.GetFiles(path, "*", SearchOption.AllDirectories).Length == 0;
    }
}