using System.Diagnostics;
using Newtonsoft.Json;
using MemoryMosaic;

string psqlPath = $"C:/Program Files/PostgreSQL/{Settings.POSTGRES_VERSION}/bin/psql.exe";

ConsoleColor ogColor = Console.ForegroundColor;
Console.WriteLine("-------------------------------MemoryMosaic Initialization-------------------------------");
Console.WriteLine("This C# script will initialize the server for first time use.");
Console.WriteLine("Enter root path to where the MM project (MemoryMosaic.csproj) is.\nIt should look something like:\nC:/Users/Elliott/Documents/GitHub/MemoryMosaic/MemoryMosaic\nEnter it here:");
string mmRoot = Console.ReadLine()!;

Console.WriteLine("\nThese settings can be changed later when MM is running.");
Console.WriteLine("Enter folder path to where you want your library stored.");
string mm_library = Console.ReadLine()!;
if (!mm_library.EndsWith("mm_library"))
      mm_library = Path.Combine(mm_library, "mm_library");
Console.WriteLine($"Your photos and videos will be stored in \"{mm_library}\"\n");
Directory.CreateDirectory(mm_library);

Console.WriteLine("Where should items be stored before being imported in to your library?");
string mm_import = Console.ReadLine()!;
if (!mm_import.EndsWith("mm_import"))
      mm_import = Path.Combine(mm_import, "mm_import");
Console.WriteLine($"Uploaded photos and videos will be (temporarily) stored in \"{mm_import}\"\n");
Directory.CreateDirectory(mm_import);

Console.WriteLine("Enter folder path to where temporary items should be stored:");
string mm_tmp = Console.ReadLine()!;
if (!mm_tmp.EndsWith("mm_tmp"))
      mm_tmp = Path.Combine(mm_tmp, "mm_tmp");
Console.WriteLine($"Temporary files will be stored in \"{mm_tmp}\"\n");
Directory.CreateDirectory(mm_tmp);

Console.WriteLine("Where should your library and the database be backed up to?");
string mm_backup = Console.ReadLine()!;
if (!mm_backup.EndsWith("mm_backup"))
      mm_backup = Path.Combine(mm_backup, "mm_backup");
Console.WriteLine($"Your photos and videos will be backed up to \"{mm_backup}\"\n");
Directory.CreateDirectory(mm_backup);

Console.WriteLine("Enter the username of the server where MM will be running:");
string serverUsername = Console.ReadLine()!;

Console.WriteLine("\nEnter ip of the server:");
string serverIP = Console.ReadLine()!;

Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("\nSaving settings...");
Settings.serverIP = serverIP;
Settings.importFolderPath = mm_import;
Settings.libFolderPath = mm_library;
Settings.backupFolderPath = mm_backup;
Settings.tmpFolderPath = mm_tmp;
Settings.showPrompts = true;
Settings s = new();
File.WriteAllText(Path.Combine(mmRoot, "mm_settings.json"), JsonConvert.SerializeObject(s));
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

//Create just the database.
ProcessStartInfo dbCreateCmd = new()
{
    FileName = psqlPath,
    Arguments = $"-U postgres -f \"{Path.Combine(mmRoot, "Backend/SQL Scripts/Create Database.sql")}\""
};
Process.Start(dbCreateCmd)!.WaitForExit();

//Everything else.
ProcessStartInfo dbInitCmd = new()
{
    FileName = psqlPath,
    Arguments = $"-U postgres -d MemoryMosaic -f \"{Path.Combine(mmRoot, "Backend/SQL Scripts/Create Tables.sql")}\""
}; //Note the "-d MemoryMosaic" ↑. That is necessary to tell it which DB to connect to/use. Thus why the first command needs to be run first and separately.
Process.Start(dbInitCmd)!.WaitForExit(); //User needs to enter password to get into database to run this ↑ script 

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Done Setting up Database");
Console.WriteLine("MemoryMosaic is now fully initialized and ready to run!");
Console.ForegroundColor = ogColor;
Console.WriteLine("Now, to run MemoryMosaic, open a terminal window (preferably as admin/root), navigate to");
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine(mmRoot);
Console.ForegroundColor = ogColor;
Console.WriteLine("and run this command:");
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("dotnet run");