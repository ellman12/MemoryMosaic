using System.Collections.Generic;

namespace PSS.Backend;

///Represents an item that is pending importing in Import.razor.
public class ImportFile
{
	///The original filename of this item, without the extension.
	public string originalFilename;

	///Tracks what the user has updated the filename to, if applicable, without the extension.
	public string renamedFilename;

	///The file extension of the item.
	public string extension;

	///The path relative to pss_import of this item, which does NOT contain 'pss_import' at the front.
	public string shortPath;

	///The absolute path to the file to import.
	public string absolutePath;

	///null for images; otherwise a base64 string for video files that contains a compressed thumbnail of the first frame of the video, generated by ffmpepg.
	public string thumbnail;

	///If this item is already in pss_library.
	public bool alreadyInLib; //TODO: idk if this will be needed

	///The date and time this file was taken, taken from the file's metadata. null if none found.
	public DateTime? metadataDateTaken;

	///The date and time this file was taken, taken from the file's filename. null if none found.
	public DateTime? filenameDateTaken;

	///The new date and time (or null) that the user chose in Import.
	public DateTime? customDateTaken;

	///Where the Date Taken data for an item came from.
	public enum DateTakenSource
	{
		Metadata,
		Filename,
		None,
		Custom
	}

	///Where the dateTaken for this item is coming from.
	public DateTakenSource dateTakenSource;

	///The uuid of the item, which will be added to the database upon completion of importing.
	public Guid uuid; //TODO: idk if this will be needed

	///If this item will be marked with a star when added to the library.
	public bool starred;
	
	///The album(s) or folder to add this item to.
	public HashSet<C.Collection> collections;

	///<summary>Constructs a new instance of an <see cref="ImportFile"/>.</summary>
	///<param name="absPath">The absolute path to where this item is.</param>
	public ImportFile(string absPath)
	{
		absolutePath = absPath.Replace('\\', '/');
		shortPath = absolutePath.Replace(S.importFolderPath, "");
		originalFilename = renamedFilename = Path.GetFileNameWithoutExtension(absolutePath);
		extension = Path.GetExtension(absolutePath);
		thumbnail = D.IsVideoExt(extension!) ? F.GenerateThumbnail(absolutePath) : null;
		D.GetDateTakenFromBoth(absolutePath!, out metadataDateTaken, out filenameDateTaken);
		uuid = Guid.NewGuid();

		//Determine default DT source for select control.
		if (metadataDateTaken == null && filenameDateTaken == null)
			dateTakenSource = DateTakenSource.None;
		else if (metadataDateTaken != null || metadataDateTaken == filenameDateTaken)
			dateTakenSource = DateTakenSource.Metadata;
		else if (filenameDateTaken != null)
			dateTakenSource = DateTakenSource.Filename;
	}
}