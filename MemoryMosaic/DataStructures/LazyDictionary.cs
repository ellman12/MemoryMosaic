namespace MemoryMosaic.DataStructures;

///A more flexible alternative to the traditional Dictionary.
public class LazyDictionary<TKey, TElement> : IDictionary<TKey, TElement> where TKey : notnull
{
	private readonly Dictionary<TKey, TElement> _internalDictionary;

	#nullable disable
	public Func<TElement> GetDefault { get; init; } = null;
	public bool AddMissingKeys { get; init; } = false;
	public IEqualityComparer<TKey> Comparer => _internalDictionary.Comparer;
	#nullable restore

	public LazyDictionary() => _internalDictionary = new();
	public LazyDictionary(int capacity) => _internalDictionary = new(capacity);
	public LazyDictionary(IEqualityComparer<TKey> comparer) => _internalDictionary = new(comparer);
	public LazyDictionary(int capacity, IEqualityComparer<TKey> comparer) => _internalDictionary = new(capacity, comparer);
	public LazyDictionary(IEnumerable<KeyValuePair<TKey, TElement>> dictionary) => _internalDictionary = new(dictionary);
	public LazyDictionary(IDictionary<TKey, TElement> dictionary) => _internalDictionary = new(dictionary);
	public LazyDictionary(IEnumerable<KeyValuePair<TKey, TElement>> dictionary, IEqualityComparer<TKey> comparer) => _internalDictionary = new(dictionary, comparer);
	public LazyDictionary(IDictionary<TKey, TElement> dictionary, IEqualityComparer<TKey> comparer) => _internalDictionary = new(dictionary, comparer);
	internal LazyDictionary(Dictionary<TKey, TElement> dictionary) => _internalDictionary = dictionary;

	public TElement this[TKey key]
	{
		get
		{
			if (_internalDictionary.TryGetValue(key, out var value))
			{
				return value;
			}
			else
			{
				if (GetDefault is null)
				{
					return default!;
				}
				else
				{
					var newValue = GetDefault();
					if (AddMissingKeys)
					{
						_internalDictionary.Add(key, newValue);
					}

					return newValue;
				}
			}
		}

		set
		{
			if (!_internalDictionary.TryAdd(key, value))
			{
				_internalDictionary[key] = value;
			}
		}
	}

	public ICollection<TKey> Keys => _internalDictionary.Keys;
	public ICollection<TElement> Values => _internalDictionary.Values;
	public int Count => _internalDictionary.Count;
	public bool IsReadOnly => false;

	public void Add(TKey key, TElement value) => this[key] = value;
	void ICollection<KeyValuePair<TKey, TElement>>.Add(KeyValuePair<TKey, TElement> item) => ((ICollection<KeyValuePair<TKey, TElement>>) _internalDictionary).Add(item);
	public void Clear() => _internalDictionary.Clear();
	public bool ContainsKey(TKey key) => _internalDictionary.ContainsKey(key);
	IEnumerator<KeyValuePair<TKey, TElement>> IEnumerable<KeyValuePair<TKey, TElement>>.GetEnumerator() => _internalDictionary.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => _internalDictionary.GetEnumerator();
	public bool Remove(TKey key) => _internalDictionary.Remove(key);
	public bool Remove(TKey key, out TElement value) => _internalDictionary.Remove(key, out value!);

	public bool TryGetValue(TKey key, out TElement value) => _internalDictionary.TryGetValue(key, out value!);

	bool ICollection<KeyValuePair<TKey, TElement>>.Contains(KeyValuePair<TKey, TElement> item) => ((ICollection<KeyValuePair<TKey, TElement>>) _internalDictionary).Contains(item);
	void ICollection<KeyValuePair<TKey, TElement>>.CopyTo(KeyValuePair<TKey, TElement>[] array, int arrayIndex) => ((ICollection<KeyValuePair<TKey, TElement>>) _internalDictionary).CopyTo(array, arrayIndex);
	bool ICollection<KeyValuePair<TKey, TElement>>.Remove(KeyValuePair<TKey, TElement> item) => ((ICollection<KeyValuePair<TKey, TElement>>) _internalDictionary).Remove(item);
}