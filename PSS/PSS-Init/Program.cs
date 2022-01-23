using System;
using System.IO;

Console.WriteLine("-------------------------------PSS Initialization-------------------------------");
Console.WriteLine("These settings can be changed later.");

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

Console.WriteLine("Enter username of server where PSS will be running:");
string serverUsername = Console.ReadLine()!;

Console.WriteLine("Enter ip of the server:");
string serverIP = Console.ReadLine()!;

//TODO
Console.WriteLine("Creating PSS database...");
Console.WriteLine("Done");