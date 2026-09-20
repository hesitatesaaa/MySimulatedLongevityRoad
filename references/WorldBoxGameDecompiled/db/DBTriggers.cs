using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;

namespace db;

public static class DBTriggers
{
	public static void createTrigger(SQLiteConnectionWithLock pDBConn, HistoryMetaDataAsset tHistoryAsset)
	{
		List<string> list = new List<string>();
		List<HistoryDataAsset> list2 = new List<HistoryDataAsset>();
		foreach (HistoryDataAsset category in tHistoryAsset.categories)
		{
			list2.Add(category);
			list.Add(category.id);
		}
		foreach (HistoryInterval key in tHistoryAsset.table_types.Keys)
		{
			Type type = tHistoryAsset.table_types[key];
			var (pYearDiviver, historyInterval) = key.fillFrom();
			if (historyInterval != HistoryInterval.None)
			{
				Type type2 = tHistoryAsset.table_types[historyInterval];
				createInsertionTrigger(pDBConn, type.Name, list2, type2.Name, pYearDiviver);
			}
			createNullDuplicateValuesTrigger(pDBConn, type.Name, list);
			int maxTimeFrame = key.getMaxTimeFrame();
			createTrimTableTrigger(pDBConn, type.Name, list, maxTimeFrame);
		}
	}

	public static void dropTrigger(SQLiteConnectionWithLock pDBConn, HistoryMetaDataAsset tHistoryAsset)
	{
		foreach (HistoryInterval key in tHistoryAsset.table_types.Keys)
		{
			Type type = tHistoryAsset.table_types[key];
			(int tEveryYears, HistoryInterval tFromInterval) tuple = key.fillFrom();
			var (pYearDiviver, _) = tuple;
			if (tuple.tFromInterval != HistoryInterval.None)
			{
				dropInsertionTrigger(pDBConn, type.Name, pYearDiviver);
			}
			dropNullDuplicateValuesTrigger(pDBConn, type.Name);
			dropTrimTableTrigger(pDBConn, type.Name);
		}
	}

	public static void dropTrimTableTrigger(SQLiteConnectionWithLock pDBConn, string pTableName)
	{
		((SQLiteConnection)pDBConn).ExecuteScalar<string>("DROP TRIGGER IF EXISTS DELETE_OLD_" + pTableName, Array.Empty<object>());
	}

	public static void createTrimTableTrigger(SQLiteConnectionWithLock pDBConn, string pTableName, List<string> pColumns, int pMaxYears)
	{
		string text = string.Format("CREATE TRIGGER IF NOT EXISTS DELETE_OLD_{0}\n\t\t\tAFTER INSERT ON {1}\n\t\t\tWHEN\n\t\t\t\tNEW.timestamp % {2} = 0\n\t\t\tAND\n\t\t\t\tEXISTS (\n    \t\t\t\tSELECT 1 FROM {3}\n    \t\t\t\tWHERE\n\t\t\t\t\t\tid = NEW.id AND\n\t\t\t\t\t\ttimestamp < (NEW.timestamp - {4})\n\t\t\t\t\tLIMIT 1\n\t\t\t\t)\n\t\t\tBEGIN\n\t\t\t\tINSERT OR REPLACE INTO {5}\n\t\t\t\t(\n\t\t\t\t\tid,\n\t\t\t\t\ttimestamp,\n\t\t\t\t\t{6},\n\t\t\t\t\tauto\n\t\t\t\t) VALUES (\n\t\t\t\t\tNEW.id,\n\t\t\t\t\tNEW.timestamp - {7},\n\t\t\t\t\t{8},\n\t\t\t\t\t1\n\t\t\t\t);\n\n-- \t\t\t\tUPDATE {9}\n-- \t\t\t\tSET\n-- \t\t\t\t\t{10}\n-- \t\t\t\tWHERE\n-- \t\t\t\t\tid = NEW.id AND\n-- \t\t\t\t\ttimestamp = (NEW.timestamp - {11})\n-- \t\t\t\t;\n\n\t\t\t\tDELETE FROM {12}\n\t\t\t\tWHERE\n\t\t\t\t\tid = NEW.id AND\n\t\t\t\t\ttimestamp < (NEW.timestamp - {13})\n\t\t\t\t;\n\t\t\tEND;", pTableName, pTableName, pMaxYears, pTableName, pMaxYears, pTableName, string.Join(", ", pColumns), pMaxYears, string.Join(", ", pColumns.Select((string x) => $"(SELECT {x} FROM {pTableName} WHERE id = NEW.id AND timestamp <= (NEW.timestamp - {pMaxYears}) AND {x} IS NOT NULL ORDER BY timestamp DESC LIMIT 1)")), pTableName, string.Join(", ", pColumns.Select((string x) => $"{x} = CASE WHEN {x} IS NULL THEN (SELECT {x} FROM {pTableName} WHERE id = NEW.id AND timestamp <= (NEW.timestamp - {pMaxYears}) AND {x} IS NOT NULL ORDER BY timestamp DESC LIMIT 1) ELSE {x} END")), pMaxYears, pTableName, pMaxYears);
		((SQLiteConnection)pDBConn).ExecuteScalar<string>(text, Array.Empty<object>());
	}

	public static void dropNullDuplicateValuesTrigger(SQLiteConnectionWithLock pDBConn, string pTableName)
	{
		((SQLiteConnection)pDBConn).ExecuteScalar<string>("DROP TRIGGER IF EXISTS NULL_DUPLICATES_" + pTableName, Array.Empty<object>());
	}

	public static void createNullDuplicateValuesTrigger(SQLiteConnectionWithLock pDBConn, string pTableName, List<string> pColumns)
	{
		string text = "CREATE TRIGGER IF NOT EXISTS NULL_DUPLICATES_" + pTableName + "\n\t\t\tAFTER INSERT ON " + pTableName + "\n\t\t\t\tWHEN NOT EXISTS (\n\t\t\t\t\tSELECT 1 FROM " + pTableName + " WHERE id = NEW.id AND timestamp > NEW.timestamp LIMIT 1\n\t\t\t\t)\n\t\t\tBEGIN\n\t\t\t\tUPDATE " + pTableName + "\n\t\t\t\tSET " + string.Join(", ", pColumns.Select((string x) => x + " = CASE WHEN (SELECT " + x + " FROM " + pTableName + " WHERE id = NEW.id AND " + x + " IS NOT NULL ORDER BY timestamp DESC LIMIT 1,1) = NEW." + x + " THEN NULL ELSE NEW." + x + " END")) + "\n\t\t\t\tWHERE rowid = NEW.rowid;\n\n\t\t\t\tDELETE FROM " + pTableName + " WHERE rowid = NEW.rowid AND " + string.Join(" AND ", pColumns.Select((string x) => x + " IS NULL")) + ";\n\t\t\tEND;";
		((SQLiteConnection)pDBConn).ExecuteScalar<string>(text, Array.Empty<object>());
	}

	public static void dropInsertionTrigger(SQLiteConnectionWithLock pDBConn, string pTargetTable, int pYearDiviver)
	{
		((SQLiteConnection)pDBConn).ExecuteScalar<string>($"DROP TRIGGER IF EXISTS FILL_{pTargetTable}_{pYearDiviver}", Array.Empty<object>());
	}

	public static void createInsertionTrigger(SQLiteConnectionWithLock pDBConn, string pTargetTable, List<HistoryDataAsset> pColumns, string pSourceTable, int pYearDiviver)
	{
		string text = string.Format("CREATE TRIGGER IF NOT EXISTS FILL_{0}_{1}\n\t\t\tAFTER INSERT ON {2}\n\t\t\t\tWHEN NEW.timestamp % {3} = 0 AND NEW.auto IS NOT 1\n\t\t\tBEGIN\n\t\t\tINSERT INTO\n\t\t\t\t{4}(\n\t\t\t\t\t{5},\n\t\t\t\t\tid,\n\t\t\t\t\ttimestamp\n\t\t\t\t)\n\t\t\tSELECT\n\t\t\t\t{6},\n\t\t\t\tNEW.id,\n\t\t\t\tNEW.timestamp\n\t\t\tFROM\n\t\t\t\t(\n\t\t\t\t\tSELECT\n\t\t\t\t\t\t*\n\t\t\t\t\tFROM\n\t\t\t\t\t\t{7}\n\t\t\t\t\tWHERE\n\t\t\t\t\t\tid = NEW.id\n\t\t\t\t\tAND\n\t\t\t\t\t\ttimestamp >= NEW.timestamp - {8}\n\t\t\t\t\tAND\n\t\t\t\t\t\ttimestamp < NEW.timestamp\n\t\t\t\t\tORDER BY\n\t\t\t\t\t\ttimestamp DESC\n\t\t\t\t);\n\n\t\t\tEND;", pTargetTable, pYearDiviver, pSourceTable, pYearDiviver, pTargetTable, string.Join(", ", pColumns.Select((HistoryDataAsset x) => x.id)), string.Join(", ", pColumns.Select(delegate(HistoryDataAsset x)
		{
			if (x.max)
			{
				return "MAX(" + x.id + ")";
			}
			if (x.sum)
			{
				return "SUM(" + x.id + ")";
			}
			return x.average ? ("CAST(AVG(" + x.id + ")+1-1e-1 AS INT)") : ("ROUND(AVG(" + x.id + "))");
		})), pSourceTable, pYearDiviver);
		((SQLiteConnection)pDBConn).ExecuteScalar<string>(text, Array.Empty<object>());
	}
}
