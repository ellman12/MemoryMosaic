namespace Initialization;

public static class Constants
{
	public const string PostgresVersion = "15";

	public const string PostgresBinPath = $"C:/Program Files/PostgreSQL/{PostgresVersion}/bin"; 
	
	public const string PsqlPath = $"{PostgresBinPath}/psql.exe";

	public static readonly string CreateTablesFilePath;

	static Constants()
	{
		int index = Environment.CurrentDirectory.LastIndexOf("bin", StringComparison.Ordinal);
		string initializationRoot = Environment.CurrentDirectory.Substring(0, index).Replace('\\', '/');
		CreateTablesFilePath = Path.Combine(initializationRoot, "Create Tables.sql");
	}
}