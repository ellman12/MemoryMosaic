using Microsoft.AspNetCore.Components;

namespace MemoryMosaic.Shared;

///Extends the ComponentBase class provided by Microsoft.
public abstract class Component : ComponentBase
{
	[Parameter] public string? ID { get; set; }
	
	[Parameter] public string? Class { get; set; }
}