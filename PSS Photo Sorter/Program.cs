using System;
using System.Globalization;

namespace PSS_Photo_Sorter
{
    class Program
    {
        static void Main()
        {
            try
            {
                string filename = "Screenshot_20210416-075337_Outlook.jpg";
                if (filename.Length > PSSPhotoSorter.MIN_LENGTH)
                {
                    DateTime yes = PSSPhotoSorter.GetFilenameTimestamp("C:/Users/Elliott/Documents/GitHub/Photos-Storage-Server/Pics and Vids/Test Vids/", filename);
                    Console.WriteLine(yes);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
