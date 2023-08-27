using System.Data;
using MemoryMosaic.Backend;
using Npgsql;

namespace Initialization;

public static class Database
{
	private static NpgsqlConnection Connection { get; } = new("Host=localhost; Port=5432; Username=postgres; Password=Ph0t0s_Server; Database=postgres");

	private static void Connect()
	{
		if (Connection.State != ConnectionState.Open)
			Connection.Open();
	}

	private static void Close()
	{
		if (Connection.State != ConnectionState.Closed)
			Connection.Close();
	}

	public static void Create(string name)
	{
		Connect();
		using var cmd = new NpgsqlCommand($"CREATE DATABASE \"{name}\" WITH OWNER = postgres ENCODING = 'UTF8' CONNECTION LIMIT = -1;", Connection);
		cmd.ExecuteNonQuery();
		Close();
	}

	public static void Delete(string name)
	{
		Connect();
		using var cmd = new NpgsqlCommand($"DROP DATABASE \"{name}\"", Connection);
		cmd.ExecuteNonQuery();
		Close();
	}

	///Returns true if the database with this name exists, false otherwise.
	public static bool Exists(string name)
	{
		Connect();
		using var cmd = new NpgsqlCommand($"SELECT datname FROM pg_database WHERE datname = '{name}'", Connection);
		using var reader = cmd.ExecuteReader();
		if (reader.HasRows && reader.Read())
			return reader.GetString(0) == name;
		return false;
	}
}