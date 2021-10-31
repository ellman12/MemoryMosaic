using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace PSS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Populate config with default values if file doesn't exist. If exists, read in values.
            if (File.Exists(Environment.CurrentDirectory + "/pss_settings.json") && File.ReadAllText(Environment.CurrentDirectory + "/pss_settings.json") != "")
                Settings.ReadSettings();
            else
                Settings.ResetSettings();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
