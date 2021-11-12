using System;
using System.Diagnostics;
using System.IO;
using ExifLib;
using static System.Int32;

namespace PSS.Backend
{
    static class Metadata
    {
        //Get the Date Taken for an item, if possible.
        //Return true if data was found or false if using DateTime.Now
        //Steps:
        //1. Determine type.
        //2. Try reading embedded metadata (if the type is even capable of doing so).
        //3. If no metadata found, try reading filename.
        //4. If all else fails, make it the min value to flag to user to fix it.
        public static bool GetDateTaken(string path, out DateTime dateTaken)
        {
            bool hasData = false;
            dateTaken = DateTime.Now;

            switch (Path.GetExtension(path))
            {
                case ".jpg" or ".jpeg":
                    hasData = GetJpgDate(path, out dateTaken);
                    break;

                case ".png":
                    hasData = GetFilenameTimestamp(Path.GetFileName(path), out dateTaken);
                    break;

                case ".mp4":
                    hasData = GetVidDate(path, out dateTaken);
                    break;

                case ".mkv":
                    hasData = GetFilenameTimestamp(Path.GetFileName(path), out dateTaken);
                    break;
            }

            return hasData;
        }

        //Try and examine JPG metadata. If necessary, it analyzes filename. If can't find data in either, default to DateTime.Now.
        private static bool GetJpgDate(string path, out DateTime dateTaken)
        {
            bool hasData;
            try
            {
                ExifReader reader = new(path);

                //I think if this ↓ returns false it means no data found. 0 documentation on this... 
                hasData = reader.GetTagValue(ExifTags.DateTimeDigitized, out dateTaken);

                if (dateTaken == DateTime.MinValue || hasData == false)
                    throw new ExifLibException(); //If GetTagValue returns DateTime.MinValue, means no data found (hasData == false means same thing), so try reading filename instead.
            }
            catch (ExifLibException) //No metadata in file.
            {
                hasData = GetFilenameTimestamp(Path.GetFileName(path), out dateTaken);
                if (!hasData) dateTaken = DateTime.Now;
            }

            return hasData;
        }

        //Uses ffprobe shell command to get video date from file metadata.
        //TODO: untested
        private static bool GetVidDate(string path, out DateTime dateTaken)
        {
            // bool hasData = false;
            string cmdOutput = ""; //The output of the ffprobe command.
            ProcessStartInfo ffprobeInfo = new()
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                FileName = "ffprobe",
                Arguments = "-v 0 -print_format compact=print_section=0:nk=1 -show_entries format_tags=creation_time \"" + path + '"',
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            Process ffprobeProcess = Process.Start(ffprobeInfo);
            cmdOutput = ffprobeProcess.StandardOutput.ReadLine();
            ffprobeProcess.WaitForExit();

            if (cmdOutput == "") //mkv files don't have date data in them at all (I think). cmd just returns blank if no data
            {
                dateTaken = DateTime.Now;
                return false;
            }

            dateTaken = Convert.ToDateTime(cmdOutput);
            return true;
        }

        //Used if program can't find date/time metadata in the file. Often, filenames will have a timestamp in them.
        //E.g., the Nintendo Switch generates pics/vids filenames like: 2018022016403700_s.mp4. This can be stripped and
        //converted into an actual DateTime object.
        private static bool GetFilenameTimestamp(string filename, out DateTime dateTaken)
        {
            bool hasData = true;
            string timestamp = ""; //The actual timestamp in the filename, without the extra chars we don't want. Converted to DateTime at the end.

            try
            {
                if (filename.Contains("Screenshot_")) //If Android screenshot. E.g., 'Screenshot_20201028-141626_Messages.jpg'
                {
                    //Console.WriteLine("Screenshot");
                    timestamp = filename.Substring(11, 8) + filename.Substring(20, 6); //Strip the chars we don't want.
                    timestamp = timestamp.Insert(4, "-");
                    timestamp = timestamp.Insert(7, "-");
                    timestamp = timestamp.Insert(10, " ");
                    timestamp = timestamp.Insert(13, ":");
                    timestamp = timestamp.Insert(16, ":");
                }
                else if (filename.Contains("IMG_") || filename.Contains("VID_"))
                {
                    //Console.WriteLine("img or vid");
                    timestamp = filename.Substring(4, 8) + filename.Substring(13, 6);
                }
                else if (filename[4] == '-' && filename[13] == '-' && filename[16] == '-' && filename.Contains(".mkv")) //Check if an OBS-generated file. It would have '-' at these 3 indices.
                {
                    //Console.WriteLine("OBS");
                    timestamp = filename;
                    timestamp = filename.Substring(0, timestamp.Length - 4); //Remove extension https://stackoverflow.com/questions/15564944/remove-the-last-three-characters-from-a-string
                    timestamp = timestamp.Replace("-", "").Replace(" ", "");
                }
                else if (filename[8] == '_') //A filename like this: '20201031_090459.jpg'. I think these come from (Android(?)) phones. Not 100% sure.
                {
                    //Console.WriteLine("Android _");
                    timestamp = filename.Substring(0, 8) + filename.Substring(9, 6);
                }
                else if (filename.Contains("_s")) //A Nintendo Switch screenshot/video clip, like '2018022016403700_s.mp4'.
                {
                    //Console.WriteLine("Switch");
                    timestamp = filename.Substring(0, 14);
                }
                else if (filename.Contains("Capture") && filename.Contains(".png")) //Terraria's Capture Mode 'Capture 2020-05-16 21_04_54.png'
                {
                    //Console.WriteLine("Terraria");
                    timestamp = filename.Substring(8, 19);
                    timestamp = timestamp.Replace('_', ':');
                }
                else if (filename.Contains("Screenshot ") && filename.Contains(".png")) //Snip & Sketch generates these filenames. E.g., 'Screenshot 2020-11-17 104051.png'
                {
                    //Console.WriteLine("Snip and sketch");
                    timestamp = filename.Substring(11, 17);
                    timestamp = timestamp.Replace("-", "").Replace(" ", "");
                }
                else
                    hasData = false;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in GetFilenameTimestamp() {e.Message}");
            }
            finally
            {
                if (timestamp == "")
                {
                    dateTaken = DateTime.Now;
                    hasData = false;
                }
                else
                    hasData = ParseTimestamp(timestamp, out dateTaken);
            }
            return hasData;
        }

        //Try parsing timestamp like this: "20211031155822"
        public static bool ParseTimestamp(string timestamp, out DateTime dateTime)
        {
            if (DateTime.TryParse(timestamp, out dateTime) == false && timestamp.Length == 14) //Not successful
            {
                //Try my way
                int year = Parse(timestamp[0..4]);
                int month = Parse(timestamp[4..6]);
                int day = Parse(timestamp[6..8]);
                int hour = Parse(timestamp[8..10]);
                int min = Parse(timestamp[10..12]);
                int sec = Parse(timestamp[12..14]);

                dateTime = new(year, month, day, hour, min, sec);
                return true;
            }
            return false;
        }
    }
}