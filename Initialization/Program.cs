using System.Diagnostics;
using Initialization;
using MemoryMosaic;
using MemoryMosaic.Backend.Enums;

LogLevel originalLogLevel = Settings.logLevel;
Settings.logLevel = LogLevel.Error;

string PsqlPath = $"C:/Program Files/PostgreSQL/{Settings.POSTGRES_VERSION}/bin/psql.exe";

ProcessStartInfo startInfo = new()
{
	FileName = PsqlPath,
	Arguments = "--version",
	RedirectStandardOutput = true,
	CreateNoWindow = true
};
Process process = new() {StartInfo = startInfo};
process.Start();
process.WaitForExit();
string output = process.StandardOutput.ReadToEnd();

if (!output.Contains("psql (PostgreSQL)"))
{
	Output.WriteLine($"PostgreSQL {Settings.POSTGRES_VERSION} not installed! Download it here: https://www.postgresql.org/download/", ConsoleColor.Red);
	return;
}

bool debug = Input.GetYN("Create a separate instance of MemoryMosaic for debugging and testing?", false);

if (debug)
{
	if (Database.Exists("MemoryMosaicTest") && Input.GetYN("Test database exists. Overwrite?", true))
		Database.Delete("MemoryMosaicTest");
	Database.Create("MemoryMosaicTest");
	
	
}


Settings.logLevel = originalLogLevel;