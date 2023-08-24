using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace MemoryMosaic;

public class Settings
{
    ///The full path to the mm_import folder on the server. This is where items live before being added to the library.
    [JsonProperty] public static string importFolderPath = null!;

    ///The full path to the library folder (mm_library) on the server.
    [JsonProperty] public static string libFolderPath = null!;

    ///Where to backup library and database (mm_backup).
    [JsonProperty] public static string backupFolderPath = null!;
        
    ///Where the temporary folder (mm_tmp) is on the server. This is used for things like temporarily storing thumbnail files when converting them to base64, etc.
    [JsonProperty] public static string tmpFolderPath = null!;
        
    ///Should prompts be shown when doing things like deleting items and albums, etc.?
    [JsonProperty] public static bool showPrompts;

    ///Should items without a Date Taken be shown in albums and folders?
    [JsonProperty] public static bool displayNoDTInCV;

    ///Controls the quality of thumbnails when they are generated. Values are between 1 and 31. Lower the number, higher the quality.
    [JsonProperty] public static int thumbnailQuality;

    ///Controls what items are printed out by the Logger.
    [JsonProperty] public static LogLevel logLevel;
        
    public const int POSTGRES_VERSION = 15;

#if DEBUG
    public static readonly string FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MemoryMosaicTest");
    public static readonly string FilePath = Path.Combine(FolderPath, "mm_debug_settings.json");
#else
    public static readonly string FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MemoryMosaic");
    public static readonly string FilePath = Path.Combine(FolderPath, "mm_settings.json");
#endif

    public static void WriteSettings() => File.WriteAllText(FilePath, JsonConvert.SerializeObject(new Settings())); //https://stackoverflow.com/a/16921677

    public static void ReadSettings()
    {
        new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory).AddJsonFile(FilePath).Build();
        JsonConvert.DeserializeObject<Settings>(File.ReadAllText(FilePath));
        importFolderPath = importFolderPath.Replace('\\', '/');
        libFolderPath = libFolderPath.Replace('\\', '/');
        backupFolderPath = backupFolderPath.Replace('\\', '/');
        tmpFolderPath = tmpFolderPath.Replace('\\', '/');
    }

    public static void ResetSettings()
    {
    #if DEBUG
        const string root = "C:/Users/Elliott/Pictures/MemoryMosaic_Debug";
    #else
        const string root = "C:/Users/Elliott/Pictures/MemoryMosaic";
    #endif

        importFolderPath = $"{root}/mm_import";
        libFolderPath = $"{root}/mm_library";
        backupFolderPath = $"{root}/mm_backup";
        tmpFolderPath = $"{root}/mm_tmp";
        showPrompts = displayNoDTInCV = true;
        thumbnailQuality = 7;
        logLevel = LogLevel.Error;
        File.WriteAllText(FilePath, JsonConvert.SerializeObject(new Settings()));
    }
}