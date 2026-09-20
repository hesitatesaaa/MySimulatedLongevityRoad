using System.Collections.Generic;
using UnityEngine;

public class LibraryMaterials : MonoBehaviour
{
	public static LibraryMaterials instance;

	public const string mat_id_world_object = "mat_world_object";

	public const string mat_id_world_object_lit_always = "mat_world_object_lit";

	public Dictionary<string, Material> dict = new Dictionary<string, Material>();

	public Material mat_damaged;

	public Material mat_highlighted;

	public Material mat_world_object;

	public Material mat_world_object_lit;

	public Material mat_buildings;

	public Material mat_socialize;

	public Material mat_minis;

	public Material mat_tree;

	public Material mat_tree_celestial;

	public Material mat_jelly;

	public Material mat_buildings_light;

	public Material mat_lava_glow;

	public Material mat_overlapped_shadows;

	private float _shadow_alpha_target = 0.40392157f;

	private float _shadow_alpha = 0.40392157f;

	private Color _shadows_color;

	private List<Material> _night_affected_colors = new List<Material>();

	private float _time;

	private void Awake()
	{
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		instance = this;
		mat_damaged = loadMaterial("materials/damaged", pCopy: true);
		mat_highlighted = loadMaterial("materials/highlighted", pCopy: true);
		mat_buildings = loadMaterial("materials/building", pCopy: true);
		mat_tree = loadMaterial("materials/tree", pCopy: true);
		mat_socialize = loadMaterial("materials/socialize");
		mat_minis = loadMaterial("materials/minis");
		mat_tree_celestial = loadMaterial("materials/tree_celestial", pCopy: true);
		mat_jelly = loadMaterial("materials/jelly", pCopy: true);
		mat_overlapped_shadows = loadMaterial("materials/OverlappedShadows", pCopy: true);
		mat_buildings_light = loadMaterial("materials/MatBuildingsLight");
		mat_world_object = loadMaterial("materials/mat_world_object");
		mat_world_object_lit = loadMaterial("materials/mat_world_object_lit");
		mat_lava_glow = loadMaterial("materials/lava_glow", pCopy: true);
		_night_affected_colors.Add(mat_buildings);
		_night_affected_colors.Add(mat_tree);
		_night_affected_colors.Add(mat_jelly);
		_night_affected_colors.Add(mat_world_object);
		_shadows_color = mat_overlapped_shadows.GetColor("_Color");
		AssetManager.status.linkMaterials();
		Shader.SetGlobalFloat("GlobalTime", 1f);
	}

	private Material loadMaterial(string pPath, bool pCopy = false)
	{
		Material val = Resources.Load<Material>(pPath);
		if (pCopy)
		{
			val = Object.Instantiate<Material>(val);
		}
		string name = ((Object)val).name;
		name = name.Replace("(Clone)", "");
		dict.Add(name, val);
		return val;
	}

	internal void updateMat()
	{
		if (!World.world.isPaused())
		{
			_time += World.world.elapsed;
		}
		updateNight();
		Shader.SetGlobalFloat("GlobalTime", _time);
	}

	private void updateNight()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		float nightMod = World.world.era_manager.getNightMod();
		Color color = Toolbox.blendColor(Color32.op_Implicit(Toolbox.color_night), Toolbox.color_white, nightMod);
		foreach (Material night_affected_color in _night_affected_colors)
		{
			night_affected_color.color = color;
		}
		Color backgroundColor = Toolbox.blendColor(Color32.op_Implicit(Toolbox.color_night), Color32.op_Implicit(Toolbox.color_ocean), nightMod);
		if (nightMod > 0f)
		{
			backgroundColor.r -= 0.007843138f;
			backgroundColor.b -= 1f / 51f;
		}
		World.world.camera.backgroundColor = backgroundColor;
	}

	public void updateZoomoutValue(float pValue)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		float num = pValue;
		num = 4f - num * 3f;
		if (!DebugConfig.isOn(DebugOption.ScaleEffectEnabled))
		{
			num = 1f;
		}
		_shadow_alpha = num * _shadow_alpha_target;
		_shadows_color.a = _shadow_alpha;
		mat_overlapped_shadows.SetColor("_Color", _shadows_color);
	}
}
