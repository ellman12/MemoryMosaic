using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExifLib;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace Metadata_Test
{
    class Program
    {
        static void Main()
        {  //                   01234567891123
            string timestamp = "20211031155822";

            int year = Int32.Parse(timestamp[0..4]);
            int month = Int32.Parse(timestamp[4..6]);
            int day = Int32.Parse(timestamp[6..8]);
            int hour = Int32.Parse(timestamp[8..10]);
            int min = Int32.Parse(timestamp[10..12]);
            int sec = Int32.Parse(timestamp[12..14]);

            var yes = new DateTime(year, month, day, hour, min, sec);
            Console.WriteLine(yes.ToString("F"));

            var dateTime = new DateTime();
            bool yes2 = DateTime.TryParse(timestamp, out dateTime);
            Console.WriteLine(yes2);

            Console.WriteLine(ParseTimestamp(timestamp, out dateTime));
            
            static bool ParseTimestamp(string timestamp, out DateTime dateTime)
            {
                if (DateTime.TryParse(timestamp, out dateTime) == false && timestamp.Length == 14) //Not successful
                {
                    //Try my way
                    int year = Int32.Parse(timestamp[0..4]);
                    int month = Int32.Parse(timestamp[4..6]);
                    int day = Int32.Parse(timestamp[6..8]);
                    int hour = Int32.Parse(timestamp[8..10]);
                    int min = Int32.Parse(timestamp[10..12]);
                    int sec = Int32.Parse(timestamp[12..14]);

                    dateTime = new(year, month, day, hour, min, sec);
                    return true;
                }
                return false;
            }

            //Works for just jpeg
            //try
            //{
            //    ExifReader reader = new ExifReader("C:/Users/Elliott/Pictures/Internships/Los Alamos Internship.png");
            //    DateTime datetime;
            //    if (reader.GetTagValue(ExifTags.DateTimeDigitized, out datetime))
            //        Console.WriteLine(datetime);
            //    else
            //        Console.WriteLine("No data");
            //}
            //catch (ExifLibException e)
            //{
            //    Console.WriteLine(e.Message);
            //}

            //Also works
            ////Read all metadata from the image
            //IReadOnlyList<MetadataExtractor.Directory> metadata = ImageMetadataReader.ReadMetadata("C:/Users/Elliott/Downloads/Screenshot 2020-11-24 102109.png");

            //ExifSubIfdDirectory subIfdDirectory = metadata.OfType<ExifSubIfdDirectory>().FirstOrDefault();

            ////Read the DateTime tag value
            //DateTime? dateTaken = subIfdDirectory?.GetDateTime(ExifDirectoryBase.TagDateTimeOriginal);
            //Console.WriteLine(dateTaken);

            //try
            //{
            //Console.WriteLine(MetadataReader.GetDateTaken("C:/Users/Elliott/Downloads/Screenshot 2020-11-24 102109.png"));
            //Console.WriteLine(MetadataReader.GetDateTaken("C:/Users/Elliott/Videos/Temporary Stuff/phone pics backup/DCIM/Camera/00000IMG_00000_BURST20191019152408762_COVER.jpg"));
            //}
            //catch (Exception)
            //{
            //Console.WriteLine("Please enter a new date");
            //}
        }
    }
}