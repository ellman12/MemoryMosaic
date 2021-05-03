using System;

namespace PSS_Photo_Sorter
{
    class Program
    {
        static void Main(string[] args)
        {
            //PSSPhotoSorter.GetVideoMetadata("C:/Users/Elliott/Documents/GitHub/Photos-Storage-Server/Pics and Vids/Test Vids/20210501_192909.mp4");
            //PSSPhotoSorter.ExecuteCommand("C:/FFmpeg/bin/ffprobe.exe", "-v 0 -print_format compact=print_section=0:nk=1 -show_entries format_tags=creation_time C:/Users/Elliott/Documents/GitHub/Photos-Storage-Server/Pics and Vids/Test Vids/20210501_192909.mp4");

            //try
            //{
            //    // create the ProcessStartInfo using "cmd" as the program to be run,
            //    // and "/c " as the parameters.
            //    // Incidentally, /c tells cmd that we want it to execute the command that follows,
            //    // and then exit.
            //    System.Diagnostics.ProcessStartInfo procStartInfo =
            //        new System.Diagnostics.ProcessStartInfo("cmd", "/c " + "ffprobe");

            //    // The following commands are needed to redirect the standard output.
            //    // This means that it will be redirected to the Process.StandardOutput StreamReader.
            //    procStartInfo.RedirectStandardOutput = true;
            //    procStartInfo.UseShellExecute = false;
            //    // Do not create the black window.
            //    procStartInfo.CreateNoWindow = true;
            //    // Now we create a process, assign its ProcessStartInfo and start it
            //    System.Diagnostics.Process proc = new System.Diagnostics.Process();
            //    proc.StartInfo = procStartInfo;
            //    proc.Start();
            //    // Get the output into a string
            //    string result = proc.StandardOutput.ReadToEnd();
            //    // Display the command output.
            //    Console.WriteLine(result);
            //}
            //catch (Exception objException)
            //{
            //    Console.WriteLine("hi");
            //}
        }
    }
}
