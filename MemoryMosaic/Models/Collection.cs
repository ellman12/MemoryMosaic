namespace MemoryMosaic.Models;

///Represents a row from the Collections table.
public class Collection
{
	public int Id { get; init; }
	public string Name { get; set; }
	public string? Cover { get; set; }
	public bool Folder { get; set; }
	public bool ReadOnly { get; set; }
	public DateTime DateUpdated { get; set; }

	public Collection(int id, string name, string cover)
	{
		Id = id;
		Name = name;
		Cover = cover;
	}
            
	public Collection(int id, string name, string cover, DateTime dateUpdated)
	{
		Id = id;
		Name = name;
		Cover = cover;
		DateUpdated = dateUpdated;
	}

	public Collection(int id, string name, DateTime dateUpdated, bool folder, bool readOnly)
	{
		Id = id;
		Name = name;
		DateUpdated = dateUpdated;
		Folder = folder;
		ReadOnly = readOnly;
	}
}