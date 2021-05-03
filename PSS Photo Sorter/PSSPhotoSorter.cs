using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace PSS_Photo_Sorter
{
    static class PSSPhotoSorter
    {
        //Uses ffprobe shell command to get video date.
        public static DateTime GetVidDate(string dir)
        {
            string date = "";
            ProcessStartInfo ffprobeInfo = new ProcessStartInfo();
            ffprobeInfo.CreateNoWindow = true;
            ffprobeInfo.UseShellExecute = false;
            ffprobeInfo.FileName = "ffprobe";
            ffprobeInfo.Arguments = "-v 0 -print_format compact=print_section=0:nk=1 -show_entries format_tags=creation_time \"" + dir + '"';
            ffprobeInfo.RedirectStandardOutput = true;
            ffprobeInfo.RedirectStandardError = true;

            try
            {
                Process ffprobeProcess = Process.Start(ffprobeInfo);
                date = ffprobeProcess.StandardOutput.ReadLine();
                ffprobeProcess.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something happened in GetVidDate()\n" + ex.Message);
            }
            return Convert.ToDateTime(date);
        }

        //Used if program can't find date/time metadata in the file.
        //TODO: get them as strings, THEN figure out datetime conversion
        public static string GetFilenameTimestamp(string dir, string filename)
        {
            string timestamp = "";
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
            else if (filename[8] == '_') //A filename like this: '20201031_090459.jpg'. I think these come from (Android(?)) phones.
            {
                Console.WriteLine("Android _");
                timestamp = filename.Substring(0, 8) + filename.Substring(9, 6);
            }
            else if (filename.Contains("_s")) //A Nintendo Switch screenshot/video clip: '2018022016403700_s.mp4'.
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
            {
                Console.WriteLine("Date could not be determined");
            }
            //return DateTime.Now;
            return timestamp;
        }

        static void PhotoSorter()
        {

        }
    }
}