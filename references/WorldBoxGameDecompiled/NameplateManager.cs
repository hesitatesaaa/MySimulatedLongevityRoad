using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NameplateManager : MonoBehaviour
{
	private readonly Stack<NameplateText> _pool = new Stack<NameplateText>();

	private readonly List<NameplateText> _active = new List<NameplateText>();

	private int _next_index;

	public NameplateText prefab;

	private Canvas _canvas;

	internal CanvasScaler canvas_scaler;

	internal RectTransform canvas_rect;

	internal Vector2 canvas_size_delta;

	internal float canvas_size_delta_mod_x;

	internal float canvas_size_delta_mod_y;

	private MetaType _last_mode;

	public NameplateText cursor_over_text;

	private int _latest_touch_id;

	private bool _touch_released;

	private float _tween_timer;

	private float _tween_scale;

	internal bool cached_favorites_only;

	internal float cached_canvas_scale;

	private NameplateRenderingType _nameplate_mode;

	private bool _nano_object_set;

	private NanoObject _selected_nano_object;

	private void Awake()
	{
		_canvas = ((Component)this).GetComponent<Canvas>();
		canvas_rect = ((Component)_canvas).GetComponent<RectTransform>();
		canvas_scaler = ((Component)this).GetComponent<CanvasScaler>();
	}

	private void prepare()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		_next_index = 0;
		_nameplate_mode = ((PlayerConfig.getOptionInt("map_names") == 0) ? NameplateRenderingType.Full : NameplateRenderingType.BannerOnly);
		_nano_object_set = SelectedObjects.isNanoObjectSet();
		_selected_nano_object = SelectedObjects.getSelectedNanoObject();
		cached_favorites_only = PlayerConfig.optionBoolEnabled("only_favorited_meta");
		canvas_size_delta = canvas_rect.sizeDelta;
		cached_canvas_scale = canvas_scaler.scaleFactor;
		canvas_size_delta_mod_x = canvas_size_delta.x * 0.5f;
		canvas_size_delta_mod_y = canvas_size_delta.y * 0.5f;
	}

	internal void update()
	{
		Bench.bench("nameplates", "nameplates_total");
		Bench.bench("prepare", "nameplates");
		prepare();
		Bench.benchEnd("prepare", "nameplates", pSaveCounter: false, 0L);
		Bench.bench("check_mode", "nameplates");
		MetaType currentMode = getCurrentMode();
		setMode(currentMode);
		NameplateAsset nameplateAsset = null;
		MetaTypeAsset metaTypeAsset = null;
		if (!currentMode.isNone())
		{
			nameplateAsset = AssetManager.nameplates_library.map_modes_nameplates[currentMode];
			metaTypeAsset = currentMode.getAsset();
		}
		Bench.benchEnd("check_mode", "nameplates", pSaveCounter: false, 0L);
		Bench.bench("set_nameplates", "nameplates");
		if (CanvasMain.isNameplatesAllowed())
		{
			if (currentMode == MetaType.None)
			{
				if (((Component)this).gameObject.activeSelf)
				{
					((Component)this).gameObject.SetActive(false);
				}
			}
			else
			{
				if (!((Component)this).gameObject.activeSelf)
				{
					((Component)this).gameObject.SetActive(true);
				}
				nameplateAsset.action_main(this, nameplateAsset);
			}
		}
		Bench.benchEnd("set_nameplates", "nameplates", pSaveCounter: false, _active.Count);
		Bench.bench("updateOverlappingPositions", "nameplates");
		bool flag = false;
		if (!currentMode.isNone() && metaTypeAsset != null)
		{
			if (metaTypeAsset.isMetaZoneOptionSelectedFluid())
			{
				if (nameplateAsset.overlap_for_fluid_mode)
				{
					flag = true;
				}
			}
			else
			{
				flag = true;
			}
		}
		if (flag)
		{
			updateOverlappingPosition();
		}
		Bench.benchEnd("updateOverlappingPositions", "nameplates", pSaveCounter: false, 0L);
		Bench.bench("updateTweenScale", "nameplates");
		updateTweenScale();
		Bench.benchEnd("updateTweenScale", "nameplates", pSaveCounter: false, 0L);
		Bench.bench("checkActive", "nameplates");
		checkActive();
		Bench.benchEnd("checkActive", "nameplates", pSaveCounter: false, 0L);
		Bench.bench("findObjectForTooltip", "nameplates");
		NanoObject nanoObject = findObjectForTooltip();
		Bench.benchEnd("findObjectForTooltip", "nameplates", pSaveCounter: false, 0L);
		Bench.bench("showTooltip", "nameplates");
		nanoObject?.getMetaTypeAsset().cursor_tooltip_action(nanoObject);
		Bench.benchEnd("showTooltip", "nameplates", pSaveCounter: false, 0L);
		Bench.bench("check_siblings", "nameplates");
		checkSiblingsToFront();
		Bench.benchEnd("check_siblings", "nameplates", pSaveCounter: false, 0L);
		Bench.bench("finale", "nameplates");
		finale();
		Bench.benchEnd("finale", "nameplates", pSaveCounter: false, 0L);
		Bench.benchEnd("nameplates", "nameplates_total", pSaveCounter: false, 0L);
	}

	private void checkSiblingsToFront()
	{
		if ((Object)(object)cursor_over_text != (Object)null)
		{
			((Component)cursor_over_text).transform.SetAsLastSibling();
		}
		if (!SelectedObjects.isNanoObjectSet())
		{
			return;
		}
		foreach (NameplateText item in _active)
		{
			if (item.nano_object == SelectedObjects.getSelectedNanoObject())
			{
				((Component)item).transform.SetAsLastSibling();
				break;
			}
		}
	}

	private void checkActive()
	{
		for (int num = _next_index - 1; num >= 0; num--)
		{
			_active[num].checkActive();
		}
	}

	private void updateTweenScale()
	{
		_tween_timer += Time.deltaTime * 2f;
		_tween_timer = Mathf.Clamp(_tween_timer, 0f, 1f);
		float num = iTween.easeOutBack(0f, 1f, _tween_timer);
		num *= 0.5f;
		_tween_scale = num;
	}

	private NanoObject findObjectForTooltip()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		cursor_over_text = null;
		if (World.world.isBusyWithUI())
		{
			return null;
		}
		if (ControllableUnit.isControllingUnit())
		{
			return null;
		}
		if (!InputHelpers.mouseSupported && !checkTouch(out var pPosition))
		{
			return null;
		}
		pPosition = World.world.getMousePos();
		Vector2 val = Vector2.op_Implicit(World.world.camera.WorldToScreenPoint(Vector2.op_Implicit(pPosition)));
		bool mouseSupported = InputHelpers.mouseSupported;
		NanoObject result = null;
		float num = float.MaxValue;
		NameplateText nameplateText = null;
		for (int i = 0; i < _active.Count; i++)
		{
			NameplateText nameplateText2 = _active[i];
			if (nameplateText2.isShowing())
			{
				Vector2 lastScreenPosition = nameplateText2.getLastScreenPosition();
				float num2 = Toolbox.SquaredDist(lastScreenPosition.x, lastScreenPosition.y, val.x, val.y);
				if (((Rect)(ref nameplateText2.map_text_rect_click)).Contains(val) && (!((Object)(object)nameplateText != (Object)null) || (!(num2 > num) && !(num2 > 625f))))
				{
					nameplateText = nameplateText2;
					num = num2;
				}
			}
		}
		if ((Object)(object)nameplateText != (Object)null)
		{
			NanoObject nano_object = nameplateText.nano_object;
			if (Input.mousePresent)
			{
				result = nano_object;
			}
			cursor_over_text = nameplateText;
			Vector3 localScale = ((Component)nameplateText).transform.localScale;
			localScale *= 1.1f;
			cursor_over_text.forceScale(localScale);
			if ((nano_object is IMetaObject) & mouseSupported)
			{
				((IMetaObject)nano_object).setCursorOver();
			}
		}
		return result;
	}

	public bool isOverNameplate()
	{
		return (Object)(object)cursor_over_text != (Object)null;
	}

	private bool checkTouch(out Vector2 pPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Invalid comparison between Unknown and I4
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		pPosition = Vector2.op_Implicit(Globals.POINT_IN_VOID);
		if (Input.touchCount == 0)
		{
			return false;
		}
		Touch val = Input.touches[0];
		if ((int)((Touch)(ref val)).phase == 0 && _touch_released)
		{
			_latest_touch_id = ((Touch)(ref val)).fingerId;
			_touch_released = false;
			return false;
		}
		if (((Touch)(ref val)).fingerId != _latest_touch_id || (int)((Touch)(ref val)).phase != 3 || _touch_released)
		{
			return false;
		}
		_touch_released = true;
		pPosition = Vector2.op_Implicit(World.world.camera.ScreenToWorldPoint(Vector2.op_Implicit(((Touch)(ref val)).position)));
		return true;
	}

	private MetaType getCurrentMode()
	{
		MetaType metaType = MetaType.None;
		if (Zones.showMapNames())
		{
			if (!Zones.hasPowerForceMapMode())
			{
				metaType = Zones.getCurrentMapBorderMode();
				if (metaType.isNone())
				{
					metaType = MetaType.City;
				}
			}
			else
			{
				metaType = Zones.getForcedMapMode();
			}
		}
		return metaType;
	}

	private void setMode(MetaType pMode)
	{
		if (_last_mode != pMode)
		{
			_last_mode = pMode;
			clearAll();
		}
	}

	private void updateOverlappingPosition()
	{
		if (_next_index <= 0)
		{
			return;
		}
		using ListPool<NameplateText> listPool = new ListPool<NameplateText>(_next_index);
		for (int i = 0; i < _next_index; i++)
		{
			NameplateText item = _active[i];
			listPool.Add(item);
		}
		if (listPool.Count <= 1)
		{
			return;
		}
		listPool.Sort(compareNameplates);
		using ListPool<NameplateText> listPool2 = new ListPool<NameplateText>(_next_index);
		foreach (ref NameplateText item2 in listPool)
		{
			NameplateText current = item2;
			bool flag = false;
			foreach (ref NameplateText item3 in listPool2)
			{
				NameplateText current2 = item3;
				if (current.overlapsWithOtherPlate(current2))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				current.setShowing(pVal: false);
			}
			else
			{
				listPool2.Add(current);
			}
		}
	}

	private void OnDrawGizmos()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		Camera main = Camera.main;
		if (!((Object)(object)main == (Object)null))
		{
			Vector3 val = default(Vector3);
			Vector3 val2 = default(Vector3);
			Vector3 val6 = default(Vector3);
			for (int i = 0; i < _next_index; i++)
			{
				NameplateText nameplateText = _active[i];
				Rect map_text_rect_overlap = nameplateText.map_text_rect_overlap;
				((Vector3)(ref val))._002Ector(((Rect)(ref map_text_rect_overlap)).xMin, ((Rect)(ref map_text_rect_overlap)).yMin, main.nearClipPlane);
				((Vector3)(ref val2))._002Ector(((Rect)(ref map_text_rect_overlap)).xMax, ((Rect)(ref map_text_rect_overlap)).yMax, main.nearClipPlane);
				Vector3 val3 = main.ScreenToWorldPoint(val);
				Vector3 val4 = main.ScreenToWorldPoint(val2);
				Vector3 val5 = (val3 + val4) * 0.5f;
				((Vector3)(ref val6))._002Ector(val4.x - val3.x, val4.y - val3.y, 0.1f);
				Gizmos.color = (nameplateText.isShowing() ? Color.green : Color.red);
				Gizmos.DrawWireCube(val5, val6);
			}
		}
	}

	private int compareNameplates(NameplateText pText1, NameplateText pText2)
	{
		NanoObject selectedNanoObject = SelectedObjects.getSelectedNanoObject();
		bool flag = pText1.nano_object == selectedNanoObject;
		bool flag2 = pText2.nano_object == selectedNanoObject;
		if (flag != flag2)
		{
			return (flag2 ? 1 : 0) - (flag ? 1 : 0);
		}
		if (pText1.favorited != pText2.favorited)
		{
			return (pText2.favorited ? 1 : 0) - (pText1.favorited ? 1 : 0);
		}
		if (pText1.priority_capital != pText2.priority_capital)
		{
			return (pText2.priority_capital ? 1 : 0) - (pText1.priority_capital ? 1 : 0);
		}
		int num = pText2.priority_population.CompareTo(pText1.priority_population);
		if (num != 0)
		{
			return num;
		}
		return pText1.nano_object.id.CompareTo(pText2.nano_object.id);
	}

	public NameplateText prepareNext(NameplateAsset pAsset, NanoObject pMeta)
	{
		NameplateText nameplateToRender = getNameplateToRender();
		nameplateToRender.prepare(pAsset, pMeta, _tween_scale, _nameplate_mode, _nano_object_set, _selected_nano_object);
		return nameplateToRender;
	}

	private NameplateText getNameplateToRender()
	{
		NameplateText nameplateText;
		if (_active.Count > _next_index)
		{
			nameplateText = _active[_next_index];
		}
		else
		{
			nameplateText = ((_pool.Count != 0) ? _pool.Pop() : createNew());
			_active.Add(nameplateText);
		}
		_next_index++;
		return nameplateText;
	}

	protected virtual NameplateText createNew()
	{
		NameplateText nameplateText = Object.Instantiate<NameplateText>(prefab, ((Component)this).transform);
		nameplateText.newNameplate(this, $"map text {_pool.Count + _active.Count}");
		return nameplateText;
	}

	internal void clearAll()
	{
		_tween_timer = 0.5f;
		_tween_scale = 0f;
		if (_active.Count != 0)
		{
			for (int i = 0; i < _active.Count; i++)
			{
				NameplateText nameplateText = _active[i];
				nameplateText.clearFull();
				((Component)nameplateText).gameObject.SetActive(false);
				_pool.Push(nameplateText);
			}
			_active.Clear();
		}
	}

	private void finale()
	{
		clearLast();
	}

	public void clearCaches()
	{
		foreach (NameplateText item in _active)
		{
			item.clearCaches();
		}
	}

	public void clearLast()
	{
		int num = _active.Count - _next_index;
		if (num > 0)
		{
			while (num > 0)
			{
				int index = _active.Count - 1;
				NameplateText nameplateText = _active[index];
				nameplateText.clearFull();
				((Component)nameplateText).gameObject.SetActive(false);
				_active.RemoveAt(index);
				_pool.Push(nameplateText);
				num--;
			}
		}
	}
}
