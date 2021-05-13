using System;

namespace PSS_Photo_Sorter
{
    class Program
    {
        static void Main()
        {
            // try
            // {
            string filename = "Screenshot_20210416-075337_Outlook.jpg";
            DateTime yes = PSSPhotoSorter.GetFilenameTimestamp(filename);
            //     DateTime yes = PSSPhotoSorter.ToDateTime(filename);
            //     Console.WriteLine(yes);
            // }
            // catch (ArgumentException)
            // {
            //     Console.WriteLine("error lmao");
            // }
        }
    }
}
