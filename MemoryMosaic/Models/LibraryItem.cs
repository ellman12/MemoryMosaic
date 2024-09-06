namespace MemoryMosaic.Models;

///Represents a row in the library table.
public sealed class LibraryItem : Media
{
	public DateTime? DateTaken { get; set; }

	public DateTime DateAdded { get; }

	public DateTime? DateDeleted { get; }

	///Creates a new LibraryItem from a query of this form: path, id, date_taken, date_added, starred, description, date_deleted, thumbnail
	public LibraryItem(string path, Guid id, DateTime? dateTaken, DateTime dateAdded, bool starred, string? description, double? latitude, double? longitude, DateTime? dateDeleted, string thumbnail)
	{
		Path = path;
		Id = id;
		DateTaken = dateTaken;
		DateAdded = dateAdded;
		Starred = starred;
		Description = description;
		Latitude = latitude;
		Longitude = longitude;
		DateDeleted = dateDeleted;
		Thumbnail = thumbnail;
		Video = F.SupportedVideoExts.Contains(P.GetExtension(Path).ToLower());

		Size = File.Exists(FullPath) ? new FileInfo(FullPath).Length : -1;
	}

	///Creates a new LibraryItem from a query of this form: path, id, date_taken, date_added, starred, description, date_deleted, thumbnail
	public LibraryItem(NpgsqlDataReader reader) : this(reader.GetString(0), reader.GetGuid(1), reader.TryGetDateTime(2), reader.GetDateTime(3), reader.GetBoolean(4), reader.TryGetString(5), reader.TryGetDouble(6), reader.TryGetDouble(7), reader.TryGetDateTime(8), reader.GetString(9)) {}

	///Used in Import.AddItems() for converting an ImportItem into a LibraryItem.
	public LibraryItem(ImportItem importItem)
	{
		Path = importItem.DestinationPath;
		Id = importItem.Id;
		DateTaken = importItem.SelectedDateTaken;
		DateAdded = DateTime.Now;
		Starred = importItem.Starred;
		Description = importItem.Description;
		Latitude = importItem.Latitude;
		Longitude = importItem.Longitude;
		DateDeleted = null;
		Thumbnail = importItem.Thumbnail;
		Video = F.SupportedVideoExts.Contains(P.GetExtension(Path).ToLower());
		Size = importItem.Size;
	}

	public string Filename => P.GetFileName(Path);

	public string FilenameWithoutExtension => P.GetFileNameWithoutExtension(Path);

	public override string RequestPath => "mm_library";

	public override string FullPath => P.Combine(S.LibFolderPath, Path);
}