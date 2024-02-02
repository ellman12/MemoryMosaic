namespace MemoryMosaic.Enums;

///How items in Collections.razor should be sorted.
public enum CollectionsSortMode
{
	Title,
	TitleReversed,
	LastModified,
	LastModifiedReversed,
	HighestCount,
	LowestCount,
	NewestItemFirst,
	OldestItemFirst
}