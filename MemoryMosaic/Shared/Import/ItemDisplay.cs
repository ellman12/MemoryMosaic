namespace MemoryMosaic.Shared.Import;

using Microsoft.AspNetCore.Components;
using Pages.Import;

///Base class for ImportItemDisplay and LibraryItemDisplay.
public abstract class ItemDisplay<T> : Component where T : Media
{
	[Parameter, EditorRequired] public required Import Import { get; set; }
	
	[Parameter, EditorRequired] public required T Item { get; set; }
	
	[Parameter, EditorRequired] public required int Index { get; set; }
	
	[Parameter, EditorRequired] public required string TextClass { get; set; }
	
	protected string PathWidth => Import.PathWidth == "full" || TextClass != "none" ? "full" : "short";

	protected void ThumbnailClicked()
	{
		Import.fv.Index = Index;
		Import.fv.Enable();
		Import.Rerender(); 
	}
	
	private const int HoverPreloadDelay = 150; //ms

	protected async Task OnMouseOver()
	{
		Import.HoveringThumbnail = true;
		Import.HoverStartTime = DateTime.Now;
		await Task.Delay(HoverPreloadDelay);

		if (Import.HoveringThumbnail && (DateTime.Now - Import.HoverStartTime).TotalMilliseconds >= HoverPreloadDelay)
			Import.fv.Preload(Index);
	}

	protected void OnMouseOut() => Import.HoveringThumbnail = false;
}