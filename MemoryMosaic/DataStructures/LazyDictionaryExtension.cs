namespace MemoryMosaic.DataStructures;

public static class LazyDictionaryExtension
{
	public static LazyDictionary<TKey, TElement> ToLazyDictionary<TKey, TElement>(
		this IEnumerable<KeyValuePair<TKey, TElement>> enumerable)
		where TKey : notnull
		=> new(enumerable);

	public static LazyDictionary<TKey, TElement> ToLazyDictionary<TSource, TKey, TElement>(
		this IEnumerable<TSource> enumerable,
		Func<TSource, TKey> keySelector,
		Func<TSource, TElement> elementSelector,
		IEqualityComparer<TKey> comparer)
		where TKey : notnull
		=> new(enumerable.ToDictionary(keySelector, elementSelector, comparer));

	public static LazyDictionary<TKey, TElement> ToLazyDictionary<TSource, TKey, TElement>(
		this IEnumerable<TSource> enumerable,
		Func<TSource, TKey> keySelector,
		Func<TSource, TElement> elementSelector)
		where TKey : notnull
		=> new(enumerable.ToDictionary(keySelector, elementSelector));

	public static LazyDictionary<TKey, TSource> ToLazyDictionary<TSource, TKey>(
		this IEnumerable<TSource> enumerable,
		Func<TSource, TKey> keySelector,
		IEqualityComparer<TKey> comparer)
		where TKey : notnull
		=> new(enumerable.ToDictionary(keySelector, comparer));

	public static LazyDictionary<TKey, TSource> ToLazyDictionary<TSource, TKey>(
		this IEnumerable<TSource> enumerable,
		Func<TSource, TKey> keySelector)
		where TKey : notnull
		=> new(enumerable.ToDictionary(keySelector));
}