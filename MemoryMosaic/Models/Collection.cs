namespace MemoryMosaic.Models;

///Represents a row from the Collections table.
public sealed class Collection
{
	public int Id { get; init; }
	public string Name { get; set; }
	public string? Cover { get; set; }
	public bool Folder { get; set; }
	public bool ReadOnly { get; set; }
	public DateTime LastModified { get; set; }
	public int Count { get; set; }

	public Collection(int id, string name, string? cover)
	{
		Id = id;
		Name = name;
		Cover = cover;
	}
            
	public Collection(int id, string name, string? cover, DateTime lastModified, int count)
	{
		Id = id;
		Name = name;
		Cover = cover;
		LastModified = lastModified;
		Count = count;
	}

	public Collection(int id, string name, bool folder, bool readOnly, DateTime lastModified)
	{
		Id = id;
		Name = name;
		Folder = folder;
		ReadOnly = readOnly;
		LastModified = lastModified;
	}
}