using Microsoft.AspNetCore.Components;

namespace MemoryMosaic.Shared.Buttons;

public abstract class Button : Component
{
	[Parameter, EditorRequired] public string Icon { get; set; } = null!;
	
	[Parameter, EditorRequired] public Action OnClick { get; set; } = null!;
	
	[Parameter] public string? Title { get; set; }
}