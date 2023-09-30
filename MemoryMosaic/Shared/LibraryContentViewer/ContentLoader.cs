namespace MemoryMosaic.Shared.LibraryContentViewer;

public sealed class ContentLoader
{
	private readonly LibraryContentViewer LCV;

	private readonly bool Bottom;
	
	private readonly NpgsqlConnection conn = C.CreateLocalConnection();

	private readonly NpgsqlDataReader reader;

	private string Query
	{
		get
		{
			List<string> filters = new();
			
			if (LCV.CtrlGInput.NewStartDate != null)
				filters.Add($"{LCV.OrderByField} {(Bottom ? "<=" : ">=")} '{LCV.CtrlGInput.NewStartDate?.ToString("yyyy-MM-dd HH:mm:ss")}'");
			
			if (!String.IsNullOrWhiteSpace(LCV.Where))
				filters.Add(LCV.Where);
			
			return $"SELECT {LCV.Columns} FROM {LCV.Table} {(filters.Count > 0 ? $"WHERE {String.Join(" AND ", filters.ToArray())}" : "")} ORDER BY {LCV.OrderBy}";
		}
	}

	private const int ReadLimit = 100;

	public ContentLoader(LibraryContentViewer lcv, bool bottom, bool initializer)
	{
		LCV = lcv;
		Bottom = bottom;

		using NpgsqlCommand cmd = new(Query, conn);
		reader = cmd.ExecuteReader();

		if (initializer)
			AddContent();
	}
	
	public void AddContent()
	{
		int rowsAdded = 0;

		while (rowsAdded < ReadLimit && reader.Read())
		{
			LCV.Content.Add(new MediaRow(reader.GetString(0), reader.IsDBNull(1) ? null : reader.GetDateTime(1), reader.GetDateTime(2), reader.GetBoolean(3), reader.GetGuid(4), reader.GetString(5), reader.IsDBNull(6) ? null : reader.GetString(6)));
			rowsAdded++;
		}
		LCV.Rerender();
	}
}