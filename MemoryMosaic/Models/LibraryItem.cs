namespace MemoryMosaic.Models;

///Represents a row in the library table.
public sealed class LibraryItem : Media
{
	public DateTime? DateTaken { get; set; }

	public DateTime DateAdded { get; init; }

	public DateTime? DateDeleted { get; init; }

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
		Video = F.SupportedVideoExts.Contains(System.IO.Path.GetExtension(Path).ToLower());
	}

	///Creates a new LibraryItem from a query of this form: path, id, date_taken, date_added, starred, description, date_deleted, thumbnail
	public LibraryItem(NpgsqlDataReader reader) : this(reader.GetString(0), reader.GetGuid(1), reader.TryGetDateTime(2), reader.GetDateTime(3), reader.GetBoolean(4), reader.IsDBNull(5) ? null : reader.GetString(5), reader.IsDBNull(6) ? null : reader.GetDateTime(6), reader.GetString(7)) {}

	public override string RequestPath => "mm_library";

	public override string FullPath => System.IO.Path.Combine(S.LibFolderPath, Path);
}