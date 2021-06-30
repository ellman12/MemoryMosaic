//TODO
//Get list of items to sort [done]
//For each item to sort:
//Get date taken
//Generate dir if not present
//Copy or move the item there
//Add this new path to the DB
//Rinse and repeat

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
            //TODO: add this to a config file that is read every time this thing is ran.
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
