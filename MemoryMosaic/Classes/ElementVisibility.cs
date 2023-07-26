namespace MemoryMosaic.Classes;

public sealed class ElementVisibility
{
	public string Style { get; private set; } = "visibility: hidden";

	private bool visible;
	public bool Visible
	{
		get => visible;
		set
		{
			visible = value;
			Style = $"visibility: {(visible ? "visible" : "hidden")}";
		}
	}
	
	public void Toggle() => Visible = !Visible;
	public void Disable() => Visible = false;
	public void Enable() => Visible = true;
}