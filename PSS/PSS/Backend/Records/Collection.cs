namespace PSS.Backend.Records;

///Represents a row from the Collections table.
public record Collection
{
	public readonly int id;
	public readonly string name;
	public readonly string cover;
	public readonly DateTime dateUpdated;

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
}