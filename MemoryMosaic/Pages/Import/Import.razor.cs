using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Threading;
using MemoryMosaic.Shared.Modal;
using MemoryMosaic.Shared.Modal.FullscreenViewer;
using MemoryMosaic.Shared.Input;

namespace MemoryMosaic.Pages.Import;

public sealed partial class Import
{
	public HashSet<Guid> SelectedItems { get; } = new();

	public string PathWidth { get; private set; } = "short";

	public bool DestinationPathsVisible { get; private set; }

	public bool EditingFilename { get; set; }

	public bool DisplayFileSizes { get; private set; }

	public int LastCheckedIndex { get; set; }

	public int MinYear { get; private set; } = 2000;
	public int MaxYear { get; private set; } = DateTime.Now.Year;

	private TextInput searchInput = null!;
	
	private string searchText = "", status = "";

	public ConcurrentDictionary<string, LibraryItem> LibraryCache { get; private set; } = null!;

	public FullscreenViewer<Media> fv = null!;

	private bool itemsLoading = true, thumbnailsLoading = true, addingItems, displayWarnings = true, onlyDisplayErrors;

	private DateTakenSource newDateTakenSource = DateTakenSource.None;

	private ImportSortMode sortMode = ImportSortMode.SelectedDateTakenAsc;

	private List<ImportItem> importItems = new();

	private ProgressPopUp popUp = null!;

	private CollectionSelector cs = null!;

	private Dropdown moreSettings = null!;

	public bool HoveringThumbnail { get; set; }
	public DateTime HoverStartTime { get; set; }

	public void Rerender() => StateHasChanged();
	public async Task RerenderAsync() => await InvokeAsync(StateHasChanged);

	protected override async Task OnInitializedAsync()
	{
		L.LogLine("Begin Import Initialization", LogLevel.Info);
		await InitializeItems();
		L.LogLine("Finish Import Initialization", LogLevel.Info);
	}

	private async Task InitializeItems()
	{
		ConcurrentBag<ImportItem> bag = new();

		await Task.Run(() => Parallel.ForEach(F.GetSupportedFiles(S.ImportFolderPath), (fullPath, _) =>
		{
			bag.Add(new ImportItem(fullPath.Replace('\\', '/')));
		}));

		importItems = bag.ToList();
		LibraryCache = new ConcurrentDictionary<string, LibraryItem>(D.GetEntireLibrary().ToDictionary(key => key.Path, value => value));
		itemsLoading = false;
		SortItems();
		await RerenderAsync();

		await InitializeThumbnails();
	}

	private async Task InitializeThumbnails()
	{
		status = "Initializing thumbnails...";
		L.LogLine(status, LogLevel.Debug);

		await Parallel.ForEachAsync(importItems.ToImmutableList(), async (importItem, _) =>
		{
			importItem.Thumbnail = await FF.GenerateThumbnailAsync(importItem.FullPath);
			await RerenderAsync();
		});
		thumbnailsLoading = false;

		status = "Thumbnails initialized";
		await RerenderAsync();

		status = "";
		await RerenderAsync();
	}

	private int ErrorAmount
	{
		get
		{
			if (addingItems)
				return 0;

			int total = 0;
			total += importItems.GroupBy(item => item.DestinationPath).Count(group => group.Count() > 1);
			total += LibraryCache.Values.Count(libraryItem => importItems.Any(importItem => importItem.DestinationPath == libraryItem.Path));
			return total;
		}
	}

	private int WarningAmount => addingItems ? 0 : importItems.GroupBy(importItem => importItem.DestinationPath).Count(group => LibraryCache.Values.Any(libraryItem => group.Any(importItem => importItem.NewFilename == libraryItem.FilenameWithoutExtension)));

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

			foreach (var group in CurrentItems.GroupBy(item => item.DestinationPath))
			{
				var existingItems = addingItems
					? ImmutableArray<LibraryItem>.Empty
					: LibraryCache.Values.Where(libraryItem => group.Any(importItem => importItem.DestinationPath == libraryItem.Path)).ToImmutableArray();
	
				var warnings = displayWarnings && !addingItems
					? LibraryCache.Values.Where(libraryItem => group.Any(importItem => importItem.NewFilename == libraryItem.FilenameWithoutExtension && !existingItems.Contains(libraryItem))).ToImmutableArray()
					: ImmutableArray<LibraryItem>.Empty;
				
				content.AddRange(group);
				content.AddRange(existingItems);
				content.AddRange(warnings);
			}

			return content;
		}
	}

	private ImmutableArray<ImportItem> Selected => importItems.Where(item => SelectedItems.Contains(item.Id)).ToImmutableArray();

	private IEnumerable<ImportItem> SearchResults => importItems.Where(item => item.NewFilenameWithExtension.IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase) != -1);

	private IEnumerable<ImportItem> Errors
	{
		get
		{
			return SearchResults
				.GroupBy(item => item.DestinationPath)
				.Where(group => group.Count() + LibraryCache.Values.Count(libraryItem => group.Any(importItem => importItem.DestinationPath == libraryItem.Path)) > 1)
				.SelectMany(item => item);
		}
	}

	private IEnumerable<ImportItem> CurrentItems
	{
		get
		{
			if (onlyDisplayErrors)
				return Errors;

			if (!String.IsNullOrWhiteSpace(searchText))
				return SearchResults;

			return importItems;
		}
	}

	private static readonly Dictionary<string, string> Shortcuts = new()
	{
		{"Ctrl A", "Select All"},
		{"Alt A", "Add Selected, If No Errors"},
		{"Alt C", "Toggle CollectionSelector"},
		{"Alt D", "Toggles If Destination Paths Are Shown"},
		{"Alt F", "Toggle Displaying File Sizes"},
		{"Alt S", "Toggles If Paths Are Condensed or Full Size"},
		{"Del", "Delete Selected"},
		{"Esc", "Clear Selection"},
		{"Ctrl Click", "Always Select Item"},
		{"Alt Click", "Always Fullscreen Item"},
		{"Shift Click", "Select Range of Items"}
	};

	private void ClearSelection()
	{
		if (!fv.Visible)
		{
			SelectedItems.Clear();
			Rerender();
		}
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

	private void SelectAll()
	{
		foreach (ImportItem item in CurrentItems)
			SelectedItems.Add(item.Id);

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

	private async void DeleteSelected()
	{
		if (fv.Visible)
			return;

		fv.Index = 0;
		var itemsToDelete = Selected;
		importItems.RemoveAll(importItem => SelectedItems.Contains(importItem.Id));
		ClearSelection();
		await RerenderAsync();

		foreach (var importItem in itemsToDelete)
			DeleteFile(importItem.FullPath);

		status = $"Deleted {F.GetPluralized(itemsToDelete, "Item")}";
		L.LogLine(status, LogLevel.Info);

		await RerenderAsync();
	}

	private async void DeleteCurrent()
	{
		if (fv.Index == importItems.Count)
			fv.Index--;

		await fv.CleanupVideo();

		await DeleteItem(fv.CurrentIi!);
	}

	public async Task DeleteItem(ImportItem item)
	{
		importItems.RemoveAll(i => i.Id == item.Id);
		SelectedItems.Remove(item.Id);
		await fv.RerenderAsync();
		await RerenderAsync();
		await Task.Delay(0);
		await Task.Delay(1);
		await Task.Delay(2000);
		DeleteFile(item.FullPath);
	}
	
	private static void DeleteFile(string path)
	{
		GC.Collect();
		GC.WaitForPendingFinalizers();
		
		try { FileSystem.DeleteFile(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin); }
		catch (Exception e) { L.LogException(e); }
	}

	public void ChangeRangeState(int startIndex, int endIndex)
	{
		int increment = startIndex < endIndex ? 1 : -1;

		var items = CurrentItems.ToImmutableArray();
		for (int i = startIndex; i != endIndex + increment; i += increment)
			SelectedItems.Add(items[i].Id);

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

	private async void AltAFired()
	{
		if (fv.Visible)
			await AddItem(fv.CurrentIi!);
		else
			await AddItems();
	}

	///Adds items to the library.
	private async Task AddItems()
	{
		if (ErrorAmount > 0 || EditingFilename)
			return;

		fv.Index = 0;
		
		ImmutableArray<ImportItem> items;
		if (SelectedItems.Count == 0 || SelectedItems.Count == importItems.Count)
			items = importItems.ToImmutableArray();
		else if (SelectedItems.Count > 0)
			items = Selected;
		else
			items = importItems.ToImmutableArray();

		popUp.Message = status = $"Adding {F.GetPluralized(items, "Item")}";
		L.LogLine(status, LogLevel.Info);

		if (items.Length > 1)
		{
			popUp.Enable();
			await RerenderAsync();
			await Task.Delay(1);
		}

		addingItems = true;
		await Parallel.ForEachAsync(items, async (item, _) => await AddItem(item));
		addingItems = false;

		popUp.Message = status = $"Added {F.GetPluralized(items, "Item")}";
		L.LogLine(status, LogLevel.Info);
		popUp.Disable();
		await Task.Delay(1);
		await RerenderAsync();

		ClearSelection();
	}

	///Adds a single item to the library.
	public async Task AddItem(ImportItem item)
	{
		await InvokeAsync(() => { importItems.RemoveAll(i => i.Id == item.Id); SelectedItems.Remove(item.Id); });
		await fv.RerenderAsync();
		await RerenderAsync();
		await Task.Delay(2000);
		
		GC.Collect();
		GC.WaitForPendingFinalizers();
		
		await Task.Delay(2000);
		
		ThreadPool.QueueUserWorkItem(_ =>
		{
			Directory.CreateDirectory(D.CreateFullDateFolderPath(item.SelectedDateTaken));
			
			try { File.Move(item.FullPath, item.AbsoluteDestinationPath); }
			catch (Exception e) { L.LogException(e); }
			
			DTE.UpdateDateTaken(item.AbsoluteDestinationPath, item.SelectedDateTaken);
		});

		await D.InsertItem(item);
    
    	if (item.Collections != null)
    	{
    		foreach (var collection in item.Collections)
    			await D.AddToCollectionAsync(collection.Id, item.Id);
    	}
		
		LibraryCache.TryAdd(item.DestinationPath, new LibraryItem(item));
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
}