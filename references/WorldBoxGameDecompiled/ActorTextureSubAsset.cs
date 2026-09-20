using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
public class ActorTextureSubAsset : ICloneable
{
	[DefaultValue("male_1")]
	public const string skin_civ_default_male = "male_1";

	[DefaultValue("female_1")]
	public const string skin_civ_default_female = "female_1";

	public static List<Sprite> all_preloaded_sprites_units = new List<Sprite>();

	[NonSerialized]
	public readonly Dictionary<string, Sprite[]> dict_mains = new Dictionary<string, Sprite[]>();

	private static Dictionary<string, Sprite> _shadow_sprites = new Dictionary<string, Sprite>();

	public string texture_path_base;

	public string texture_path_base_male;

	public string texture_path_base_female;

	public string texture_path_main;

	public string texture_path_baby;

	public string texture_path_king;

	public string texture_path_leader;

	public string texture_path_warrior;

	public string texture_path_zombie;

	public bool has_advanced_textures;

	public bool has_old_heads;

	[DefaultValue("")]
	public string texture_heads = string.Empty;

	[DefaultValue("")]
	public string texture_head_king = string.Empty;

	[DefaultValue("")]
	public string texture_head_warrior = string.Empty;

	[DefaultValue("")]
	public string texture_heads_old_male = string.Empty;

	[DefaultValue("")]
	public string texture_heads_old_female = string.Empty;

	[DefaultValue("")]
	public string texture_heads_male = string.Empty;

	[DefaultValue("")]
	public string texture_heads_female = string.Empty;

	public bool render_heads_for_children;

	public bool prevent_unconscious_rotation;

	[DefaultValue(true)]
	public bool shadow = true;

	[DefaultValue("unitShadow_5")]
	public string shadow_texture = "unitShadow_5";

	[NonSerialized]
	internal Sprite shadow_sprite;

	[NonSerialized]
	internal Vector2 shadow_size;

	[DefaultValue("unitShadow_2")]
	public string shadow_texture_egg = "unitShadow_2";

	[NonSerialized]
	internal Sprite shadow_sprite_egg;

	[NonSerialized]
	internal Vector2 shadow_size_egg;

	[DefaultValue("unitShadow_4")]
	public string shadow_texture_baby = "unitShadow_4";

	[NonSerialized]
	internal Sprite shadow_sprite_baby;

	[NonSerialized]
	internal Vector2 shadow_size_baby;

	private string _base_path;

	private static int _total;

	private static readonly Regex _regex_heads_sorter = new Regex("(\\D*)(\\d+)");

	public ActorTextureSubAsset(string pBasePath, bool pHasAdvancedTextures)
	{
		_total++;
		has_advanced_textures = pHasAdvancedTextures;
		_base_path = pBasePath;
		texture_path_base = pBasePath;
		texture_path_base_male = pBasePath + "male_1";
		texture_path_base_female = pBasePath + "female_1";
		if (string.IsNullOrEmpty(texture_head_warrior))
		{
			texture_head_warrior = pBasePath + "head_warrior";
		}
		if (string.IsNullOrEmpty(texture_head_king))
		{
			texture_head_king = pBasePath + "head_king";
		}
		if (string.IsNullOrEmpty(texture_heads_old_female))
		{
			texture_heads_old_female = pBasePath + "head_old_female";
		}
		if (string.IsNullOrEmpty(texture_heads_old_male))
		{
			texture_heads_old_male = pBasePath + "head_old_male";
		}
		if (string.IsNullOrEmpty(texture_heads_male))
		{
			texture_heads_male = pBasePath + "heads_male";
		}
		if (string.IsNullOrEmpty(texture_heads_female))
		{
			texture_heads_female = pBasePath + "heads_female";
		}
		texture_path_main = pBasePath + "main";
		if (!hasSpriteInResources(texture_path_main))
		{
			texture_path_main = texture_path_base_male;
		}
		if (string.IsNullOrEmpty(texture_path_king))
		{
			texture_path_king = pBasePath + "king";
		}
		if (string.IsNullOrEmpty(texture_path_leader))
		{
			texture_path_leader = pBasePath + "leader";
		}
		if (string.IsNullOrEmpty(texture_path_warrior))
		{
			texture_path_warrior = pBasePath + "warrior_1";
		}
		if (string.IsNullOrEmpty(texture_path_zombie))
		{
			texture_path_zombie = pBasePath + "zombie";
		}
		if (string.IsNullOrEmpty(texture_heads))
		{
			texture_heads = pBasePath + "heads";
			if (!hasSpriteInResources(texture_path_main))
			{
				texture_path_main = texture_heads_male;
			}
		}
		if (hasSpriteInResources(texture_heads_old_male))
		{
			has_old_heads = true;
		}
		texture_path_baby = pBasePath + "child";
	}

	private void logAssetError(string pMessage, string pPath)
	{
		BaseAssetLibrary.logAssetError(pMessage, pPath);
	}

	public string getUnitTexturePath(Actor pActor)
	{
		Subspecies subspecies = pActor.subspecies;
		if (pActor.isEgg())
		{
			return subspecies.egg_sprite_path;
		}
		if (pActor.isBaby())
		{
			return texture_path_baby;
		}
		if (pActor.hasSubspecies() && pActor.subspecies.has_mutation_reskin && pActor.asset.unit_zombie)
		{
			return texture_path_zombie;
		}
		string result = texture_path_main;
		ProfessionAsset profession_asset = pActor.profession_asset;
		if (profession_asset == null || profession_asset.profession_id == UnitProfession.Nothing)
		{
			return result;
		}
		if (!has_advanced_textures)
		{
			return result;
		}
		switch (profession_asset.profession_id)
		{
		case UnitProfession.Warrior:
		{
			string text = texture_path_warrior;
			if (pActor.hasSubspecies())
			{
				text = pActor.subspecies.getSkinWarrior();
			}
			if (subspecies.has_mutation_reskin)
			{
				List<string> skin_warrior = subspecies.mutation_skin_asset.skin_warrior;
				int index = Toolbox.loopIndex(pActor.asset.skin_warrior.IndexOf(text), skin_warrior.Count);
				text = skin_warrior[index];
			}
			return texture_path_base + text;
		}
		case UnitProfession.King:
			return texture_path_king;
		case UnitProfession.Leader:
			return texture_path_leader;
		default:
			return getTextureSkinBasedOnSex(pActor);
		}
	}

	private string getTextureSkinBasedOnSex(Actor pActor)
	{
		if (pActor.isSexFemale())
		{
			if (pActor.hasSubspecies())
			{
				return texture_path_base + pActor.subspecies.getSkinFemale();
			}
			return texture_path_base_female;
		}
		if (pActor.hasSubspecies())
		{
			return texture_path_base + pActor.subspecies.getSkinMale();
		}
		return texture_path_base_male;
	}

	public string getUnitTexturePathForUI(ActorAsset pAsset)
	{
		string result = texture_path_main;
		if (!pAsset.civ)
		{
			return result;
		}
		if (AssetsDebugManager.actors_sex == ActorSex.Male)
		{
			return texture_path_base + pAsset.skin_citizen_male[0];
		}
		return texture_path_base + pAsset.skin_citizen_female[0];
	}

	private bool hasSpriteInResources(string pPath)
	{
		Sprite[] spriteList = SpriteTextureLoader.getSpriteList(pPath, pSkipIfEmpty: true);
		if (spriteList == null)
		{
			return false;
		}
		all_preloaded_sprites_units.AddRange(spriteList);
		return spriteList.Length != 0;
	}

	public object Clone()
	{
		return new ActorTextureSubAsset(_base_path, has_advanced_textures);
	}

	public void preloadSprites(bool pCivTextures, bool pHasBabyForm, IAnimationFrames pAnimationAsset)
	{
		if (!pCivTextures)
		{
			preloadSpritePath("texture_path_main", texture_path_main, pAnimationAsset);
		}
		if (pHasBabyForm)
		{
			preloadSpritePath("texture_path_baby", texture_path_baby, pAnimationAsset);
		}
		if (has_advanced_textures)
		{
			preloadSpritePath("texture_path_base_male", texture_path_base_male, pAnimationAsset);
			preloadSpritePath("texture_path_base_female", texture_path_base_female, pAnimationAsset);
			preloadSpritePath("texture_path_king", texture_path_king, pAnimationAsset);
			preloadSpritePath("texture_path_leader", texture_path_leader, pAnimationAsset);
			preloadSpritePath("texture_path_warrior", texture_path_warrior, pAnimationAsset);
			preloadSpritePath("texture_head_king", texture_head_king, pAnimationAsset, pLoadHeads: true, pThrowError: true, pSpecialHead: true);
			preloadSpritePath("texture_head_warrior", texture_head_warrior, pAnimationAsset, pLoadHeads: true, pThrowError: true, pSpecialHead: true);
			preloadSpritePath("texture_heads_male", texture_heads_male, pAnimationAsset, pLoadHeads: true);
			preloadSpritePath("texture_heads_female", texture_heads_female, pAnimationAsset, pLoadHeads: true);
		}
		else
		{
			preloadSpritePath("texture_heads", texture_heads, pAnimationAsset, pLoadHeads: true, pThrowError: false);
		}
	}

	private bool preloadSpritePath(string pType, string pPath, IAnimationFrames pAnimationAsset, bool pLoadHeads = false, bool pThrowError = true, bool pSpecialHead = false)
	{
		if (string.IsNullOrEmpty(pPath))
		{
			return false;
		}
		if (dict_mains.ContainsKey(pPath))
		{
			return false;
		}
		Sprite[] spriteList = SpriteTextureLoader.getSpriteList(pPath);
		if (!pLoadHeads)
		{
			dict_mains.Add(pPath, spriteList);
		}
		all_preloaded_sprites_units.AddRange(spriteList);
		if (spriteList.Length == 0)
		{
			if (pThrowError)
			{
				logAssetError("ActorAssetLibrary: <e>" + pType + "</e> doesn't exist for <e>" + ((Asset)pAnimationAsset).id + "</e> at ", pPath);
			}
			return false;
		}
		if (pLoadHeads)
		{
			if (has_advanced_textures)
			{
				for (int i = 0; i < spriteList.Length; i++)
				{
					if (pSpecialHead)
					{
						ActorAnimationLoader.getHeadSpecial(pPath);
					}
					else
					{
						ActorAnimationLoader.getHead(pPath, i);
					}
				}
			}
			checkHeads(spriteList, pPath, pAnimationAsset);
		}
		else
		{
			checkAnimations(spriteList, pPath, (Asset)pAnimationAsset, pAnimationAsset);
		}
		return true;
	}

	internal void loadShadow()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = (shadow_sprite = getShadowSprite(shadow_texture)).rect;
		shadow_size = ((Rect)(ref rect)).size;
		rect = (shadow_sprite_egg = getShadowSprite(shadow_texture_egg)).rect;
		shadow_size_egg = ((Rect)(ref rect)).size;
		rect = (shadow_sprite_baby = getShadowSprite(shadow_texture_baby)).rect;
		shadow_size_baby = ((Rect)(ref rect)).size;
	}

	private Sprite getShadowSprite(string pTexturePath)
	{
		if (!_shadow_sprites.ContainsKey(pTexturePath))
		{
			Sprite sprite = SpriteTextureLoader.getSprite("shadows/" + pTexturePath);
			if ((Object)(object)sprite == (Object)null)
			{
				Debug.LogError((object)("Shadow not found " + pTexturePath));
			}
			_shadow_sprites.Add(pTexturePath, sprite);
		}
		Sprite obj = _shadow_sprites[pTexturePath];
		return DynamicSprites.getShadowUnit(obj, ((object)obj).GetHashCode());
	}

	private void checkHeads(Sprite[] pSprites, string pPath, IAnimationFrames pAnimationAsset)
	{
		using ListPool<string> listPool = new ListPool<string>();
		for (int i = 0; i < pSprites.Length; i++)
		{
			string name = ((Object)pSprites[i]).name;
			if (name.Any(char.IsDigit))
			{
				if (listPool.Contains(name))
				{
					Debug.LogError((object)("Duplicate head " + name));
				}
				listPool.Add(name);
			}
		}
		listPool.Sort(headSorter);
		string text = "";
		int num = -1;
		foreach (ref string item in listPool)
		{
			string[] array = item.Split("_");
			string text2 = array[0];
			if (!int.TryParse(array[1], out var result))
			{
				continue;
			}
			if (text2 != text)
			{
				text = text2;
				if (result != 0)
				{
					logAssetError("ActorAssetLibrary: <e>" + ((Asset)pAnimationAsset).id + "</e> missing head: <e>" + text2 + "_0</e> at ", pPath);
				}
			}
			else if (result != num + 1)
			{
				logAssetError($"ActorAssetLibrary: <e>{((Asset)pAnimationAsset).id}</e> missing head: <e>{text2}_{num + 1}</e> at ", pPath);
			}
			num = result;
		}
	}

	private int headSorter(string x, string y)
	{
		Match match = _regex_heads_sorter.Match(x);
		Match match2 = _regex_heads_sorter.Match(y);
		if (match.Success && match2.Success && match.Groups[1].Value == match2.Groups[1].Value && int.TryParse(match.Groups[2].Value, out var result) && int.TryParse(match2.Groups[2].Value, out var result2))
		{
			return result.CompareTo(result2);
		}
		return x.CompareTo(y);
	}

	private void checkAnimations(Sprite[] pSprites, string pPath, Asset pAsset, IAnimationFrames pAnimationFrames)
	{
		using ListPool<string> listPool = new ListPool<string>();
		foreach (Sprite val in pSprites)
		{
			listPool.Add(((Object)val).name);
		}
		using ListPool<string> listPool2 = new ListPool<string>();
		string[] walk = pAnimationFrames.getWalk();
		string[] walk2;
		if (walk != null && walk.Length != 0)
		{
			listPool2.Clear();
			bool flag = false;
			walk2 = pAnimationFrames.getWalk();
			foreach (string item in walk2)
			{
				if (!listPool.Contains(item))
				{
					listPool2.Add(item);
				}
				else
				{
					flag = true;
				}
			}
			if (!flag)
			{
				logAssetError("ActorAssetLibrary: <e>" + pAsset.id + "</e> missing all animation_walk sprites: <e>" + string.Join(", ", listPool2) + "</e> at ", pPath);
			}
		}
		string[] swim = pAnimationFrames.getSwim();
		if (swim != null && swim.Length != 0)
		{
			listPool2.Clear();
			bool flag2 = false;
			walk2 = pAnimationFrames.getSwim();
			foreach (string item2 in walk2)
			{
				if (!listPool.Contains(item2))
				{
					listPool2.Add(item2);
				}
				else
				{
					flag2 = true;
				}
			}
			if (!flag2)
			{
				logAssetError("ActorAssetLibrary: <e>" + pAsset.id + "</e> missing all animation_swim sprites: <e>" + string.Join(", ", listPool2) + "</e> at ", pPath);
			}
		}
		string[] idle = pAnimationFrames.getIdle();
		if (idle == null || idle.Length == 0)
		{
			return;
		}
		listPool2.Clear();
		bool flag3 = false;
		walk2 = pAnimationFrames.getIdle();
		foreach (string item3 in walk2)
		{
			if (!listPool.Contains(item3))
			{
				listPool2.Add(item3);
			}
			else
			{
				flag3 = true;
			}
		}
		if (!flag3)
		{
			logAssetError("ActorAssetLibrary: <e>" + pAsset.id + "</e> missing all animation_idle sprites: <e>" + string.Join(", ", listPool2) + "</e> at ", pPath);
		}
	}

	public static int getTotal()
	{
		return _total;
	}
}
