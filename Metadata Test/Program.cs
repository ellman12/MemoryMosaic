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
        {
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
