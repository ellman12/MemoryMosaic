//Most, if not all of these are very commonly used throughout the project.
global using System;
global using System.IO;
global using Npgsql;
global using PSS.Backend;
global using PSS.Backend.Enums;
global using PSS.Backend.Records;
global using C = PSS.Backend.Connection;
global using D = DateTakenExtractor.DateTakenExtractor;
global using F = PSS.Backend.Functions;
global using M = PSS.Backend.Maintenance;
global using S = PSS.Settings;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace PSS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Populate config with default values if file doesn't exist. If exists, read in values.
            if (File.Exists(Environment.CurrentDirectory + "/pss_settings.json") && File.ReadAllText(Environment.CurrentDirectory + "/pss_settings.json") != "")
                S.ReadSettings();
            else
                S.ResetSettings();

            Pages.Settings.whenWentOnline = DateTime.Now;

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
