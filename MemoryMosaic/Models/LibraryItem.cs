namespace MemoryMosaic.Models;

///Represents a row in the media table.
public class LibraryItem : Media
{
	public DateTime? DateTaken { get; init; }

	public DateTime DateAdded { get; init; }

	public DateTime? DateDeleted { get; init; }

	public bool Video { get; init; }
	
	public LibraryItem(string path, DateTime? dateTaken, DateTime dateAdded, bool starred, Guid id, string thumbnail, string? description)
	{
		Path = path;
		DateTaken = dateTaken;
		DateAdded = dateAdded;
		Starred = starred;
		Id = id;
		Thumbnail = thumbnail;
		Description = description;
	}
}