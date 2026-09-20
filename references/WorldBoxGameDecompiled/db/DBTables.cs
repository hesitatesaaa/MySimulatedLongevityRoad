using System;
using SQLite;
using UnityEngine;

namespace db;

public static class DBTables
{
	public static void createOrMigrateTables()
	{
		createTable<WorldLogMessage>();
		createTable<KingdomData>();
	}

	public unsafe static void createOrMigrateTable(Type pType)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		SQLiteConnectionWithLock syncConnection = DBManager.getSyncConnection();
		LockWrapper val = syncConnection.Lock();
		try
		{
			if ((int)((SQLiteConnection)syncConnection).CreateTable(pType, (CreateFlags)0) != 1)
			{
				TableMapping mapping = ((SQLiteConnection)syncConnection).GetMapping(pType, (CreateFlags)0);
				string text = "SELECT sql FROM sqlite_master WHERE type='table' AND name=?";
				string text2 = ((SQLiteConnection)syncConnection).ExecuteScalar<string>(text, new object[1] { mapping.TableName });
				((SQLiteConnection)syncConnection).DropTable(mapping);
				text2 = text2[..text2.LastIndexOf(')')];
				text2 = text2.Replace(" integer ", " INT ").Trim().Replace("  ", " ")
					.Replace(" ,", ",")
					.Replace(", ", ",")
					.Replace("\"", "");
				text2 += ",\nauto INT";
				text2 += ",\nPRIMARY KEY(id, timestamp)";
				text2 += "\n)";
				((SQLiteConnection)syncConnection).Execute(text2, Array.Empty<object>());
			}
		}
		finally
		{
			((IDisposable)(*(LockWrapper*)(&val))/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public unsafe static void checkTablesOK(bool pDropTable = false)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		int currentYear = Date.getCurrentYear();
		bool flag = true;
		SQLiteConnectionWithLock syncConnection = DBManager.getSyncConnection();
		LockWrapper val = syncConnection.Lock();
		try
		{
			foreach (HistoryMetaDataAsset item in AssetManager.history_meta_data_library.list)
			{
				if (!flag)
				{
					break;
				}
				foreach (Type value in item.table_types.Values)
				{
					if (checkTableExists(value) && !checkTableOK(value, currentYear))
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				return;
			}
			if (pDropTable)
			{
				Debug.Log((object)"Statistics have future data, dropping...");
				foreach (HistoryMetaDataAsset item2 in AssetManager.history_meta_data_library.list)
				{
					foreach (Type value2 in item2.table_types.Values)
					{
						if (checkTableExists(value2))
						{
							TableMapping mapping = ((SQLiteConnection)syncConnection).GetMapping(value2, (CreateFlags)0);
							((SQLiteConnection)syncConnection).DropTable(mapping);
						}
					}
				}
				if (checkTableExists<KingdomData>())
				{
					((SQLiteConnection)syncConnection).DropTable<KingdomData>();
				}
				if (checkTableExists<WorldLogMessage>())
				{
					((SQLiteConnection)syncConnection).DropTable<WorldLogMessage>();
				}
				return;
			}
			Debug.Log((object)"Statistics have future data, clearing...");
			foreach (HistoryMetaDataAsset item3 in AssetManager.history_meta_data_library.list)
			{
				foreach (Type value3 in item3.table_types.Values)
				{
					if (checkTableExists(value3))
					{
						TableMapping mapping2 = ((SQLiteConnection)syncConnection).GetMapping(value3, (CreateFlags)0);
						((SQLiteConnection)syncConnection).DeleteAll(mapping2);
					}
				}
			}
			if (checkTableExists<KingdomData>())
			{
				((SQLiteConnection)syncConnection).DeleteAll<KingdomData>();
			}
			if (checkTableExists<WorldLogMessage>())
			{
				((SQLiteConnection)syncConnection).DeleteAll<WorldLogMessage>();
			}
		}
		finally
		{
			((IDisposable)(*(LockWrapper*)(&val))/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public unsafe static bool checkTableExists(Type pType)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		SQLiteConnectionWithLock syncConnection = DBManager.getSyncConnection();
		LockWrapper val = syncConnection.Lock();
		try
		{
			TableMapping mapping = ((SQLiteConnection)syncConnection).GetMapping(pType, (CreateFlags)0);
			string text = "SELECT count(1) FROM sqlite_master WHERE type='table' AND name=?";
			if (((SQLiteConnection)syncConnection).ExecuteScalar<int>(text, new object[1] { mapping.TableName }) == 0)
			{
				return false;
			}
			return true;
		}
		finally
		{
			((IDisposable)(*(LockWrapper*)(&val))/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public unsafe static bool checkTableOK(Type pType, int pTimestamp)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		SQLiteConnectionWithLock syncConnection = DBManager.getSyncConnection();
		LockWrapper val = syncConnection.Lock();
		try
		{
			TableMapping mapping = ((SQLiteConnection)syncConnection).GetMapping(pType, (CreateFlags)0);
			string text = "SELECT count(1) FROM '" + mapping.TableName + "' WHERE timestamp>?";
			if (((SQLiteConnection)syncConnection).ExecuteScalar<int>(text, new object[1] { pTimestamp }) == 0)
			{
				return true;
			}
			return false;
		}
		finally
		{
			((IDisposable)(*(LockWrapper*)(&val))/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public static bool checkTableExists<T>()
	{
		return checkTableExists(typeof(T));
	}

	public static void createTableIfNotExists<T>()
	{
		if (!checkTableExists<T>())
		{
			createTable<T>();
		}
	}

	public unsafe static void createTable<T>()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		SQLiteConnectionWithLock syncConnection = DBManager.getSyncConnection();
		LockWrapper val = syncConnection.Lock();
		try
		{
			((SQLiteConnection)syncConnection).CreateTable<T>((CreateFlags)0);
		}
		finally
		{
			((IDisposable)(*(LockWrapper*)(&val))/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public static void createOrMigrateTablesLoader(bool pCreating = true)
	{
		string text = (pCreating ? "Creating" : "Migrating");
		if (!pCreating)
		{
			SmoothLoader.add(delegate
			{
				SQLiteConnectionWithLock syncConnection = DBManager.getSyncConnection();
				foreach (HistoryMetaDataAsset item in AssetManager.history_meta_data_library.list)
				{
					DBTriggers.dropTrigger(syncConnection, item);
				}
			}, "Dropping Triggers");
		}
		SmoothLoader.add(delegate
		{
			createOrMigrateTables();
		}, text + " Stats");
		foreach (HistoryMetaDataAsset tHistoryAsset in AssetManager.history_meta_data_library.list)
		{
			SmoothLoader.add(delegate
			{
				foreach (Type value in tHistoryAsset.table_types.Values)
				{
					createOrMigrateTable(value);
				}
			}, text + " Stats (" + tHistoryAsset.table_type.Name + ")");
		}
		SmoothLoader.add(delegate
		{
			SQLiteConnectionWithLock syncConnection = DBManager.getSyncConnection();
			foreach (HistoryMetaDataAsset item2 in AssetManager.history_meta_data_library.list)
			{
				DBTriggers.createTrigger(syncConnection, item2);
			}
		}, text + " Triggers");
	}
}
