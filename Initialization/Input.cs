namespace Initialization;

public static class Input
{
	///Writes a message to the console, and prompts for a y/n input.
	public static bool GetYN(string prompt, bool defaultValue)
	{
		prompt = $"{prompt} [{(defaultValue ? 'Y' : 'y')}/{(defaultValue ? 'n' : 'N')}] ";

		Console.Write(prompt);

		ConsoleKeyInfo key = Console.ReadKey();
		Console.WriteLine();

		return key.Key switch
		{
			ConsoleKey.Enter => defaultValue,
			ConsoleKey.Y => true,
			ConsoleKey.N => false,
			_ => defaultValue
		};
	}

	///Prompts for input until a proper folder path is entered.
	public static string GetFolderPath(string prompt) => GetCheck(input => !String.IsNullOrWhiteSpace(input) && Path.IsPathFullyQualified(input) && Path.IsPathRooted(input) && !Path.HasExtension(input), prompt);

	///Prompts for input until check returns true.
	private static string GetCheck(Predicate<string> check, string prompt)
	{
		string input;
		Console.Write(prompt);
		while (true)
		{
			input = Console.ReadLine()!;
			if (check(input)) break;
			Output.WriteLine("Invalid format. Please try again.", ConsoleColor.Red);
		}
		return input;
	}
}