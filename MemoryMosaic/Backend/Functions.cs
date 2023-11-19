namespace MemoryMosaic.Backend;

///<summary>Static class of misc functions.</summary>
public static class Functions
{
    public static readonly HashSet<string> SupportedImageExts = new() {".jpg", ".jpeg", ".png", ".gif"};
    public static readonly HashSet<string> SupportedVideoExts = new() {".mp4", ".mkv", ".mov"};
    public static readonly HashSet<string> SupportedExts = new() {".jpg", ".jpeg", ".png", ".gif", ".mp4", ".mkv", ".mov"};

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

    ///<summary>Given the absolute path to a image/video file, use ffmpeg to generate a compressed thumbnail of the image or the first frame.</summary>
    ///<param name="filePath">The absolute path to where the file is.</param>
    ///<returns>A base64 string representing the thumbnail.</returns>
    public static string GenerateThumbnail(string filePath)
    {
        string thumbnailFullPath = Path.Join(S.TmpFolderPath, Guid.NewGuid() + ".jpg");
        ProcessStartInfo ffmpegInfo = new()
        {
            CreateNoWindow = true,
            FileName = "ffmpeg",
            Arguments = $"-i \"{filePath}\" -loglevel quiet "
        };
            
        string ext = Path.GetExtension(filePath).ToLower();
        if (ext == ".png")
            ffmpegInfo.Arguments += $"-compression_level 100 \"{thumbnailFullPath}\""; //0-100 for quality. 100 is lowest.
        else
            ffmpegInfo.Arguments += $"{(SupportedVideoExts.Contains(ext) ? "-vf \"select=eq(n\\,0)\"" : "")} -vf scale=320:-2 -q:v {S.ThumbnailQuality} \"{thumbnailFullPath}\"";

        Process ffmpegProcess = Process.Start(ffmpegInfo) ?? throw new InvalidOperationException();
        ffmpegProcess.WaitForExit();

        byte[] bytes = File.ReadAllBytes(thumbnailFullPath);
            
        try { File.Delete(thumbnailFullPath); }
        catch (Exception e) { L.LogException(e); }
            
        return Convert.ToBase64String(bytes);
    }
        
    ///<summary>
    ///<para>Return an IEnumerable&lt;string&gt; of the full paths of all supported file types in rootPath.</para>
    ///Supported file types are: ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".mkv", ".mov". Case is ignored.
    ///</summary>
    public static IEnumerable<string> GetSupportedFiles(string rootPath)
    {
        string[] allPaths = Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories);
        return allPaths.Where(path => SupportedExts.Contains(Path.GetExtension(path).ToLower())).ToList();
    }
        
    ///<summary>Returns the size of a folder in bytes.</summary>
    public static long GetFolderSize(string path)
    {
        return new DirectoryInfo(path).EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
    }
}