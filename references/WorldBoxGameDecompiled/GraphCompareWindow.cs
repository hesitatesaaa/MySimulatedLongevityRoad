using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using db;

public class GraphCompareWindow : MonoBehaviour
{
	public GraphCompareMetaObject meta_object_1;

	public GraphCompareMetaObject meta_object_2;

	public GraphCompareMetaObject meta_object_3;

	public GraphController graph_controller;

	[SerializeField]
	private GameObject _empty_list_message;

	[SerializeField]
	private RectTransform _meta_drag_object;

	private ObjectPoolGenericMono<RectTransform> _pool_drag_objects;

	private MultiBannerPool _pool_banners;

	private MultiBannerPool _pool_drop_banners;

	[SerializeField]
	private Button _noos_button;

	[SerializeField]
	private Image _noos_icon;

	[SerializeField]
	private Transform _noos_list_container;

	[SerializeField]
	private Transform _pool_banner_container;

	[SerializeField]
	private Transform _pool_drop_banner_container;

	private MetaTypeAsset _current_asset;

	private List<MetaTypeAsset> _noos_list = new List<MetaTypeAsset>();

	private List<NanoObject> _noos_items = new List<NanoObject>();

	private Coroutine _load_noos_items;

	private const int VISIBLE_ITEMS = 6;

	[SerializeField]
	private CanvasGroup[] _block_during_random;

	private bool _is_randomizing;

	private bool _stop_randomizer;

	private void Awake()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected O, but got Unknown
		foreach (Transform item in _noos_list_container)
		{
			Transform val = item;
			if (((Object)((Component)val).gameObject).name.StartsWith("MetaContainer"))
			{
				Object.Destroy((Object)(object)((Component)val).gameObject);
			}
		}
		_pool_drag_objects = new ObjectPoolGenericMono<RectTransform>(_meta_drag_object, _noos_list_container);
		_pool_banners = new MultiBannerPool(_pool_banner_container);
		_pool_drop_banners = new MultiBannerPool(_pool_drop_banner_container);
		((UnityEvent)_noos_button.onClick).AddListener((UnityAction)delegate
		{
			nextNoos();
		});
	}

	internal MultiBannerPool getDropBannerPool()
	{
		return _pool_drop_banners;
	}

	private void OnEnable()
	{
		ScrollWindow.addCallbackHide(resetPoolsAndParents);
		loadNoos();
		if (hasAny())
		{
			if (Config.selected_objects_graph.Count == 0)
			{
				((MonoBehaviour)this).StartCoroutine(displayRandom());
			}
			else
			{
				((MonoBehaviour)this).StartCoroutine(displaySelected());
			}
		}
	}

	private IEnumerator selectNoosCoroutine()
	{
		if (Config.selected_objects_graph.Count != 0)
		{
			selectNoos(Config.selected_objects_graph.First());
			SoundBox.click();
			yield return (object)new WaitForEndOfFrame();
		}
	}

	private IEnumerator updateGraph()
	{
		if (Config.selected_objects_graph.Count != 0)
		{
			string activeCategory = graph_controller.getActiveCategory();
			graph_controller.resetAndUpdateGraph();
			graph_controller.tryEnableCategory(activeCategory);
			yield return (object)new WaitForEndOfFrame();
		}
	}

	private IEnumerator displaySelected(bool pUpdate = true)
	{
		if (Config.selected_objects_graph.Count == 0)
		{
			yield break;
		}
		using ListPool<NanoObject> tSelectedObjects = new ListPool<NanoObject>(3);
		tSelectedObjects.Add(Config.selected_objects_graph[0]);
		tSelectedObjects.Add(Config.selected_objects_graph[1]);
		tSelectedObjects.Add(Config.selected_objects_graph[2]);
		meta_object_1.empty();
		meta_object_2.empty();
		meta_object_3.empty();
		Config.selected_objects_graph.Clear();
		meta_object_1.setObject(tSelectedObjects[0]);
		yield return (object)new WaitForEndOfFrame();
		meta_object_2.setObject(tSelectedObjects[1]);
		yield return (object)new WaitForEndOfFrame();
		meta_object_3.setObject(tSelectedObjects[2]);
		yield return (object)new WaitForEndOfFrame();
		if (pUpdate)
		{
			yield return selectNoosCoroutine();
			yield return updateGraph();
		}
	}

	private void OnDisable()
	{
		clearNoosItems();
		clearAsset();
	}

	private void clearAsset()
	{
		_current_asset = null;
	}

	private void loadNoos()
	{
		_noos_list.Clear();
		foreach (HistoryMetaDataAsset item in AssetManager.history_meta_data_library.list)
		{
			MetaTypeAsset metaTypeAsset = AssetManager.meta_type_library.get(item.id);
			if (metaTypeAsset.has_any())
			{
				_noos_list.Add(metaTypeAsset);
			}
		}
		showItems(hasAny());
	}

	private bool hasAny()
	{
		return _noos_list.Count > 0;
	}

	private void showItems(bool pShow)
	{
		Transform val = ((Component)this).transform.FindRecursive("Content");
		for (int i = 0; i < val.childCount; i++)
		{
			((Component)val.GetChild(i)).gameObject.SetActive(pShow);
		}
		_empty_list_message.SetActive(!pShow);
	}

	private void updateNoosIcon(MetaTypeAsset pAsset)
	{
		Sprite sprite = SpriteTextureLoader.getSprite("ui/Icons/" + pAsset.icon_list);
		_noos_icon.sprite = sprite;
	}

	public void clearNoosItems()
	{
		_noos_items.Clear();
		_pool_banners.clear();
		_pool_drag_objects.clear();
	}

	private void resetNoosList()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		((Component)_noos_list_container).GetComponent<RectTransform>().offsetMin = new Vector2(0f, 0f);
		((Component)_noos_list_container).GetComponent<RectTransform>().offsetMax = new Vector2(0f, 0f);
		clearNoosItems();
	}

	private void resetPoolsAndParents(string pID)
	{
		if (!(pID != "chart_comparer"))
		{
			((MonoBehaviour)this).StopAllCoroutines();
			clearNoosItems();
			meta_object_1.empty();
			meta_object_2.empty();
			meta_object_3.empty();
			ScrollWindow.removeCallbackHide(resetPoolsAndParents);
		}
	}

	public IEnumerator loadNoosItemsCoroutine(bool pSilent = false)
	{
		resetNoosList();
		_noos_items.AddRange(_current_asset.get_list());
		_noos_items.Sort(sortByUnits);
		using ListPool<NanoObject> tItems = new ListPool<NanoObject>(_noos_items);
		int tCount = 0;
		foreach (ref NanoObject item in tItems)
		{
			NanoObject current = item;
			if (current != meta_object_1.current_item && current != meta_object_2.current_item && current != meta_object_3.current_item)
			{
				RectTransform next = _pool_drag_objects.getNext();
				((Object)((Component)next).gameObject).name = "MetaContainer " + current.getID();
				IBanner banner = setupDragBanner(current, ((Component)next).transform, _pool_banners);
				if (tCount++ < 6)
				{
					banner.jump(0.1f, pSilent);
					yield return (object)new WaitForEndOfFrame();
				}
			}
		}
	}

	public int countNoosItems()
	{
		return _noos_items.Count;
	}

	public static int sortByUnits(NanoObject pNanoObject1, NanoObject pNanoObject2)
	{
		return ((IMetaObject)pNanoObject2).countUnits().CompareTo(((IMetaObject)pNanoObject1).countUnits());
	}

	private void nextNoos()
	{
		int num = _noos_list.IndexOf(_current_asset);
		num = Toolbox.loopIndex(++num, _noos_list.Count);
		selectNoos(_noos_list[num]);
	}

	private void selectNoos(NanoObject pObject)
	{
		MetaTypeAsset pAsset = AssetManager.meta_type_library.get(pObject.getType());
		selectNoos(pAsset);
	}

	private void selectNoos(MetaTypeAsset pAsset)
	{
		if (_current_asset != pAsset)
		{
			clearNoosItems();
			_current_asset = pAsset;
			updateNoosIcon(_current_asset);
			loadNoosItems();
		}
	}

	public IBanner setupBanner(NanoObject pObject, Transform pBannerArea, MultiBannerPool pBannerPool)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		IBanner next = pBannerPool.getNext(pObject);
		next.load(pObject);
		next.transform.localScale = new Vector3(1f, 1f, 1f);
		next.transform.SetParent(pBannerArea);
		UiButtonHoverAnimation component = next.GetComponent<UiButtonHoverAnimation>();
		((Behaviour)component).enabled = false;
		component.scale_size = 1f;
		component.default_scale = new Vector3(1f, 1f, 1f);
		next.GetComponent<TipButton>().setDefaultScale(pBannerArea.localScale);
		if (!((IBaseMono)next).HasComponent<LayoutElement>())
		{
			((IBaseMono)next).AddComponent<LayoutElement>().ignoreLayout = true;
		}
		RectTransform component2 = next.GetComponent<RectTransform>();
		component2.SetAnchor(AnchorPresets.MiddleCenter);
		((Transform)component2).localScale = new Vector3(1f, 1f, 1f);
		component2.anchoredPosition = new Vector2(0f, 0f);
		return next;
	}

	private IBanner setupDragBanner(NanoObject pObject, Transform pBannerArea, MultiBannerPool pBannerPool)
	{
		IBanner banner = setupBanner(pObject, pBannerArea, pBannerPool);
		if (!((IBaseMono)banner).HasComponent<GraphCompareMetaSelector>())
		{
			GraphCompareMetaSelector graphCompareMetaSelector = ((IBaseMono)banner).AddComponent<GraphCompareMetaSelector>();
			graphCompareMetaSelector.addWindow(this);
			graphCompareMetaSelector.addDropzones(((Component)meta_object_1).GetComponent<RectTransform>(), ((Component)meta_object_2).GetComponent<RectTransform>(), ((Component)meta_object_3).GetComponent<RectTransform>());
		}
		return banner;
	}

	private ListPool<NanoObject> getPossibleItems()
	{
		ListPool<NanoObject> listPool = new ListPool<NanoObject>();
		foreach (MetaTypeAsset item in _noos_list)
		{
			foreach (NanoObject item2 in item.get_list())
			{
				listPool.Add(item2);
			}
		}
		return listPool;
	}

	internal void loadNoosItems(bool pSilent = false)
	{
		if (_load_noos_items != null)
		{
			((MonoBehaviour)this).StopCoroutine(_load_noos_items);
		}
		_load_noos_items = ((MonoBehaviour)this).StartCoroutine(loadNoosItemsCoroutine(pSilent));
	}

	private void selectRandom()
	{
		using ListPool<NanoObject> listPool = getPossibleItems();
		Config.selected_objects_graph.Clear();
		int pMax = Mathf.Min(listPool.Count, 3);
		foreach (NanoObject item in listPool.LoopRandom(pMax))
		{
			Config.selected_objects_graph.Add(item);
		}
		if (listPool.Count <= 7)
		{
			_stop_randomizer = true;
		}
	}

	public void randomizeSelection()
	{
		if (_is_randomizing)
		{
			_stop_randomizer = true;
			return;
		}
		((MonoBehaviour)this).StopAllCoroutines();
		((MonoBehaviour)this).StartCoroutine(displayRandom());
	}

	private IEnumerator displayRandom()
	{
		_is_randomizing = true;
		CanvasGroup[] block_during_random = _block_during_random;
		foreach (CanvasGroup obj in block_during_random)
		{
			obj.interactable = false;
			obj.blocksRaycasts = false;
		}
		for (int j = 0; j < 10; j++)
		{
			if (_stop_randomizer)
			{
				break;
			}
			selectRandom();
			yield return displaySelected(pUpdate: false);
			yield return randomizeCategories();
			yield return updateGraph();
			yield return randomNoosItems();
			updateNoosIcon(_noos_list.GetRandom());
		}
		yield return randomizeCategories();
		yield return randomizeTimescale();
		clearAsset();
		yield return selectNoosCoroutine();
		block_during_random = _block_during_random;
		foreach (CanvasGroup obj2 in block_during_random)
		{
			obj2.interactable = true;
			obj2.blocksRaycasts = true;
		}
		_stop_randomizer = false;
		_is_randomizing = false;
	}

	private IEnumerator randomizeCategories()
	{
		graph_controller.pickRandomCategory();
		SoundBox.click();
		yield return (object)new WaitForEndOfFrame();
	}

	private IEnumerator randomizeTimescale()
	{
		if (graph_controller.randomTimeScale())
		{
			SoundBox.click();
			yield return (object)new WaitForEndOfFrame();
		}
	}

	public IEnumerator randomNoosItems()
	{
		resetNoosList();
		using ListPool<NanoObject> tPossibleItems = getPossibleItems();
		int pMax = Mathf.Min(6, tPossibleItems.Count);
		foreach (NanoObject item in tPossibleItems.LoopRandom(pMax))
		{
			RectTransform next = _pool_drag_objects.getNext();
			((Object)((Component)next).gameObject).name = "MetaContainer " + item.getID();
			IBanner banner = setupDragBanner(item, ((Component)next).transform, _pool_banners);
			if (Randy.randomBool())
			{
				banner.jump(0.025f, pSilent: true);
			}
			yield return (object)new WaitForEndOfFrame();
		}
	}
}
