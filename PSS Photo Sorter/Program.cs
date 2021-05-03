using System;

namespace PSS_Photo_Sorter
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime date = PSSPhotoSorter.GetVidDate("C:/Users/Elliott/Documents/GitHub/Photos-Storage-Server/Pics and Vids/Test Vids/20210501_193046.mp4");
            Console.WriteLine(date);
        }
    }
}
