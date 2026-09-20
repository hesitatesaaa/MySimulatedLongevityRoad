using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class WindowListElementBase<TMetaObject, TData> : MonoBehaviour, IPointerMoveHandler, IEventSystemHandler where TMetaObject : CoreSystemObject<TData> where TData : BaseSystemData
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static Func<Transform, bool> _003C_003E9__6_0;

		public static UnityAction _003C_003E9__7_0;

		internal bool _003CinitMonoFields_003Eb__6_0(Transform p)
		{
			return ((Component)p).gameObject.activeInHierarchy;
		}

		internal void _003CinitTooltip_003Eb__7_0()
		{
			Tooltip.hideTooltip();
		}
	}

	[HideInInspector]
	public TMetaObject meta_object;

	[SerializeField]
	private BannerGeneric<TMetaObject, TData> _main_banner;

	[SerializeField]
	private GameObject _icon_favorite;

	[SerializeField]
	private Image _icon_species;

	private void Awake()
	{
		create();
	}

	private void create()
	{
		initMonoFields();
		initTooltip();
	}

	protected virtual void initMonoFields()
	{
		if ((Object)(object)_main_banner == (Object)null)
		{
			BannerGeneric<TMetaObject, TData>[] array = ((Component)this).gameObject.transform.FindAllRecursive<BannerGeneric<TMetaObject, TData>>((Transform p) => ((Component)p).gameObject.activeInHierarchy);
			if (array.Length == 1)
			{
				_main_banner = array[0];
			}
			else
			{
				Debug.LogError((object)("WindowListElementBase: Failed to auto-find main banner. Assign manually. Found : " + array.Length + " of type " + typeof(BannerGeneric<TMetaObject, TData>)));
			}
		}
	}

	private void initTooltip()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		Button component = ((Component)this).GetComponent<Button>();
		object obj = _003C_003Ec._003C_003E9__7_0;
		if (obj == null)
		{
			UnityAction val = delegate
			{
				Tooltip.hideTooltip();
			};
			_003C_003Ec._003C_003E9__7_0 = val;
			obj = (object)val;
		}
		component.OnHoverOut((UnityAction)obj);
	}

	public void click()
	{
		if (!InputHelpers.mouseSupported && !Tooltip.isShowingFor(this))
		{
			tooltipAction();
			return;
		}
		MetaType metaType = meta_object.getMetaType();
		MetaTypeAsset asset = AssetManager.meta_type_library.getAsset(metaType);
		asset.set_selected(meta_object);
		if (asset.get_selected() != null)
		{
			ScrollWindow.showWindow(asset.window_name);
		}
	}

	internal virtual void show(TMetaObject pObject)
	{
		meta_object = pObject;
		loadBanner();
		toggleFavorited(meta_object.isFavorite());
		if ((Object)(object)_icon_species != (Object)null)
		{
			_icon_species.sprite = getActorAsset().getSpriteIcon();
		}
	}

	protected virtual void loadBanner()
	{
		_main_banner.load(meta_object);
	}

	protected virtual void tooltipAction()
	{
		throw new NotImplementedException();
	}

	public void toggleFavorited(bool pState)
	{
		if ((Object)(object)_icon_favorite != (Object)null)
		{
			_icon_favorite.SetActive(pState);
		}
	}

	protected virtual void OnDisable()
	{
		meta_object = null;
	}

	public void OnPointerMove(PointerEventData pData)
	{
		if (InputHelpers.mouseSupported && !Tooltip.anyActive())
		{
			tooltipAction();
		}
	}

	protected virtual ActorAsset getActorAsset()
	{
		throw new NotImplementedException();
	}
}
