using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BaseTraitLibrary<T> : BaseLibraryWithUnlockables<T> where T : BaseTrait<T>
{
	protected List<T> _pot_allowed_to_be_given_randomly = new List<T>();

	protected virtual string icon_path
	{
		get
		{
			throw new NotImplementedException(GetType().Name);
		}
	}

	public override void post_init()
	{
		base.post_init();
		list.Sort((T pT1, T pT2) => StringComparer.Ordinal.Compare(pT2.id, pT1.id));
		autoSetRarity();
		checkIcons();
	}

	protected virtual void autoSetRarity()
	{
		foreach (T item in list)
		{
			if (item.unlocked_with_achievement)
			{
				item.rarity = Rarity.R3_Legendary;
				continue;
			}
			bool num = item.action_death != null || item.action_special_effect != null || item.action_get_hit != null || item.action_birth != null || item.action_attack_target != null || item.action_on_augmentation_add != null || item.action_on_augmentation_remove != null || item.action_on_augmentation_load != null;
			bool flag = item.decision_ids != null;
			bool flag2 = item.spells_ids != null;
			bool flag3 = item.combat_actions_ids != null;
			bool flag4 = item.base_stats.hasTags();
			bool flag5 = !string.IsNullOrEmpty(item.plot_id);
			int num2 = 0;
			if (num)
			{
				num2++;
			}
			if (flag)
			{
				num2++;
			}
			if (flag2)
			{
				num2++;
			}
			if (flag3)
			{
				num2++;
			}
			if (flag4)
			{
				num2++;
			}
			if (flag5)
			{
				num2++;
			}
			if (num2 > 0)
			{
				if (num2 == 1)
				{
					item.rarity = Rarity.R1_Rare;
				}
				else
				{
					item.rarity = Rarity.R2_Epic;
				}
				item.needs_to_be_explored = true;
			}
			else if (item.rarity == Rarity.R0_Normal)
			{
				item.needs_to_be_explored = false;
			}
		}
	}

	public override void linkAssets()
	{
		base.linkAssets();
		fillOppositeHashsetsWithAssets();
		linkDecisions();
		linkCombatActions();
		linkSpells();
		linkActorAssets();
		foreach (T item in list)
		{
			if (item.spawn_random_trait_allowed)
			{
				_pot_allowed_to_be_given_randomly.AddTimes(item.spawn_random_rate, item);
			}
		}
	}

	private void linkCombatActions()
	{
		foreach (T item in list)
		{
			item.linkCombatActions();
		}
	}

	private void linkSpells()
	{
		foreach (T item in list)
		{
			item.linkSpells();
		}
	}

	private void linkDecisions()
	{
		foreach (T item in list)
		{
			if (item.decision_ids != null)
			{
				item.decisions_assets = new DecisionAsset[item.decision_ids.Count];
				for (int i = 0; i < item.decision_ids.Count; i++)
				{
					string pID = item.decision_ids[i];
					DecisionAsset decisionAsset = AssetManager.decisions_library.get(pID);
					item.decisions_assets[i] = decisionAsset;
				}
			}
		}
	}

	private void linkActorAssets()
	{
		foreach (ActorAsset item in AssetManager.actor_library.list)
		{
			List<string> defaultTraitsForMeta = getDefaultTraitsForMeta(item);
			if (defaultTraitsForMeta == null)
			{
				continue;
			}
			foreach (string item2 in defaultTraitsForMeta)
			{
				T val = get(item2);
				if (val.default_for_actor_assets == null)
				{
					val.default_for_actor_assets = new List<ActorAsset>();
				}
				val.default_for_actor_assets.Add(item);
			}
		}
	}

	public override void editorDiagnostic()
	{
		checkOppositeErrors();
		foreach (T item in list)
		{
			if (string.IsNullOrEmpty(item.group_id))
			{
				BaseAssetLibrary.logAssetError("Group id not assigned", item.id);
			}
			if (!item.special_icon_logic && (Object)(object)SpriteTextureLoader.getSprite(item.path_icon) == (Object)null)
			{
				BaseAssetLibrary.logAssetError("Missing icon file", item.path_icon);
			}
		}
		base.editorDiagnostic();
	}

	public override void editorDiagnosticLocales()
	{
		foreach (T item in list)
		{
			checkLocale(item, item.getLocaleID());
			checkLocale(item, item.getDescriptionID());
			checkLocale(item, item.getDescriptionID2());
		}
	}

	private void checkOppositeErrors()
	{
		foreach (T item in list)
		{
			HashSet<T> opposite_traits = item.opposite_traits;
			if (opposite_traits == null)
			{
				continue;
			}
			foreach (T item2 in opposite_traits)
			{
				HashSet<T> opposite_traits2 = item2.opposite_traits;
				if (opposite_traits2 == null || !opposite_traits2.Contains(item))
				{
					logErrorOpposites(item.id, item2.id);
				}
			}
		}
	}

	private void fillOppositeHashsetsWithAssets()
	{
		foreach (T item2 in list)
		{
			if (item2.opposite_list == null || item2.opposite_list.Count <= 0)
			{
				continue;
			}
			item2.opposite_traits = new HashSet<T>(item2.opposite_list.Count);
			foreach (string item3 in item2.opposite_list)
			{
				T item = get(item3);
				item2.opposite_traits.Add(item);
			}
		}
		foreach (T item4 in list)
		{
			if (item4.traits_to_remove_ids != null)
			{
				int num = item4.traits_to_remove_ids.Length;
				item4.traits_to_remove = new T[num];
				for (int i = 0; i < num; i++)
				{
					string pID = item4.traits_to_remove_ids[i];
					T val = get(pID);
					item4.traits_to_remove[i] = val;
				}
			}
		}
	}

	private void checkIcons()
	{
		foreach (T item in list)
		{
			if (string.IsNullOrEmpty(item.path_icon))
			{
				item.path_icon = icon_path + item.getLocaleID();
			}
		}
	}

	public override T add(T pAsset)
	{
		T val = base.add(pAsset);
		if (val.base_stats == null)
		{
			val.base_stats = new BaseStats();
		}
		if (val.base_stats_meta == null)
		{
			val.base_stats_meta = new BaseStats();
		}
		return val;
	}

	public string addToGameplayReportShort(string pWhatFor)
	{
		string empty = string.Empty;
		empty = empty + pWhatFor + "\n";
		foreach (T item in list)
		{
			string text = item.id;
			if (!(text == "Phenotype"))
			{
				string translatedDescription = item.getTranslatedDescription();
				string text2 = "\n" + text;
				if (!string.IsNullOrEmpty(translatedDescription))
				{
					text2 = text2 + ": " + translatedDescription;
				}
				empty += text2;
			}
		}
		return empty + "\n\n";
	}

	public string addToGameplayReport(string pWhatFor)
	{
		string empty = string.Empty;
		empty = empty + pWhatFor + "\n";
		foreach (T item in list)
		{
			string translatedName = item.getTranslatedName();
			if (!(translatedName == "Phenotype"))
			{
				string translatedDescription = item.getTranslatedDescription();
				string translatedDescription2 = item.getTranslatedDescription2();
				string text = "\n" + translatedName;
				text += "\n";
				if (!string.IsNullOrEmpty(translatedDescription))
				{
					text = text + "1: " + translatedDescription;
				}
				if (!string.IsNullOrEmpty(translatedDescription2))
				{
					text = text + "\n2: " + translatedDescription2;
				}
				empty += text;
			}
		}
		return empty + "\n\n";
	}

	public T getRandomSpawnTrait()
	{
		return _pot_allowed_to_be_given_randomly.GetRandom();
	}

	protected virtual List<string> getDefaultTraitsForMeta(ActorAsset pAsset)
	{
		throw new NotImplementedException();
	}
}
