using System;
using System.IO;
using Newtonsoft.Json;
using PSS;

Console.WriteLine("-------------------------------PSS Initialization-------------------------------");
Console.WriteLine("This C# script will initialize the server for first time use.");
Console.WriteLine("Enter root path to where the PSS project is:");
string pssRoot = Console.ReadLine()!;

Console.WriteLine("\nThese settings can be changed later when PSS is running.");
Console.WriteLine("Enter folder path to where you want your library stored:");
string pss_library = Console.ReadLine()!;
if (!pss_library.EndsWith("pss_library"))
    pss_library = Path.Combine(pss_library, "pss_library");
Console.WriteLine($"Your photos and videos will be stored in \"{pss_library}\"\n");
Directory.CreateDirectory(pss_library);

Console.WriteLine("Where should uploaded items be stored before being added to your library?");
string pss_upload = Console.ReadLine()!;
if (!pss_upload.EndsWith("pss_upload"))
    pss_upload = Path.Combine(pss_upload, "pss_upload");
Console.WriteLine($"Uploaded photos and videos will be (temporarily) stored in \"{pss_upload}\"\n");
Directory.CreateDirectory(pss_upload);

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

Console.WriteLine("\nSaving settings...");
Settings.username = serverUsername;
Settings.serverIP = serverIP;
Settings.scpFlags = "-r";
Settings.uploadRootPath = pss_upload;
Settings.libFolderFullPath = pss_library;
Settings.backupFolderPath = pss_backup;
Settings.tmpFolderPath = pss_tmp;
Settings.showPrompts = true;
Settings s = new();
File.WriteAllText(Path.Combine(pssRoot, "pss_settings.json"), JsonConvert.SerializeObject(s));
Console.WriteLine("Done");

//TODO
Console.WriteLine("\nCreating PSS database...");
Console.WriteLine("Done");