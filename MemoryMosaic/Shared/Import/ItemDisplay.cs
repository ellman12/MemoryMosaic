namespace MemoryMosaic.Shared.Import;

using Microsoft.AspNetCore.Components;
using Pages;

///Base class for ImportItemDisplay and LibraryItemDisplay.
public abstract class ItemDisplay<T> : Component where T : Media
{
	[Parameter, EditorRequired] public required Import Import { get; set; }
	
	[Parameter, EditorRequired] public required T Item { get; set; }
	
	[Parameter, EditorRequired] public required int Index { get; set; }
	
	[Parameter, EditorRequired] public required string TextClass { get; set; }
	
	protected string PathWidth => Import.PathWidth == "full" || TextClass == "error" ? "full" : "short";
}