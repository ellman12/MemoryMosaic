namespace MemoryMosaic.Pages;

using System.Collections.Immutable;
using Shared.Modal;
using Shared.Modal.FullscreenViewer;

public sealed partial class Import
{
	public HashSet<Guid> SelectedItems { get; } = new();

	public string PathWidth { get; private set; } = "short";

	public bool DestinationPathsVisible { get; private set; }

	public bool EditingFilename { get; set; }

	public int LastCheckedIndex { get; set; }

	public int MinYear { get; private set; } = 2000;
	public int MaxYear { get; private set; } = DateTime.Now.Year;

	public Dictionary<string, LibraryItem> LibraryCache { get; private set; } = null!;

	public FullscreenViewer<Media> fv = null!;

	private bool displayWarnings = true, finishedLoading;

	private DateTakenSource newDateTakenSource;

	private List<ImportItem> importItems = new();

	private CollectionSelector cs = null!;

	private Dropdown moreOptions = null!;

	public void Rerender() => StateHasChanged();

	protected override async Task OnInitializedAsync()
	{
		L.LogLine("Begin Import Initialization", LogLevel.Info);

		LibraryCache = C.GetEntireLibrary().ToDictionary(key => key.Path, value => value);

		Parallel.ForEach(F.GetSupportedFiles(S.ImportFolderPath), (absPath, _) =>
		{
			ImportItem importFile = new(absPath.Replace('\\', '/'));
			importItems.Add(importFile);
		});

		SortItems();

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

	private void ClearSelection()
	{
		SelectedItems.Clear();
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
		foreach (var importItem in importItems.Where(item => SelectedItems.Contains(item.Id)))
			importItem.DateTakenSource = newDateTakenSource;

		Rerender();
	}

	private void ToggleStars()
	{
		var items = importItems.Where(item => SelectedItems.Contains(item.Id)).ToImmutableArray();
		bool allStarred = items.All(file => file.Starred);

		foreach (var importItem in items)
			importItem.Starred = !allStarred;

		Rerender();
	}

	private void DeleteSelected()
	{
		var itemsToDelete = importItems.Where(importItem => SelectedItems.Contains(importItem.Id));

		foreach (var importItem in itemsToDelete)
			FileSystem.DeleteFile(importItem.FullPath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);

		importItems.RemoveAll(importItem => SelectedItems.Contains(importItem.Id));
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
		importItems = importItems.OrderByDescending(item => LibraryCache.Count(libItem => P.GetFileName(libItem.Key).Contains(item.Filename)))
			.ThenBy(item => item.Filename)
			.ToList();
	}

	private async void AddItems()
	{
		IEnumerable<ImportItem> items = SelectedItems.Count == 0 || SelectedItems.Count == importItems.Count ? importItems : importItems.Where(item => SelectedItems.Contains(item.Id));

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
				File.Move(item.AbsolutePath, item.AbsoluteDestinationPath);
				D.UpdateDateTaken(item.AbsoluteDestinationPath, item.SelectedDateTaken);
				LibraryCache.Add(item.DestinationPath, new LibraryItem(item));
			}, cancellationToken);
		});

		if (SelectedItems.Count > 0)
			importItems.RemoveAll(file => SelectedItems.Contains(file.Id));
		else
			importItems.Clear();

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
		foreach (ImportItem item in importItems.Where(item => SelectedItems.Contains(item.Id)))
			UpdateItemCollections(item);
	}
}