namespace MemoryMosaic.Models;

///Used for controlling what FFMpeg does when compressing an item.
public sealed class CompressionParameters
{
	public Resolution Resolution { get; init; }
	public Resolution NewResolution { get; set; }

	public uint Width { get; init; }
	public uint Height { get; init; }
	public string AspectRatio { get; init; }

	public float NewVolumeLevel { get; set; } = 1;

	///If a 60 fps video should be reduced to 30. Has no effect on videos that are already 30 fps.
	public bool LowerFrameRate { get; set; } = false;

	public string VolumeParameter => NewVolumeLevel switch
	{
		1 => "-c:a copy",
		0 => "-an",
		_ => $"-af \"volume={NewVolumeLevel}\""
	};

	private Media Media { get; }

	public CompressionParameters(Media media)
	{
		Media = media;
		(Width, Height, AspectRatio) = FF.GetDimensions(media.FullPath);
		NewResolution = Resolution = InitialResolution;
	}

	///Determines what the initial named resolution of this item is.
	private Resolution InitialResolution => (Width, Height) switch
	{
		(3840, 2160) or (2160, 3840) => Resolution.UHD_4K,
		(2560, 1440) or (1440, 2560) => Resolution.WQHD_1440p,
		(1920, 1080) or (1080, 1920) => Resolution.FHD_1080p,
		(1280, 720) or (720, 1280) => Resolution.HD_720p,

		(4032, 3024) or (3024, 4032) => Resolution.UHD_4K,
		(1920, 1440) or (1440, 1920) => Resolution.WQHD_1440p,
		(1440, 1080) or (1080, 1440) => Resolution.FHD_1080p,
		(960, 720) or (720, 960) => Resolution.HD_720p,

		_ => throw new ArgumentOutOfRangeException()
	};

	///Returns what the new width and height of this item should be.
	private (uint, uint) NewDimensions => (AspectRatio, NewResolution) switch
	{
		("16:9", Resolution.WQHD_1440p) => (2560, 1440),
		("16:9", Resolution.FHD_1080p) => (1920, 1080),
		("16:9", Resolution.HD_720p) => (1280, 720),

		("9:16", Resolution.WQHD_1440p) => (1440, 2560),
		("9:16", Resolution.FHD_1080p) => (1080, 1920),
		("9:16", Resolution.HD_720p) => (720, 1280),

		("4:3", Resolution.WQHD_1440p) => (1920, 1440),
		("4:3", Resolution.FHD_1080p) => (1440, 1080),
		("4:3", Resolution.HD_720p) => (960, 720),

		("3:4", Resolution.WQHD_1440p) => (1440, 1920),
		("3:4", Resolution.FHD_1080p) => (1080, 1440),
		("3:4", Resolution.HD_720p) => (720, 960),

		(_, Resolution.UHD_4K) => throw new ArgumentException("4K resolution is not allowed with any aspect ratio."),
		_ => throw new ArgumentOutOfRangeException()
	};

	private string GetNewScale()
	{
		if (NewResolution == Resolution)
			return "";

		(uint width, uint height) = NewDimensions;
		return $"-vf \"scale={width}:{height}\"";
	}

	public async Task Compress()
	{
		L.LogLine($"Begin compressing {Media.Path}", LogLevel.Debug);
		
		string originalFilePath = Media.FullPath;
		string ext = P.GetExtension(originalFilePath);
		string compressedFilePath = originalFilePath.Replace(ext, $"_compressed{ext}");

		ProcessStartInfo ffmpegInfo = new()
		{
			CreateNoWindow = true,
			FileName = "ffmpeg",
			Arguments = $"-y -v error -i \"{originalFilePath}\" {GetNewScale()} {(LowerFrameRate ? "-r 30" : "")} {(Media.Video ? VolumeParameter : "")} -q:v 1 \"{compressedFilePath}\""
		};

		Process ffmpegProcess = Process.Start(ffmpegInfo) ?? throw new InvalidOperationException();
		await ffmpegProcess.WaitForExitAsync();
		
		string folderPath = P.Combine(S.TmpFolderPath, "Before Compression", P.GetDirectoryName(Media.Path)!);
		Directory.CreateDirectory(folderPath);
		
		string uncompressedNewPath = P.Combine(folderPath, P.GetFileName(Media.Path));
		if (File.Exists(uncompressedNewPath))
			File.Delete(uncompressedNewPath);
		
		File.Move(originalFilePath, uncompressedNewPath);
		File.Move(compressedFilePath, originalFilePath);
		
		L.LogLine($"Finish compressing {Media.Path}", LogLevel.Debug);
	}
}