namespace MemoryMosaic.Backend;

///Contains static methods for interacting with FFmpeg.
public static class FFmpeg
{
	///<summary>Given the absolute path to a image/video file, use FFmpeg to generate a compressed thumbnail of the image or the first frame.</summary>
	///<param name="filePath">The absolute path to where the file is.</param>
	///<returns>A base64 string representing the thumbnail.</returns>
	public static string GenerateThumbnail(string filePath) => GenerateThumbnailAsync(filePath).Result;

	///<summary>Given the absolute path to a image/video file, use FFmpeg to generate a compressed thumbnail of the image or the first frame.</summary>
	///<param name="filePath">The absolute path to where the file is.</param>
	///<returns>A base64 string representing the thumbnail.</returns>
	public static async Task<string> GenerateThumbnailAsync(string filePath)
	{
		string thumbnailFullPath = P.Join(S.TmpFolderPath, Guid.NewGuid() + ".jpg");
		ProcessStartInfo ffmpegInfo = new()
		{
			CreateNoWindow = true,
			FileName = "ffmpeg",
			Arguments = $"-i \"{filePath}\" -loglevel quiet -vf \"select=eq(n\\,0)\" -vf scale=320:-2 -q:v {S.ThumbnailQuality} \"{thumbnailFullPath}\""
		};

		Process ffmpegProcess = Process.Start(ffmpegInfo) ?? throw new InvalidOperationException();
		await ffmpegProcess.WaitForExitAsync();

		byte[] bytes = await File.ReadAllBytesAsync(thumbnailFullPath);

		try { File.Delete(thumbnailFullPath); }
		catch (Exception e) { L.LogException(e); }

		return Convert.ToBase64String(bytes);
	}

	///Returns the width, height, and aspect ratio of a file, given its full path.
	public static (uint, uint, string) GetDimensions(string filePath)
	{
		ProcessStartInfo ffprobeInfo = new()
		{
			CreateNoWindow = true,
			FileName = "ffprobe",
			Arguments = $"-v error -select_streams v:0 -show_entries stream=width,height,display_aspect_ratio -of default=nw=1:nk=1 \"{filePath}\"",
			RedirectStandardOutput = true
		};
	
		Process ffprobeProcess = Process.Start(ffprobeInfo) ?? throw new InvalidOperationException();
		ffprobeProcess.WaitForExit();

		uint width = Convert.ToUInt32(ffprobeProcess.StandardOutput.ReadLine());
		uint height = Convert.ToUInt32(ffprobeProcess.StandardOutput.ReadLine());
		string aspectRatio = ffprobeProcess.StandardOutput.ReadLine()!;

		return (width, height, aspectRatio);
	}
}