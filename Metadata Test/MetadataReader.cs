using System;
using System.Collections.Generic;
using System.Linq;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace Metadata_Test
{
    public class NoMetadataException : Exception
    {
        public NoMetadataException()
        {
        }

        public NoMetadataException(string message)
            : base(message)
        {
        }
    }

    public static class MetadataReader
    {
        //Attempts to retrieve metadata from an image file
        //Could potentially throw a NoMetadataException.
        public static DateTime GetImageDateTaken(string path)
        {
            //This is useful: https://stackoverflow.com/questions/180030/how-can-i-find-out-when-a-picture-was-actually-taken-in-c-sharp-running-on-vista

            //Read all metadata from the image.
            IReadOnlyList<Directory> metadata = ImageMetadataReader.ReadMetadata(path);

            //This could be null, meaning it doesn't have the data.
            ExifSubIfdDirectory subIfdDirectory = metadata.OfType<ExifSubIfdDirectory>().FirstOrDefault();

            if (subIfdDirectory == null)
                throw new Exception("No date taken metadata found.");

            //Return just the Date Taken metadata
            return subIfdDirectory.GetDateTime(ExifDirectoryBase.TagDateTimeOriginal);
        }
    }
}