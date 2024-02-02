namespace MemoryMosaic.Backend;

///<summary>Used for printing messages of variable importance to the terminal.</summary>
public static class Logger
{
	private static void SetConsoleColor(LogLevel itemImportance)
	{
		Console.ForegroundColor = itemImportance switch
		{
			LogLevel.None => throw new ArgumentException(),
			LogLevel.Debug => ConsoleColor.Green,
			LogLevel.Info => ConsoleColor.Cyan,
			LogLevel.Warn => ConsoleColor.Yellow,
			LogLevel.Error => ConsoleColor.Red,
			_ => throw new ArgumentException()
		};
	}

	private static string GetCallingMethodName() => new StackTrace().GetFrame(2)!.GetMethod()!.Name; //If it's set to 1 it'd print LogLine.

	public static void LogLine(object value, LogLevel itemImportance)
	{
		if (itemImportance == LogLevel.None || S.LogLevel > itemImportance) return;
		SetConsoleColor(itemImportance);
		Console.WriteLine($"{DateTime.Now} {value}");
		Console.ResetColor();
	}

	public static void LogException(Exception e) => LogLine($"****\nException raised in {GetCallingMethodName()}: {e.Message}\n****", LogLevel.Error);

	public static void LogException(NpgsqlException e) => LogLine($"****\nException raised in {GetCallingMethodName()}: {e.ErrorCode} {e.Message}\n****", LogLevel.Error);
}