namespace MemoryMosaic.Backend;

///Facilitates reading and writing GPS coordinates for items.
public static class CoordinateExtractor
{
	///Given a file path, use ExifTool to read the coordinates of the file.
	public static (double, double)? ReadCoordinates(string fullPath)
	{
		using Process p = new();
		p.StartInfo = new ProcessStartInfo
		{
			FileName = "exiftool",
			Arguments = $"\"{fullPath}\" -GPSlatitude -GPSlongitude -s3 -c \"%+6f\"",
			CreateNoWindow = true,
			RedirectStandardInput = true,
			RedirectStandardOutput = true, //Necessary for reading output of ExifTool.
			WindowStyle = ProcessWindowStyle.Hidden
		};
		p.Start();
		p.WaitForExit();

		if (Double.TryParse(p.StandardOutput.ReadLine(), out var latitude) && Double.TryParse(p.StandardOutput.ReadLine(), out var longitude))
			return new (latitude, longitude);

		return null;
	}

	///Given a file path, use ExifTool to update the coordinates of the file.
	public static void UpdateCoordinates(string fullPath, double? latitude, double? longitude)
	{
		using Process p = new();
		p.StartInfo = new ProcessStartInfo
		{
			FileName = "exiftool",
			Arguments = $"\"{fullPath}\" -GPSlatitude={(latitude == null ? "" : latitude)} -GPSlongitude={(longitude == null ? "" : longitude)}",
			CreateNoWindow = true,
			WindowStyle = ProcessWindowStyle.Hidden
		};
		p.Start();
		p.WaitForExit();
	}
}