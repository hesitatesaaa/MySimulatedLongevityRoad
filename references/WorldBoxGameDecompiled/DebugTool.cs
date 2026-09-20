using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DebugTool : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static UnityAction _003C_003E9__30_24;

		internal void _003CinitButtons_003Eb__30_24()
		{
			Bench.bench_enabled = !Bench.bench_enabled;
		}
	}

	public const int DT_WIDTH = 126;

	public const int DT_HEIGHT = 60;

	protected ObjectPoolGenericMono<DebugToolTextElement> pool_texts;

	public DebugToolTextElement element_prefab;

	internal int textCount;

	public Dropdown dropdown;

	internal bool sort_order_reversed;

	internal bool sort_by_names;

	internal bool sort_by_values = true;

	internal bool show_averages = true;

	internal bool percentage_slowest;

	internal bool hide_zeroes = true;

	internal bool show_counter = true;

	internal bool show_max = true;

	internal DebugToolState state = DebugToolState.FrameBudget;

	public DebugToolType type;

	internal bool paused;

	internal DebugToolAsset asset;

	[HideInInspector]
	public DebugDropdown active_dropdown;

	private double last_update_timestamp;

	private List<DebugIconOptionAction> list_actions = new List<DebugIconOptionAction>();

	private List<Image> list_icons = new List<Image>();

	private Transform transform_texts;

	private Transform benchmark_icons;

	private string _latest_text;

	private void Awake()
	{
		populateOptions();
		benchmark_icons = ((Component)this).transform.FindRecursive("Benchmark Icons");
		initButtons();
		initElements();
	}

	private void initElements()
	{
		transform_texts = ((Component)this).transform.FindRecursive("Texts");
		pool_texts = new ObjectPoolGenericMono<DebugToolTextElement>(element_prefab, transform_texts);
		((Component)element_prefab).gameObject.SetActive(false);
	}

	private float calculateLineHeight(Text pText)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		Rect rectExtents = pText.cachedTextGenerator.rectExtents;
		Vector2 val = ((Rect)(ref rectExtents)).size * 0.5f;
		return pText.cachedTextGeneratorForLayout.GetPreferredHeight("A", pText.GetGenerationSettings(val));
	}

	internal void populateOptions()
	{
		dropdown.ClearOptions();
		List<string> list = new List<string>();
		foreach (DebugToolAsset item in AssetManager.debug_tool_library.list)
		{
			if (item.type == type)
			{
				list.Add(item.name);
			}
		}
		dropdown.AddOptions(list);
		((UnityEvent<int>)(object)dropdown.onValueChanged).RemoveListener((UnityAction<int>)switchTool);
		((UnityEvent<int>)(object)dropdown.onValueChanged).AddListener((UnityAction<int>)switchTool);
	}

	public void filterOptions(string pInput)
	{
		DebugDropdownOption[] componentsInChildren = ((Component)((Component)active_dropdown).transform).GetComponentsInChildren<DebugDropdownOption>(true);
		foreach (DebugDropdownOption debugDropdownOption in componentsInChildren)
		{
			string text = debugDropdownOption.title.text;
			if (text == "Debug option")
			{
				((Component)debugDropdownOption).gameObject.SetActive(false);
			}
			else if (!string.IsNullOrEmpty(pInput) && !text.ToLower().Contains(pInput.ToLower()))
			{
				((Component)debugDropdownOption).gameObject.SetActive(false);
			}
			else
			{
				((Component)debugDropdownOption).gameObject.SetActive(true);
			}
		}
	}

	private void initButtons()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Expected O, but got Unknown
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Expected O, but got Unknown
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Expected O, but got Unknown
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Expected O, but got Unknown
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Expected O, but got Unknown
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Expected O, but got Unknown
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Expected O, but got Unknown
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Expected O, but got Unknown
		newButton("SortByName", new UnityAction(clickSortByName), delegate(Image pIcon)
		{
			checkIcon(pIcon, sort_by_names);
		});
		newButton("SortByValues", new UnityAction(clickSortByValues), delegate(Image pIcon)
		{
			checkIcon(pIcon, sort_by_values);
		});
		newButton("SortReversed", (UnityAction)delegate
		{
			sort_order_reversed = !sort_order_reversed;
		}, delegate(Image pIcon)
		{
			checkIcon(pIcon, sort_order_reversed);
		});
		newButton("ShowAverages", (UnityAction)delegate
		{
			show_averages = !show_averages;
		}, delegate(Image pIcon)
		{
			checkIcon(pIcon, isValueAverage());
		});
		newButton("PercentBasedOnSlowest", (UnityAction)delegate
		{
			percentage_slowest = !percentage_slowest;
		}, delegate(Image pIcon)
		{
			checkIcon(pIcon, percentage_slowest);
		});
		newButton("HideZeroes", (UnityAction)delegate
		{
			hide_zeroes = !hide_zeroes;
		}, delegate(Image pIcon)
		{
			checkIcon(pIcon, hide_zeroes);
		});
		newButton("ShowCounter", (UnityAction)delegate
		{
			show_counter = !show_counter;
		}, delegate(Image pIcon)
		{
			checkIcon(pIcon, show_counter);
		});
		newButton("ShowMax", (UnityAction)delegate
		{
			show_max = !show_max;
		}, delegate(Image pIcon)
		{
			checkIcon(pIcon, show_max);
		});
		newButton("ShowSeconds", (UnityAction)delegate
		{
			state = DebugToolState.Values;
		}, delegate(Image pIcon)
		{
			checkIcon(pIcon, state == DebugToolState.Values);
		});
		newButton("ShowPercentages", (UnityAction)delegate
		{
			state = DebugToolState.Percent;
		}, delegate(Image pIcon)
		{
			checkIcon(pIcon, state == DebugToolState.Percent);
		});
		newButton("ShowTimeSpent", (UnityAction)delegate
		{
			state = DebugToolState.TimeSpent;
		}, delegate(Image pIcon)
		{
			checkIcon(pIcon, state == DebugToolState.TimeSpent);
		});
		newButton("ShowFrameBudget", (UnityAction)delegate
		{
			state = DebugToolState.FrameBudget;
		}, delegate(Image pIcon)
		{
			checkIcon(pIcon, state == DebugToolState.FrameBudget);
		});
		newButton("Paused", (UnityAction)delegate
		{
			paused = !paused;
		}, delegate(Image pIcon)
		{
			checkIcon(pIcon, paused);
		});
		object obj = _003C_003Ec._003C_003E9__30_24;
		if (obj == null)
		{
			UnityAction val = delegate
			{
				Bench.bench_enabled = !Bench.bench_enabled;
			};
			_003C_003Ec._003C_003E9__30_24 = val;
			obj = (object)val;
		}
		newButton("EnableBenchmarks", (UnityAction)obj, delegate(Image pIcon)
		{
			checkIcon(pIcon, Bench.bench_enabled);
		});
	}

	private void newButton(string pID, UnityAction pAction, DebugIconOptionAction pCheckIcon)
	{
		Transform val = ((Component)this).transform.FindRecursive(pID);
		((UnityEvent)((Component)val).GetComponent<Button>().onClick).AddListener(pAction);
		list_actions.Add(pCheckIcon);
		list_icons.Add(((Component)val).GetComponent<Image>());
	}

	public bool isValueAverage()
	{
		return show_averages;
	}

	public bool isState(DebugToolState pState)
	{
		return state == pState;
	}

	private void updateIcons()
	{
		for (int i = 0; i < list_actions.Count; i++)
		{
			DebugIconOptionAction debugIconOptionAction = list_actions[i];
			Image pButton = list_icons[i];
			debugIconOptionAction(pButton);
		}
	}

	private void checkIcon(Image pImageIcon, bool pValue)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		if (pValue)
		{
			((Graphic)pImageIcon).color = Color.white;
		}
		else
		{
			((Graphic)pImageIcon).color = Color32.op_Implicit(Toolbox.color_transparent_grey);
		}
	}

	private void switchTool(int pIndex)
	{
		string text = dropdown.options[pIndex].text;
		DebugToolAsset debugToolAsset = AssetManager.debug_tool_library.get(text);
		setAsset(debugToolAsset);
	}

	public void setAsset(DebugToolAsset pAsset)
	{
		asset = pAsset;
		type = asset.type;
		((Component)benchmark_icons).gameObject.SetActive(asset.show_benchmark_buttons);
		if (asset.action_start != null)
		{
			asset.action_start(this);
		}
	}

	private void Update()
	{
		if (SmoothLoader.isLoading())
		{
			return;
		}
		updateIcons();
		double curSessionTime = World.world.getCurSessionTime();
		if (!(curSessionTime < last_update_timestamp + (double)asset.update_timeout) && !paused)
		{
			if (asset.action_update != null)
			{
				asset.action_update(this);
			}
			clearTexts();
			_ = dropdown.captionText.text;
			last_update_timestamp = curSessionTime;
			if (asset.action_1 != null)
			{
				asset.action_1(this);
			}
			if (asset.action_2 != null)
			{
				asset.action_2(this);
			}
			updateSize();
			pool_texts.disableInactive();
			((MonoBehaviour)this).StartCoroutine(updateSizeAfterFrame());
		}
	}

	public IEnumerator updateSizeAfterFrame()
	{
		yield return CoroutineHelper.wait_for_end_of_frame;
		updateSize();
	}

	private void updateSize()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		float num = LayoutUtility.GetPreferredWidth(((Component)transform_texts).GetComponent<RectTransform>()) * 1.2f;
		float num2 = LayoutUtility.GetPreferredHeight(((Component)transform_texts).GetComponent<RectTransform>()) + 40f;
		if (num < 126f)
		{
			num = 126f;
		}
		if (num2 < 60f)
		{
			num2 = 60f;
		}
		((Component)this).GetComponent<RectTransform>().sizeDelta = new Vector2(num, num2);
	}

	public void clickSortByName()
	{
		sort_by_names = !sort_by_names;
		sort_by_values = !sort_by_names;
	}

	public void clickSortByValues()
	{
		sort_by_values = !sort_by_values;
		sort_by_names = !sort_by_values;
	}

	public int kingdomSorter(Kingdom k1, Kingdom k2)
	{
		return k2.units.Count.CompareTo(k1.units.Count);
	}

	public int citySorter(City c1, City c2)
	{
		return c2.getPopulationPeople().CompareTo(c1.getPopulationPeople());
	}

	internal void setText(string pT1, object pT2, float pBarValue = 0f, bool pShowBar = false, long pCounter = 0L, bool pShowCounter = false, bool pShowMax = false, string pMaxValue = "")
	{
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		DebugToolTextElement next = pool_texts.getNext();
		string text = ((pT2 == null) ? "-" : pT2.ToString());
		if (pT2 != null)
		{
			if (pShowCounter && show_counter && (asset.split_benchmark || asset.show_last_count))
			{
				text = pCounter + " | " + text;
			}
			if (pShowMax)
			{
				text = pMaxValue + " | " + text;
			}
		}
		next.text_left.text = pT1;
		next.text_right.text = text;
		textCount++;
		if (pShowBar)
		{
			((Component)next.text_bar).gameObject.SetActive(true);
			if (pBarValue > 100f)
			{
				pBarValue = 101f;
			}
			float num = pBarValue * 0.5f;
			((Component)next.text_bar).GetComponent<RectTransform>().sizeDelta = new Vector2(num, 4.2f);
			if (pBarValue > 70f && pBarValue != 100f)
			{
				((Graphic)next.text_bar).color = Color32.op_Implicit(Toolbox.color_debug_bar_red);
			}
			else
			{
				((Graphic)next.text_bar).color = Color32.op_Implicit(Toolbox.color_debug_bar_blue);
			}
		}
		else
		{
			((Component)next.text_bar).gameObject.SetActive(false);
		}
	}

	internal void setSeparator()
	{
		DebugToolTextElement next = pool_texts.getNext();
		next.text_left.text = string.Empty;
		next.text_right.text = string.Empty;
		((Component)next.text_bar).gameObject.SetActive(false);
	}

	private void clearTexts()
	{
		textCount = 0;
		pool_texts.clear(pDisable: false);
	}

	public void clickClose()
	{
		Object.Destroy((Object)(object)((Component)this).gameObject, 0.01f);
	}

	public void clickDuplicate()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		int pX = (int)((Component)this).transform.localPosition.x + 126 + 2;
		int pY = (int)((Component)this).transform.localPosition.y;
		DebugConfig.createTool(asset.id, pX, pY);
	}
}
