using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
//https://stackoverflow.com/questions/26051273/whats-the-best-way-to-get-video-metadata-from-a-video-file-in-asp-net-mvc-using

namespace PSS_Photo_Sorter
{
    static class PSSPhotoSorter
    {
        private static void GetVideoDuration()
        {
            string basePath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar;
            string cmd = string.Format("-v error -select_streams v:0 -show_entries stream=duration -of default=noprint_wrappers=1:nokey=1  {0}", "C:/Users/Elliott/Documents/GitHub/Photos-Storage-Server/Pics and Vids/Test Vids/20210501_192909.mp4");
            Process proc = new Process();
            proc.StartInfo.FileName = Path.Combine(basePath, @"ffprobe.exe");
            proc.StartInfo.Arguments = cmd;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.UseShellExecute = false;
            if (!proc.Start())
            {
                Console.WriteLine("Error starting");
                return;
            }
            string duration = proc.StandardOutput.ReadToEnd().Replace("\r\n", "");
            // Remove the milliseconds
            duration = duration.Substring(0, duration.LastIndexOf("."));
            proc.WaitForExit();
            proc.Close();
        }

        public static void ExecuteCommand(string exeDir, string args)
        {

            //public static void GetVideoMetadata(string dir)
            //{
            //    ProcessStartInfo cmdStartInfo = new ProcessStartInfo();
            //    cmdStartInfo.FileName = @"C:\Windows\System32\cmd.exe";
            //    cmdStartInfo.RedirectStandardOutput = true;
            //    cmdStartInfo.RedirectStandardError = true;
            //    cmdStartInfo.RedirectStandardInput = true;
            //    cmdStartInfo.UseShellExecute = false;
            //    cmdStartInfo.CreateNoWindow = true;

            //    Process cmdProcess = new Process();
            //    cmdProcess.StartInfo = cmdStartInfo;
            //    cmdProcess.ErrorDataReceived += cmd_Error;
            //    cmdProcess.OutputDataReceived += cmd_DataReceived;
            //    cmdProcess.EnableRaisingEvents = true;
            //    cmdProcess.Start();
            //    cmdProcess.BeginOutputReadLine();
            //    cmdProcess.BeginErrorReadLine();

            //    cmdProcess.StandardInput.WriteLine("ping www.bing.com");     //Execute ping bing.com
            //    cmdProcess.StandardInput.WriteLine("exit");                  //Execute exit.

            //    cmdProcess.WaitForExit();

            //    //ProcessStartInfo cmdStartInfo = new ProcessStartInfo();
            //    //cmdStartInfo.FileName = @"C:\FFmpeg\bin\ffprobe.exe";
            //    //cmdStartInfo.RedirectStandardOutput = true;
            //    //cmdStartInfo.RedirectStandardError = true;
            //    //cmdStartInfo.RedirectStandardInput = true;
            //    //cmdStartInfo.UseShellExecute = false;
            //    //cmdStartInfo.CreateNoWindow = true;

            //    //Process cmdProcess = new Process();
            //    //cmdProcess.StartInfo = cmdStartInfo;
            //    ////cmdProcess.ErrorDataReceived += cmd_Error;
            //    ////cmdProcess.OutputDataReceived += cmd_DataReceived;
            //    //cmdProcess.EnableRaisingEvents = true;
            //    //cmdProcess.Start();
            //    //cmdProcess.BeginOutputReadLine();
            //    //cmdProcess.BeginErrorReadLine();

            //    ////cmdProcess.StandardInput.WriteLine("ffprobe -v 0 -print_format compact=print_section=0:nk=1 -show_entries format_tags=creation_time " + dir);

            //    //cmdProcess.StandardInput.WriteLine("ping www.bing.com");

            //    //cmdProcess.WaitForExit();



            //    //Process process = new Process();
            //    //process.StartInfo.FileName = "ffprobe";
            //    //process.StartInfo.Arguments = "-v 0 -print_format compact=print_section=0:nk=1 -show_entries format_tags=creation_time " + dir;
            //    //process.StartInfo.UseShellExecute = false;
            //    //process.StartInfo.RedirectStandardOutput = true;
            //    //process.Start();

            //    ////Synchronously read the standard output of the spawned process.
            //    ////StreamReader reader = process.StandardOutput;
            //    ////string output = reader.ReadToEnd();
            //    ////string output = process.StandardOutput.ReadToEnd();
            //    ////Console.WriteLine(output);

            //    //process.StandardInput.WriteLine("ffprobe -v 0 -print_format compact=print_section=0:nk=1 -show_entries format_tags=creation_time " + dir);

            //    //process.WaitForExit();

            //    //Process ffprobe = new Process();
            //    //ffprobe.StartInfo = cmdStartInfo;
            //    //ffprobe.ErrorDataReceived += cmd_Error;
            //    //ffprobe.OutputDataReceived += cmd_DataReceived;
            //    //ffprobe.EnableRaisingEvents = true;
            //    //ffprobe.Start();
            //    //ffprobe.BeginOutputReadLine();
            //    //ffprobe.BeginErrorReadLine();
            //    //ffprobe.Start("ffprobe -v 0 -print_format compact=print_section=0:nk=1 -show_entries format_tags=creation_time" + dir);

            //}

            static void PhotoSorter()
            {

            }
        }
    }
}
