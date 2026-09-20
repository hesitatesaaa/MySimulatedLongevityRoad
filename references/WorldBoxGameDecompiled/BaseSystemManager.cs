using System;

public abstract class BaseSystemManager
{
	protected static int _latest_hash = 1;

	internal static bool anything_changed = false;

	public virtual int Count
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public virtual void ClearAllDisposed()
	{
	}

	public virtual void parallelDirtyUnitsCheck()
	{
	}

	public virtual void checkLists()
	{
	}

	public virtual void clear()
	{
		ClearAllDisposed();
		anything_changed = false;
	}

	public virtual void checkDeadObjects()
	{
	}

	public virtual bool isUnitsDirty()
	{
		return false;
	}

	public virtual bool isLocked()
	{
		return isUnitsDirty();
	}

	public virtual void startCollectHistoryData()
	{
	}

	public virtual void clearLastYearStats()
	{
	}

	public virtual void showDebugTool(DebugTool pTool)
	{
	}

	public virtual bool hasAny()
	{
		return Count > 0;
	}

	public virtual string debugShort()
	{
		return $"[c:{Count}]";
	}
}
