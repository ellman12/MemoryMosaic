namespace MemoryMosaic.Backend.Enums;

///How items in Trash should be sorted.
public enum TrashSortMode
{
	NewestDateDeleted, //Default
	NewestDateTaken,
	NewestDateAdded,
	OldestDateDeleted,
	OldestDateTaken,
	OldestDateAdded
}