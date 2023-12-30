﻿using System.Text.Json.Serialization;

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
	public DateTime? CustomDateTaken
	{
		get => customDateTaken;
		set
		{
			customDateTaken = value;
			DateTakenSource = DateTakenSource.Custom;
		}
	}
	private DateTime? customDateTaken;

	public DateTakenSource DateTakenSource { get; set; }

	///The album(s) or folder to add this item to.
	public HashSet<Collection>? Collections { get; set; }
	
	///Returns the date taken value from the currently selected source.
	[JsonIgnore]
	public DateTime? SelectedDateTaken
	{
		get
		{
			return DateTakenSource switch
			{
				DateTakenSource.Metadata => MetadataDateTaken,
				DateTakenSource.Filename => FilenameDateTaken,
				DateTakenSource.Custom => CustomDateTaken,
				DateTakenSource.None => null,
				_ => throw new ArgumentOutOfRangeException()
			};
		}
	}

	[JsonIgnore]
	public string DestinationPath => C.CreateShortPath(SelectedDateTaken, NewFilename + Extension);

	[JsonIgnore]
	public string AbsoluteDestinationPath => $"{P.Combine(S.LibFolderPath, DestinationPath)}";

	[JsonIgnore]
	public override string RequestPath => "mm_import";
	
	[JsonIgnore]
	public override string FullPath => P.Combine(S.ImportFolderPath, Path);

	public ImportItem(string absolutePath)
	{
		Id = Guid.NewGuid();
		Thumbnail = F.GenerateThumbnail(absolutePath);
		
		Path = absolutePath.Replace(S.ImportFolderPath, "").Substring(1);
		AbsolutePath = absolutePath;
		OriginalFilename = NewFilename = P.GetFileNameWithoutExtension(AbsolutePath);
		Extension = P.GetExtension(AbsolutePath);
		Video = D.IsVideoExt(Extension);

		D.GetDateTakenFromBoth(AbsolutePath, out DateTime? metadataDT, out DateTime? filenameDT);
		MetadataDateTaken = metadataDT;
		FilenameDateTaken = filenameDT;
		if (MetadataDateTaken != null)
			DateTakenSource = DateTakenSource.Metadata;
		else if (FilenameDateTaken != null)
			DateTakenSource = DateTakenSource.Filename;
		else
			DateTakenSource = DateTakenSource.None;
		
		customDateTaken = SelectedDateTaken;
	}
}