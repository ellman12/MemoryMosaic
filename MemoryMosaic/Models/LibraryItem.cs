namespace MemoryMosaic.Models;

///Represents a row in the media table.
public class LibraryItem : Media
{
	public DateTime? DateTaken { get; init; }
	
	public DateTime DateAdded { get; init; }
	
	public DateTime? DateDeleted { get; init; }
	
	public bool Video { get; init; }
}