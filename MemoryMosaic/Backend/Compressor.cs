using System.Threading;
using ThreadState = System.Threading.ThreadState;

namespace MemoryMosaic.Backend;

///Manages automatically compressing <see cref="Media"/> in the background.
public static class Compressor
{
	public static Media? Current { get; set; } //TODO: set this back to private set;

	public static PriorityQueue<Media, bool> Items { get; } = new();

	public static bool Compressing { get; private set; }

	private static readonly Thread compressionThread;

	static Compressor()
	{
		compressionThread = new Thread(CompressItems)
		{
			IsBackground = true,
			Priority = ThreadPriority.Lowest
		};
	}

	public static void Enqueue(Media item)
	{
		Items.Enqueue(item, item.Video);

		if (compressionThread.ThreadState != ThreadState.Running)
			compressionThread.Start();
	}

	private static void CompressItems()
	{
		if (Compressing)
			return;

		L.LogLine($"Begin {nameof(CompressItems)}", LogLevel.Debug);

		Compressing = true;

		while (Items.TryDequeue(out Media? item, out _))
		{
			Current = item;
			Compress(item);
		}

		Compressing = false;

		L.LogLine($"Finish {nameof(CompressItems)}", LogLevel.Debug);
	}

	///Lightly compresses an item using FFmpeg's "-q:v 1" parameter. Uses ExifTool to copy the metadata from the original to the new item, then moves the original item to mm_tmp/Before Compression/.
	private static void Compress(Media item)
	{
		L.LogLine($"Begin compressing {item.Path}", LogLevel.Debug);

		string originalFilePath = item.FullPath;
		string ext = P.GetExtension(originalFilePath);
		string compressedFilePath = originalFilePath.Replace(ext, $"_compressed{ext}");

		ProcessStartInfo ffmpegInfo = new()
		{
			CreateNoWindow = true,
			FileName = "ffmpeg",
			Arguments = $"-y -v error -i \"{originalFilePath}\" -q:v 1 \"{compressedFilePath}\""
		};
		var ffmpegProcess = Process.Start(ffmpegInfo) ?? throw new InvalidOperationException();
		ffmpegProcess.PriorityClass = ProcessPriorityClass.BelowNormal;
		ffmpegProcess.WaitForExit();

		CopyMetadata(originalFilePath, compressedFilePath);

		string folderPath = P.Combine(S.TmpFolderPath, "Before Compression", P.GetDirectoryName(item.Path)!);
		Directory.CreateDirectory(folderPath);

		string uncompressedNewPath = P.Combine(folderPath, P.GetFileName(item.Path));
		if (File.Exists(uncompressedNewPath))
			File.Delete(uncompressedNewPath);

		File.Move(originalFilePath, uncompressedNewPath);
		File.Move(compressedFilePath, originalFilePath);

		L.LogLine($"Finish compressing {item.Path}", LogLevel.Debug);
	}

	///Uses ExifTool to copy the metadata from the original file to the new compressed one.
	private static void CopyMetadata(string originalFilePath, string compressedFilePath)
	{
		L.LogLine($"Begin copying metadata for {compressedFilePath}", LogLevel.Debug);

		ProcessStartInfo exiftoolInfo = new()
		{
			CreateNoWindow = true,
			FileName = "exiftool",
			Arguments = $"-TagsFromFile \"{originalFilePath}\" -All:All -overwrite_original \"{compressedFilePath}\""
		};

		var exiftoolProcess = Process.Start(exiftoolInfo) ?? throw new InvalidOperationException();
		exiftoolProcess.WaitForExit();

		L.LogLine($"Finish copying metadata for {compressedFilePath}", LogLevel.Debug);
	}
}