using System;
using System.Collections.Generic;

namespace SQLite;

public static class SQLiteExtensions
{
	public static ListPool<Dictionary<string, long>> Query(this SQLiteConnection conn, string query, params object[] args)
	{
		return new ListPool<Dictionary<string, long>>(conn.CreateCommand(query, args).ExecuteDeferredQuery());
	}

	public static IEnumerable<Dictionary<string, long>> ExecuteDeferredQuery(this SQLiteConnection conn, string query, params object[] args)
	{
		return conn.CreateCommand(query, args).ExecuteDeferredQuery();
	}

	public static int Delete(this SQLiteConnection conn, string columnName, object obj, Type objType)
	{
		return conn.Delete(columnName, obj, conn.GetMapping(objType, (CreateFlags)0));
	}

	public static int Delete(this SQLiteConnection conn, string columnName, object columnValue, TableMapping map)
	{
		Column val = map.FindColumn(columnName);
		if (val == null)
		{
			throw new NotSupportedException("Cannot delete " + map.TableName + ": it has no column named " + columnName);
		}
		string text = $"delete from \"{map.TableName}\" where \"{val.Name}\" = ?";
		int num = conn.Execute(text, new object[1] { columnValue });
		if (num > 0)
		{
			conn.OnTableChanged(map, (NotifyTableChangedAction)2);
		}
		return num;
	}

	public static ListPool<T> ExecuteQueryPool<T>(this SQLiteCommand cmd, TableMapping map)
	{
		return new ListPool<T>(cmd.ExecuteDeferredQuery<T>(map));
	}

	public static ListPool<T> ExecuteQueryPool<T>(this SQLiteCommand cmd, SQLiteConnection conn)
	{
		return new ListPool<T>(cmd.ExecuteDeferredQuery<T>(conn.GetMapping(typeof(T), (CreateFlags)0)));
	}

	public static ListPool<object> QueryPool(this SQLiteConnection conn, TableMapping map, string query, params object[] args)
	{
		return conn.CreateCommand(query, args).ExecuteQueryPool<object>(map);
	}

	public static ListPool<T> QueryPool<T>(this SQLiteConnection conn, string query, params object[] args) where T : new()
	{
		return conn.CreateCommand(query, args).ExecuteQueryPool<T>(conn);
	}
}
