using System.Runtime.CompilerServices;
using UnityEngine;

public class QualityChanger : MonoBehaviour
{
	private const float TIMER_SPEED_MULTIPLIER = 3.5f;

	private const float BUILDING_HIDE_VALUE = 0.3f;

	public const float BUILDING_SHADER_VALUE_BOUND = 4f;

	public const float BUILDING_SHADER_VALUE_BOUND_2 = 3f;

	private const float BOUND_RATE_SHADOWS_UNITS = 0.7f;

	private const float BOUND_RATE_SHADOWS_BUILDINGS = 0.85f;

	private const float BOUND_STRATOSPHERE_ORTHOGRAPHIC = 400f;

	private bool _low_resolution;

	private float _transition_animation_factor;

	private float _tween_buildings;

	private Color _color_alpha;

	private bool _render_buildings;

	private float _current_zoom_orthographic;

	private float _main_zoom;

	internal void reset()
	{
		setLowRes(pValue: true);
		_transition_animation_factor = 1f;
		if (Config.isMobile)
		{
			_main_zoom = 160f;
		}
		else
		{
			_main_zoom = 240f;
		}
		World.world.world_layer.setRendererEnabled(pBool: true);
	}

	internal void update()
	{
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		LibraryMaterials.instance.updateMat();
		if (isLowRes() && _tween_buildings < 0.3f)
		{
			_render_buildings = false;
		}
		else
		{
			_render_buildings = !isFullLowRes() && _tween_buildings != 0f;
		}
		if (_color_alpha.a != _transition_animation_factor || (_transition_animation_factor != 0f && _transition_animation_factor != 1f))
		{
			float num = iTween.easeInCirc(0f, 1f, _transition_animation_factor);
			_tween_buildings = 1f - num;
			LibraryMaterials.instance.updateZoomoutValue(_tween_buildings);
		}
		if (isLowRes() && _transition_animation_factor < 1f)
		{
			_transition_animation_factor += Time.deltaTime * 3.5f;
			if (_transition_animation_factor > 1f)
			{
				_transition_animation_factor = 1f;
				_tween_buildings = 0f;
			}
		}
		else if (!isLowRes() && _transition_animation_factor > 0f)
		{
			_transition_animation_factor -= Time.deltaTime * 3.5f;
			if (_transition_animation_factor < 0f)
			{
				_transition_animation_factor = 0f;
				_tween_buildings = 1f;
			}
		}
		if (!PlayerConfig.optionBoolEnabled("minimap_transition_animation"))
		{
			if (isLowRes())
			{
				_transition_animation_factor = 1f;
				_tween_buildings = 0f;
			}
			else
			{
				_transition_animation_factor = 0f;
				_tween_buildings = 1f;
			}
			setZoomOrthographic(MoveCamera.instance.main_camera.orthographicSize);
		}
		_color_alpha.a = _transition_animation_factor;
		Color color_alpha = _color_alpha;
		if (isLowRes())
		{
			color_alpha.a = Mathf.Max(0.9f, color_alpha.a);
		}
		World.world.world_layer.sprRnd.color = color_alpha;
		World.world.world_layer_edges.sprRnd.color = _color_alpha;
		World.world.unit_layer.sprRnd.color = _color_alpha;
		World.world.tilemap.checkEnableForWaterRunups(isLowRes());
		if (isLowRes())
		{
			World.world.world_layer.setRendererEnabled(pBool: true);
			if (((Component)World.world.tilemap).gameObject.activeSelf)
			{
				Color color = World.world._world_layer_switch_effect.color;
				color.a = 0.1f;
				World.world._world_layer_switch_effect.color = color;
			}
			World.world.tilemap.enableTiles(pValue: false);
		}
		else
		{
			World.world.tilemap.enableTiles(pValue: true);
			if (_transition_animation_factor == 0f)
			{
				World.world.world_layer.setRendererEnabled(pBool: false);
			}
		}
	}

	public bool isFullLowRes()
	{
		if (isLowRes())
		{
			return _transition_animation_factor == 1f;
		}
		return false;
	}

	public bool shouldRenderUnitShadows()
	{
		if (TrailerMonolith.enable_trailer_stuff)
		{
			return true;
		}
		if (!Config.shadows_active)
		{
			return false;
		}
		if (isLowRes())
		{
			return false;
		}
		if (isZoomLevelWithinUnitShadows())
		{
			return true;
		}
		return false;
	}

	public bool isZoomLevelWithinUnitShadows()
	{
		if (_current_zoom_orthographic < getZoomBoundUnitShadows())
		{
			return true;
		}
		return false;
	}

	public float getZoomRateBoundLow()
	{
		return getCameraMultiplier(_main_zoom);
	}

	public float getZoomRateShadows()
	{
		return getCameraMultiplier(_main_zoom * 0.85f);
	}

	public bool shouldRenderBuildingShadows()
	{
		if (TrailerMonolith.enable_trailer_stuff)
		{
			return true;
		}
		if (!Config.shadows_active)
		{
			return false;
		}
		if (World.world.camera.orthographicSize > getZoomRateShadows())
		{
			return false;
		}
		return _render_buildings;
	}

	public float getZoomBoundUnitShadows()
	{
		return getCameraMultiplier(_main_zoom * 0.7f);
	}

	private float getCameraMultiplier(float pVal)
	{
		if (!DebugConfig.isOn(DebugOption.UseCameraAspect))
		{
			return pVal;
		}
		int pixelWidth = World.world.camera.pixelWidth;
		int pixelHeight = World.world.camera.pixelHeight;
		float aspect = World.world.camera.aspect;
		float num = pixelHeight;
		if (World.world.camera.pixelWidth > pixelHeight)
		{
			num = pixelWidth;
		}
		if (aspect > 2f)
		{
			num /= aspect * 0.5f;
		}
		pVal *= (float)pixelHeight / num * 0.5f;
		return pVal;
	}

	internal void setZoomOrthographic(float pZoom)
	{
		_current_zoom_orthographic = pZoom;
		bool flag = _current_zoom_orthographic > getZoomRateBoundLow();
		if (flag != isLowRes())
		{
			setLowRes(flag);
			if (isLowRes())
			{
				MusicBox.playSound("event:/SFX/MAP/BitScaleWhooshIn");
			}
			else
			{
				MusicBox.playSound("event:/SFX/MAP/BitScaleWhooshOut");
			}
			World.world.zone_calculator.clearCurrentDrawnZones();
			World.world.resetRedrawTimer();
		}
	}

	public float getZoomRatioLow()
	{
		float num = _current_zoom_orthographic - 10f;
		float num2 = getZoomRateBoundLow() - 10f;
		return Mathf.Clamp(num / num2, 0f, 1f);
	}

	public float getZoomRatioHigh()
	{
		if (getZoomRatioLow() < 1f)
		{
			return 0f;
		}
		float num = _current_zoom_orthographic - getZoomRateBoundLow();
		float num2 = 400f - getZoomRateBoundLow();
		return Mathf.Clamp(num / num2, 0f, 1f);
	}

	public float getZoomRatioFull()
	{
		float num = _current_zoom_orthographic - 10f;
		float num2 = 400f;
		return Mathf.Clamp(num / num2, 0f, 1f);
	}

	private void setLowRes(bool pValue)
	{
		_low_resolution = pValue;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool shouldRenderBuildings()
	{
		return _render_buildings;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool isLowRes()
	{
		return _low_resolution;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float getTweenBuildingsValue()
	{
		return _tween_buildings;
	}

	public void debug(DebugTool pTool)
	{
		pTool.setText("Zoom_Low:", getZoomRatioLow(), 0f, pShowBar: false, 0L);
		pTool.setText("Zoom_High:", getZoomRatioHigh(), 0f, pShowBar: false, 0L);
		pTool.setText("Zoom_Full:", getZoomRatioFull(), 0f, pShowBar: false, 0L);
		pTool.setSeparator();
		pTool.setText("lowRes", isLowRes(), 0f, pShowBar: false, 0L);
		pTool.setText("_timer_animation", _transition_animation_factor, 0f, pShowBar: false, 0L);
		pTool.setText("getTweenBuildinsValue", getTweenBuildingsValue(), 0f, pShowBar: false, 0L);
		pTool.setText("isBuildingRendered", shouldRenderBuildings(), 0f, pShowBar: false, 0L);
		pTool.setText("_current_zoom", _current_zoom_orthographic, 0f, pShowBar: false, 0L);
		pTool.setText("_main_zoom", _main_zoom, 0f, pShowBar: false, 0L);
		pTool.setText("camera_zoom_max", World.world.move_camera.orthographic_size_max, 0f, pShowBar: false, 0L);
		pTool.setText("camera_ortho", World.world.move_camera.main_camera.orthographicSize, 0f, pShowBar: false, 0L);
		pTool.setText("camera_zoom_min", 10f, 0f, pShowBar: false, 0L);
		pTool.setText("BOUND_STRATOSPHERE_ORTHOGRAPHIC", 400f, 0f, pShowBar: false, 0L);
		pTool.setText("isFullLowRes()", isFullLowRes(), 0f, pShowBar: false, 0L);
		pTool.setText("renderShadowsUnits()", shouldRenderUnitShadows(), 0f, pShowBar: false, 0L);
	}

	public QualityChanger()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		_transition_animation_factor = 1f;
		_color_alpha = new Color(1f, 1f, 1f);
		((MonoBehaviour)this)._002Ector();
	}
}
