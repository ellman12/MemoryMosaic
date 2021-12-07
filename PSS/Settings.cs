using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace PSS
{
    public class Settings
    {
        /// <summary>
        /// Server username. Used for scp command for uploading
        /// </summary>
        [JsonProperty] public static string username;
        [JsonProperty] public static string serverIP;
        [JsonProperty] public static string scpFlags;

        /// <summary>
        /// Where scp should upload files. 
        /// </summary>
        [JsonProperty] public static string uploadRootPath;

        /// <summary>
        /// The full path to the library folder on the server.
        /// </summary>
        [JsonProperty] public static string libFolderFullPath;

        /// <summary>
        /// Should prompts be shown when doing things like deleting items and albums, etc.?
        /// </summary>
        [JsonProperty] public static bool showPrompts;
        
        /// <summary>
        /// Used (in Startup.cs) for accessing files outside of wwwroot.
        /// </summary>
        public const string requestPath = "/pss_library";

        public static void WriteSettings()
        {
            //https://stackoverflow.com/a/16921677
            Settings settings = new();
            string json = JsonConvert.SerializeObject(settings);
            File.WriteAllText(Environment.CurrentDirectory + "/pss_settings.json", json);
        }

        public static void ReadSettings()
        {
            new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory).AddJsonFile(Environment.CurrentDirectory + "/pss_settings.json").Build();
            JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Environment.CurrentDirectory + "/pss_settings.json"));
            uploadRootPath = uploadRootPath.Replace('\\', '/');
            libFolderFullPath = libFolderFullPath.Replace('\\', '/');
        }

        //Delete .json file and reset settings to default.
        public static void ResetSettings()
        {
            username = "elliott";
            serverIP = "localhost"; 
            scpFlags = "-r";
            uploadRootPath = @"C:/Users/Elliott/Documents/GitHub/Photos-Storage-Server/PSS/wwwroot/pss_upload"; //TODO: temp
            libFolderFullPath = @"C:/Users/Elliott/Documents/GitHub/Photos-Storage-Server/PSS/wwwroot/pss_library"; //TODO: temp
            showPrompts = true;
            File.WriteAllText(Environment.CurrentDirectory + "/pss_settings.json", JsonConvert.SerializeObject(new Settings()));
        }
    }
}