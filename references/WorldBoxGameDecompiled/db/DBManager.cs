using System;
using System.IO;
using SQLite;
using UnityEngine;

namespace db;

public class DBManager : MonoBehaviour
{
	private static SQLiteAsyncConnection _dbconn;

	private static string _dbpath;

	private static void resetDataPath()
	{
		_dbpath = Application.persistentDataPath + "/stats.s3db";
	}

	public static bool loadDBFrom(string pPath)
	{
		try
		{
			closeDB();
			if (!File.Exists(pPath))
			{
				return false;
			}
			resetDataPath();
			if (File.Exists(_dbpath))
			{
				File.Delete(_dbpath);
			}
			File.Copy(pPath, _dbpath);
			openDB();
			return true;
		}
		catch (Exception ex)
		{
			Debug.Log((object)"[SQLITE] error loading db");
			Debug.LogError((object)ex);
			return false;
		}
	}

	public static void createDB()
	{
		try
		{
			closeDB();
			resetDataPath();
			if (File.Exists(_dbpath))
			{
				File.Delete(_dbpath);
			}
			Debug.Log((object)("[SQLITE] new db " + _dbpath));
			openDB();
		}
		catch (Exception ex)
		{
			Debug.Log((object)"[SQLITE] error creating db");
			Debug.Log((object)ex);
		}
	}

	public unsafe static void openDB()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		if (Config.disable_db || _dbconn != null)
		{
			return;
		}
		_dbconn = new SQLiteAsyncConnection(_dbpath, (SQLiteOpenFlags)32774, true);
		Debug.Log((object)("[SQLITE] opening db " + _dbconn.LibVersionNumber));
		_dbconn.Trace = false;
		SQLiteConnectionWithLock connection = _dbconn.GetConnection();
		LockWrapper val = connection.Lock();
		try
		{
			((SQLiteConnection)connection).ExecuteScalar<string>("PRAGMA temp_store=MEMORY;", Array.Empty<object>());
			((SQLiteConnection)connection).ExecuteScalar<string>("PRAGMA synchronous=OFF;", Array.Empty<object>());
			((SQLiteConnection)connection).ExecuteScalar<string>("PRAGMA cache_size=4000;", Array.Empty<object>());
			((SQLiteConnection)connection).ExecuteScalar<string>("PRAGMA journal_mode=MEMORY;", Array.Empty<object>());
		}
		finally
		{
			((IDisposable)(*(LockWrapper*)(&val))/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public static SQLiteAsyncConnection getAsyncConnection()
	{
		openDB();
		return _dbconn;
	}

	public static SQLiteConnectionWithLock getSyncConnection()
	{
		openDB();
		return _dbconn.GetConnection();
	}

	public static void clearAndClose()
	{
		DBInserter.waitForAsync();
		DBInserter.clearCommands();
		closeDB();
	}

	public static void closeDB()
	{
		if (!Config.disable_db && _dbconn != null)
		{
			Debug.Log((object)"[SQLITE] closing db");
			try
			{
				_dbconn.CloseAsync().WaitAndUnwrapException();
			}
			catch (Exception ex)
			{
				Debug.LogError((object)"[SQLITE] error closing db");
				Debug.LogError((object)ex);
			}
			_dbconn = null;
			Debug.Log((object)"[SQLITE] db closed");
		}
	}

	private unsafe static void vacuum()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		openDB();
		SQLiteConnectionWithLock connection = _dbconn.GetConnection();
		LockWrapper val = connection.Lock();
		try
		{
			((SQLiteConnection)connection).Execute("vacuum", Array.Empty<object>());
		}
		finally
		{
			((IDisposable)(*(LockWrapper*)(&val))/*cast due to constrained. prefix*/).Dispose();
		}
	}

	private unsafe static void backupTo(string pPath)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		openDB();
		SQLiteConnectionWithLock connection = _dbconn.GetConnection();
		LockWrapper val = connection.Lock();
		try
		{
			((SQLiteConnection)connection).Backup(pPath, "main");
		}
		finally
		{
			((IDisposable)(*(LockWrapper*)(&val))/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public static void saveToPath(string pPath)
	{
		if (File.Exists(pPath))
		{
			File.Delete(pPath);
		}
		if (Config.disable_db)
		{
			return;
		}
		string text = "Stats DB";
		string text2 = pPath + ".bak";
		bool flag = false;
		try
		{
			DBInserter.executeCommands();
			vacuum();
			backupTo(text2);
		}
		catch (IOException ex)
		{
			if (Toolbox.IsDiskFull(ex))
			{
				WorldTip.showNow("Error saving " + text + " : Disk full!", pTranslate: false, "top");
			}
			else
			{
				Debug.Log((object)("Could not save " + text + " due to hard drive / IO Error : "));
				Debug.Log((object)ex);
				WorldTip.showNow("Error saving " + text + " due to IOError! Check console for details", pTranslate: false, "top");
			}
			flag = true;
		}
		catch (Exception ex2)
		{
			Debug.Log((object)("Could not save " + text + " due to error : "));
			Debug.Log((object)ex2);
			WorldTip.showNow("Error saving " + text + "! Check console for errors", pTranslate: false, "top");
			flag = true;
		}
		if (flag)
		{
			if (File.Exists(text2))
			{
				File.Delete(text2);
			}
		}
		else
		{
			Toolbox.MoveSafely(text2, pPath);
		}
	}

	private void Awake()
	{
		ScrollWindow.addCallbackShowStarted(delegate
		{
			DBInserter.executeCommands();
		});
	}

	private void OnApplicationQuit()
	{
		DBInserter.quitting();
		closeDB();
	}
}
