using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FieldInfoList : MonoBehaviour
{
	public static string color_null;

	public static string color_white;

	public static string color_string;

	public static string color_enum;

	public static string color_type;

	public static string color_collection;

	public static Dictionary<string, string> selected_field_data;

	public KeyValueField field_prefab;

	public InputField search_input_field;

	public Transform fields_transform;

	private ObjectPoolGenericMono<KeyValueField> _pool_fields;

	internal List<FieldInfo> field_infos = new List<FieldInfo>();

	internal Dictionary<string, FieldInfoListItem> fields_collection_data = new Dictionary<string, FieldInfoListItem>();

	public void init<T>() where T : class
	{
		init<T>(null);
	}

	public void init<T>(ListPool<string> pFieldsToLoad) where T : class
	{
		checkInitPool();
		field_infos.Clear();
		fields_collection_data.Clear();
		FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		Array.Reverse(fields);
		bool flag = pFieldsToLoad != null && pFieldsToLoad.Count > 0;
		int num = 0;
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			if (!flag || pFieldsToLoad.Contains(fieldInfo.Name))
			{
				field_infos.Add(fieldInfo);
				num++;
			}
		}
		if ((Object)(object)search_input_field != (Object)null)
		{
			((UnityEvent<string>)(object)search_input_field.onValueChanged).AddListener((UnityAction<string>)setDataSearched);
		}
	}

	public void checkInitPool()
	{
		if (_pool_fields == null)
		{
			_pool_fields = new ObjectPoolGenericMono<KeyValueField>(field_prefab, fields_transform);
		}
		else
		{
			clear();
		}
	}

	public void setData(object pReference)
	{
		foreach (FieldInfo field_info in field_infos)
		{
			FieldInfoListItem fieldData = getFieldData(field_info, pReference);
			fields_collection_data.Add(fieldData.field_name, fieldData);
			addRow(fieldData.field_name, fieldData.field_value);
		}
	}

	public FieldInfoListItem getFieldData(FieldInfo pField, object pReference)
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		string text = "";
		object value = pField.GetValue(pReference);
		Type fieldType = pField.FieldType;
		Dictionary<string, string> pCollectionData = null;
		if (value != null)
		{
			if (!(value is bool flag))
			{
				if (!(value is string text2))
				{
					if (!(value is int num))
					{
						if (!(value is float pFloat))
						{
							if (!(value is Vector2 val))
							{
								if (!(value is Vector2Int val2))
								{
									if (!(value is Enum arg))
									{
										if (!(value is Array array))
										{
											if (!(value is IList list))
											{
												if (value is IDictionary dictionary)
												{
													pCollectionData = dictionaryToRows(dictionary);
													Type[] genericArguments = fieldType.GetGenericArguments();
													string text3 = Toolbox.coloredText(genericArguments[0].Name, color_type);
													string text4 = Toolbox.coloredText(genericArguments[1].Name, color_type);
													string text5 = Toolbox.coloredText(dictionary.Count.ToString(), color_white);
													text = Toolbox.coloredText("Dictionary<" + text3 + ", " + text4 + ">[" + text5 + "]", color_collection);
												}
												else if (fieldType.IsGenericType && typeof(HashSet<>) == fieldType.GetGenericTypeDefinition())
												{
													pCollectionData = enumerableToRows(value as IEnumerable);
													string text6 = Toolbox.coloredText(fieldType.GetGenericArguments()[0].Name, color_type);
													string text7 = Toolbox.coloredText(fieldType.GetProperty("Count").GetValue(value).ToString(), color_white);
													text = Toolbox.coloredText("HashSet<" + text6 + ">[" + text7 + "]", color_collection);
												}
												else
												{
													text = Toolbox.coloredText(value.GetType().Name, color_type);
												}
											}
											else
											{
												pCollectionData = enumerableToRowsCompacted(list);
												string text8 = Toolbox.coloredText(fieldType.GetGenericArguments()[0].Name, color_type);
												string text9 = Toolbox.coloredText(list.Count.ToString(), color_white);
												text = Toolbox.coloredText("List<" + text8 + ">[" + text9 + "]", color_collection);
											}
										}
										else
										{
											pCollectionData = enumerableToRowsCompacted(array);
											string text10 = Toolbox.coloredText(fieldType.GetElementType().Name, color_type);
											string text11 = Toolbox.coloredText(array.Length.ToString(), color_white);
											text = Toolbox.coloredText("Array<" + text10 + ">[" + text11 + "]", color_collection);
										}
									}
									else
									{
										text = Toolbox.coloredText($"{fieldType.Name}.{arg}", color_enum);
									}
								}
								else
								{
									string text12 = Toolbox.coloredText(((Vector2Int)(ref val2)).x.ToText(), color_white);
									string text13 = Toolbox.coloredText(((Vector2Int)(ref val2)).y.ToText(), color_white);
									text = Toolbox.coloredText("Vector2Int(" + text12 + ", " + text13 + ")", color_collection);
								}
							}
							else
							{
								string text14 = Toolbox.coloredText(val.x.ToText() + "f", color_white);
								string text15 = Toolbox.coloredText(val.y.ToText() + "f", color_white);
								text = Toolbox.coloredText("Vector2(" + text14 + ", " + text15 + ")", color_collection);
							}
						}
						else
						{
							text = Toolbox.coloredText(pFloat.ToText() + "f", color_white);
						}
					}
					else
					{
						text = Toolbox.coloredText($"{num}", color_white);
					}
				}
				else
				{
					string text16 = Toolbox.coloredText("\"", color_null);
					text = Toolbox.coloredText(text16 + text2 + text16, color_string);
				}
			}
			else
			{
				text = Toolbox.coloredText($"{flag}", flag ? "#43FF43" : "#FB2C21");
			}
		}
		else
		{
			text = Toolbox.coloredText("—", color_null);
		}
		return new FieldInfoListItem(pField.Name, text, pCollectionData);
	}

	public KeyValueField addRow(string pName, string pValue)
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Expected O, but got Unknown
		KeyValueField next = _pool_fields.getNext();
		next.name_text.text = pName;
		next.value.text = pValue;
		if (fields_collection_data.TryGetValue(pName, out var value))
		{
			Dictionary<string, string> tCollectionContent = value.collection_data;
			if (tCollectionContent == null || tCollectionContent.Count == 0)
			{
				((Behaviour)((Component)next.value).GetComponent<TipButton>()).enabled = false;
			}
			else
			{
				((Behaviour)((Component)next.value).GetComponent<TipButton>()).enabled = true;
				next.on_hover_value = (UnityAction)delegate
				{
					selected_field_data = tCollectionContent;
				};
				next.on_hover_value_out = new UnityAction(Tooltip.hideTooltip);
			}
		}
		return next;
	}

	internal void setDataSearched(string pValue)
	{
		clear();
		pValue = pValue.ToLower();
		if (string.IsNullOrEmpty(pValue))
		{
			int num = 0;
			{
				foreach (FieldInfoListItem value in fields_collection_data.Values)
				{
					KeyValueField pComponent = addRow(value.field_name, value.field_value);
					setOddEvenColor(pComponent, num);
					num++;
				}
				return;
			}
		}
		int num2 = 0;
		foreach (FieldInfoListItem value2 in fields_collection_data.Values)
		{
			if (value2.field_name.ToLower().Contains(pValue))
			{
				KeyValueField pComponent2 = addRow(value2.field_name, value2.field_value);
				setOddEvenColor(pComponent2, num2);
				num2++;
			}
		}
	}

	private void setOddEvenColor(KeyValueField pComponent, int pIndex)
	{
		if (pIndex % 2 == 0)
		{
			pComponent.setEvenColor();
		}
		else
		{
			pComponent.setOddColor();
		}
	}

	private Dictionary<string, string> enumerableToRowsCompacted(IEnumerable pEnumerable)
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		int num = 0;
		foreach (object item in pEnumerable)
		{
			string key = item.ToString();
			if (dictionary.ContainsKey(key))
			{
				dictionary[key]++;
				continue;
			}
			dictionary.Add(key, 1);
			num++;
		}
		string pColor = Toolbox.colorToHex(Color32.op_Implicit(Toolbox.color_yellow));
		Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
		int num2 = 0;
		foreach (KeyValuePair<string, int> item2 in dictionary)
		{
			string text = item2.Value.ToString();
			dictionary2.Add(item2.Key + "    ", Toolbox.coloredText("x      " + text, pColor));
			num2++;
		}
		return dictionary2;
	}

	private Dictionary<string, string> enumerableToRows(IEnumerable pEnumerable)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		int num = 0;
		foreach (object item in pEnumerable)
		{
			dictionary.Add($"[{num}]     ", item.ToString());
			num++;
		}
		return dictionary;
	}

	private Dictionary<string, string> dictionaryToRows(IDictionary pDictionary)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (object key in pDictionary.Keys)
		{
			dictionary.Add($"[\"{key}\"]", pDictionary[key].ToString());
		}
		return dictionary;
	}

	public void clear()
	{
		_pool_fields.clear();
	}

	static FieldInfoList()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		color_null = "#9F9F9F";
		color_white = Toolbox.colorToHex(Color32.op_Implicit(Toolbox.color_white));
		color_string = "#F3961F";
		color_enum = Toolbox.colorToHex(Color32.op_Implicit(Toolbox.color_plague));
		color_type = Toolbox.colorToHex(Color32.op_Implicit(Toolbox.color_yellow));
		color_collection = color_null;
	}
}
