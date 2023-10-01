namespace MemoryMosaic.Shared.LibraryContentViewer;

public sealed class ContentLoader
{
	private readonly LibraryContentViewer LCV;

	private readonly bool Bottom;

	public ElementVisibility Visibility { get; } = new();
	
	private readonly NpgsqlConnection conn = C.CreateLocalConnection();

	private readonly NpgsqlDataReader reader;

	private string Query
	{
		get
		{
			List<string> filters = new();
			
			if (LCV.CtrlGInput.NewStartDate != null)
				filters.Add($"{LCV.OrderByFields.First()} {(Bottom ? "<=" : ">=")} '{LCV.CtrlGInput.NewStartDate?.ToString("yyyy-MM-dd HH:mm:ss")}'");
			
			if (!String.IsNullOrWhiteSpace(LCV.Where))
				filters.Add(LCV.Where);

			string sortOrder = Bottom ? "DESC" : "ASC";
			string orderBy = $"{String.Join($" {sortOrder}, ", LCV.OrderByFields)} {sortOrder}";
			
			string query = $"SELECT {LCV.Columns} FROM {LCV.Table} {(filters.Count > 0 ? $"WHERE {String.Join(" AND ", filters)}" : "")} ORDER BY {orderBy}";

			#if DEBUG
			L.LogLine($"Query for CL marked Bottom = {Bottom}: {query}", LogLevel.Debug);
			#endif
			return query;
		}
	}

	private const int ReadLimit = 100;

	public ContentLoader(LibraryContentViewer lcv, bool startingState, bool bottom, bool initializer)
	{
		LCV = lcv;
		Visibility.Visible = startingState;
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
			MediaRow newRow = new(reader.GetString(0), reader.IsDBNull(1) ? null : reader.GetDateTime(1), reader.GetDateTime(2), reader.GetBoolean(3), reader.GetGuid(4), reader.GetString(5), reader.IsDBNull(6) ? null : reader.GetString(6));
			
			if (Bottom)
                LCV.Content.Add(newRow);
			else
                LCV.Content.Insert(0, newRow);
			
			rowsAdded++;
		}
		
		if (rowsAdded < ReadLimit)
			Visibility.Disable();
		
		#if DEBUG
		L.LogLine($"Added {rowsAdded} items in CL marked Bottom = {Bottom}", LogLevel.Debug);
		L.LogLine($"LCV.Content.Count = {LCV.Content.Count}", LogLevel.Debug);
		#endif
		
		LCV.Rerender();
	}
}