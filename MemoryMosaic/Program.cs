//Most, if not all of these are very commonly used throughout the project.
global using Npgsql;
global using System;
global using System.IO;
global using System.Data;
global using System.Linq;
global using System.Diagnostics;
global using System.Collections;
global using System.Collections.Generic;
global using System.Threading.Tasks;
global using MemoryMosaic.Backend;
global using MemoryMosaic.Enums;
global using MemoryMosaic.Models;
global using MemoryMosaic.Extensions;
global using Microsoft.VisualBasic.FileIO;
global using C = MemoryMosaic.Backend.Compressor;
global using D = MemoryMosaic.Backend.Database;
global using DTE = DateTakenExtractor.DateTakenExtractor;
global using F = MemoryMosaic.Backend.Functions;
global using FF = MemoryMosaic.Backend.FFmpeg;
global using L = MemoryMosaic.Backend.Logger;
global using M = MemoryMosaic.Backend.Maintenance;
global using P = System.IO.Path;
global using S = MemoryMosaic.Settings;
global using SearchOption = System.IO.SearchOption;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace MemoryMosaic;

public sealed class Program
{
#if DEBUG
    public const bool Debug = true;
#else
    public const bool Debug = false;
#endif

    public const string Version = "3.0.0";

    public static void Main(string[] args)
    {
        Directory.CreateDirectory(S.FolderPath);

        if (File.Exists(S.FilePath) && !String.IsNullOrWhiteSpace(File.ReadAllText(S.FilePath)))
            S.ReadSettings();
        else
            S.ResetSettings();

        Pages.Settings.whenWentOnline = DateTime.Now;

        CreateHostBuilder(args).Build().Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
}