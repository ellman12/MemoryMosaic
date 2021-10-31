using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace PSS
{
    public class Settings
    {
        [JsonProperty] public static string username;
        [JsonProperty] public static string serverIP;
        [JsonProperty] public static string scpFlags;
        [JsonProperty] public static string uploadRootPath;
        [JsonProperty] public static string libraryRootPath;

        public static void WriteSettings()
        {
            //https://stackoverflow.com/a/16921677
            Settings settings = new();
            string json = JsonConvert.SerializeObject(settings);
            File.WriteAllText(Environment.CurrentDirectory + "/pss_settings.json", json);
        }

        public static void ReadSettings()
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile(Environment.CurrentDirectory + "/pss_settings.json").Build();
            Settings settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Environment.CurrentDirectory + "/pss_settings.json"));
            uploadRootPath = uploadRootPath.Replace('\\', '/');
            libraryRootPath = libraryRootPath.Replace('\\', '/');
        }

        //Delete .json file and reset settings to default.
        public static void ResetSettings()
        {
            Settings defaultSettings = new(); //I wish I didn't need to make an instance :/
            username = "elliott";
            serverIP = "localhost";
            scpFlags = "-r";
            uploadRootPath = @"C:/Users/Elliott/Documents/GitHub/Photos-Storage-Server/PSS/wwwroot/pss_upload"; //TODO: temp
            libraryRootPath = @"C:/Users/Elliott/Documents/GitHub/Photos-Storage-Server/PSS/wwwroot/pss_library"; //TODO: temp

            File.WriteAllText(Environment.CurrentDirectory + "/pss_settings.json", JsonConvert.SerializeObject(defaultSettings));
        }
    }
}