using Newtonsoft.Json;

namespace MemoryMosaic;

public sealed class Settings
{
    ///The full path to the mm_import folder on the server. This is where items live before being added to the library.
    [JsonProperty] public static string ImportFolderPath { get; set; } = "";

    ///The full path to the library folder (mm_library) on the server.
    [JsonProperty] public static string LibFolderPath { get; set; } = "";

    ///Where to backup library and database (mm_backup).
    [JsonProperty] public static string BackupFolderPath { get; set; } = "";
        
    ///Where the temporary folder (mm_tmp) is on the server. This is used for things like temporarily storing thumbnail files when converting them to base64, etc.
    [JsonProperty] public static string TmpFolderPath { get; set; } = "";
        
    ///Should prompts be shown when doing things like deleting items and albums, etc.?
    [JsonProperty] public static bool ShowPrompts { get; set; } = true;

    ///Controls the quality of thumbnails when they are generated. Values are between 1 and 31. Lower the number, higher the quality.
    [JsonProperty] public static int ThumbnailQuality { get; set; } = 7;

    ///Controls what items are printed out by the Logger.
    [JsonProperty] public static LogLevel LogLevel { get; set; } = LogLevel.Info;

    [JsonProperty]
    public static Dictionary<string, bool> CompressibleExtensions = new();
        
    [JsonIgnore]
    private static readonly Dictionary<string, bool> ExtensionsDefaults = new()
    {
        {".jpg", true},
        {".jpeg", true},
        {".png", false},
        {".gif", false},
        {".mp4", true},
        {".mov", true},
        {".mkv", true}
    };
    
    public const int POSTGRES_VERSION = 15;

#if DEBUG
    public static readonly string FolderPath = P.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MemoryMosaicTest");
    public static readonly string FilePath = P.Combine(FolderPath, "mm_debug_settings.json");
#else
    public static readonly string FolderPath = P.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MemoryMosaic");
    public static readonly string FilePath = P.Combine(FolderPath, "mm_settings.json");
#endif

    public static void WriteSettings() => File.WriteAllText(FilePath, JsonConvert.SerializeObject(new Settings())); //https://stackoverflow.com/a/16921677

    public static void ReadSettings()
    {
        JsonConvert.DeserializeObject<Settings>(File.ReadAllText(FilePath));
        ImportFolderPath = ImportFolderPath.Replace('\\', '/');
        LibFolderPath = LibFolderPath.Replace('\\', '/');
        BackupFolderPath = BackupFolderPath.Replace('\\', '/');
        TmpFolderPath = TmpFolderPath.Replace('\\', '/');
    }

    public static void ResetSettings()
    {
    #if DEBUG
        const string root = "C:/Users/Elliott/Pictures/MemoryMosaic_Debug";
    #else
        const string root = "C:/Users/Elliott/Pictures/MemoryMosaic";
    #endif

        ImportFolderPath = $"{root}/mm_import";
        LibFolderPath = $"{root}/mm_library";
        BackupFolderPath = $"{root}/mm_backup";
        TmpFolderPath = $"{root}/mm_tmp";
        ThumbnailQuality = 7;
        LogLevel = LogLevel.Error;
        CompressibleExtensions = ExtensionsDefaults;
        
        File.WriteAllText(FilePath, JsonConvert.SerializeObject(new Settings()));
    }
}