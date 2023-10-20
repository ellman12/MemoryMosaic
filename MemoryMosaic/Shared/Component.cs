using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace MemoryMosaic.Shared;

///Extends the ComponentBase class provided by Microsoft.
public abstract class Component : ComponentBase
{
	[Parameter] public string? ID { get; set; }
	
	[Parameter] public string? Class { get; set; }

	[Parameter] public string? Title { get; set; }

	[Parameter] public string? Style { get; set; }
	
	[Parameter] public Action? OnClick { get; set; }

	[Parameter] public Action<MouseEventArgs>? EventArgsOnClick { get; set; }

	protected void HandleClick(MouseEventArgs e)
	{
		OnClick?.Invoke();
		EventArgsOnClick?.Invoke(e);
	}
}