namespace MemoryMosaic.Models;

///Represents a row from the Collections table.
public sealed class Collection
{
	public int Id { get; init; }
	public string Name { get; set; }
	public string? Cover { get; set; }
	public string? Description { get; set; }
	public bool Folder { get; set; }
	public bool ReadOnly { get; set; }
	public DateTime LastModified { get; set; }
	public int Count { get; set; }
	public DateTime? RangeStart { get; init; }
	public DateTime? RangeEnd { get; init; }

	public Collection(int id, string name, string? cover)
	{
		Id = id;
		Name = name;
		Cover = cover;
	}

	public Collection(int id, string name, string? cover, string? description, DateTime lastModified, int count, DateTime? rangeStart, DateTime? rangeEnd)
	{
		Id = id;
		Name = name;
		Cover = cover;
		Description = description;
		LastModified = lastModified;
		Count = count;
		RangeStart = rangeStart;
		RangeEnd = rangeEnd;
	}

	public Collection(int id, string name, bool folder, bool readOnly, DateTime lastModified)
	{
		Id = id;
		Name = name;
		Folder = folder;
		ReadOnly = readOnly;
		LastModified = lastModified;
	}

	public string FormatDateRange()
	{
		const string dot = "&nbsp;&#x2022;&nbsp;";
		
		if (RangeStart == null || RangeEnd == null)
			return "";
		
		if (RangeStart.Value.Date == RangeEnd.Value.Date)
			return $"{dot} {RangeStart.Value:M/d/yyyy}";

		return $"{dot} {RangeStart.Value:M/d/yyyy} – {RangeEnd.Value:M/d/yyyy}";
	}
}