namespace PSS.Backend;

///<summary>Used for printing messages of variable importance to the terminal.</summary>
public static class Logger
{
	public static LogLevel logLevel;

	private static void SetConsoleColor(LogLevel itemImportance)
	{
		Console.ForegroundColor = itemImportance switch
		{
			LogLevel.Info => ConsoleColor.Cyan,
			LogLevel.Debug => ConsoleColor.Green,
			LogLevel.Warning => ConsoleColor.Yellow,
			LogLevel.Error => ConsoleColor.Red,
			LogLevel.None => throw new ArgumentException(),
			_ => throw new ArgumentException()
		};
	}

	private static void ResetConsoleColor() => Console.ResetColor();
	
	public static void LogLine(object value, LogLevel itemImportance)
	{
		if (itemImportance == LogLevel.None || itemImportance > logLevel) return;
		SetConsoleColor(itemImportance);
		Console.WriteLine($"{DateTime.Now} {value}");
		ResetConsoleColor();
	}

	public static void LogException(Exception e, string methodName) => LogLine($"****\nException raised in {methodName}: {e.Message}\n****", LogLevel.Error);

	public static void LogException(NpgsqlException e, string methodName) => LogLine($"****\nException raised in {methodName}: {e.ErrorCode} {e.Message}\n****", LogLevel.Error);
}