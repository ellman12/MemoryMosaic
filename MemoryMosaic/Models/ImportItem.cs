using System.Text.Json.Serialization;

namespace MemoryMosaic.Models;

///Represents an item in Import that's not yet part of the library.
public sealed class ImportItem : Media
{
	///The original filename of this item, without the extension.
	public string OriginalFilename { get; init; } 

	///What the file has been renamed to, if applicable, without the extension.
	public string NewFilename { get; set; } 

	public string Extension { get; init; }

	public string AbsolutePath { get; init; }

	public DateTime? MetadataDateTaken { get; init; }

	public DateTime? FilenameDateTaken { get; init; }

	///The new date taken (or null) that the user picks in Import.
	public DateTime? CustomDateTaken { get; set; }

	public DateTakenSource DateTakenSource { get; set; }

	///The album(s) or folder to add this item to.
	public HashSet<Collection>? Collections { get; init; }
	
	[JsonIgnore]
	public override string RequestPath => "mm_import";
	
	[JsonIgnore]
	public override string FullPath => P.Combine(S.ImportFolderPath, Path);

	public ImportItem(string absolutePath)
	{
		Id = Guid.NewGuid();
		Thumbnail = F.GenerateThumbnail(absolutePath);
		
		Path = absolutePath.Replace(S.ImportFolderPath, "");
		AbsolutePath = absolutePath;
		OriginalFilename = NewFilename = P.GetFileNameWithoutExtension(AbsolutePath);
		Extension = P.GetExtension(AbsolutePath);

		D.GetDateTakenFromBoth(AbsolutePath, out DateTime? metadataDT, out DateTime? filenameDT);
		MetadataDateTaken = metadataDT;
		FilenameDateTaken = filenameDT;

		if (MetadataDateTaken != null)
			DateTakenSource = DateTakenSource.Metadata;
		else if (FilenameDateTaken != null)
			DateTakenSource = DateTakenSource.Filename;
		else
			DateTakenSource = DateTakenSource.None;
	}
}