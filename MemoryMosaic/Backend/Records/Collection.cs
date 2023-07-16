namespace MemoryMosaic.Backend.Records;

///Represents a row from the Collections table.
public record Collection
{
	public readonly int id;
	public readonly string name;
	public readonly string cover;
	public readonly DateTime dateUpdated;
	public bool folder, readOnly;

	public Collection(int id, string name, string cover)
	{
		this.id = id;
		this.name = name;
		this.cover = cover;
	}
            
	public Collection(int id, string name, string cover, DateTime dateUpdated)
	{
		this.id = id;
		this.name = name;
		this.cover = cover;
		this.dateUpdated = dateUpdated;
	}

	public Collection(int id, string name, DateTime dateUpdated, bool folder, bool readOnly)
	{
		this.id = id;
		this.name = name;
		this.dateUpdated = dateUpdated;
		this.folder = folder;
		this.readOnly = readOnly;
	}

	public Collection(int id, string name, string cover, DateTime dateUpdated, bool readOnly)
	{
		this.id = id;
		this.name = name;
		this.cover = cover;
		this.dateUpdated = dateUpdated;
		this.readOnly = readOnly;
	}
}