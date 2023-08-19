using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace MemoryMosaic.Shared.Input;

public class InputComponent<T> : Component
{
	[Parameter]
    public T Input { get; set; } //TODO: figure out how to handle this warning.

	[Parameter]
    public EventCallback<T> InputChanged { get; set; }

	[Parameter]
    public string? Placeholder { get; set; }

	[Parameter]
	public int FontSize { get; set; } = 16;

	[Parameter]
	public string Width { get; set; } = "200px";

	[Parameter]
	public string Style { get; set; } = null!;

	[Parameter]
	public bool Disabled { get; set; }

	[Parameter]
	public Action<KeyboardEventArgs>? OnKeyDown { get; set; }

	[Parameter]
	public Action? OnFocusOut { get; set; }

	protected ElementReference input;

	public async void Focus() => await input.FocusAsync();
}