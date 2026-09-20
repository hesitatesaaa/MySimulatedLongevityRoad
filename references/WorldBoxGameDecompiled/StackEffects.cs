using System.Collections.Generic;
using UnityEngine;

public class StackEffects : BaseMapObject
{
	public static double last_thunder_timestamp;

	public BaseEffectController prefabController;

	private Dictionary<string, BaseEffectController> dictionary;

	internal List<BaseEffectController> list;

	public List<LightBlobData> light_blobs = new List<LightBlobData>();

	public List<PlotIconData> plot_removals = new List<PlotIconData>();

	public List<ActorDamageEffectData> actor_effect_hit = new List<ActorDamageEffectData>();

	public List<ActorHighlightEffectData> actor_effect_highlight = new List<ActorHighlightEffectData>();

	public BaseEffectController controller_slash_effects;

	public BaseEffectController controller_tile_effects;

	internal override void create()
	{
		base.create();
		dictionary = new Dictionary<string, BaseEffectController>();
		list = new List<BaseEffectController>();
		checkInit();
		controller_slash_effects = get("fx_slash");
		controller_tile_effects = get("fx_tile_effect");
	}

	private void checkInit()
	{
		foreach (EffectAsset item in AssetManager.effects_library.list)
		{
			if (!dictionary.ContainsKey(item.id))
			{
				add(item);
			}
		}
	}

	internal int countActive()
	{
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			BaseEffectController baseEffectController = list[i];
			num += baseEffectController.getActiveIndex();
		}
		return num;
	}

	internal bool isLocked()
	{
		if (get("fx_spawn_big").getActiveIndex() > 0)
		{
			return true;
		}
		return false;
	}

	internal BaseEffectController get(string pID)
	{
		return dictionary[pID];
	}

	private BaseEffectController add(EffectAsset pAsset)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		GameObject val = null;
		string text = ((!pAsset.use_basic_prefab) ? pAsset.prefab_id : "effects/prefabs/PrefabEffectBasic");
		val = Object.Instantiate<GameObject>((GameObject)Resources.Load(text, typeof(GameObject)), ((Component)this).transform);
		((Object)val.transform).name = "[base] " + pAsset.id;
		val.gameObject.SetActive(false);
		if (pAsset.use_basic_prefab || pAsset.load_texture)
		{
			SpriteAnimation component = val.GetComponent<SpriteAnimation>();
			component.timeBetweenFrames = pAsset.time_between_frames;
			component.frames = SpriteTextureLoader.getSpriteList(pAsset.sprite_path);
			if (component.frames == null || component.frames.Length == 0)
			{
				Debug.LogError((object)("NO SPRITES FOR EFFECT " + pAsset.id + " " + pAsset.sprite_path));
			}
			((Renderer)component.spriteRenderer).sortingLayerName = pAsset.sorting_layer_id;
		}
		BaseEffectController baseEffectController = Object.Instantiate<BaseEffectController>(prefabController, ((Component)this).transform, true);
		baseEffectController.create();
		baseEffectController.asset = pAsset;
		((Object)((Component)baseEffectController).transform).name = "[controller] " + pAsset.id;
		baseEffectController.prefab = val.transform;
		baseEffectController.setLimits(pAsset.limit, pAsset.limit_unload);
		dictionary.Add(pAsset.id, baseEffectController);
		list.Add(baseEffectController);
		baseEffectController.addNewObject(val.GetComponent<BaseEffect>());
		return baseEffectController;
	}

	internal void clear()
	{
		for (int i = 0; i < list.Count; i++)
		{
			list[i].clear();
		}
		last_thunder_timestamp = 0.0;
		light_blobs.Clear();
		plot_removals.Clear();
		actor_effect_hit.Clear();
		actor_effect_highlight.Clear();
	}

	public override void update(float pElapsed)
	{
		if (AssetManager.effects_library.list.Count > list.Count)
		{
			checkInit();
		}
		Bench.bench("stack_effects", "game_total");
		for (int i = 0; i < list.Count; i++)
		{
			list[i].update(pElapsed);
		}
		Bench.benchEnd("stack_effects", "game_total", pSaveCounter: false, 0L);
	}
}
