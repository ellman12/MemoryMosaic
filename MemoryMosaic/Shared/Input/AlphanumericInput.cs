using Microsoft.AspNetCore.Components;

namespace MemoryMosaic.Shared.Input;

///Represents an input component that accepts either text or numbers.
public class AlphanumericInput<T> : InputComponent<T>
{
	[Parameter] public string? Placeholder { get; set; }

	[Parameter] public string Width { get; set; } = "200px";

	protected override void OnParametersSet()
	{
		Width = Width == "available" ? "available; width: -moz-available; width: -webkit-fill-available;" : Width;
	}
}