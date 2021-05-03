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

        static void PhotoSorter()
        {

        }
    }
}
