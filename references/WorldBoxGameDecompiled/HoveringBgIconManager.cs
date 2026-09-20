using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HoveringBgIconManager : MonoBehaviour
{
	[SerializeField]
	private HoveringIcon _icon_prefab;

	private ObjectPoolGenericMono<HoveringIcon> _pool_icons;

	private CanvasGroup _canvas_group;

	private RectTransform _rect;

	private List<RectTransform> _places = new List<RectTransform>();

	[SerializeField]
	public bool _random_scale = true;

	[SerializeField]
	private Transform _icon_pool;

	[SerializeField]
	private Transform _icons;

	private static HoveringBgIconManager _instance;

	private void Awake()
	{
		if (_pool_icons == null)
		{
			_instance = this;
			_rect = ((Component)this).GetComponent<RectTransform>();
			_canvas_group = ((Component)this).GetComponent<CanvasGroup>();
			_pool_icons = new ObjectPoolGenericMono<HoveringIcon>(_icon_prefab, _icon_pool);
			for (int i = 0; i < _icons.childCount; i++)
			{
				Transform child = _icons.GetChild(i);
				RectTransform val = (RectTransform)(object)((child is RectTransform) ? child : null);
				_places.Add(val);
				((Object)((Component)val).gameObject).name = "Placing " + i;
			}
		}
	}

	private void OnDisable()
	{
		_pool_icons.clear();
	}

	public void fadeIn()
	{
		((Component)_icons).gameObject.SetActive(true);
		DOTweenModuleUI.DOFade(_canvas_group, 1f, 0.2f);
		_canvas_group.interactable = true;
		_canvas_group.blocksRaycasts = true;
	}

	public void fadeOut()
	{
		_canvas_group.interactable = false;
		_canvas_group.blocksRaycasts = false;
		DOTweenModuleUI.DOFade(_canvas_group, 0f, 0.2f);
		clear();
		resetPlaces();
		((Component)_icons).gameObject.SetActive(false);
	}

	private void resetPlaces()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if (!Randy.randomBool())
		{
			Rect rect = _rect.rect;
			float num = ((Rect)(ref rect)).width / 2f;
			rect = _rect.rect;
			float num2 = ((Rect)(ref rect)).height / 2f;
			Vector3 val = default(Vector3);
			((Vector3)(ref val))._002Ector(num, num2, 0f);
			for (int i = 0; i < _places.Count; i++)
			{
				RectTransform obj = _places[i];
				ShortcutExtensions.DOKill((Component)(object)obj, false);
				obj.anchoredPosition = Vector2.op_Implicit(val);
			}
		}
	}

	private void shufflePlaces()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		resetPlaces();
		Rect rect = _rect.rect;
		float width = ((Rect)(ref rect)).width;
		rect = _rect.rect;
		float height = ((Rect)(ref rect)).height;
		for (int i = 0; i < _places.Count; i++)
		{
			RectTransform obj = _places[i];
			float num = Randy.randomFloat(0.15f, 0.35f);
			DOTweenModuleUI.DOAnchorPos(obj, Vector2.op_Implicit(new Vector3(Randy.randomFloat(0f, width), Randy.randomFloat(0f, height), 0f)), num, false);
		}
	}

	public void animate(WindowAsset pWindowAsset)
	{
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		clear();
		shufflePlaces();
		float num = Randy.randomFloat(0f, 360f);
		string text = "ui/Icons/";
		using ListPool<string> listPool = new ListPool<string>(16);
		Delegate[] invocationList = pWindowAsset.get_hovering_icons.GetInvocationList();
		for (int i = 0; i < invocationList.Length; i++)
		{
			foreach (string item3 in ((HoveringBGIconsGetter)invocationList[i])(pWindowAsset))
			{
				if (item3.EndsWith("/"))
				{
					Sprite[] spriteList = SpriteTextureLoader.getSpriteList(text + item3);
					for (int j = 0; j < spriteList.Length; j++)
					{
						string item = text + item3 + ((Object)spriteList[j]).name;
						listPool.Add(item);
					}
				}
				else
				{
					string item2 = text + item3;
					listPool.Add(item2);
				}
			}
		}
		foreach (RectTransform place in _places)
		{
			string random = listPool.GetRandom();
			HoveringIcon next = _pool_icons.getNext();
			next.clear();
			((Component)next).transform.SetParent((Transform)(object)place, false);
			next.rect.anchoredPosition = Vector2.op_Implicit(Vector3.zero);
			((Component)next).transform.rotation = Quaternion.identity;
			next.image.sprite = SpriteTextureLoader.getSprite(random);
			if (_random_scale)
			{
				float num2 = Randy.randomFloat(0.4f, 1f);
				((Component)next).transform.localScale = new Vector3(num2, num2, num2);
			}
			else
			{
				((Component)next).transform.localScale = ((Transform)place).localScale;
			}
			Vector3 localScale = ((Component)next).transform.localScale;
			((Graphic)next.image).color = new Color(localScale.x, localScale.x, localScale.x, 1f);
			num += Randy.randomFloat(20f, 130f);
			((Component)next).transform.eulerAngles = new Vector3(0f, 0f, num);
			next.init();
		}
	}

	public static void show()
	{
		_instance.fadeIn();
	}

	public static void hide()
	{
		_instance.fadeOut();
	}

	public static void showWindow(WindowAsset pWindowAsset)
	{
		_instance.animate(pWindowAsset);
	}

	public static void dropAll()
	{
		foreach (HoveringIcon item in _instance._pool_icons.getListTotal())
		{
			if (((Component)item).gameObject.activeSelf)
			{
				UiCreature component = ((Component)item).GetComponent<UiCreature>();
				if (!component.dropped)
				{
					component.click();
				}
			}
		}
	}

	public static void randomDrop()
	{
		using ListPool<UiCreature> listPool = new ListPool<UiCreature>(_instance._pool_icons.countActive());
		foreach (HoveringIcon item in _instance._pool_icons.getListTotal())
		{
			if (((Component)item).gameObject.activeSelf)
			{
				UiCreature component = ((Component)item).GetComponent<UiCreature>();
				if (!component.dropped)
				{
					listPool.Add(component);
				}
			}
		}
		if (listPool.Count != 0)
		{
			listPool.GetRandom().click();
		}
	}

	private void clear()
	{
		_pool_icons.clear();
		_pool_icons.resetParent();
	}
}
