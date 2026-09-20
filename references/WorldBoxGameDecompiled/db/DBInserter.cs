using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using SQLite;
using UnityEngine;
using db.tables;

namespace db;

public static class DBInserter
{
	internal static int _insert_commands_count = 0;

	internal static readonly List<(long MetaID, Type MetaType)> _delete_commands = new List<(long, Type)>(4096);

	internal static readonly Dictionary<string, ListPool<HistoryTable>> _insert_commands = new Dictionary<string, ListPool<HistoryTable>>(64);

	internal static readonly Dictionary<string, ListPool<BaseSystemData>> _insert_metas = new Dictionary<string, ListPool<BaseSystemData>>(64);

	internal static readonly List<WorldLogMessage> _insert_logs = new List<WorldLogMessage>(64);

	private const float SQL_TIMEOUT_TIME = 10f;

	private static float _sql_timeout = 10f;

	private static Task _thread;

	private static bool _locked = false;

	public static void Lock()
	{
		_locked = true;
	}

	public static void Unlock()
	{
		_locked = false;
	}

	public static bool isLocked()
	{
		if (!_locked)
		{
			return Config.disable_db;
		}
		return true;
	}

	public static void deleteData(long pID, string pMetaType)
	{
		if (isLocked())
		{
			return;
		}
		foreach (Type value in AssetManager.history_meta_data_library.get(pMetaType).table_types.Values)
		{
			_delete_commands.Add((pID, value));
		}
	}

	public static void insertLog(WorldLogMessage pObject)
	{
		if (!isLocked())
		{
			_insert_logs.Add(pObject);
			_insert_commands_count++;
		}
	}

	public static void insertData(BaseSystemData pObject, string tMetaType)
	{
		if (!isLocked())
		{
			if (!_insert_metas.TryGetValue(tMetaType, out var value))
			{
				value = new ListPool<BaseSystemData>();
				_insert_metas.Add(tMetaType, value);
			}
			value.Add(pObject);
			_insert_commands_count++;
			if (ScrollWindow.isWindowActive())
			{
				executeCommands();
			}
		}
	}

	public static void insertData(HistoryTable pObject, string tMetaType)
	{
		if (!isLocked())
		{
			if (!_insert_commands.TryGetValue(tMetaType, out var value))
			{
				value = new ListPool<HistoryTable>();
				_insert_commands.Add(tMetaType, value);
			}
			value.Add(pObject);
			_insert_commands_count++;
		}
	}

	public static bool hasCommands()
	{
		if (_insert_commands_count <= 0)
		{
			return _delete_commands.Count > 0;
		}
		return true;
	}

	public static void clearCommands()
	{
		_insert_logs.Clear();
		_insert_commands.Clear();
		_insert_metas.Clear();
		_insert_commands_count = 0;
		_delete_commands.Clear();
	}

	public unsafe static void executeCommands()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		waitForAsync();
		if (isLocked() || !hasCommands())
		{
			return;
		}
		SQLiteConnectionWithLock tDBConn = DBManager.getSyncConnection();
		LockWrapper val = tDBConn.Lock();
		try
		{
			if (!hasCommands())
			{
				return;
			}
			ListPool<ListPool<BaseSystemData>> tMetasList = ((_insert_metas.Values.Count > 0) ? new ListPool<ListPool<BaseSystemData>>(_insert_metas.Values.Count) : null);
			ListPool<ListPool<HistoryTable>> tCommandsList = ((_insert_commands.Values.Count > 0) ? new ListPool<ListPool<HistoryTable>>(_insert_commands.Values.Count) : null);
			ListPool<WorldLogMessage> tInsertLogsList = ((_insert_logs.Count > 0) ? new ListPool<WorldLogMessage>(_insert_logs) : null);
			ListPool<(long MetaID, Type MetaType)> tDeleteCommandsList = ((_delete_commands.Count > 0) ? new ListPool<(long, Type)>(_delete_commands) : null);
			foreach (ListPool<HistoryTable> value in _insert_commands.Values)
			{
				if (value.Count == 0)
				{
					value.Dispose();
				}
				else
				{
					tCommandsList.Add(value);
				}
			}
			foreach (ListPool<BaseSystemData> value2 in _insert_metas.Values)
			{
				if (value2.Count == 0)
				{
					value2.Dispose();
				}
				else
				{
					tMetasList.Add(value2);
				}
			}
			clearCommands();
			((SQLiteConnection)tDBConn).RunInTransaction((Action)delegate
			{
				sendToDB((SQLiteConnection)(object)tDBConn, tMetasList, tCommandsList, tDeleteCommandsList, tInsertLogsList);
			});
		}
		finally
		{
			((IDisposable)(*(LockWrapper*)(&val))/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public static void executeCommandsAsync()
	{
		if (isLocked())
		{
			return;
		}
		if (_sql_timeout > 0f)
		{
			_sql_timeout -= Time.deltaTime;
			return;
		}
		_sql_timeout = 10f;
		if ((_thread != null && !_thread.IsCompleted) || !hasCommands())
		{
			return;
		}
		SQLiteAsyncConnection asyncConnection = DBManager.getAsyncConnection();
		if (!hasCommands())
		{
			return;
		}
		ListPool<ListPool<BaseSystemData>> tMetasList = ((_insert_metas.Values.Count > 0) ? new ListPool<ListPool<BaseSystemData>>(_insert_metas.Values.Count) : null);
		ListPool<ListPool<HistoryTable>> tCommandsList = ((_insert_commands.Values.Count > 0) ? new ListPool<ListPool<HistoryTable>>(_insert_commands.Values.Count) : null);
		ListPool<WorldLogMessage> tInsertLogsList = ((_insert_logs.Count > 0) ? new ListPool<WorldLogMessage>(_insert_logs) : null);
		ListPool<(long MetaID, Type MetaType)> tDeleteCommandsList = ((_delete_commands.Count > 0) ? new ListPool<(long, Type)>(_delete_commands) : null);
		foreach (ListPool<HistoryTable> value in _insert_commands.Values)
		{
			if (value.Count == 0)
			{
				value.Dispose();
			}
			else
			{
				tCommandsList.Add(value);
			}
		}
		foreach (ListPool<BaseSystemData> value2 in _insert_metas.Values)
		{
			if (value2.Count == 0)
			{
				value2.Dispose();
			}
			else
			{
				tMetasList.Add(value2);
			}
		}
		clearCommands();
		_thread = asyncConnection.RunInTransactionAsync((Action<SQLiteConnection>)delegate(SQLiteConnection pDBConn)
		{
			sendToDB(pDBConn, tMetasList, tCommandsList, tDeleteCommandsList, tInsertLogsList);
		});
	}

	private static void sendToDB(SQLiteConnection pDBConn, ListPool<ListPool<BaseSystemData>> tMetasList = null, ListPool<ListPool<HistoryTable>> tCommandsList = null, ListPool<(long MetaID, Type MetaType)> tDeleteCommandsList = null, ListPool<WorldLogMessage> tInsertLogsList = null)
	{
		if (tMetasList != null)
		{
			foreach (ref ListPool<BaseSystemData> tMetas in tMetasList)
			{
				ListPool<BaseSystemData> current = tMetas;
				try
				{
					pDBConn.InsertAll((IEnumerable)current, Orm.GetType((object)current[0]), false);
					current.Dispose();
				}
				catch (Exception ex)
				{
					Debug.LogError((object)ex);
				}
			}
			tMetasList.Clear();
			tMetasList.Dispose();
		}
		if (tCommandsList != null)
		{
			foreach (ref ListPool<HistoryTable> tCommands in tCommandsList)
			{
				ListPool<HistoryTable> current2 = tCommands;
				try
				{
					pDBConn.InsertAll((IEnumerable)current2, Orm.GetType((object)current2[0]), false);
					current2.Dispose();
				}
				catch (Exception ex2)
				{
					Debug.LogError((object)ex2);
				}
			}
			tCommandsList.Clear();
			tCommandsList.Dispose();
		}
		if (tDeleteCommandsList != null)
		{
			foreach (ref(long, Type) tDeleteCommands in tDeleteCommandsList)
			{
				var (num, objType) = tDeleteCommands;
				try
				{
					pDBConn.Delete("id", num, objType);
				}
				catch (Exception ex3)
				{
					Debug.LogError((object)ex3);
				}
			}
			tDeleteCommandsList.Clear();
			tDeleteCommandsList.Dispose();
		}
		if (tInsertLogsList != null && tInsertLogsList.Count > 0)
		{
			try
			{
				pDBConn.InsertAll((IEnumerable)tInsertLogsList, typeof(WorldLogMessage), false);
			}
			catch (Exception ex4)
			{
				Debug.LogError((object)ex4);
			}
			tInsertLogsList.Clear();
			TableMapping mapping = pDBConn.GetMapping<WorldLogMessage>((CreateFlags)0);
			try
			{
				pDBConn.Execute($"DELETE FROM {mapping.TableName} WHERE ROWID IN ( SELECT ROWID FROM {mapping.TableName} ORDER by timestamp DESC, ROWID DESC LIMIT {2000}, 1000 )", Array.Empty<object>());
			}
			catch (Exception ex5)
			{
				Debug.LogError((object)ex5);
			}
		}
		tInsertLogsList?.Dispose();
	}

	public static void quitting()
	{
		_sql_timeout = float.MaxValue;
		waitForAsync();
	}

	public static void waitForAsync()
	{
		if (_thread != null && !_thread.IsCompleted)
		{
			Debug.Log((object)"DBInserter thread is still running");
			_thread.WaitAndUnwrapException();
			Debug.Log((object)"DBInserter closed");
			_thread = null;
		}
	}
}
