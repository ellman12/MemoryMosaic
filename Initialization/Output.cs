namespace Initialization;

public static class Output
{
	///Writes a colored message to the console.
	public static void WriteLine(string message, ConsoleColor color)
	{
		Console.ForegroundColor = color;
		Console.WriteLine(message);
		Console.ResetColor();
	}
}