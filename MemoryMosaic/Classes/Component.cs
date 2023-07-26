using Microsoft.AspNetCore.Components;

namespace MemoryMosaic.Classes;

///Extends the ComponentBase class provided by Microsoft.
public abstract class Component : ComponentBase
{
	public string? Id { get; set; }
	
	public string? CssClass { get; set; }
}