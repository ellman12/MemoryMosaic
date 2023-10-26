namespace MemoryMosaic.Shared.LibraryContentViewer;

public sealed class ContentLoader
{
	private readonly LibraryContentViewer LCV;

	private readonly bool Bottom;

	public ElementVisibility Visibility { get; } = new();
	
	public bool HasMoreRows { get; private set; }
	
	private readonly NpgsqlConnection conn = C.CreateLocalConnection();

	private readonly NpgsqlDataReader reader;

	private string Query
	{
		get
		{
			List<string> filters = new() {$"date_deleted IS {(LCV.TrashPage ? "NOT" : "")} NULL"};

			(string comparisonOperator, string sortOrder) = (LCV.SortDesc, Bottom) switch
			{
				(true, true) or (false, false) => ("<=", "DESC"),
				(true, false) or (false, true) => (">=", "ASC")
			};
			
			if (LCV.CtrlGInput.NewStartDate != null)
				filters.Add($"{LCV.OrderByFields.First()} {comparisonOperator} '{LCV.CtrlGInput.NewStartDate?.ToString("yyyy-MM-dd HH:mm:ss")}'");
			
			if (!String.IsNullOrWhiteSpace(LCV.Where))
				filters.Add(LCV.Where);

			string orderBy = $"{String.Join($" {sortOrder}, ", LCV.OrderByFields)} {sortOrder}";
			
			return $"SELECT {LCV.Columns} FROM {LCV.Table} WHERE {String.Join(" AND ", filters)} ORDER BY {orderBy}";
		}
	}

	public ContentLoader(LibraryContentViewer lcv, bool bottom, bool initializer)
	{
		LCV = lcv;
		Visibility.Visible = initializer;
		Bottom = bottom;

		using NpgsqlCommand cmd = new(Query, conn);
		reader = cmd.ExecuteReader();

		HasMoreRows = initializer;
		if (initializer)
			AddContent();
	}

	public void AddContent() => AddContent(100);

	public int AddContent(int readLimit)
	{
		if (!HasMoreRows) return 0;
		
		int rowsAdded = 0;

		while (rowsAdded < readLimit && reader.Read())
		{
			LibraryItem newItem = new(reader.GetString(0), reader.IsDBNull(1) ? null : reader.GetDateTime(1), reader.GetDateTime(2), reader.GetBoolean(3), reader.GetGuid(4), reader.GetString(5), reader.IsDBNull(6) ? null : reader.GetString(6), reader.IsDBNull(7) ? null : reader.GetDateTime(7));
			
			if (Bottom)
                LCV.Content.Add(newItem);
			else
                LCV.Content.Insert(0, newItem);
			
			rowsAdded++;
		}

		if (!reader.IsOnRow)
		{
			HasMoreRows = false;
			Visibility.Disable();
		}
		LCV.Rerender();
		return rowsAdded;
	}
}