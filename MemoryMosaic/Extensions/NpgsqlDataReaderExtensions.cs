namespace MemoryMosaic.Extensions;

public static class NpgsqlDataReaderExtensions
{
	public static DateTime? TryGetDateTime(this NpgsqlDataReader reader, int ordinal) => reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);

	public static string? TryGetString(this NpgsqlDataReader reader, int ordinal) => reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
	
	public static double? TryGetDouble(this NpgsqlDataReader reader, int ordinal) => reader.IsDBNull(ordinal) ? null : reader.GetDouble(ordinal);
}