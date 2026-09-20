using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComponentListSapient<TListElement, TMetaObject, TData, TComponent> : ComponentListBase<TListElement, TMetaObject, TData, TComponent>, ISapientListComponent where TListElement : WindowListElementBase<TMetaObject, TData> where TMetaObject : CoreSystemObject<TData> where TData : BaseSystemData where TComponent : ComponentListBase<TListElement, TMetaObject, TData, TComponent>
{
	[SerializeField]
	private Text _sapient_counter;

	[SerializeField]
	private Text _non_sapient_counter;

	private SapientListFilter _filter;

	protected override void show()
	{
		if (Config.game_loaded)
		{
			base.show();
			if ((Object)(object)_sapient_counter != (Object)null)
			{
				_sapient_counter.text = latest_counted.ToString();
			}
			if ((Object)(object)_non_sapient_counter != (Object)null)
			{
				_non_sapient_counter.text = latest_counted.ToString();
			}
		}
	}

	protected override IEnumerable<TMetaObject> getFiltered(IEnumerable<TMetaObject> pList)
	{
		switch (_filter)
		{
		case SapientListFilter.Default:
			foreach (TMetaObject item in base.getFiltered(pList))
			{
				yield return item;
			}
			break;
		case SapientListFilter.Sapient:
			foreach (ISapient p in pList)
			{
				if (p.isSapient())
				{
					yield return (TMetaObject)p;
				}
			}
			break;
		case SapientListFilter.NonSapient:
			foreach (ISapient p2 in pList)
			{
				if (!p2.isSapient())
				{
					yield return (TMetaObject)p2;
				}
			}
			break;
		}
	}

	public void setShowSapientOnly()
	{
		_filter = SapientListFilter.Sapient;
	}

	public void setShowNonSapientOnly()
	{
		_filter = SapientListFilter.NonSapient;
	}

	public override void setDefault()
	{
		_filter = SapientListFilter.Default;
	}

	public void setSapientCounter(Text pCounter)
	{
		_sapient_counter = pCounter;
	}

	public void setNonSapientCounter(Text pCounter)
	{
		_non_sapient_counter = pCounter;
	}
}
