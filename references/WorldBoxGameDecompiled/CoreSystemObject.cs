using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public abstract class CoreSystemObject<TData> : NanoObject, ICoreObject, ILoadable<TData>, IFavoriteable where TData : BaseSystemData
{
	public TData data;

	public virtual BaseSystemManager manager
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override string name
	{
		get
		{
			return data.name;
		}
		protected set
		{
			data.name = value;
		}
	}

	public string obsidian_name_id => data.obsidian_name_id;

	public bool isFavorite()
	{
		return data.favorite;
	}

	public void switchFavorite()
	{
		data.favorite = !data.favorite;
	}

	public virtual void setFavorite(bool pState)
	{
		data.favorite = pState;
	}

	public virtual bool updateColor(ColorAsset pColor)
	{
		return false;
	}

	public virtual void save()
	{
		data.save();
	}

	public virtual void loadData(TData pData)
	{
		setData(pData);
		data.load();
	}

	public virtual void setData(TData pData)
	{
		data = pData;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override void setAlive(bool pValue)
	{
		_alive = pValue;
		if (!pValue && data.died_time == 0.0)
		{
			data.died_time = World.world.getCurWorldTime();
		}
	}

	public override bool hasDied()
	{
		return data.died_time > 0.0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public virtual int getAge()
	{
		return Date.getYearsSince(data.created_time);
	}

	public override double getFoundedTimestamp()
	{
		return data.created_time;
	}

	public bool isJustCreated()
	{
		return Math.Abs(data.created_time - World.world.getCurWorldTime()) <= 0.05000000074505806;
	}

	public string getFoundedDate()
	{
		return Date.getDate(data.created_time);
	}

	public string getDiedDate()
	{
		return Date.getDate(data.died_time);
	}

	public string getFoundedYear()
	{
		return Date.getYearDate(data.created_time);
	}

	public string getDiedYear()
	{
		return Date.getYearDate(data.died_time);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int getAgeMonths()
	{
		return Date.getMonthsSince(data.created_time);
	}

	public override void trackName(bool pPostChange = false)
	{
		if (!string.IsNullOrEmpty(data.name) && (!pPostChange || (data.past_names != null && data.past_names.Count != 0)))
		{
			BaseSystemData baseSystemData = data;
			if (baseSystemData.past_names == null)
			{
				baseSystemData.past_names = new List<NameEntry>();
			}
			if (data.past_names.Count == 0)
			{
				NameEntry item = new NameEntry(data.name, pCustom: false, -1, data.created_time);
				data.past_names.Add(item);
			}
			else if (!(data.past_names.Last().name == data.name))
			{
				NameEntry item2 = new NameEntry(data.name, data.custom_name);
				data.past_names.Add(item2);
			}
		}
	}

	public override void Dispose()
	{
		data?.Dispose();
		data = null;
		base.Dispose();
	}

	public sealed override long getID()
	{
		return data.id;
	}

	public string getBirthday()
	{
		return Date.getDate(data.created_time);
	}
}
