using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Beebyte.Obfuscator;
using UnityEngine;

[Serializable]
[ObfuscateLiterals]
public abstract class AssetLibrary<T> : BaseAssetLibrary where T : Asset
{
	public List<T> list = new List<T>();

	[NonSerialized]
	public Dictionary<string, T> dict = new Dictionary<string, T>();

	protected T t;

	private T[] _array;

	public string file_path;

	private HashSet<string> _not_found = new HashSet<string>();

	public override int total_items => list.Count;

	public virtual T get(string pID)
	{
		if (dict.TryGetValue(pID, out var value))
		{
			return value;
		}
		_not_found.Add(pID);
		return null;
	}

	public T getSimple(string pID)
	{
		if (!has(pID))
		{
			return null;
		}
		if (dict.TryGetValue(pID, out var value))
		{
			return value;
		}
		return null;
	}

	public virtual bool has(string pID)
	{
		return dict.ContainsKey(pID);
	}

	public virtual T add(T pAsset)
	{
		string text = pAsset.id;
		if (dict.ContainsKey(text))
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (!(list[i].id != text))
				{
					list.RemoveAt(i);
					break;
				}
			}
			dict.Remove(text);
			BaseAssetLibrary.logAssetError("<e>AssetLibrary<" + typeof(T).Name + "></e>: duplicate asset - overwriting...", text);
		}
		t = pAsset;
		t.create();
		t.setHash(BaseAssetLibrary._latest_hash++);
		if (!pAsset.isTemplateAsset())
		{
			list.Add(pAsset);
		}
		t.setIndexID(list.Count);
		dict.Add(text, pAsset);
		return pAsset;
	}

	public virtual T clone(string pNew, string pFrom)
	{
		clone(out var pNew2, dict[pFrom]);
		t = pNew2;
		t.id = pNew;
		add(t);
		return t;
	}

	public virtual void clone(out T pNew, T pFrom)
	{
		pNew = Activator.CreateInstance<T>();
		FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public);
		foreach (FieldInfo fieldInfo in fields)
		{
			if (!fieldInfo.IsNotSerialized)
			{
				object value = fieldInfo.GetValue(pFrom);
				if (value == null || fieldInfo.isString())
				{
					fieldInfo.SetValue(pNew, value);
				}
				else if (fieldInfo.isCloneable())
				{
					ICloneable cloneable = value as ICloneable;
					fieldInfo.SetValue(pNew, cloneable.Clone());
				}
				else if (fieldInfo.isCollection())
				{
					ICollection collection = value as ICollection;
					fieldInfo.SetValue(pNew, Activator.CreateInstance(fieldInfo.FieldType, collection));
				}
				else if (fieldInfo.isEnumerable())
				{
					IEnumerable enumerable = value as IEnumerable;
					fieldInfo.SetValue(pNew, Activator.CreateInstance(fieldInfo.FieldType, enumerable));
				}
				else
				{
					fieldInfo.SetValue(pNew, value);
				}
			}
		}
	}

	internal void loadFromFile<TAssetLib>() where TAssetLib : AssetLibrary<T>
	{
		foreach (T item in JsonUtility.FromJson<TAssetLib>(Resources.Load<TextAsset>(file_path).text).list)
		{
			add(item);
		}
	}

	public T[] getArray()
	{
		if (_array == null)
		{
			_array = list.ToArray();
		}
		return _array;
	}

	public override void editorDiagnostic()
	{
		Type type = typeof(T);
		while (type != null)
		{
			if (!type.IsSerializable)
			{
				BaseAssetLibrary.logAssetError("<e>AssetLibrary<" + typeof(T).Name + "></e>: Asset not marked serializable", type.Name);
			}
			type = type.BaseType;
		}
		FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo.IsAssembly && !fieldInfo.IsNotSerialized)
			{
				BaseAssetLibrary.logAssetError("<e>AssetLibrary<" + typeof(T).Name + "></e>: Asset field is marked <e>internal</e> - set it to <e>public</e> and/or <e>[NonSerialized]</e> instead. Currently it's not being cloned.", fieldInfo.Name);
			}
			if (fieldInfo.IsFamily && !fieldInfo.IsNotSerialized)
			{
				BaseAssetLibrary.logAssetError("<e>AssetLibrary<" + typeof(T).Name + "></e>: Asset field is marked <e>protected</e> - set it to <e>public</e> and/or <e>[NonSerialized]</e> instead. Currently it's not being cloned.", fieldInfo.Name);
			}
			if (fieldInfo.IsPrivate && ((MemberInfo)fieldInfo).GetCustomAttribute<SerializeField>() != null)
			{
				BaseAssetLibrary.logAssetError("<e>AssetLibrary<" + typeof(T).Name + "></e>: Asset field is marked <e>private</e> and has <e>[SerializeField]</e> attribute - it won't be cloned. Set it to <e>public</e> instead", fieldInfo.Name);
			}
		}
		base.editorDiagnostic();
	}

	public override void checkLocale(Asset pAsset, string pLocaleID)
	{
		string text = pLocaleID?.Underscore();
		if (text != pLocaleID)
		{
			BaseAssetLibrary.logAssetError("<e>AssetLibrary<" + typeof(T).Name + "></e>: Translation key is not in lowercase - <e>" + pLocaleID + "</e> should be <e>" + text + "</e>", pAsset.id);
		}
		if (!(pAsset is ILocalizedAsset) && !(pAsset is IMultiLocalesAsset))
		{
			BaseAssetLibrary.logAssetError("<e>AssetLibrary<" + typeof(T).Name + "></e>: Interface missing for <e>" + text + "</e>", pAsset.id);
		}
		if (!string.IsNullOrEmpty(text) && !LocalizedTextManager.stringExists(text))
		{
			BaseAssetLibrary.logAssetError("<e>AssetLibrary<" + typeof(T).Name + "></e>: Missing translation key <e>" + text + "</e>", pAsset.id);
			AssetManager.missing_locale_keys.Add(text);
		}
	}

	public string getEditorPathForSave()
	{
		return Application.dataPath + "/Resources/" + file_path + ".json";
	}

	public void saveToFile(string pPath = "units.json")
	{
		_ = Application.streamingAssetsPath + "/modules/core/" + pPath;
	}

	protected bool checkSpriteExists(string pVariableID, string pPath, Asset pAsset)
	{
		if (!string.IsNullOrEmpty(pPath) && !hasSpriteInResourcesDebug(pPath))
		{
			BaseAssetLibrary.logAssetError(id + ": <e>" + pVariableID + "</e> doesn't exist for <e>" + pAsset.id + "</e> at ", pPath);
			return false;
		}
		return true;
	}

	protected static TA[] a<TA>(params TA[] pArgs)
	{
		return Toolbox.a(pArgs);
	}

	protected static List<TL> l<TL>(params TL[] pArgs)
	{
		return Toolbox.l(pArgs);
	}

	protected static HashSet<TH> h<TH>(params TH[] pArgs)
	{
		return Toolbox.h(pArgs);
	}

	public override IEnumerable<Asset> getList()
	{
		return list;
	}
}
