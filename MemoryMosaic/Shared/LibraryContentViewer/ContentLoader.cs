namespace MemoryMosaic.Shared.LibraryContentViewer;

public sealed class ContentLoader : IDisposable, IAsyncDisposable
{
	private readonly LibraryContentViewer LCV;

	private readonly bool Bottom;

	public ElementVisibility Visibility { get; } = new();
	
	public bool HasMoreRows { get; set; }

	private readonly NpgsqlConnection? conn;
	
	private readonly NpgsqlDataReader? reader;

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
			
			return $"SELECT {LCV.Columns} FROM {LCV.Table} {(String.IsNullOrWhiteSpace(LCV.Join) ? "" : $"INNER JOIN {LCV.Join}")} WHERE {String.Join(" AND ", filters)} ORDER BY {orderBy}";
		}
	}

	public ContentLoader(LibraryContentViewer lcv, bool bottom, bool initializer)
	{
		LCV = lcv;
		Visibility.Visible = initializer;
		Bottom = bottom;
		HasMoreRows = initializer;

		if (!initializer) return;
		
		conn = D.CreateLocalConnection();
		using NpgsqlCommand cmd = new(Query, conn);
		reader = cmd.ExecuteReader();
		AddContent();
	}
	
	public void Dispose()
	{
		conn?.Dispose();
		reader?.Dispose();
	}

	public async ValueTask DisposeAsync()
	{
		if (conn != null)
			await conn.DisposeAsync();

		if (reader != null)
			await reader.DisposeAsync();
	}

	public void AddContent() => AddContent(100);

	public int AddContent(int readLimit)
	{
		if (!HasMoreRows || reader == null) return 0;
		
		int rowsAdded = 0;

		while (rowsAdded < readLimit && reader.Read())
		{
			LibraryItem newItem = new(reader);
			
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