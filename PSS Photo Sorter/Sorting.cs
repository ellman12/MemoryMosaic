//TODO
//Get list of items to sort [done]
//for each item to sort:
//get date taken
//generate dir if not present
//copy or move the item there
//add this new path to the DB
//rinse and repeat

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PSS_Photo_Sorter
{
    class Sorting
    {
        public static void UploadItems()
        {
            const string root = @"C:\Users\Elliott\Documents\GitHub\Photos-Storage-Server\Pics and Vids\";

            List<string> files = new();
            files.AddRange(Directory.GetFiles(root, "*.jpg", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(root, "*.jpeg", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(root, "*.png", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(root, "*.mp4", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(root, "*.mkv", SearchOption.AllDirectories));

            foreach (var file in files)
            {
                
            }
        }
    }
}
