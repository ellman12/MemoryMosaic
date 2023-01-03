namespace PSS.Backend;

///Represents an item that is pending uploading in Import.razor.
public class ImportFile
{
	///The original (or new if changed by user) filename of this item.
	public string filename;

	///The file extension of the item.
	public string extension;

	///The path relative to pss_upload of this item.
	public string shortPath;

	///The absolute path to the file to upload.
	public string fullPath;

	///null for images, otherwise a base64 string for video files.
	public string thumbnail;

	///If this item is already in pss_library.
	public bool alreadyInLib;

	///The date and time this image or video was captured. null if this item doesnt have any.
	public DateTime? dateTaken;

	///Where the date taken data came from (Filename, Metadata, or None).
	public D.DateTakenSrc dateTakenSrc;

	///The uuid of the item, which will be added to the database upon upload.
	public Guid uuid;
}