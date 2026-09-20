using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class GraphCompareMetaObject : MonoBehaviour, IDropHandler, IEventSystemHandler
{
	private GraphCompareWindow _graph_window;

	private GraphController _graph_controller;

	private MultiBannerPool _pool_drop_banners;

	public NanoObject current_item;

	public GameObject empty_drop_icon;

	public LocalizedText meta_title;

	public Text meta_name;

	private IBanner _current_banner;

	public static bool disable_raycasts;

	private bool _disable_raycasts;

	private List<Graphic> _raycast_children = new List<Graphic>();

	private bool _initialized;

	public void Awake()
	{
		init();
	}

	private void init()
	{
		if (!_initialized)
		{
			_initialized = true;
			_graph_window = ((Component)this).GetComponentInParent<GraphCompareWindow>();
			_graph_controller = _graph_window.graph_controller;
			_pool_drop_banners = _graph_window.getDropBannerPool();
		}
	}

	public void OnEnable()
	{
		if (current_item == null)
		{
			empty_drop_icon.SetActive(true);
			meta_title.setKeyAndUpdate("graph_drop_to_compare");
			((Component)meta_name).gameObject.SetActive(false);
		}
	}

	public void Update()
	{
		if (_disable_raycasts != disable_raycasts)
		{
			_disable_raycasts = disable_raycasts;
			if (disable_raycasts)
			{
				disableRaycastChildren();
			}
			else
			{
				enableRaycastChildren();
			}
		}
	}

	public void disableRaycastChildren()
	{
		_raycast_children.Clear();
		Graphic[] componentsInChildren = ((Component)this).GetComponentsInChildren<Graphic>();
		foreach (Graphic val in componentsInChildren)
		{
			if (!((Object)(object)((Component)val).gameObject == (Object)(object)((Component)this).gameObject) && val.raycastTarget)
			{
				_raycast_children.Add(val);
				val.raycastTarget = false;
			}
		}
	}

	public void enableRaycastChildren()
	{
		foreach (Graphic raycast_child in _raycast_children)
		{
			raycast_child.raycastTarget = true;
		}
		_raycast_children.Clear();
	}

	public void OnDrop(PointerEventData pEventData)
	{
		if ((Object)(object)pEventData.pointerDrag == (Object)null)
		{
			return;
		}
		BannerBase component = pEventData.pointerDrag.GetComponent<BannerBase>();
		if (!((Object)(object)component == (Object)null))
		{
			GraphCompareMetaSelector component2 = pEventData.pointerDrag.GetComponent<GraphCompareMetaSelector>();
			if (!((Object)(object)component2 == (Object)null) && component2.isBeingDragged())
			{
				component2.OnEndDrag(pEventData);
				SoundBox.click();
				setObjectAndUpdate(component.GetNanoObject());
				((AbstractEventData)pEventData).Use();
			}
		}
	}

	public void empty()
	{
		init();
		clearObject();
		empty_drop_icon.SetActive(true);
	}

	public void clear()
	{
		init();
		Config.selected_objects_graph.Remove(current_item);
		clearObject();
		empty_drop_icon.SetActive(true);
	}

	public void clearAndSetObject(NanoObject pObject)
	{
		clear();
		setObject(pObject);
	}

	public void setObject(NanoObject pObject)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		if (!pObject.isRekt())
		{
			empty_drop_icon.SetActive(false);
			current_item = pObject;
			_current_banner = _graph_window.setupBanner(current_item, ((Component)this).transform, _pool_drop_banners);
			_current_banner.jump();
			((UnityEvent)_current_banner.GetComponent<Button>().onClick).AddListener(new UnityAction(removeOnClick));
			if (!Config.selected_objects_graph.Contains(current_item))
			{
				Config.selected_objects_graph.Add(current_item);
			}
			Color colorText = current_item.getColor().getColorText();
			((Graphic)meta_title.text).color = colorText;
			MetaCustomizationAsset asset = AssetManager.meta_customization_library.getAsset(current_item.getMetaType());
			meta_title.setKeyAndUpdate(asset.localization_title);
			((Component)meta_name).gameObject.SetActive(true);
			meta_name.text = current_item.name;
			((Graphic)meta_name).color = colorText;
		}
	}

	private void setObjectAndUpdate(NanoObject pObject)
	{
		string activeCategory = _graph_controller.getActiveCategory();
		clearAndSetObject(pObject);
		_graph_window.loadNoosItems(pSilent: true);
		_graph_controller.resetAndUpdateGraph();
		_graph_controller.tryEnableCategory(activeCategory);
	}

	private void removeOnClick()
	{
		SoundBox.click();
		if (!InputHelpers.mouseSupported && !Tooltip.isShowingFor(_current_banner))
		{
			_current_banner.showTooltip();
		}
		else
		{
			setObjectAndUpdate(null);
		}
	}

	private void clearObject()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (current_item != null)
		{
			releaseChild();
			current_item = null;
			((Graphic)meta_title.text).color = Toolbox.color_text_default;
			((Graphic)meta_name).color = Toolbox.color_text_default;
			((Component)meta_name).gameObject.SetActive(false);
		}
	}

	private void releaseChild()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		if (_current_banner != null)
		{
			((UnityEvent)_current_banner.GetComponent<Button>().onClick).RemoveListener(new UnityAction(removeOnClick));
			_pool_drop_banners.resetParent(_current_banner);
			_pool_drop_banners.release(_current_banner);
			_current_banner = null;
		}
	}
}
