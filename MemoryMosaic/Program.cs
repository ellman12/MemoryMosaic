//Most, if not all of these are very commonly used throughout the project.
global using System;
global using System.IO;
global using Npgsql;
global using MemoryMosaic.Backend;
global using MemoryMosaic.Backend.Enums;
global using MemoryMosaic.Backend.Records;
global using C = MemoryMosaic.Backend.Connection;
global using D = DateTakenExtractor.DateTakenExtractor;
global using F = MemoryMosaic.Backend.Functions;
global using L = MemoryMosaic.Backend.Logger;
global using M = MemoryMosaic.Backend.Maintenance;
global using S = MemoryMosaic.Settings;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace MemoryMosaic;

public class Program
{
    public static void Main(string[] args)
    {
        //Populate config with default values if file doesn't exist. If exists, read in values.
        if (File.Exists(Environment.CurrentDirectory + "/pss_settings.json") && File.ReadAllText(Environment.CurrentDirectory + "/pss_settings.json") != "")
            Settings.ReadSettings();
        else
            Settings.ResetSettings();

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