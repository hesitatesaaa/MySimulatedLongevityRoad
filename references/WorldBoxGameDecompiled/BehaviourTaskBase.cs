using System;
using System.Collections.Generic;
using System.ComponentModel;

[Serializable]
public class BehaviourTaskBase<T> : BehaviourElementAI, ILocalizedAsset where T : BehaviourElementAI
{
	[DefaultValue(1f)]
	public float single_interval = 1f;

	public float single_interval_random;

	public List<T> list = new List<T>();

	public T task_verifier;

	public bool has_verifier;

	[DefaultValue("")]
	public string locale_key = string.Empty;

	public bool debug_flag;

	[DefaultValue(true)]
	protected virtual bool has_locales => true;

	protected virtual string locale_key_prefix
	{
		get
		{
			throw new NotImplementedException(GetType().Name);
		}
	}

	public BehaviourTaskBase()
	{
		create();
	}

	public T get(int pIndex)
	{
		return list[pIndex];
	}

	public string getLocaleID()
	{
		if (!has_locales)
		{
			return null;
		}
		if (string.IsNullOrEmpty(locale_key))
		{
			return locale_key_prefix + "_" + id;
		}
		return locale_key;
	}

	public string getLocalizedText()
	{
		if (!has_locales)
		{
			return "???";
		}
		return getLocaleID().Localize();
	}

	public void addRepeatActions(int pIndexAmount, int pHowManyTimes)
	{
		List<T> list = new List<T>();
		int count = this.list.Count;
		for (int i = 0; i < pHowManyTimes; i++)
		{
			for (int j = 0; j < pIndexAmount; j++)
			{
				int index = count - (pIndexAmount - j);
				T item = this.list[index];
				list.Add(item);
			}
		}
		this.list.AddRange(list);
	}

	public void addBeh(T pAction)
	{
		pAction.id = pAction.GetType().ToString();
		pAction.id = pAction.id.Replace("ai.behaviours.", "");
		list.Add(pAction);
		pAction.create();
	}

	public void addTaskVerifier(T pAction)
	{
		task_verifier = pAction;
		has_verifier = true;
	}
}
