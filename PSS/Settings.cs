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

        /// <summary>
        /// The full path to the library folder on the server.
        /// </summary>
        [JsonProperty] public static string libFolderFullPath;
        
        public static void WriteSettings()
        {
            //https://stackoverflow.com/a/16921677
            Settings settings = new();
            string json = JsonConvert.SerializeObject(settings);
            File.WriteAllText(Environment.CurrentDirectory + "/pss_settings.json", json);
        }

        public static void ReadSettings()
        {
            IConfigurationRoot config = new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory).AddJsonFile(Environment.CurrentDirectory + "/pss_settings.json").Build();
            Settings settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Environment.CurrentDirectory + "/pss_settings.json"));
            uploadRootPath = uploadRootPath.Replace('\\', '/');
            libFolderFullPath = libFolderFullPath.Replace('\\', '/');
        }

        //Delete .json file and reset settings to default.
        public static void ResetSettings()
        {
            Settings defaultSettings = new(); //I wish I didn't need to make an instance :/
            username = "elliott";
            serverIP = "localhost";
            scpFlags = "-r";
            File.WriteAllText(Environment.CurrentDirectory + "/pss_settings.json", JsonConvert.SerializeObject(defaultSettings));
        }
    }
}