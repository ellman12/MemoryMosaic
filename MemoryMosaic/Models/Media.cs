namespace MemoryMosaic.Models;

///Represents any kind of photo/video file.
public abstract class Media
{
	///The path to where the item is located, relative to a folder like mm_library or mm_import.
	public string Path { get; set; } = "";
	
	public Guid Id { get; init; }

	public bool Starred { get; set; }

	public string? Description { get; set; }
	
	///A compressed base64 string generated by ffmpeg. For videos, it stores the very first frame.
	public string Thumbnail { get; init; } = "";
	
	public bool Video { get; protected init; }

	public string Filename => System.IO.Path.GetFileName(Path);
	
	public string FilenameWithoutExtension => System.IO.Path.GetFileNameWithoutExtension(Path);

	public abstract string RequestPath { get; }
	
	public abstract string FullPath { get; }
}