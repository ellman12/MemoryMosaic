using System.Threading;
using System.Collections.Concurrent;

namespace MemoryMosaic.Backend;

///Manages automatically compressing <see cref="ImportItem"/> in the background.
public static class Compressor
{
	public static ImportItem? Current { get; private set; }

	public static ConcurrentQueue<ImportItem> Items { get; } = new();

	public static bool Compressing { get; private set; }

	public static event EventHandler? ItemEnqueued, ItemDequeued;

	private static Thread compressionThread = null!;

	static Compressor()
	{
		InitializeThread();
	}

	private static void InitializeThread()
	{
		compressionThread = new Thread(CompressItems)
		{
			IsBackground = true,
			Priority = ThreadPriority.Lowest
		};
	}

	public static void Enqueue(ImportItem item)
	{
		Items.Enqueue(item);
		ItemEnqueued?.Invoke(null, EventArgs.Empty);
	}

	public static void BeginCompression()
	{
		if (Compressing || Items.Count == 0)
			return;
		
		InitializeThread();
		compressionThread.Start();
	}

	private static void CompressItems()
	{
		L.LogLine($"Begin {nameof(CompressItems)}", LogLevel.Debug);

		Compressing = true;

		while (Items.TryDequeue(out ImportItem? item))
		{
			Current = item;
			ItemDequeued?.Invoke(null, EventArgs.Empty);
			Compress(item);
		}

		Current = null;
		Compressing = false;

		L.LogLine($"Finish {nameof(CompressItems)}", LogLevel.Debug);
	}

	///Lightly compresses an item using FFmpeg's "-q:v 1" parameter. Uses ExifTool to copy the metadata from the original to the new item, then moves the original item to mm_tmp/Before Compression/.
	private static void Compress(ImportItem item)
	{
		L.LogLine($"Begin compressing {item.Path}", LogLevel.Debug);
		
		//Move the original, uncompressed file to mm_tmp.
		string folderPath = P.Combine(S.TmpFolderPath, "Before Compression", P.GetDirectoryName(item.Path)!);
		Directory.CreateDirectory(folderPath);
		string uncompressedFileNewPath = P.Combine(folderPath, item.NewFilenameWithExtension);
		File.Move(item.AbsoluteDestinationPath, uncompressedFileNewPath);

		string compressedFilePath = item.AbsoluteDestinationPath;

		ProcessStartInfo ffmpegInfo = new()
		{
			CreateNoWindow = true,
			FileName = "ffmpeg",
			Arguments = $"-y -v error -noautorotate -i \"{uncompressedFileNewPath}\" -q:v 1 \"{compressedFilePath}\""
		};
		var ffmpegProcess = Process.Start(ffmpegInfo) ?? throw new InvalidOperationException();
		ffmpegProcess.WaitForExit();

		CopyMetadata(uncompressedFileNewPath, compressedFilePath);
		
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