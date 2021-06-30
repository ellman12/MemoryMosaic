using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExifLib;

namespace PSS_Photo_Sorter
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine(
                PSSPhotoSorter.GetDateTime("C:/Users/Elliott/Pictures/413150_20210121203734_1.png")
                );
        }
    }
}
