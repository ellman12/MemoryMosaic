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
}