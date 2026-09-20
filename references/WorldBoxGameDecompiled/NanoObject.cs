using System;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

public class NanoObject : IComparable<NanoObject>, IEquatable<NanoObject>, IDisposable
{
	protected bool _alive;

	public bool exists;

	protected int _hashcode;

	protected int stats_dirty_version;

	protected virtual MetaType meta_type
	{
		get
		{
			throw new NotImplementedException(GetType().Name);
		}
	}

	public double created_time_unscaled { get; set; }

	[JsonProperty(Order = -1)]
	public long id => getID();

	public virtual string name
	{
		get
		{
			throw new NotImplementedException(GetType().Name);
		}
		protected set
		{
			throw new NotImplementedException(GetType().Name);
		}
	}

	public NanoObject()
	{
		setDefaultValues();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void revive()
	{
		setDefaultValues();
	}

	protected virtual void setDefaultValues()
	{
		exists = true;
		_alive = true;
		stats_dirty_version = 0;
		created_time_unscaled = 0.0;
	}

	public virtual long getID()
	{
		throw new NotImplementedException(GetType().Name);
	}

	public virtual string getType()
	{
		return meta_type.AsString();
	}

	public virtual MetaType getMetaType()
	{
		return meta_type;
	}

	public MetaTypeAsset getMetaTypeAsset()
	{
		return meta_type.getAsset();
	}

	public virtual string getTypeID()
	{
		return getType() + "_" + getID();
	}

	public void setName(string pName, bool pTrack = true)
	{
		if (pTrack)
		{
			trackName();
		}
		name = pName;
		if (pTrack)
		{
			trackName(pPostChange: true);
		}
	}

	public virtual void trackName(bool pPostChange = false)
	{
		throw new NotImplementedException(GetType().Name);
	}

	public virtual ColorAsset getColor()
	{
		return null;
	}

	public virtual double getFoundedTimestamp()
	{
		return 0.0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool isAlive()
	{
		return _alive;
	}

	public virtual bool hasDied()
	{
		throw new NotImplementedException(GetType().Name);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public virtual void setAlive(bool pValue)
	{
		_alive = pValue;
	}

	public int getStatsDirtyVersion()
	{
		return stats_dirty_version;
	}

	public void setHash(int pHash)
	{
		_hashcode = pHash;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(NanoObject pObject)
	{
		return _hashcode == pObject.GetHashCode();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int CompareTo(NanoObject pTarget)
	{
		return GetHashCode().CompareTo(pTarget.GetHashCode());
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override int GetHashCode()
	{
		return _hashcode;
	}

	public virtual void Dispose()
	{
	}
}
