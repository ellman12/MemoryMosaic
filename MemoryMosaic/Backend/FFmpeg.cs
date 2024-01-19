namespace MemoryMosaic.Backend;

///Contains static methods for interacting with FFmpeg.
public static class FFmpeg
{
	///<summary>Given the absolute path to a image/video file, use FFmpeg to generate a compressed thumbnail of the image or the first frame.</summary>
	///<param name="filePath">The absolute path to where the file is.</param>
	///<returns>A base64 string representing the thumbnail.</returns>
	public static string GenerateThumbnail(string filePath)
	{
		string thumbnailFullPath = P.Join(S.TmpFolderPath, Guid.NewGuid() + ".jpg");
		ProcessStartInfo ffmpegInfo = new()
		{
			CreateNoWindow = true,
			FileName = "ffmpeg",
			Arguments = $"-i \"{filePath}\" -loglevel quiet -vf \"select=eq(n\\,0)\" -vf scale=320:-2 -q:v {S.ThumbnailQuality} \"{thumbnailFullPath}\""
		};

		Process ffmpegProcess = Process.Start(ffmpegInfo) ?? throw new InvalidOperationException();
		ffmpegProcess.WaitForExit();

		byte[] bytes = File.ReadAllBytes(thumbnailFullPath);

		try { File.Delete(thumbnailFullPath); }
		catch (Exception e) { L.LogException(e); }

		return Convert.ToBase64String(bytes);
	}

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
        
	
}