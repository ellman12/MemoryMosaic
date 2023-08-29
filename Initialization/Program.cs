using System.Diagnostics;
using Initialization;
using MemoryMosaic;
using MemoryMosaic.Backend.Enums;

#region Setup
LogLevel originalLogLevel = Settings.logLevel;
Settings.logLevel = LogLevel.Error;

string PsqlPath = $"C:/Program Files/PostgreSQL/{Settings.POSTGRES_VERSION}/bin/psql.exe";

int index = Environment.CurrentDirectory.LastIndexOf("Initialization", StringComparison.Ordinal);
string SolutionRoot = Environment.CurrentDirectory.Substring(0, index).Replace('\\', '/');
#endregion

if (!File.Exists(PsqlPath))
{
	Output.WriteLine($"PostgreSQL {Settings.POSTGRES_VERSION} not installed! Download it here: https://www.postgresql.org/download/", ConsoleColor.Red);
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
	Output.WriteLine("Creating MemoryMosaicTest", ConsoleColor.Cyan);	
	Database.Create("MemoryMosaicTest");
	
	

	string testLibPath = Input.GetFolderPath("Enter path to where the library should be stored: ");
	Console.WriteLine(testLibPath);

	// var directoryInfo = new DirectoryInfo("");
	// if (Directory.Exists(testLibPath))
	
	
	
	string testImportPath = Input.GetFolderPath("Enter path to where items waiting to be imported should be stored: ");
	string testTmpPath = Input.GetFolderPath("Enter path to where temporary items should be stored: ");
	string testBackupPath = Input.GetFolderPath("Enter path to where backups should be stored: ");
	
	
	//TODO: test the paths and create the folders
	// void VerifyPath?
}


Settings.logLevel = originalLogLevel;