using System;
using System.Collections.Generic;
using System.IO;

namespace PSS_Photo_Sorter
{
    //Represents the config file for PSS.
    //The config file is a simple txt file where each line represents an option.
    public static class Config
    {
        //TODO: there has to be a way to make this a "relative(?)" path.
        private const string FilePath = "C:/Users/Elliott/Documents/GitHub/Photos-Storage-Server/PSS Photo Sorter/Config.txt";

        //Config values
        public static string unsortedDir, sortedDir; //Where newly uploaded items go for processing and then where the processed items are sent to.

        //Read file on startup and assign variables their values from the file.
        public static void ReadFile()
        {
            try
            {
                string[] fileText = File.ReadAllLines(FilePath);
                unsortedDir = fileText[0]; //Each index is the line number minus 1.
                sortedDir = fileText[1];
            }
            catch (FileNotFoundException)
            {
                WriteFile(); //Create file
            }
        }

        //File is updated every time a value is changed.
        public static void WriteFile()
        {
            string fileText = unsortedDir + "\n" + sortedDir + "\n";
            File.WriteAllText(FilePath, fileText);
        }

    }
}
