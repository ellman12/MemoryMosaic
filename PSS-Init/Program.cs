﻿using System.Diagnostics;
using Newtonsoft.Json;
using PSS;

const string psqlPath = "C:/Program Files/PostgreSQL/14/bin/psql.exe";

ConsoleColor ogColor = Console.ForegroundColor;
Console.WriteLine("-------------------------------PSS Initialization-------------------------------");
Console.WriteLine("This C# script will initialize the server for first time use.");
Console.WriteLine("Enter root path to where the PSS project (PSS.csproj) is.\nIt should look something like:\nC:/Users/Elliott/Documents/GitHub/Photos-Storage-Server/PSS/PSS\nEnter it here:");
string pssRoot = Console.ReadLine()!;

Console.WriteLine("\nThese settings can be changed later when PSS is running.");
Console.WriteLine("Enter folder path to where you want your library stored.");
string pss_library = Console.ReadLine()!;
if (!pss_library.EndsWith("pss_library"))
      pss_library = Path.Combine(pss_library, "pss_library");
Console.WriteLine($"Your photos and videos will be stored in \"{pss_library}\"\n");
Directory.CreateDirectory(pss_library);

Console.WriteLine("Where should items be stored before being imported in to your library?");
string pss_import = Console.ReadLine()!;
if (!pss_import.EndsWith("pss_import"))
      pss_import = Path.Combine(pss_import, "pss_import");
Console.WriteLine($"Uploaded photos and videos will be (temporarily) stored in \"{pss_import}\"\n");
Directory.CreateDirectory(pss_import);

Console.WriteLine("Enter folder path to where temporary items should be stored:");
string pss_tmp = Console.ReadLine()!;
if (!pss_tmp.EndsWith("pss_tmp"))
      pss_tmp = Path.Combine(pss_tmp, "pss_tmp");
Console.WriteLine($"Temporary files will be stored in \"{pss_tmp}\"\n");
Directory.CreateDirectory(pss_tmp);

Console.WriteLine("Where should your library and the database be backed up to?");
string pss_backup = Console.ReadLine()!;
if (!pss_backup.EndsWith("pss_backup"))
      pss_backup = Path.Combine(pss_backup, "pss_backup");
Console.WriteLine($"Your photos and videos will be backed up to \"{pss_backup}\"\n");
Directory.CreateDirectory(pss_backup);

Console.WriteLine("Enter the username of the server where PSS will be running:");
string serverUsername = Console.ReadLine()!;

Console.WriteLine("\nEnter ip of the server:");
string serverIP = Console.ReadLine()!;

Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("\nSaving settings...");
Settings.serverIP = serverIP;
Settings.importFolderPath = pss_import;
Settings.libFolderPath = pss_library;
Settings.backupFolderPath = pss_backup;
Settings.tmpFolderPath = pss_tmp;
Settings.showPrompts = true;
Settings s = new();
File.WriteAllText(Path.Combine(pssRoot, "pss_settings.json"), JsonConvert.SerializeObject(s));
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Done\n");

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("Database Time");
Console.WriteLine("To get started, install Postgres with all the default settings: https://www.postgresql.org/download/");
Console.WriteLine("Then when that's done, come back here and press any key to begin setting up the database. Press 'Q' or ^C to abort.");
if (Console.ReadKey(true).Key == ConsoleKey.Q)
{
     Console.ForegroundColor = ConsoleColor.DarkRed;
     Console.WriteLine("Aborting...");
     return;
}

Console.ForegroundColor = ogColor;

//Create just the database PSS.
ProcessStartInfo dbCreateCmd = new()
{
    FileName = psqlPath,
    Arguments = $"-U postgres -f \"{Path.Combine(pssRoot, "Backend/SQL Scripts/Create Database.sql")}\""
};
Process.Start(dbCreateCmd)!.WaitForExit();

//Everything else.
ProcessStartInfo dbInitCmd = new()
{
    FileName = psqlPath,
    Arguments = $"-U postgres -d PSS -f \"{Path.Combine(pssRoot, "Backend/SQL Scripts/Create Tables.sql")}\""
}; //Note the "-d PSS" ↑. That is necessary to tell it which DB to connect to/use. Thus why the first command needs to be run first and separately.
Process.Start(dbInitCmd)!.WaitForExit(); //User needs to enter password to get into database to run this ↑ script 

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Done Setting up Database");
Console.WriteLine("PSS is now fully initialized and ready to run!");
Console.ForegroundColor = ogColor;
Console.WriteLine("Now, to run PSS, open a terminal window (preferably as admin/root), navigate to");
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine(pssRoot);
Console.ForegroundColor = ogColor;
Console.WriteLine("and run this command:");
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("dotnet run");