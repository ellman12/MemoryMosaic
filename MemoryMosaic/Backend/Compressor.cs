namespace MemoryMosaic.Backend;

///Manages automatically compressing <see cref="Media"/> in the background.
public static class Compressor
{
	public static Media? Current { get; private set; }

	public static PriorityQueue<Media, bool> Items { get; } = new();

	public static bool Compressing { get; private set; }

	public static void Enqueue(Media item)
	{
		Items.Enqueue(item, item.Video);
		StartCompressing();
	}

	public static void Enqueue(IEnumerable<Media> items)
	{
		foreach (Media item in items)
			Enqueue(item);
	}

	private static void StartCompressing()
	{
		if (Compressing)
			return;

		Compressing = true;

		while (Items.TryDequeue(out Media? current, out _))
		{
			Console.WriteLine("dequeue is " + current.Path);
		}

		Compressing = false;
	}

	private static void CompressItems() {}
}