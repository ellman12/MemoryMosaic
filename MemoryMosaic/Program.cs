//Most, if not all of these are very commonly used throughout the project.
global using Npgsql;
global using System;
global using System.IO;
global using System.Linq;
global using System.Diagnostics;
global using System.Collections;
global using System.Collections.Generic;
global using System.Threading.Tasks;
global using MemoryMosaic.Backend;
global using MemoryMosaic.Backend.Enums;
global using MemoryMosaic.Backend.Records;
global using Microsoft.VisualBasic.FileIO;
global using C = MemoryMosaic.Backend.Connection;
global using D = DateTakenExtractor.DateTakenExtractor;
global using F = MemoryMosaic.Backend.Functions;
global using L = MemoryMosaic.Backend.Logger;
global using M = MemoryMosaic.Backend.Maintenance;
global using S = MemoryMosaic.Settings;
global using SearchOption = System.IO.SearchOption;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace MemoryMosaic;

public class Program
{
    public static void Main(string[] args)
    {
        //Populate config with default values if file doesn't exist. If exists, read in values.
        if (File.Exists(Environment.CurrentDirectory + "/mm_settings.json") && File.ReadAllText(Environment.CurrentDirectory + "/mm_settings.json") != "")
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