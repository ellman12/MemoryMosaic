using System.Collections.Generic;

namespace PSS;

public static class HashListExtension
{
	public static HashList<T> ToHashList<T>(this IEnumerable<T> enumerable) => new(enumerable);
}