global using C = Initialization.Constants;
using Initialization;
using Microsoft.VisualBasic.FileIO;

if (!File.Exists(C.PsqlPath))
{
	Output.WriteLine($"PostgreSQL {C.PostgresVersion} not installed! Download it here: https://www.postgresql.org/download/", ConsoleColor.Red);
	return;
}

bool debug = Input.GetYN("Is this a debugging and testing instance of MemoryMosaic?", false);
string name = debug ? "MemoryMosaicTest" : "MemoryMosaic";

if (Database.Exists(name) && Input.GetYN("Database exists. Overwrite?", true) && Input.GetYN("Are you sure?", true))
{
	Output.WriteLine($"Deleting {name}", ConsoleColor.Cyan);
	Database.Delete(name);
}

if (!Database.Exists(name))
{
	Output.WriteLine($"Creating {name} database", ConsoleColor.Cyan);
	Database.Create(name);
	Output.WriteLine("Creating tables", ConsoleColor.Cyan);
	Database.CreateTables(name);
}

string testLibPath = Input.GetFolderPath("Enter path for mm_library, where the library should be stored: ");
VerifyPathAndCreateFolder(ref testLibPath, "mm_library");

string testImportPath = Input.GetFolderPath("Enter path for mm_import, where items waiting to be imported should be stored: ");
VerifyPathAndCreateFolder(ref testImportPath, "mm_import");

string testTmpPath = Input.GetFolderPath("Enter path to mm_tmp, where temporary files should be stored: ");
VerifyPathAndCreateFolder(ref testTmpPath, "mm_tmp");

string testBackupPath = Input.GetFolderPath("Enter path to mm_backup, where backups should be stored: ");
VerifyPathAndCreateFolder(ref testBackupPath, "mm_backup");

Output.WriteLine("MemoryMosaic is now ready to use.", ConsoleColor.Green);

return;

void VerifyPathAndCreateFolder(ref string path, string mmFolder)
{
	path = path.Trim();
	if (path.Last() is '/' or '\\')
		path = path.Substring(0, path.Length - 1);

	if (!path.EndsWith(mmFolder))
		path = Path.Join(path, mmFolder);

	if (Directory.Exists(path) && Input.GetYN($"{path} exists and has {Directory.EnumerateFiles(path).Count()} files. Overwrite?", true) && Input.GetYN("Are you sure?", true))
		FileSystem.DeleteDirectory(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
	Directory.CreateDirectory(path);
}