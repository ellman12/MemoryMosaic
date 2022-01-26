using System.Diagnostics;

namespace PSS.Backend
{
    ///<summary>
    ///Static class of misc functions.
    ///</summary>
    public static class Functions
    {
        ///<summary>
        ///Take a byte long like 10900000000 and turn it into a more readable string like 10.9 GB.
        ///One thing to note is this uses things like kibibyte instead of the usual things like kilobyte because this is usually what's used for disk storage.
        ///</summary>
        public static string FormatBytes(long bytes)
        {
            string unit;
            double compactBytes;

            switch (bytes)
            {
                case >= 1100000000000:
                    unit = "TB";
                    compactBytes = bytes / 1100000000000.0;
                    break;
                case >= 1074000000:
                    unit = "GB";
                    compactBytes = bytes / 1074000000.0;
                    break;
                case >= 1049000:
                    unit = "MB";
                    compactBytes = bytes / 1049000.0;
                    break;
                case >= 1024:
                    unit = "KB";
                    compactBytes = bytes / 1024.0;
                    break;
                default:
                    unit = "bytes";
                    compactBytes = bytes;
                    break;
            }

            return $"{Math.Round(compactBytes, 3)} {unit}";
        }

        ///<summary>
        ///Toggle a string variable to either "visible" or "hidden".
        ///</summary>
        public static void VisToggle(ref string visibility) => visibility = visibility == "visible" ? "hidden" : "visible";
        
        ///<summary>
        ///Given the full final path where the video file will end up on the server, generate a temporary compressed thumbnail file for it,
        ///turn that into its base64 representation, and return the base64 string.<br/>
        ///Make sure to clear the pss_tmp directory when done in UploadApply!
        ///</summary>
        ///<param name="videoFullFinalPath">The full path to where the video file either is right now, or where it will be.</param>
        public static string GenerateThumbnail(string videoFullFinalPath)
        {
            //First create the thumbnail from the first frame of the video file.
            //https://stackoverflow.com/questions/4425413/how-to-extract-the-1st-frame-and-restore-as-an-image-with-ffmpeg/4425466
            //Store this file in the tmp folder and name it as the same name as the video file except with the extension '.tmp.jpg'.
            string thumbnailFullPath = Path.Combine(S.tmpFolderPath, $"{Path.GetFileNameWithoutExtension(videoFullFinalPath)}_thumbnail_{DateTime.Now:M-d-yyyy}.tmp.jpg");

            if (!File.Exists(thumbnailFullPath)) //Re-use it if it exists from a previous run of this command.
            {
                //Something to note is this command produces a bunch of stupid output and also says it failed but it still produces the thumbnail so... ¯\_(ツ)_/¯ 
                ProcessStartInfo ffmpegInfo = new()
                {
                    CreateNoWindow = true,
                    FileName = "ffmpeg",
                    Arguments = $"-i \"{videoFullFinalPath}\" -vf \"select=eq(n\\,0)\" -vf scale=320:-2 -q:v 25 \"{thumbnailFullPath}\""
                };
                Process ffmpegProcess = Process.Start(ffmpegInfo);
                ffmpegProcess!.WaitForExit();
            }

            byte[] bytes = File.ReadAllBytes(thumbnailFullPath);

            //Then convert that file's bytes into its base64 equivalent. This is stored in the DB as the thumbnail (no actual file required).
            return Convert.ToBase64String(bytes);
        }
    }
}