using System;
using System.Globalization;

namespace PSS_Photo_Sorter
{
    class Program
    {
        static void Main()
        {
            DateTime yes = PSSPhotoSorter.GetFilenameTimestamp("C:/Users/Elliott/Documents/GitHub/Photos-Storage-Server/Pics and Vids/Test Vids/", "Screenshot_20210416-075337_Outlook.jpg");
            Console.WriteLine(yes);

        }
    }
}
