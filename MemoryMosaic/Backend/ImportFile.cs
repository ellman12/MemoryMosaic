using System.Reflection;
using MemoryMosaic.Shared;

namespace MemoryMosaic.Backend;

///Represents an item that is pending importing in Import.razor.
public class ImportFile
{
	///The original filename of this item, without the extension.
	public string originalFilename;

	///Tracks what the user has updated the filename to, if applicable, without the extension.
	public string renamedFilename;

	///The file extension of the item.
	public readonly string extension;

	///The path relative to mm_import of this item, which does NOT contain 'mm_import' at the front.
	public string shortPath;

	///The absolute path to the file to import.
	public string absolutePath;

	///A base64 string for video files that contains a compressed thumbnail of the first frame of the video, generated by ffmpepg.
	public readonly string thumbnail;

	///The date and time this file was taken, taken from the file's metadata. null if none found.
	public DateTime? metadataDateTaken;

	///The date and time this file was taken, taken from the file's filename. null if none found.
	public DateTime? filenameDateTaken;

	///The new date and time (or null) that the user chose in Import.
	public DateTime? customDateTaken;

	///Where the dateTaken for this item is coming from.
	public DateTakenSource dateTakenSource;

	///The uuid of the item, which will be added to the database upon completion of importing.
	public readonly Guid uuid;

	///If this item will be marked with a star when added to the library.
	public bool starred;

	///The album(s) or folder to add this item to.
	public HashSet<Collection>? collections;

	///<summary>Constructs a new instance of an <see cref="ImportFile"/>.</summary>
	///<param name="absPath">The absolute path to where this item is.</param>
	public ImportFile(string absPath)
	{
		absolutePath = absPath.Replace('\\', '/');
		shortPath = absolutePath.Substring(Settings.importFolderPath.Length + 1); //Ensures no '/' at start.
		originalFilename = renamedFilename = Path.GetFileNameWithoutExtension(absolutePath);
		extension = Path.GetExtension(absolutePath);
		thumbnail = Functions.GenerateThumbnail(absolutePath);
		D.GetDateTakenFromBoth(absolutePath!, out metadataDateTaken, out filenameDateTaken);
		uuid = Guid.NewGuid();

		//Determine default DT source for select control.
		if (metadataDateTaken == null && filenameDateTaken == null)
			dateTakenSource = DateTakenSource.None;
		else if (metadataDateTaken != null || metadataDateTaken == filenameDateTaken)
			dateTakenSource = DateTakenSource.Metadata;
		else if (filenameDateTaken != null)
			dateTakenSource = DateTakenSource.Filename;
	}

	///Used in Import for saving the items to disk for later restoration. Returns a string with the ImportFile's values separated by tabs.
	public string ToTabDelimitedString()
	{
		string result = "";
		FieldInfo[] fields = typeof(ImportFile).GetFields();
		for (int i = 0; i < fields.Length - 1; i++)
			result += $"{fields[i].GetValue(this)}\t";

		if (collections?.Count > 0)
			result += $"{String.Join(' ', collections.Select(c => c.id))}";

		return result;
	}

	///Creates a new ImportFile from a string created with ToTabDelimitedString().
	private ImportFile(IReadOnlyList<string> split)
	{
		originalFilename = split[0];
		renamedFilename = split[1];
		extension = split[2];
		shortPath = split[3];
		absolutePath = split[4];
		thumbnail = split[5];
		if (!String.IsNullOrWhiteSpace(split[6])) metadataDateTaken = DateTime.Parse(split[6]);
		if (!String.IsNullOrWhiteSpace(split[7])) filenameDateTaken = DateTime.Parse(split[7]);
		if (!String.IsNullOrWhiteSpace(split[8])) customDateTaken = DateTime.Parse(split[8]);
		dateTakenSource = Enum.Parse<DateTakenSource>(split[9]);
		uuid = Guid.Parse(split[10]);
		starred = Boolean.Parse(split[11]);

		string[] stringIDs = split[12].Split(' ');
		HashSet<int> intIDs = new();
		foreach (string s in stringIDs)
			if (Int32.TryParse(s, out int n))
				intIDs.Add(n);

		collections = CollectionSelector.albums.Where(album => intIDs.Contains(album.id)).ToHashSet();
		if (collections.Count == 0) collections = CollectionSelector.folders.Where(folder => intIDs.Contains(folder.id)).ToHashSet();
	}

	public static ImportFile ParseTabDelimitedString(string tabDelimitedString) => new(tabDelimitedString.Split('\t'));
}