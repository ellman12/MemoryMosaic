namespace MemoryMosaic;

public static class HashListExtension
{
	public static HashList<T> ToHashList<T>(this IEnumerable<T> enumerable) => new(enumerable);
}