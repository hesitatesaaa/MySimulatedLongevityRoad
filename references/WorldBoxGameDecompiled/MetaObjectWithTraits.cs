using System;
using System.Collections.Generic;
using UnityEngine;

public class MetaObjectWithTraits<TData, TBaseTrait> : MetaObject<TData>, ITraitsOwner<TBaseTrait> where TData : MetaObjectData where TBaseTrait : BaseTrait<TBaseTrait>
{
	private readonly HashSet<TBaseTrait> _traits = new HashSet<TBaseTrait>();

	public readonly BaseStats base_stats = new BaseStats();

	public readonly BaseStats base_stats_meta = new BaseStats();

	private ActorAsset _species_asset;

	public readonly List<BaseAugmentationAsset> all_actions_actor_special_effect = new List<BaseAugmentationAsset>();

	public AttackAction all_actions_actor_attack_target;

	public GetHitAction all_actions_actor_get_hit;

	public WorldAction all_actions_actor_death;

	public WorldAction all_actions_actor_growth;

	public WorldAction all_actions_actor_birth;

	public readonly List<DecisionAsset> decisions_assets = new List<DecisionAsset>();

	public readonly CombatActionHolder combat_actions = new CombatActionHolder();

	public readonly SpellHolder spells = new SpellHolder();

	protected virtual AssetLibrary<TBaseTrait> trait_library
	{
		get
		{
			throw new NotImplementedException(GetType().Name);
		}
	}

	protected virtual List<string> default_traits => null;

	protected virtual List<string> saved_traits => null;

	protected virtual string species_id => "human";

	public override void loadData(TData pData)
	{
		base.loadData(pData);
		loadTraits();
	}

	private void resetStatsAndCallbacks()
	{
		all_actions_actor_death = null;
		all_actions_actor_growth = null;
		all_actions_actor_birth = null;
		all_actions_actor_attack_target = null;
		all_actions_actor_get_hit = null;
		all_actions_actor_special_effect.Clear();
		base_stats.clear();
		base_stats_meta.clear();
		decisions_assets.Clear();
		combat_actions.reset();
		spells.reset();
	}

	public void forceRecalcBaseStats()
	{
		recalcBaseStats();
	}

	protected virtual void recalcBaseStats()
	{
		resetStatsAndCallbacks();
		foreach (TBaseTrait trait in _traits)
		{
			base_stats.mergeStats(trait.base_stats);
			base_stats_meta.mergeStats(trait.base_stats_meta);
			all_actions_actor_death = (WorldAction)Delegate.Combine(all_actions_actor_death, trait.action_death);
			all_actions_actor_growth = (WorldAction)Delegate.Combine(all_actions_actor_growth, trait.action_growth);
			all_actions_actor_birth = (WorldAction)Delegate.Combine(all_actions_actor_birth, trait.action_birth);
			all_actions_actor_attack_target = (AttackAction)Delegate.Combine(all_actions_actor_attack_target, trait.action_attack_target);
			all_actions_actor_get_hit = (GetHitAction)Delegate.Combine(all_actions_actor_get_hit, trait.action_get_hit);
			if (trait.action_special_effect != null)
			{
				all_actions_actor_special_effect.Add(trait);
			}
			if (trait.hasDecisions())
			{
				decisions_assets.AddRange(trait.decisions_assets);
			}
			if (trait.hasCombatActions())
			{
				combat_actions.mergeWith(trait.combat_actions);
			}
			if (!trait.hasSpells())
			{
				continue;
			}
			spells.mergeWith(trait.spells);
			foreach (SpellAsset spell in trait.spells)
			{
				if (spell.hasDecisions())
				{
					decisions_assets.AddRange(spell.decisions_assets);
				}
			}
		}
		setUnitStatsDirty();
	}

	private void setUnitStatsDirty()
	{
		List<Actor> list = base.units;
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			list[i].setStatsDirty();
		}
	}

	private void loadTraits()
	{
		if (saved_traits == null)
		{
			return;
		}
		fillTraitAssetsFromStringList(saved_traits);
		foreach (TBaseTrait trait in _traits)
		{
			trait.action_on_augmentation_load?.Invoke(this, trait);
		}
	}

	protected void fillTraitAssetsFromStringList(List<string> pList)
	{
		foreach (string item in pList.LoopRandom())
		{
			TBaseTrait val = trait_library.get(item);
			if (val != null && !hasOppositeTrait(val))
			{
				_traits.Add(val);
			}
		}
		recalcBaseStats();
	}

	protected override void generateNewMetaObject()
	{
		base.generateNewMetaObject();
		if (default_traits != null)
		{
			fillTraitAssetsFromStringList(default_traits);
		}
	}

	protected override void generateNewMetaObject(bool pAddDefaultTraits)
	{
		base.generateNewMetaObject();
		if ((default_traits != null) & pAddDefaultTraits)
		{
			fillTraitAssetsFromStringList(default_traits);
		}
	}

	public List<string> getTraitsAsStrings()
	{
		return Toolbox.getListForSave(_traits);
	}

	public string getTraitsAsLocalizedString()
	{
		string text = "";
		foreach (TBaseTrait trait in _traits)
		{
			text = text + trait.getTranslatedName() + ", ";
		}
		return text;
	}

	public void copyTraits(IReadOnlyCollection<TBaseTrait> pTraitsToCopy)
	{
		foreach (TBaseTrait item in pTraitsToCopy)
		{
			if (!hasOppositeTrait(item))
			{
				_traits.Add(item);
			}
		}
		recalcBaseStats();
	}

	protected void clearTraits()
	{
		if (_traits.Count != 0)
		{
			_traits.Clear();
		}
	}

	public IReadOnlyCollection<TBaseTrait> getTraits()
	{
		return _traits;
	}

	public void sortTraits(IReadOnlyCollection<TBaseTrait> pTraits)
	{
		if (!_traits.SetEquals(pTraits))
		{
			return;
		}
		_traits.Clear();
		foreach (TBaseTrait pTrait in pTraits)
		{
			_traits.Add(pTrait);
		}
	}

	public virtual void traitModifiedEvent()
	{
	}

	public override void triggerOnRemoveObject()
	{
		base.triggerOnRemoveObject();
		foreach (TBaseTrait trait in _traits)
		{
			trait.action_on_object_remove?.Invoke(this, trait);
		}
	}

	public void removeTrait(string pTraitID)
	{
		TBaseTrait pTrait = trait_library.get(pTraitID);
		removeTrait(pTrait);
	}

	public bool hasTrait(string pTrait)
	{
		TBaseTrait pTrait2 = trait_library.get(pTrait);
		return hasTrait(pTrait2);
	}

	public bool hasMetaTag(string pTag)
	{
		return base_stats_meta.hasTag(pTag);
	}

	public bool hasTraits()
	{
		return _traits.Count > 0;
	}

	public bool hasTrait(TBaseTrait pTrait)
	{
		if (_traits.Contains(pTrait))
		{
			return true;
		}
		return false;
	}

	public void removeTraits(ListPool<string> pTraits)
	{
		bool flag = false;
		foreach (ref string pTrait in pTraits)
		{
			string current = pTrait;
			TBaseTrait val = trait_library.get(current);
			if (_traits.Remove(val))
			{
				val.action_on_augmentation_remove?.Invoke(this, val);
				flag = true;
			}
		}
		if (flag)
		{
			recalcBaseStats();
		}
	}

	public virtual bool removeTrait(TBaseTrait pTrait)
	{
		bool num = _traits.Remove(pTrait);
		if (num)
		{
			pTrait.action_on_augmentation_remove?.Invoke(this, pTrait);
			recalcBaseStats();
		}
		return num;
	}

	private void removeOppositeTraits(TBaseTrait pTrait)
	{
		if (!pTrait.hasOppositeTraits())
		{
			return;
		}
		foreach (TBaseTrait opposite_trait in pTrait.opposite_traits)
		{
			removeTrait(opposite_trait);
		}
	}

	public virtual bool addTrait(string pTraitID, bool pRemoveOpposites = false)
	{
		TBaseTrait val = trait_library.get(pTraitID);
		if (val == null)
		{
			return false;
		}
		return addTrait(val, pRemoveOpposites);
	}

	public virtual bool addTrait(TBaseTrait pTrait, bool pRemoveOpposites = false)
	{
		if (hasTrait(pTrait))
		{
			return false;
		}
		if (pRemoveOpposites)
		{
			removeOppositeTraits(pTrait);
		}
		if (hasOppositeTrait(pTrait))
		{
			return false;
		}
		_traits.Add(pTrait);
		pTrait.action_on_augmentation_add?.Invoke(this, pTrait);
		recalcBaseStats();
		return true;
	}

	public override Sprite getTopicSprite()
	{
		if (_traits.Count == 0)
		{
			return null;
		}
		return _traits.GetRandom().getSprite();
	}

	internal bool hasOppositeTrait(TBaseTrait pTrait)
	{
		return pTrait.hasOppositeTrait(_traits);
	}

	public override ActorAsset getActorAsset()
	{
		if (_species_asset == null)
		{
			string pID = species_id;
			_species_asset = AssetManager.actor_library.get(pID);
		}
		return _species_asset;
	}

	public bool isSameActorAsset(ActorAsset pAsset)
	{
		return getActorAsset() == pAsset;
	}

	public void addRandomTraitFromBiome<T>(WorldTile pTile, List<string> pTraitList, AssetLibrary<T> pTraitLibrary) where T : BaseTrait<TBaseTrait>
	{
		if (!pTile.Type.is_biome || pTraitList == null || pTraitList.Count == 0)
		{
			return;
		}
		int count = pTraitList.Count;
		for (int i = 0; i < count; i++)
		{
			if (!Randy.randomBool())
			{
				string random = pTraitList.GetRandom();
				Asset asset = pTraitLibrary.get(random);
				addTrait((TBaseTrait)asset, pRemoveOpposites: true);
			}
		}
	}

	public void addTraitFromBiome<T>(WorldTile pTile, List<string> pTraitList, AssetLibrary<T> pTraitLibrary) where T : BaseTrait<TBaseTrait>
	{
		if (pTile.Type.is_biome && pTraitList != null && pTraitList.Count != 0)
		{
			for (int i = 0; i < pTraitList.Count; i++)
			{
				Asset asset = pTraitLibrary.get(pTraitList[i]);
				addTrait((TBaseTrait)asset, pRemoveOpposites: true);
			}
		}
	}

	public TBaseTrait getTraitForBook()
	{
		IReadOnlyCollection<TBaseTrait> traits = getTraits();
		using ListPool<TBaseTrait> listPool = new ListPool<TBaseTrait>(traits.Count);
		foreach (TBaseTrait item in traits)
		{
			if (item.can_be_in_book)
			{
				listPool.Add(item);
			}
		}
		if (listPool.Count == 0)
		{
			return null;
		}
		return listPool.GetRandom();
	}

	public override void Dispose()
	{
		_species_asset = null;
		clearTraits();
		resetStatsAndCallbacks();
		base.Dispose();
	}
}
