using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PSS;

///<summary>Represents a <see cref="List{T}"/> of unique values that utilizes a <see cref="HashSet{T}"/> for efficient searching.</summary>
public class HashList<T> : IList<T>, ISet<T>, IReadOnlyList<T>, IReadOnlySet<T>
{
	private readonly List<T> internalList;
	private readonly HashSet<T> internalSet;

	///<summary>Creates a <see cref="HashList{T}"/> with the default <see cref="List{T}"/> and <see cref="HashSet{T}"/> capacities.</summary>
	public HashList()
	{
		internalList = new List<T>();
		internalSet = new HashSet<T>();
	}

	///<summary>Creates a <see cref="HashList{T}"/> and sets the starting capacities for the <see cref="List{T}"/> and <see cref="HashSet{T}"/>.</summary>
	///<param name="capacity">The number of elements the <see cref="HashList{T}"/> can initially store.</param>
	public HashList(int capacity)
	{
		internalList = new List<T>(capacity);
		internalSet = new HashSet<T>(capacity);
	}

	///<summary>Creates a <see cref="HashList{T}"/> that contains elements copied from the specified collection and has sufficient capacity to accommodate the number of elements copied.</summary>
	///<param name="items">The collection whose elements are copied to the new <see cref="HashList{T}"/></param>
	public HashList(IEnumerable<T> items)
	{
		internalList = new List<T>();
		internalSet = new HashSet<T>();
		
		foreach (var item in items)
		{
			if (!internalSet.Contains(item))
			{
				internalList.Add(item);
				internalSet.Add(item);
			}
		}
	}

	public T this[int index]
	{
		get => internalList[index];
		set
		{
			if (internalSet.Contains(value))
			{
				throw new InvalidOperationException($"Cannot set value because value already exists in HashList.");
			}

			internalSet.Remove(internalList[index]);
			internalSet.Add(value);

			internalList[index] = value;
		}
	}

	///<summary>Gets the number of elements contained in the <see cref="HashList{T}"/>.</summary>
	public int Count => internalList.Count;

	///<summary>Gets a value indicating whether the <see cref="HashList{T}"/> is read-only.</summary>
	public bool IsReadOnly => ((ICollection<T>)internalList).IsReadOnly && ((ICollection<T>)internalSet).IsReadOnly;

	///<summary>Adds an object to the end of the List in the <see cref="HashList{T}"/>, if that item is not already within the List.</summary>
	///<param name="item">The object to be added to the end of the <see cref="HashList{T}"/>. The value can be null for reference types.</param>
	void ICollection<T>.Add(T item) => Add(item);

	///<summary>Clears the <see cref="List{T}"/> and <see cref="HashSet{T}"/> of the <see cref="HashList{T}"/>.</summary>
	public void Clear()
	{
		internalList.Clear();
		internalSet.Clear();
	}

	///<summary>Determines whether the <see cref="HashList{T}"/> contains a specific value.</summary>
	///<param name="item">The item to search for.</param>
	///<returns><see langword="true"/> if the <see cref="HashList{T}"/> contains the value; <see langword="false"/> if the item is not in the <see cref="HashList{T}"/>.</returns>
	public bool Contains(T item) => internalSet.Contains(item);
	public void CopyTo(T[] array, int arrayIndex) => internalList.CopyTo(array, arrayIndex);
	public IEnumerator<T> GetEnumerator() => internalList.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => internalList.GetEnumerator();
	
	///<summary>Searches for the specified object and returns the zero-based index of the first occurrence within the entire <see cref="HashList{T}"/>'s List.</summary>
	///<param name="item">The item to search for. The value can be null for reference types.</param>
	///<returns>The zero-based index of the first occurrence of item within the entire <see cref="HashList{T}"/>'s List, if found; otherwise, -1.</returns>
	public int IndexOf(T item) => internalList.IndexOf(item);

	///<summary>Inserts an item in the <see cref="HashList{T}"/> at the specified index, if that item is not already within the HashList.</summary>
	///<param name="index">The zero-based index at which the item should be inserted.</param>
	///<param name="item">The object to insert. The value can be null for reference types.</param>
	public void Insert(int index, T item)
	{
		if (!internalSet.Contains(item))
		{
			internalSet.Add(item);
			internalList.Insert(index, item);
		}
	}

	///<summary>Attempts to insert an item in the <see cref="HashList{T}"/> at the specified index.</summary>
	///<param name="index">The zero-based index at which the item should be inserted.</param>
	///<param name="item">The object to insert. The value can be null for reference types.</param>
	///<returns><see langword="true"/> if the item is added to the <see cref="HashList{T}"/>; <see langword="false"/> if the item is already in the <see cref="HashList{T}"/>.</returns>
	public bool TryInsert(int index, T item)
	{
		if (!internalSet.Contains(item))
		{
			internalSet.Add(item);
			internalList.Insert(index, item);
			return true;
		}

		return false;
	}
	
	///<summary>Attempts to set an item in the <see cref="HashList{T}"/> at the specified index.</summary>
	///<param name="index">The zero-based index that should be set to the item.</param>
	///<param name="item">The object to insert. The value can be null for reference types.</param>
	///<returns><see langword="true"/> if the item is set into the <see cref="HashList{T}"/>; <see langword="false"/> if the item is already in the <see cref="HashList{T}"/>.</returns>
	public bool TrySet(int index, T item)
	{
		if (!internalSet.Contains(item))
		{
			this[index] = item;
			return true;
		}

		return false;
	}

	///<summary>Attempts to remove an item from the <see cref="HashList{T}"/>.</summary>
	///<param name="item">The object to remove. The value can be null for reference types.</param>
	///<returns><see langword="true"/> if the item is removed from the <see cref="HashList{T}"/>, false otherwise.</returns>
	public bool Remove(T item)
	{
		internalList.Remove(item);
		return internalSet.Remove(item);
	}
	
	///<summary>Attempts to remove an item from the <see cref="HashList{T}"/> at the specified index.</summary>
	///<param name="index">The zero-based index at which the item should be removed.</param>
	public void RemoveAt(int index)
	{
		internalSet.Remove(internalList[index]);
		internalList.RemoveAt(index);
	}

	///<summary>Attempts to insert an item at the end of the <see cref="HashList{T}"/>'s List.</summary>
	///<param name="item">The object to insert. The value can be null for reference types.</param>
	///<returns><see langword="true"/> if the item is added to the <see cref="HashList{T}"/>; <see langword="false"/> if the item is already in the <see cref="HashList{T}"/>.</returns>
	public bool Add(T item)
	{
		if (!internalSet.Contains(item))
		{
			internalList.Add(item);
			internalSet.Add(item);
			return true;
		}

		return false;
	}
	
	public void ExceptWith(IEnumerable<T> other)
	{
		internalSet.ExceptWith(other);
		internalList.RemoveAll(x => !internalSet.Contains(x));
	}
	
	public void IntersectWith(IEnumerable<T> other)
	{
		internalSet.IntersectWith(other);
		internalList.RemoveAll(x => !internalSet.Contains(x));
	}

	public bool IsProperSubsetOf(IEnumerable<T> other) => internalSet.IsProperSubsetOf(other);
	public bool IsProperSupersetOf(IEnumerable<T> other) => internalSet.IsProperSupersetOf(other);
	public bool IsSubsetOf(IEnumerable<T> other) => internalSet.IsSubsetOf(other);
	public bool IsSupersetOf(IEnumerable<T> other) => internalSet.IsSupersetOf(other);
	public bool Overlaps(IEnumerable<T> other) => internalSet.Overlaps(other);
	public bool SetEquals(IEnumerable<T> other) => internalSet.SetEquals(other);
	public void SymmetricExceptWith(IEnumerable<T> other)
	{
		IEnumerable<T> enumerable = other as T[] ?? other.ToArray();
		var toAdd = enumerable.Where(x => !internalSet.Contains(x)).ToList(); // $$$ this should probably be optimized
		internalSet.SymmetricExceptWith(enumerable);
		internalList.RemoveAll(x => !internalSet.Contains(x));
		internalList.AddRange(toAdd);
	}

	public void UnionWith(IEnumerable<T> other)
	{
		IEnumerable<T> enumerable = other as T[] ?? other.ToArray();
		internalList.AddRange(enumerable.Where(x => !internalSet.Contains(x)));
		internalSet.UnionWith(enumerable);
	}
}