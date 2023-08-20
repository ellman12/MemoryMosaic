using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace MemoryMosaic.Shared.Input;

public class InputComponent<T> : Component
{
	[Parameter]
    public T? Input { get; set; }

	[Parameter]
    public EventCallback<T?> InputChanged { get; set; }

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

	public async void Focus() => await input.FocusAsync();

	protected ElementReference input;

	protected async void UpdateInput() => await InputChanged.InvokeAsync(Input);

	protected void HandleKeyDown(KeyboardEventArgs e) => OnKeyDown?.Invoke(e);

	protected void HandleFocusOut() => OnFocusOut?.Invoke();

	protected override void OnInitialized()
	{
		Width = Width == "available" ? "available; width: -moz-available; width: -webkit-fill-available;" : Width;
	}
}