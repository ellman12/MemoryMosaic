namespace MemoryMosaic.Models;

///Represents a row in the library table.
public sealed class LibraryItem : Media
{
	public DateTime? DateTaken { get; set; }

	public DateTime DateAdded { get; }

	public DateTime? DateDeleted { get; }

	///Creates a new LibraryItem from a query of this form: path, id, date_taken, date_added, starred, description, date_deleted, thumbnail
	public LibraryItem(string path, Guid id, DateTime? dateTaken, DateTime dateAdded, bool starred, string? description, DateTime? dateDeleted, string thumbnail)
	{
		Path = path;
		Id = id;
		DateTaken = dateTaken;
		DateAdded = dateAdded;
		Starred = starred;
		Description = description;
		DateDeleted = dateDeleted;
		Thumbnail = thumbnail;
		Video = F.SupportedVideoExts.Contains(P.GetExtension(Path).ToLower());

		Size = File.Exists(FullPath) ? new FileInfo(FullPath).Length : -1;
	}

	///Creates a new LibraryItem from a query of this form: path, id, date_taken, date_added, starred, description, date_deleted, thumbnail
	public LibraryItem(NpgsqlDataReader reader) : this(reader.GetString(0), reader.GetGuid(1), reader.TryGetDateTime(2), reader.GetDateTime(3), reader.GetBoolean(4), reader.IsDBNull(5) ? null : reader.GetString(5), reader.IsDBNull(6) ? null : reader.GetDateTime(6), reader.GetString(7)) {}

	///Used in Import.AddItems() for converting an ImportItem into a LibraryItem.
	public LibraryItem(ImportItem importItem)
	{
		Path = importItem.DestinationPath;
		Id = importItem.Id;
		DateTaken = importItem.SelectedDateTaken;
		DateAdded = DateTime.Now;
		Starred = importItem.Starred;
		Description = importItem.Description;
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