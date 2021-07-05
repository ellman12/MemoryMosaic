using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ExifLib;

namespace PSS_Photo_Sorter
{
    static class Metadata
    {
        //Take a timestamp string like '20210501193042' (typically from a filename) and make it into a DateTime object.
        //The dateString needs to be exactly 14 characters long.
        public static DateTime ToDateTime(string dateString)
        {
            switch (dateString.Length)
            {
                case < 14:
                    throw new ArgumentException("The specified dateString is too short. It needs to be exactly 14 characters long. The string given was " + dateString.Length + " characters long.");
                case > 14:
                    throw new ArgumentException("The specified dateString is too long. It needs to be exactly 14 characters long. The string given was " + dateString.Length + " characters long.");
                case 14:
                    {
                        int year = Int32.Parse(dateString.Substring(0, 4));
                        int month = Int32.Parse(dateString.Substring(4, 2));
                        int day = Int32.Parse(dateString.Substring(6, 2));
                        int hour = Int32.Parse(dateString.Substring(8, 2));
                        int minute = Int32.Parse(dateString.Substring(10, 2));
                        int second = Int32.Parse(dateString.Substring(12, 2));
                        return new DateTime(year, month, day, hour, minute, second);
                    }
            }
        }

        //TODO: this is (probably) incomplete.
        //Returns the Date Taken for an item, if possible.
        //Steps:
        //1. Determine type.
        //2. Try reading embedded metadata (if the type is even capable of doing so).
        //3. If no metadata found, try reading filename.
        //4. If all else fails, flag item so user can fix.
        public static DateTime GetDateTime(string path)
        {
            DateTime dateTime = new();

            switch (Path.GetExtension(path))
            {
                case ".jpg":
                    dateTime = GetJpgDate(path);
                    break;

                case ".png":
                    dateTime = GetFilenameTimestamp(path);
                    break;

                case ".mp4":
                    dateTime = GetVidDate(path);
                    break;

                case ".mkv":
                    dateTime = GetFilenameTimestamp(path);
                    break;
            }
            return dateTime;
        }

        //Try and examine JPG metadata. If necessary, it analyzes filename.
        //If still can't determine datetime, have user take a look.
        public static DateTime GetJpgDate(string path)
        {
            DateTime dateTaken;

            try
            {
                ExifReader reader = new(path);
                reader.GetTagValue(ExifTags.DateTimeDigitized, out dateTaken);
            }
            catch (ExifLibException e) //No metadata in file.
            {
                try
                {
                    dateTaken = GetFilenameTimestamp(path);
                }
                catch (ArgumentException exception)
                {
                    dateTaken = DateTime.MinValue; //Represents "error" that user needs to fix manually in the UI.
                }
            }

            return dateTaken;
        }

        //Uses ffprobe shell command to get video date from file metadata.
        public static DateTime GetVidDate(string path)
        {
            string date = ""; //The output of the ffprobe command.
            ProcessStartInfo ffprobeInfo = new ProcessStartInfo();
            ffprobeInfo.CreateNoWindow = true;
            ffprobeInfo.UseShellExecute = false;
            ffprobeInfo.FileName = "ffprobe";
            ffprobeInfo.Arguments = "-v 0 -print_format compact=print_section=0:nk=1 -show_entries format_tags=creation_time \"" + path + '"';
            ffprobeInfo.RedirectStandardOutput = true;
            ffprobeInfo.RedirectStandardError = true;

            try
            {
                Process ffprobeProcess = Process.Start(ffprobeInfo);
                date = ffprobeProcess.StandardOutput.ReadLine(); //The command should only return a single line. //TODO: what does it return on error or something?
                ffprobeProcess.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something happened in GetVidDate() while running the ffprobe command\n" + ex.Message);
            }
            return Convert.ToDateTime(date);
        }

        //Used if program can't find date/time metadata in the file. Often, filenames will have a timestamp in them.
        //E.g., the Nintendo Switch generates pics/vids filenames like: 2018022016403700_s.mp4. This can be stripped and
        //converted into an actual DateTime object. It's stripped in here and converted in ToDateTime() and returned here.
        public static DateTime GetFilenameTimestamp(string filename)
        {
            string timestamp = ""; //The actual timestamp in the filename, without the extra chars we don't want. Converted to DateTime at the end.

            if (filename.Contains("Screenshot_")) //If Android screenshot. E.g., 'Screenshot_20201028-141626_Messages.jpg'
            {
                Console.WriteLine("Screenshot");
                timestamp = filename.Substring(11, 8) + filename.Substring(20, 6); //Strip the chars we don't want.
            }
            else if (filename.Contains("IMG_") || filename.Contains("VID_"))
            {
                Console.WriteLine("img or vid");
                timestamp = filename.Substring(4, 8) + filename.Substring(13, 6);
            }
            else if (filename[4] == '-' && filename[13] == '-' && filename[16] == '-' && filename.Contains(".mkv")) //Check if an OBS-generated file. It would have '-' at these 3 indices.
            {
                Console.WriteLine("OBS");
                timestamp = filename;
                timestamp = filename.Substring(0, timestamp.Length - 4); //Remove extension https://stackoverflow.com/questions/15564944/remove-the-last-three-characters-from-a-string
                timestamp = timestamp.Replace("-", "").Replace(" ", "");
            }
            else if (filename[8] == '_') //A filename like this: '20201031_090459.jpg'. I think these come from (Android(?)) phones. Not 100% sure.
            {
                Console.WriteLine("Android _");
                timestamp = filename.Substring(0, 8) + filename.Substring(9, 6);
            }
            else if (filename.Contains("_s")) //A Nintendo Switch screenshot/video clip, like '2018022016403700_s.mp4'.
            {
                Console.WriteLine("Switch");
                timestamp = filename.Substring(0, 14);
            }
            else if (filename.Contains("Capture") && filename.Contains(".png")) //Terraria's Capture Mode 'Capture 2020-05-16 21_04_54.png'
            {
                Console.WriteLine("Terraria");
                timestamp = filename.Substring(8, 19);
                timestamp = timestamp.Replace('_', ':');
            }
            else if (filename.Contains("Screenshot ") && filename.Contains(".png")) //Snip & Sketch generates these filenames. E.g., 'Screenshot 2020-11-17 104051.png'
            {
                Console.WriteLine("Snip and sketch");
                timestamp = filename.Substring(11, 17);
                timestamp = timestamp.Replace("-", "").Replace(" ", "");
            }
            else
                throw new ArgumentException("Could not determine date/time from provided filename: " + filename);

            return ToDateTime(timestamp);
        }
    }
}