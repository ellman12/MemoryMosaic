using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace MemoryMosaic.Shared.Buttons;

public class ActionButton : Button
{
	[Parameter] public Action? OnClick { get; set; }

	[Parameter] public Action<MouseEventArgs>? EventArgsOnClick { get; set; }

	protected void HandleClick(MouseEventArgs e)
	{
		OnClick?.Invoke();
		EventArgsOnClick?.Invoke(e);
	}
}