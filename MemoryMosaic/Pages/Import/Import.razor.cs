namespace MemoryMosaic.Pages.Import;

using System.Collections.Concurrent;
using System.Collections.Immutable;
using Shared.Modal;
using Shared.Modal.FullscreenViewer;

public sealed partial class Import
{
	public HashSet<Guid> SelectedItems { get; } = new();

	public string PathWidth { get; private set; } = "short";

	public bool DestinationPathsVisible { get; private set; }

	public bool EditingFilename { get; set; }
	
	public bool DisplayFileSizes { get; set; }

	public int LastCheckedIndex { get; set; }

	public int MinYear { get; private set; } = 2000;
	public int MaxYear { get; private set; } = DateTime.Now.Year;

	private string searchText = "", status = "";
	
	public Dictionary<string, LibraryItem> LibraryCache { get; private set; } = null!;

	public FullscreenViewer<Media> fv = null!;

	private bool pageLoading = true, displayWarnings = true, onlyDisplayErrors;

	private DateTakenSource newDateTakenSource = DateTakenSource.None;

	private ImportSortMode sortMode = ImportSortMode.SelectedDateTakenAsc;

	private List<ImportItem> importItems = new();

	private CollectionSelector cs = null!;

	private ElementVisibility moreSettings = null!;

	public void Rerender() => StateHasChanged();
	public async Task RerenderAsync() => await InvokeAsync(StateHasChanged);
	
	protected override async Task OnInitializedAsync()
	{
		L.LogLine("Begin Import Initialization", LogLevel.Info);
		moreSettings = new ElementVisibility(Rerender);
		await RerenderAsync();
		
		ConcurrentBag<ImportItem> bag = new();

		Task thumbnails = Task.Run(() => Parallel.ForEach(F.GetSupportedFiles(S.ImportFolderPath), (fullPath, _) =>
		{
			bag.Add(new ImportItem(fullPath.Replace('\\', '/')));
		}));
		await thumbnails;
		
		importItems = bag.ToList();
		LibraryCache = C.GetEntireLibrary().ToDictionary(key => key.Path, value => value);
		SortItems();
		
		pageLoading = false;
		await RerenderAsync();

		L.LogLine("Finish Import Initialization", LogLevel.Info);
	}

	private int ErrorAmount
	{
		get
		{
			int total = 0;

			total += importItems.GroupBy(item => item.DestinationPath).Count(group => group.Count() > 1);

			total += LibraryCache.Values.Count(libraryItem => importItems.Any(importItem => importItem.DestinationPath == libraryItem.Path));

			return total;
		}
	}

	private int WarningAmount => importItems
		.GroupBy(importItem => importItem.DestinationPath)
		.Count(group => LibraryCache.Values.Any(libraryItem => group.Any(importItem => importItem.NewFilename == libraryItem.FilenameWithoutExtension)));

	private string AddBtnText
	{
		get
		{
			if (SelectedItems.Count == 0 || SelectedItems.Count == importItems.Count)
				return "Add All";
			if (SelectedItems.Count == 1)
				return "Add 1 Item";

			return $"Add {SelectedItems.Count} Items";
		}
	}

	private List<Media> Content
	{
		get
		{
			List<Media> content = new();

			foreach (var group in importItems.GroupBy(item => item.DestinationPath))
			{
				var existingItems = LibraryCache.Values.Where(libraryItem => group.Any(importItem => importItem.DestinationPath == libraryItem.Path)).ToImmutableArray();

				var warnings = displayWarnings
					? LibraryCache.Values.Where(libraryItem => group.Any(importItem => importItem.NewFilename == libraryItem.FilenameWithoutExtension && !existingItems.Contains(libraryItem))).ToImmutableArray()
					: ImmutableArray<LibraryItem>.Empty;

				content.AddRange(group);
				content.AddRange(existingItems);
				content.AddRange(warnings);
			}
			
			return content;
		}
	}

	private IEnumerable<ImportItem> Selected => importItems.Where(item => SelectedItems.Contains(item.Id));
	
	private ImmutableArray<ImportItem> SearchResults => importItems.Where(item => item.NewFilename.Contains(searchText)).ToImmutableArray();

	private static readonly Dictionary<string, string> Shortcuts = new()
	{
		{"Ctrl A", "Select All"},
		{"Alt A", "Add Selected, If No Errors"},
		{"Alt C", "Toggle CollectionSelector"},
		{"Alt D", "Toggles If Destination Paths Are Shown"},
		{"Alt F", "Toggle Displaying File Sizes"},
		{"Alt S", "Toggles If Paths Are Condensed or Full Size"},
		{"Del", "Delete Selected"},
		{"Esc", "Clear Selection"}
	};
	
	private void ClearSelection()
	{
		SelectedItems.Clear();
		Rerender();
	}

	private void ToggleFileSizes()
	{
		DisplayFileSizes = !DisplayFileSizes;
		Rerender();
	}

	private void TogglePathWidth()
	{
		PathWidth = PathWidth == "short" ? "full" : "short";
		Rerender();
	}

	private void ToggleDestinationPaths()
	{
		DestinationPathsVisible = !DestinationPathsVisible;
		Rerender();
	}

	private void UpdateDateTakenSources()
	{
		foreach (var importItem in Selected)
			importItem.DateTakenSource = newDateTakenSource;

		Rerender();
	}

	private void ToggleStars()
	{
		bool allStarred = Selected.All(file => file.Starred);

		foreach (var importItem in Selected)
			importItem.Starred = !allStarred;

		Rerender();
	}

	private void DeleteSelected()
	{
		foreach (var importItem in Selected)
			FileSystem.DeleteFile(importItem.FullPath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
		
		importItems.RemoveAll(importItem => SelectedItems.Contains(importItem.Id));
		status = $"Deleted {SelectedItems.Count} Items";
		
		ClearSelection();
        Rerender();
	}

	private void DeleteCurrent()
	{
		importItems.RemoveAll(importItem => fv.Current.Id == importItem.Id);
		FileSystem.DeleteFile(fv.Current.FullPath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
		Rerender();

		if (fv.Index == importItems.Count)
			fv.Index--;
	}

	public void ChangeRangeState(int startIndex, int endIndex)
	{
		int increment = startIndex < endIndex ? 1 : -1;

		for (int i = startIndex; i != endIndex + increment; i += increment)
			SelectedItems.Add(importItems[i].Id);

		Rerender();
	}
	
	private void SortItems()
	{
		Func<ImportItem, int> libraryErrorsSortFunc = item => LibraryCache.Count(libItem => P.GetFileName(libItem.Key).Contains(item.NewFilename));

		importItems = sortMode switch
		{
			ImportSortMode.FilenameAsc => importItems.OrderBy(libraryErrorsSortFunc).ThenBy(item => item.NewFilename).ToList(),
			ImportSortMode.FilenameDesc => importItems.OrderByDescending(libraryErrorsSortFunc).ThenByDescending(item => item.NewFilename).ToList(),
			ImportSortMode.SelectedDateTakenAsc => importItems.OrderBy(libraryErrorsSortFunc).ThenBy(item => item.SelectedDateTaken).ThenBy(item => item.NewFilename).ToList(),
			ImportSortMode.SelectedDateTakenDesc => importItems.OrderByDescending(libraryErrorsSortFunc).ThenByDescending(item => item.SelectedDateTaken).ThenByDescending(item => item.NewFilename).ToList(),
			_ => throw new ArgumentOutOfRangeException()
		};

		LastCheckedIndex = 0;
	}

	private async void AddItems()
	{
		if (ErrorAmount > 0 || EditingFilename)
			return;

		List<ImportItem> items = SelectedItems.Count == 0 || SelectedItems.Count == importItems.Count ? importItems : Selected.ToList();

		status = $"Adding {items.Count} Items";
		await RerenderAsync();
		
		await Parallel.ForEachAsync(items, async (item, cancellationToken) =>
		{
			await C.InsertItem(item);

			if (item.Collections != null)
			{
				foreach (var collection in item.Collections)
					await C.AddToCollectionAsync(collection.Id, item.Id);
			}

			await Task.Run(() =>
			{
				Directory.CreateDirectory(C.CreateFullDateFolderPath(item.SelectedDateTaken));
				File.Move(item.FullPath, item.AbsoluteDestinationPath);
				D.UpdateDateTaken(item.AbsoluteDestinationPath, item.SelectedDateTaken);
				LibraryCache.Add(item.DestinationPath, new LibraryItem(item));
			}, cancellationToken);
		});

		if (SelectedItems.Count > 0)
			importItems.RemoveAll(item => SelectedItems.Contains(item.Id));
		else
			importItems.Clear();

		status = $"Added {items.Count} Items";
		await RerenderAsync();
		
		ClearSelection();
	}

	private void UpdateCollections()
	{
		if (fv.Visible)
			UpdateSelectedItemCollections();
		else
			UpdateSelectedItemsCollections();

		Rerender();
	}

	private void UpdateItemCollections(ImportItem item)
	{
		item.Collections ??= new HashSet<Collection>();

		if (cs.SelectedFolderId != -1)
		{
			item.Collections.Clear();
			item.Collections.Add(cs.Folders.Find(folder => folder.Id == cs.SelectedFolderId) ?? throw new NullReferenceException());
		}
		else
		{
			item.Collections.RemoveWhere(folder => cs.Folders.Contains(folder));
			item.Collections.UnionWith(cs.Albums.Where(album => cs.SelectedAlbums.Contains(album.Id)));
		}
	}

	private void UpdateSelectedItemCollections()
	{
		UpdateItemCollections(fv.CurrentIi ?? throw new NullReferenceException("Current Import Item is null"));
	}

	private void UpdateSelectedItemsCollections()
	{
		foreach (ImportItem item in Selected)
			UpdateItemCollections(item);
	}

	private static long CalculateSizeOfItems(IEnumerable<ImportItem> items) => items.Sum(item => item.Size);
}