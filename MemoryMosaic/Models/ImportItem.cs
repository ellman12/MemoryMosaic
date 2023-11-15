namespace MemoryMosaic.Models;

///Represents an item in Import that's not yet part of the library.
public sealed class ImportItem : Media
{
	///The original filename of this item, without the extension.
	public string OriginalFilename { get; init; } = ""; 

	///What the file has been renamed to, if applicable, without the extension.
	public string NewFilename { get; set; } = ""; 

	public string Extension { get; init; } = "";

	public string AbsolutePath => System.IO.Path.Join(S.ImportFolderPath, Path);

	public DateTime? MetadataDateTaken { get; init; }

	public DateTime? FilenameDateTaken { get; init; }

	///The new date taken (or null) that the user picks in Import.
	public DateTime? CustomDateTaken { get; set; }

	public DateTakenSource DateTakenSource { get; set; }

	///The album(s) or folder to add this item to.
	public HashSet<int>? Collections { get; set; }
	
	public override string RequestPath => "mm_import";
	
	public override string FullPath => System.IO.Path.Combine(S.ImportFolderPath, Path);
}