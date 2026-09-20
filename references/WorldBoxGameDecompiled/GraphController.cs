using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ChartAndGraph;
using UnityEngine;
using UnityEngine.Events;
using db;
using db.tables;

public class GraphController : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static UnityAction<GraphEventArgs> _003C_003E9__39_0;

		public static UnityAction _003C_003E9__39_1;

		internal void _003ChookEvents_003Eb__39_0(GraphEventArgs _)
		{
			Tooltip.cancelHiding();
		}

		internal void _003ChookEvents_003Eb__39_1()
		{
			Tooltip.scheduledHide(0.15f, pSkipTouch: true);
		}
	}

	public static MinMax min_max;

	public GraphChart chart;

	[SerializeField]
	private MetaType _meta_type = MetaType.City;

	private GraphCategoriesContainer _container_graph_categories;

	private GraphTimeScaleContainer _container_time_scale;

	public bool clear_on_enable;

	public bool multi_chart;

	private VerticalAxis _vertical_axis;

	private HorizontalAxis _horizontal_axis;

	private List<HistoryDataAsset> _list_categories = new List<HistoryDataAsset>();

	private Dictionary<string, bool> _category_enabled = new Dictionary<string, bool>();

	private Dictionary<string, MinMax> _min_max_categories = new Dictionary<string, MinMax>();

	private long _min_timestamp = long.MinValue;

	private long _max_timestamp = long.MaxValue;

	private HashSet<MetaType> _current_types = new HashSet<MetaType>();

	private List<NanoObject> _current_objects = new List<NanoObject>();

	private Dictionary<NanoObject, HistoryTable> _last_data = new Dictionary<NanoObject, HistoryTable>();

	private GraphTimeAsset _current_sample;

	private HistoryInterval _current_interval;

	private Dictionary<string, CategoryData> _current_datas = new Dictionary<string, CategoryData>();

	private bool _events_hooked;

	private bool _loaded;

	private bool _categories_loaded;

	private long _last_timestamp = -1L;

	private void Awake()
	{
		_container_time_scale = ((Component)((Component)this).transform).GetComponentInChildren<GraphTimeScaleContainer>();
		_vertical_axis = ((Component)((Component)this).transform).GetComponentInChildren<VerticalAxis>();
		_horizontal_axis = ((Component)((Component)this).transform).GetComponentInChildren<HorizontalAxis>();
		_container_graph_categories = ((Component)((Component)this).transform).GetComponentInChildren<GraphCategoriesContainer>();
	}

	internal List<NanoObject> getObjects()
	{
		return _current_objects;
	}

	internal List<HistoryDataAsset> getCategories()
	{
		return _list_categories;
	}

	internal bool hasCategory(HistoryDataAsset pCategory)
	{
		return _list_categories.Contains(pCategory);
	}

	internal bool hasCategory(string pCategory)
	{
		HistoryDataAsset pCategory2 = AssetManager.history_data_library.get(pCategory);
		return hasCategory(pCategory2);
	}

	private static string getCategoryName(string pCategory)
	{
		return GraphHelpers.getCategoryName(pCategory);
	}

	private NanoObject extractObject(string pCategory)
	{
		if (!pCategory.Contains('|'))
		{
			return null;
		}
		_ = pCategory.Split('|')[0];
		string text = pCategory.Split('|')[1];
		string text2 = pCategory.Split('|')[2];
		foreach (NanoObject current_object in _current_objects)
		{
			if (current_object.getType() == text && current_object.getTypeID() == text2)
			{
				return current_object;
			}
		}
		return null;
	}

	internal bool isCategoryEnabled(string pCategory)
	{
		string categoryName = getCategoryName(pCategory);
		return _category_enabled[categoryName];
	}

	internal string getActiveCategory()
	{
		foreach (string key in _category_enabled.Keys)
		{
			if (_category_enabled[key])
			{
				return key;
			}
		}
		return null;
	}

	private void loadCategories()
	{
		if (_categories_loaded)
		{
			return;
		}
		_categories_loaded = true;
		_list_categories.Clear();
		HashSet<HistoryDataAsset> hashSet = new HashSet<HistoryDataAsset>();
		foreach (MetaType current_type in _current_types)
		{
			HistoryMetaDataAsset[] assets = AssetManager.history_meta_data_library.getAssets(current_type);
			HashSet<HistoryDataAsset> hashSet2 = new HashSet<HistoryDataAsset>();
			HistoryMetaDataAsset[] array = assets;
			foreach (HistoryMetaDataAsset historyMetaDataAsset in array)
			{
				hashSet2.UnionWith(historyMetaDataAsset.categories);
			}
			if (hashSet.Count == 0)
			{
				hashSet.UnionWith(hashSet2);
			}
			else
			{
				hashSet.IntersectWith(hashSet2);
			}
		}
		foreach (NanoObject current_object in _current_objects)
		{
			foreach (HistoryDataAsset item in hashSet)
			{
				if (!hasCategory(item))
				{
					addCategory(item, item.enabled_default);
				}
				colorCategory(item, current_object, multi_chart);
			}
		}
	}

	internal void addCategory(HistoryDataAsset pAsset, bool pEnabled = false)
	{
		_list_categories.Add(pAsset);
		_category_enabled[pAsset.id] = pEnabled;
	}

	internal void disableAllCategories(string pExcept = null)
	{
		foreach (HistoryDataAsset category in getCategories())
		{
			if (!(category.id == pExcept))
			{
				setCategoryEnabled(category.id, pIsOn: false, pUpdateGraph: false);
			}
		}
	}

	internal void pickRandomCategory()
	{
		using ListPool<string> listPool = GraphHelpers.bestCategories(_min_max_categories);
		if (listPool.Count != 0)
		{
			string random = listPool.GetRandom();
			tryEnableCategory(random);
		}
	}

	internal void tryEnableCategory(string pCategory)
	{
		if (!string.IsNullOrEmpty(pCategory) && hasCategory(pCategory))
		{
			_container_graph_categories.setCategoryEnabled(pCategory, pEnabled: true);
		}
	}

	internal void setCategoryEnabled(string pCategory, bool pIsOn, bool pUpdateGraph = true)
	{
		_category_enabled[pCategory] = pIsOn;
		foreach (string categoryName in ((GraphChartBase)chart).DataSource.CategoryNames)
		{
			if (categoryName.StartsWith(pCategory + "|"))
			{
				((GraphChartBase)chart).DataSource.SetCategoryEnabled(categoryName, pIsOn);
			}
		}
		if (pUpdateGraph)
		{
			updateGraph();
		}
	}

	private void hookEvents()
	{
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected O, but got Unknown
		if (_events_hooked)
		{
			return;
		}
		_events_hooked = true;
		((UnityEvent<GraphEventArgs>)(object)((GraphChartBase)chart).PointHovered).AddListener((UnityAction<GraphEventArgs>)delegate
		{
			Tooltip.cancelHiding();
		});
		if (multi_chart)
		{
			((UnityEvent<GraphEventArgs>)(object)((GraphChartBase)chart).PointHovered).AddListener((UnityAction<GraphEventArgs>)multiChartHover);
		}
		else
		{
			((UnityEvent<GraphEventArgs>)(object)((GraphChartBase)chart).PointHovered).AddListener((UnityAction<GraphEventArgs>)singleChartHover);
		}
		UnityEvent nonHovered = ((GraphChartBase)chart).NonHovered;
		object obj = _003C_003Ec._003C_003E9__39_1;
		if (obj == null)
		{
			UnityAction val = delegate
			{
				Tooltip.scheduledHide(0.15f, pSkipTouch: true);
			};
			_003C_003Ec._003C_003E9__39_1 = val;
			obj = (object)val;
		}
		nonHovered.AddListener((UnityAction)obj);
	}

	private void multiChartHover(GraphEventArgs pArgs)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		long tYear = (long)pArgs.Value.x;
		string tCategoryName = getCategoryName(pArgs.Category);
		if (Tooltip.anyActive())
		{
			Tooltip tooltip = Tooltip.findActive(delegate(Tooltip pTooltip)
			{
				if (pTooltip.asset.id != "graph_multi_resource")
				{
					return false;
				}
				return !(pTooltip.data.tip_name != tCategoryName) && pTooltip.data.custom_data_long["year"] == tYear;
			});
			if ((Object)(object)tooltip != (Object)null)
			{
				tooltip.reposition();
				return;
			}
		}
		CustomDataContainer<string> customDataContainer = new CustomDataContainer<string>();
		CustomDataContainer<long> customDataContainer2 = new CustomDataContainer<long>();
		customDataContainer2["year"] = tYear;
		foreach (string categoryName in ((GraphChartBase)chart).DataSource.CategoryNames)
		{
			if (isCategoryEnabled(categoryName))
			{
				NanoObject nanoObject = extractObject(categoryName);
				string name = nanoObject.name;
				(long tValue, long tPrevious) categoryValueAtTime = getCategoryValueAtTime(categoryName, (long)pArgs.Value.x);
				long item = categoryValueAtTime.tValue;
				long item2 = categoryValueAtTime.tPrevious;
				customDataContainer2[name] = item;
				customDataContainer2[name + "_previous"] = item2;
				customDataContainer[name] = Toolbox.colorToHex(Color32.op_Implicit(nanoObject.getColor().getColorText()));
			}
		}
		Tooltip.show(pArgs.Position, "graph_multi_resource", new TooltipData
		{
			tip_name = tCategoryName,
			custom_data_long = customDataContainer2,
			custom_data_string = customDataContainer
		});
	}

	private void singleChartHover(GraphEventArgs pArgs)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		Tooltip.hideTooltip();
		CustomDataContainer<long> customDataContainer = new CustomDataContainer<long>();
		customDataContainer["year"] = (long)pArgs.Value.x;
		foreach (string categoryName2 in ((GraphChartBase)chart).DataSource.CategoryNames)
		{
			if (isCategoryEnabled(categoryName2))
			{
				string categoryName = getCategoryName(categoryName2);
				(long tValue, long tPrevious) categoryValueAtTime = getCategoryValueAtTime(categoryName2, (long)pArgs.Value.x);
				long item = categoryValueAtTime.tValue;
				long item2 = categoryValueAtTime.tPrevious;
				customDataContainer[categoryName] = item;
				customDataContainer[categoryName + "_previous"] = item2;
			}
		}
		NanoObject nano_object = extractObject(pArgs.Category);
		Tooltip.show(pArgs.Position, "graph_resource", new TooltipData
		{
			custom_data_long = customDataContainer,
			nano_object = nano_object
		});
	}

	public void resetAndUpdateGraph()
	{
		_loaded = false;
		_categories_loaded = false;
		_current_interval = HistoryInterval.None;
		_container_time_scale.resetTimeScale();
		updateGraph();
		_container_time_scale.calcBounds();
	}

	public bool randomTimeScale()
	{
		if (_container_time_scale.randomizeTimeScale())
		{
			updateGraph();
			return true;
		}
		return false;
	}

	public void forceUpdateGraph()
	{
		updateGraph();
	}

	private void updateGraph()
	{
		if (!Config.disable_db && Config.graphs)
		{
			((ScrollableChartData)((GraphChartBase)chart).DataSource).StartBatch();
			loadGraph();
			if (_container_time_scale.resetTimeScale())
			{
				clearChartData();
			}
			loadSample();
			loadCategoryAndCharts();
			adjustCharts();
			((ScrollableChartData)((GraphChartBase)chart).DataSource).EndBatch();
		}
	}

	private void loadGraph()
	{
		if (!_loaded)
		{
			_loaded = true;
			((AxisBase)((Component)chart).GetComponent<HorizontalAxis>()).CustomNumberFormatWorldbox = GraphHelpers.horizontalFormatYears;
			((AnyChart)chart).CustomNumberFormat = GraphHelpers.verticalFormat;
			((Behaviour)_vertical_axis).enabled = true;
			((Behaviour)_horizontal_axis).enabled = true;
			if (multi_chart)
			{
				loadMultiChart();
			}
			else
			{
				loadSingleChart();
			}
			hookEvents();
		}
	}

	private void loadSingleChart()
	{
		NanoObject pMetaObject = AssetManager.meta_type_library.getAsset(_meta_type).get_selected();
		selectContainer(pMetaObject);
	}

	private void loadMultiChart()
	{
		_current_types.Clear();
		_current_objects.Clear();
		_last_data.Clear();
		foreach (NanoObject item in Config.selected_objects_graph)
		{
			if (item != null && item.isAlive())
			{
				addContainer(item);
			}
		}
		clearChartData();
		_category_enabled.Clear();
	}

	private void showCategory(string pCategory, NanoObject pObject)
	{
		string type = pObject.getType();
		string typeID = pObject.getTypeID();
		CategoryData categoryData = _current_datas[typeID];
		string text = pCategory + "|" + type + "|" + typeID;
		for (LinkedListNode<Dictionary<string, long>> linkedListNode = categoryData.Last; linkedListNode != null; linkedListNode = linkedListNode.Previous)
		{
			if (linkedListNode.Value.ContainsKey(pCategory))
			{
				long num = linkedListNode.Value[pCategory];
				long num2 = linkedListNode.Value["timestamp"];
				bool flag = false;
				long num3 = linkedListNode.Previous?.Value[pCategory] ?? 0;
				long num4 = linkedListNode.Next?.Value[pCategory] ?? 0;
				if (num == num3 && num == num4)
				{
					flag = true;
				}
				((GraphChartBase)chart).DataSource.AddPointToCategory(text, (double)num2, (double)num, (double)(flag ? 0f : (-1f)));
			}
		}
	}

	private (long tValue, long tPrevious) getCategoryValueAtTime(string pCategory, long pTime)
	{
		string categoryName = getCategoryName(pCategory);
		string key = pCategory.Split('|').Last();
		CategoryData categoryData = _current_datas[key];
		long item = 0L;
		long item2 = 0L;
		bool flag = false;
		for (LinkedListNode<Dictionary<string, long>> linkedListNode = categoryData.Last; linkedListNode != null; linkedListNode = linkedListNode.Previous)
		{
			if (linkedListNode.Value.ContainsKey(categoryName))
			{
				if (flag)
				{
					item2 = linkedListNode.Value[categoryName];
					break;
				}
				long num = linkedListNode.Value["timestamp"];
				if (num <= pTime)
				{
					if (num <= pTime)
					{
						item = linkedListNode.Value[categoryName];
					}
					flag = true;
				}
			}
		}
		return (tValue: item, tPrevious: item2);
	}

	private void colorCategory(HistoryDataAsset pHistoryDataAsset, NanoObject pObject, bool pColorFromObject = false)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		string type = pObject.getType();
		string typeID = pObject.getTypeID();
		string id = pHistoryDataAsset.id;
		string text = id + "|" + type + "|" + typeID;
		float num = 2f;
		MaterialTiling val = new MaterialTiling
		{
			EnableTiling = false
		};
		bool flag = true;
		Material chartLineMaterial;
		Material chartInnerFillMaterial;
		if (pColorFromObject)
		{
			Color colorText = pObject.getColor().getColorText();
			chartLineMaterial = HistoryDataAsset.getChartLineMaterial(colorText);
			chartInnerFillMaterial = HistoryDataAsset.getChartInnerFillMaterial(colorText);
		}
		else
		{
			chartLineMaterial = pHistoryDataAsset.getChartLineMaterial();
			chartInnerFillMaterial = pHistoryDataAsset.getChartInnerFillMaterial();
		}
		((GraphChartBase)chart).DataSource.AddCategory(text, chartLineMaterial, (double)num, val, chartInnerFillMaterial, flag, (Material)null, 0.0, false);
		((GraphChartBase)chart).DataSource.SetCategoryEnabled(text, isCategoryEnabled(id));
		((GraphChartBase)chart).DataSource.Set2DCategoryPrefabs(text, (ChartItemEffect)null, pHistoryDataAsset.getHoverPointMaterial());
		int num2 = 10;
		((GraphChartBase)chart).DataSource.SetCategoryPoint(text, pHistoryDataAsset.getChartPointMaterial(), (double)num2);
	}

	private MinMax getMinMax(string pCategoryName)
	{
		long num = long.MaxValue;
		long num2 = long.MinValue;
		bool flag = false;
		string key = pCategoryName.Split('|')[0];
		_ = pCategoryName.Split('|')[1];
		string key2 = pCategoryName.Split('|')[2];
		if (_current_datas.Count == 0 || !_current_datas.ContainsKey(key2))
		{
			return new MinMax(0L, 0L);
		}
		for (LinkedListNode<Dictionary<string, long>> linkedListNode = _current_datas[key2].Last; linkedListNode != null; linkedListNode = linkedListNode.Previous)
		{
			Dictionary<string, long> value = linkedListNode.Value;
			if (value.ContainsKey(key))
			{
				long num3 = value[key];
				long num4 = value["timestamp"];
				if (flag && num4 < _min_timestamp)
				{
					break;
				}
				if (num3 < num)
				{
					num = num3;
				}
				if (num3 > num2)
				{
					num2 = num3;
				}
				flag = true;
			}
		}
		if (!flag)
		{
			return new MinMax(0L, 0L);
		}
		return new MinMax(num, num2);
	}

	internal void adjustCharts()
	{
		long num = long.MaxValue;
		long num2 = 0L;
		_min_max_categories.Clear();
		foreach (string categoryName in ((GraphChartBase)chart).DataSource.CategoryNames)
		{
			MinMax minMax = getMinMax(categoryName);
			_min_max_categories.Add(categoryName, minMax);
			if (isCategoryEnabled(categoryName))
			{
				if (minMax.max > num2)
				{
					num2 = minMax.max;
				}
				if (minMax.min < num)
				{
					num = minMax.min;
				}
			}
		}
		num2 = GraphHelpers.calculateNiceMaxAxisSize((double)num2 * 1.05);
		int num3 = GraphHelpers.findVerticalDivision(num2);
		if (num >= 0)
		{
			num = 0L;
		}
		else
		{
			num = GraphHelpers.calculateNiceMaxAxisSize((double)(-num) * 1.05);
			if (num < num2)
			{
				long num4 = num2 / num3;
				int num5 = Mathf.CeilToInt((float)num / (float)num4);
				num = num5 * num4;
				num3 += num5;
			}
			else
			{
				num3 = GraphHelpers.findVerticalDivision(num);
				long num6 = num / num3;
				int num7 = Mathf.CeilToInt((float)num2 / (float)num6);
				num2 = num7 * num6;
				num3 += num7;
			}
		}
		((ScrollableChartData)((GraphChartBase)chart).DataSource).VerticalViewOrigin = -num;
		((ScrollableChartData)((GraphChartBase)chart).DataSource).VerticalViewSize = num2 + num;
		((ScrollableChartData)((GraphChartBase)chart).DataSource).HorizontalViewOrigin = GraphTimeLibrary.getMinTime(_current_sample);
		((ScrollableChartData)((GraphChartBase)chart).DataSource).HorizontalViewSize = GraphTimeLibrary.getMaxTime(_current_sample) - GraphTimeLibrary.getMinTime(_current_sample);
		((ChartDivisionInfo)((AxisBase)_horizontal_axis).MainDivisions).Total = 5;
		((ChartDivisionInfo)((AxisBase)_horizontal_axis).MainDivisions).FractionDigits = 2;
		((ChartDivisionInfo)((AxisBase)_vertical_axis).MainDivisions).Total = num3;
		min_max = new MinMax(-num, num2);
	}

	private void loadSample()
	{
		GraphTimeScale currentScale = _container_time_scale.getCurrentScale();
		_current_sample = AssetManager.graph_time_library.get(currentScale.ToString());
		bool flag = false;
		_current_interval = _current_sample.interval;
		foreach (NanoObject current_object in _current_objects)
		{
			string typeID = current_object.getTypeID();
			if (!_current_datas.TryGetValue(typeID, out var value))
			{
				value = new CategoryData();
				_current_datas[typeID] = value;
			}
			if (DBGetter.getData(value, current_object, _current_interval, _last_data[current_object]))
			{
				flag = true;
			}
		}
		if (flag)
		{
			clearChartData();
		}
		_min_timestamp = GraphTimeLibrary.getMinTime(_current_sample);
		_max_timestamp = GraphTimeLibrary.getMaxTime(_current_sample);
	}

	private void clearChartData()
	{
		_categories_loaded = false;
		((ScrollableChartData)((GraphChartBase)chart).DataSource).Clear();
	}

	private void loadCategoryAndCharts()
	{
		loadCategories();
		foreach (NanoObject current_object in _current_objects)
		{
			foreach (HistoryDataAsset list_category in _list_categories)
			{
				string id = list_category.id;
				showCategory(id, current_object);
			}
		}
		_container_graph_categories.apply();
	}

	private void selectContainer(NanoObject pMetaObject)
	{
		MetaType metaType = pMetaObject.getMetaType();
		if (!_current_types.Contains(metaType))
		{
			_category_enabled.Clear();
			clearChartData();
		}
		else if (!_current_objects.Contains(pMetaObject))
		{
			clearChartData();
		}
		_current_types.Clear();
		_current_objects.Clear();
		_last_data.Clear();
		_current_types.Add(metaType);
		_current_objects.Add(pMetaObject);
		HistoryMetaDataAsset[] assets = AssetManager.history_meta_data_library.getAssets(metaType);
		foreach (HistoryMetaDataAsset historyMetaDataAsset in assets)
		{
			_last_data[pMetaObject] = historyMetaDataAsset.collector(pMetaObject);
			_last_data[pMetaObject].timestamp = Date.getCurrentYear();
		}
	}

	private void addContainer(NanoObject pMetaObject)
	{
		MetaType metaType = pMetaObject.getMetaType();
		_current_types.Add(metaType);
		_current_objects.Add(pMetaObject);
		HistoryMetaDataAsset[] assets = AssetManager.history_meta_data_library.getAssets(metaType);
		foreach (HistoryMetaDataAsset historyMetaDataAsset in assets)
		{
			_last_data[pMetaObject] = historyMetaDataAsset.collector(pMetaObject);
			_last_data[pMetaObject].timestamp = Date.getCurrentYear();
		}
	}

	private void clearGraph()
	{
		clearChartData();
		_category_enabled.Clear();
		_list_categories.Clear();
		_loaded = false;
		_container_graph_categories.apply();
	}

	internal void load()
	{
		_loaded = false;
		if (multi_chart)
		{
			_current_interval = HistoryInterval.None;
			_container_time_scale.resetTimeScale();
		}
		if (clear_on_enable)
		{
			clearGraph();
		}
		if (_last_timestamp != Date.getMonthsSince(0.0))
		{
			_last_timestamp = Date.getMonthsSince(0.0);
			foreach (CategoryData value in _current_datas.Values)
			{
				value.Dispose();
			}
			_current_datas.Clear();
			clearChartData();
		}
		updateGraph();
		_container_time_scale.calcBounds();
	}

	private void clear()
	{
		_current_types.Clear();
		_current_objects.Clear();
		_last_data.Clear();
	}

	private void OnEnable()
	{
		load();
	}

	private void OnDisable()
	{
		clear();
	}
}
