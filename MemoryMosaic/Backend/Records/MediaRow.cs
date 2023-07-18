namespace MemoryMosaic.Backend.Records;

///Represents a row in the media table.
public record MediaRow
{
    public string path;
    public DateTime? dateTaken;
    public readonly DateTime dateAdded;
    public bool starred;
    public readonly bool separate;
    public readonly Guid uuid;
    public readonly string thumbnail;
    public readonly DateTime? dateDeleted;
    public string? description;
    public readonly bool video;

    public MediaRow(string p, DateTime? dt, Guid uuid, string thumbnail, DateTime? dateDeleted)
    {
        path = p;
        dateTaken = dt;
        this.uuid = uuid;
        this.thumbnail = thumbnail;
        this.dateDeleted = dateDeleted;
    }

    public MediaRow(string p, DateTime? dt, DateTime da, Guid uuid, string thumbnail)
    {
        path = p;
        dateTaken = dt;
        dateAdded = da;
        this.uuid = uuid;
        this.thumbnail = thumbnail;
    }

    public MediaRow(string p, DateTime? dt, DateTime da, bool starred, Guid uuid, string thumbnail, string description)
    {
        path = p;
        dateTaken = dt;
        dateAdded = da;
        this.starred = starred;
        this.uuid = uuid;
        this.thumbnail = thumbnail;
        this.description = description;
        video = D.IsVideoExt(Path.GetExtension(path));
    }
            
    public MediaRow(string p, DateTime? dt, DateTime da, bool starred, bool separate, Guid uuid, string thumbnail)
    {
        path = p;
        dateTaken = dt;
        dateAdded = da;
        this.starred = starred;
        this.separate = separate;
        this.uuid = uuid;
        this.thumbnail = thumbnail;
    }
    
    public MediaRow(string p, DateTime? dt, DateTime da, bool starred, bool separate, Guid uuid, string thumbnail, string description)
    {
        path = p;
        dateTaken = dt;
        dateAdded = da;
        this.starred = starred;
        this.separate = separate;
        this.uuid = uuid;
        this.thumbnail = thumbnail;
        this.description = description;
    }
}