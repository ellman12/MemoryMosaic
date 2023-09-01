global using C = Initialization.Constants;
using Initialization;

if (!File.Exists(C.PsqlPath))
{
	Output.WriteLine($"PostgreSQL {C.PostgresVersion} not installed! Download it here: https://www.postgresql.org/download/", ConsoleColor.Red);
	return;
}

#if DEBUG
const bool debug = true;
#else
bool debug = Input.GetYN("Create a separate instance of MemoryMosaic for debugging and testing?", false);
#endif

if (debug)
{
	if (Database.Exists("MemoryMosaicTest") && Input.GetYN("Test database exists. Overwrite?", true))
	{
		Output.WriteLine("Deleting MemoryMosaicTest", ConsoleColor.Cyan);
		Database.Delete("MemoryMosaicTest");
	}

	if (!Database.Exists("MemoryMosaicTest"))
	{
		Output.WriteLine("Creating MemoryMosaicTest", ConsoleColor.Cyan);
		Database.Create("MemoryMosaicTest");
		Output.WriteLine("Creating MemoryMosaicTest's Tables", ConsoleColor.Cyan);
		Database.CreateTables("MemoryMosaicTest");
	}

	string testLibPath = Input.GetFolderPath("Enter path for mm_library, where the library should be stored: ");
	VerifyPathAndCreateFolder(ref testLibPath, "mm_library");

	string testImportPath = Input.GetFolderPath("Enter path for mm_import, where items waiting to be imported should be stored: ");
	VerifyPathAndCreateFolder(ref testImportPath, "mm_import");

	string testTmpPath = Input.GetFolderPath("Enter path to mm_tmp, where temporary files should be stored: ");
	VerifyPathAndCreateFolder(ref testTmpPath, "mm_tmp");

	string testBackupPath = Input.GetFolderPath("Enter path to mm_backup, where backups should be stored: ");
	VerifyPathAndCreateFolder(ref testBackupPath, "mm_backup");


}

return;

void VerifyPathAndCreateFolder(ref string path, string mmFolder)
{
	path = path.Trim();
	if (path.Last() is '/' or '\\')
		path = path.Substring(0, path.Length - 1);
	
	if (!path.EndsWith(mmFolder))
		path = Path.Join(path, mmFolder);
}