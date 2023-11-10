namespace MemoryMosaic.Extensions;

public static class NpgsqlDataReaderExtensions
{
	public static DateTime? TryGetDateTime(this NpgsqlDataReader reader, int ordinal)
	{
		return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
	}

	public static string? TryGetString(this NpgsqlDataReader reader, int ordinal)
	{
		return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
	}
}