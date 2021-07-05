using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Actual_DB_Test;

namespace PSS_Photo_Sorter
{
    class Sorting
    {
        public static void UploadItems()
        {
            List<string> missingDate = new(); //Stores paths without date taken data, which need the user to fix them.
            List<string> correctDate = new(); //Stores SORTED paths with correct date taken. Shown to user so can verify everything is good.

            //TODO: add this to a config file that is read every time this thing is ran.
            const string unsortedDir = @"C:\Users\Elliott\Documents\GitHub\Photos-Storage-Server\Pics and Vids\";
            const string sortedDir = @"C:\Users\Elliott\Documents\GitHub\Photos-Storage-Server\Pics and Vids\Destination\";

            //Gets the paths of all supported file types and add them to a List.
            List<string> paths = new();
            paths.AddRange(Directory.GetFiles(unsortedDir, "*.jpg", SearchOption.AllDirectories));
            paths.AddRange(Directory.GetFiles(unsortedDir, "*.jpeg", SearchOption.AllDirectories));
            paths.AddRange(Directory.GetFiles(unsortedDir, "*.png", SearchOption.AllDirectories));
            paths.AddRange(Directory.GetFiles(unsortedDir, "*.mp4", SearchOption.AllDirectories));
            paths.AddRange(Directory.GetFiles(unsortedDir, "*.mkv", SearchOption.AllDirectories));

            Connection c = new();
            c.OpenConnection();

            foreach (var path in paths)
            {
                DateTime dateTaken = Metadata.GetDateTime(path);

                if (dateTaken == DateTime.MinValue) //Represents "error" showing that program couldn't find date taken in metadata or filename. Also shows that user needs to fix this manually in the UI.
                {
                    missingDate.Add(path);
                    continue;
                }
                
                //Add new path to the database and List
                string newPath = Path.Join(sortedDir, dateTaken.Year.ToString(), dateTaken.Month + " " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(dateTaken.Month), dateTaken.Day.ToString(), Path.GetFileName(path));
                c.InsertMedia(newPath, dateTaken);
                correctDate.Add(newPath);

                //Copy to sorted folder.
                Directory.CreateDirectory(Path.GetDirectoryName(newPath));
                File.Copy(path, newPath);
            }

            Console.WriteLine("BAD");
            foreach (string badPath in missingDate)
            {
                Console.WriteLine(Path.GetFileName(badPath));
            }

            Console.WriteLine("\nGOOD");
            foreach (string goodPath in correctDate)
            {
                Console.WriteLine(Path.GetFileName(goodPath));
            }
        }

        //Prints a DateTime in a nice, human-readable format: Sat, May 1, 2021 7:30 PM
        //public static void PrintDateTime(DateTime dt)
        //{
            //Console.WriteLine(dt.ToShortDateString() + ", " + dt.Year + " " + dt.ToShortTimeString());
        //}
    }
}
