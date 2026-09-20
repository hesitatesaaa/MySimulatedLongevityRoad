using UnityEngine;

public class CloudLibrary : AssetLibrary<CloudAsset>
{
	private static string[] _sprites_small = new string[2] { "effects/clouds/cloud_small_1", "effects/clouds/cloud_small_2" };

	private static string[] _sprites_big = new string[3] { "effects/clouds/cloud_big_1", "effects/clouds/cloud_big_2", "effects/clouds/cloud_big_3" };

	private static string[] _sprites_all = new string[5] { "effects/clouds/cloud_small_1", "effects/clouds/cloud_small_2", "effects/clouds/cloud_big_1", "effects/clouds/cloud_big_2", "effects/clouds/cloud_big_3" };

	public override void init()
	{
		base.init();
		add(new CloudAsset
		{
			id = "cloud_rain",
			color_hex = "#5D728C",
			drop_id = "rain",
			cloud_action_1 = dropAction,
			path_sprites = _sprites_big,
			speed_max = 4f
		});
		add(new CloudAsset
		{
			id = "cloud_rage",
			color_hex = "#FF3030",
			drop_id = "rage",
			cloud_action_1 = dropAction,
			path_sprites = _sprites_big,
			speed_max = 4f,
			considered_disaster = true,
			draw_light_area = true
		});
		add(new CloudAsset
		{
			id = "cloud_lightning",
			color_hex = "#445366",
			drop_id = "rain",
			cloud_action_1 = dropAction,
			cloud_action_2 = spawnLightning,
			path_sprites = _sprites_big,
			speed_max = 4f,
			considered_disaster = true
		});
		add(new CloudAsset
		{
			id = "cloud_acid",
			color_hex = "#17D41C",
			drop_id = "acid",
			cloud_action_1 = dropAction,
			path_sprites = _sprites_big,
			speed_max = 4f,
			considered_disaster = true
		});
		add(new CloudAsset
		{
			id = "cloud_lava",
			color_hex = "#D17119",
			drop_id = "lava",
			cloud_action_1 = dropAction,
			path_sprites = _sprites_big,
			speed_max = 3f,
			considered_disaster = true,
			draw_light_area = true
		});
		add(new CloudAsset
		{
			id = "cloud_fire",
			color_hex = "#D14219",
			drop_id = "fire",
			cloud_action_1 = dropAction,
			path_sprites = _sprites_big,
			speed_max = 3f,
			considered_disaster = true,
			draw_light_area = true
		});
		add(new CloudAsset
		{
			id = "cloud_snow",
			color_hex = "#B8FFFA",
			drop_id = "snow",
			cloud_action_1 = dropAction,
			path_sprites = _sprites_big,
			considered_disaster = true,
			speed_max = 4f
		});
		add(new CloudAsset
		{
			id = "cloud_ash",
			color_hex = "#C6B077",
			drop_id = "ash",
			cloud_action_1 = dropAction,
			path_sprites = _sprites_big,
			considered_disaster = true,
			speed_max = 4f
		});
		add(new CloudAsset
		{
			id = "cloud_magic",
			color_hex = "#C976CC",
			drop_id = "magic_rain",
			cloud_action_1 = dropAction,
			path_sprites = _sprites_small,
			considered_disaster = true,
			speed_max = 7f,
			draw_light_area = true
		});
		add(new CloudAsset
		{
			id = "cloud_normal",
			max_alpha = 0.5f,
			interval_action_1 = 2f,
			drop_id = "life_seed",
			cloud_action_1 = dropAction,
			path_sprites = _sprites_all,
			speed_min = 2f,
			normal_cloud = true
		});
	}

	public override void linkAssets()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		base.linkAssets();
		using ListPool<Sprite> listPool = new ListPool<Sprite>();
		foreach (CloudAsset item in list)
		{
			if (item.color_hex != null)
			{
				item.color = Toolbox.makeColor(item.color_hex);
			}
			listPool.Clear();
			string[] path_sprites = item.path_sprites;
			foreach (string text in path_sprites)
			{
				Sprite sprite = SpriteTextureLoader.getSprite(text);
				if ((Object)(object)sprite == (Object)null)
				{
					BaseAssetLibrary.logAssetError("cloud sprite not found", text);
				}
				else
				{
					listPool.Add(sprite);
				}
			}
			item.cached_sprites = listPool.ToArray();
		}
	}

	public static void dropAction(Cloud pCloud)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (!(pCloud.alive_time < 3f))
		{
			float effect_texture_width = pCloud.effect_texture_width;
			float effect_texture_height = pCloud.effect_texture_height;
			int num = (int)((Component)pCloud).transform.localPosition.x;
			int num2 = (int)((Component)pCloud).transform.localPosition.y;
			num += (int)Randy.randomFloat(0f - effect_texture_width, effect_texture_width);
			num2 += (int)Randy.randomFloat(0f - effect_texture_height + pCloud.spriteShadow.offset.y, effect_texture_height + pCloud.spriteShadow.offset.y);
			WorldTile tile = World.world.GetTile(num, num2);
			if (tile != null)
			{
				World.world.drop_manager.spawn(tile, pCloud.asset.drop_id, 10f, -1f, -1L);
			}
		}
	}

	public static void spawnLightning(Cloud pCloud)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (!Randy.randomChance(0.01f))
		{
			return;
		}
		int num = (int)((Component)pCloud).transform.localPosition.x;
		int num2 = (int)((Component)pCloud).transform.localPosition.y;
		float effect_texture_width = pCloud.effect_texture_width;
		float effect_texture_height = pCloud.effect_texture_height;
		num += (int)Randy.randomFloat(effect_texture_width * 0.5f, effect_texture_width);
		num2 += (int)Randy.randomFloat(0f - effect_texture_height + pCloud.spriteShadow.offset.y, effect_texture_height + pCloud.spriteShadow.offset.y);
		WorldTile tile = World.world.GetTile(num, num2);
		if (tile != null)
		{
			if (Randy.randomBool())
			{
				MapBox.spawnLightningMedium(tile, 0.15f);
			}
			else
			{
				MapBox.spawnLightningSmall(tile, 0.15f);
			}
		}
	}
}
