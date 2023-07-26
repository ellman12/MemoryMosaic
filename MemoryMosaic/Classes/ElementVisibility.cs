namespace MemoryMosaic.Classes;

public sealed class ElementVisibility
{
	public string Style { get; private set; } = "visibility: hidden";
	
	public Action? Rerender { get; private set; }

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
	
	public ElementVisibility() { }

	public ElementVisibility(Action rerender) => Rerender = rerender;

	public void Toggle() => ChangeState(!Visible);
	public void Disable() => ChangeState(false);
	public void Enable() => ChangeState(true);

	///Sets the states of multiple items to a new state.
	public static void ChangeStates(bool newState, params ElementVisibility[] visibilities)
	{
		foreach (var visibility in visibilities)
			visibility.ChangeState(newState);
	}

	private void ChangeState(bool newState)
	{
		Visible = newState;
		Rerender?.Invoke();
	}
}