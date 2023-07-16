using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace MemoryMosaic;

public class Settings
{
    [JsonProperty] public static string serverIP;
        
    ///The full path to the mm_import folder on the server. This is where items live before being added to the library.
    [JsonProperty] public static string importFolderPath;

    ///The full path to the library folder (mm_library) on the server.
    [JsonProperty] public static string libFolderPath;

    ///Where to backup library and database (mm_backup).
    [JsonProperty] public static string backupFolderPath;
        
    ///Where the temporary folder (mm_tmp) is on the server. This is used for things like temporarily storing thumbnail files when converting them to base64, etc.
    [JsonProperty] public static string tmpFolderPath;
        
    ///Should prompts be shown when doing things like deleting items and albums, etc.?
    [JsonProperty] public static bool showPrompts;

    ///Should items without a Date Taken be shown in albums and folders?
    [JsonProperty] public static bool displayNoDTInCV;

    ///Controls the quality of thumbnails when they are generated. Values are between 1 and 31. Lower the number, higher the quality.
    [JsonProperty] public static int thumbnailQuality;
        
    ///Acts as a kind of shortcut to where the library, import, and tmp folders are on the server. Normally, static files like images and videos cannot be displayed if they are outside of wwwroot, but by using the stuff in Startup.cs, you can.
    public const string LIB_REQUEST_PATH = "/mm_library";
    public const string IMPORT_REQUEST_PATH = "/mm_import";
    public const string TMP_REQUEST_PATH = "/mm_tmp";
    public const int POSTGRES_VERSION = 15;

    public static readonly string SettingsPath = Path.Combine(Environment.CurrentDirectory, "mm_settings.json");

    public static void WriteSettings() => File.WriteAllText(SettingsPath, JsonConvert.SerializeObject(new Settings())); //https://stackoverflow.com/a/16921677

    public static void ReadSettings()
    {
        new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory).AddJsonFile(SettingsPath).Build();
        JsonConvert.DeserializeObject<Settings>(File.ReadAllText(SettingsPath));
        importFolderPath = importFolderPath.Replace('\\', '/');
        libFolderPath = libFolderPath.Replace('\\', '/');
        backupFolderPath = backupFolderPath.Replace('\\', '/');
        tmpFolderPath = tmpFolderPath.Replace('\\', '/');
    }

    //Delete .json file and reset settings to default.
    public static void ResetSettings()
    {
        serverIP = "localhost"; 
        importFolderPath = @"C:/Users/Elliott/Documents/GitHub/Photos-Storage-Server/PSS/PSS/wwwroot/mm_import";
        libFolderPath = @"C:/Users/Elliott/Documents/GitHub/Photos-Storage-Server/PSS/PSS/wwwroot/mm_library";
        backupFolderPath = @"C:/Users/Elliott/Documents/GitHub/Photos-Storage-Server/PSS/PSS/wwwroot/mm_backup";
        tmpFolderPath = @"C:/Users/Elliott/Documents/GitHub/Photos-Storage-Server/PSS/PSS/wwwroot/mm_tmp";
        showPrompts = displayNoDTInCV = true;
        thumbnailQuality = 7;
        File.WriteAllText(SettingsPath, JsonConvert.SerializeObject(new Settings()));
    }
}