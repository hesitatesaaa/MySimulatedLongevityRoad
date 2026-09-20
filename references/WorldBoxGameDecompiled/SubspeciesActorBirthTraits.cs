using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class SubspeciesActorBirthTraits : ITraitsOwner<ActorTrait>
{
	private ActorAsset _asset;

	private readonly HashSet<ActorTrait> _traits = new HashSet<ActorTrait>();

	private Subspecies _subspecies;

	public void init(ActorAsset pActorAsset, Subspecies pSubspecies)
	{
		_asset = pActorAsset;
		setSubspecies(pSubspecies);
		if (_asset.traits != null)
		{
			foreach (string trait in _asset.traits)
			{
				addTrait(trait);
			}
		}
		if (!WorldLawLibrary.world_law_mutant_box.isEnabled())
		{
			return;
		}
		int num = Randy.randomInt(1, 4);
		for (int i = 0; i < num; i++)
		{
			ActorTrait random = AssetManager.traits.pot_traits_mutation_box.GetRandom();
			if (random.isAvailable())
			{
				addTrait(random, pRemoveOpposites: true);
			}
		}
	}

	public void reset()
	{
		_traits.Clear();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IReadOnlyCollection<ActorTrait> getTraits()
	{
		return _traits;
	}

	public bool hasTraits()
	{
		return _traits.Count > 0;
	}

	public List<string> getTraitsAsStrings()
	{
		return Toolbox.getListForSave(_traits);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool hasTrait(ActorTrait pTrait)
	{
		return _traits.Contains(pTrait);
	}

	internal bool hasOppositeTrait(ActorTrait pTrait)
	{
		return pTrait.hasOppositeTrait(_traits);
	}

	public bool addTrait(string pTraitID, bool pRemoveOpposites = false)
	{
		ActorTrait actorTrait = AssetManager.traits.get(pTraitID);
		if (actorTrait == null)
		{
			return false;
		}
		return addTrait(actorTrait, pRemoveOpposites);
	}

	public bool addTrait(ActorTrait pTrait, bool pRemoveOpposites = false)
	{
		if (hasTrait(pTrait))
		{
			return false;
		}
		if (pTrait.traits_to_remove != null)
		{
			removeTraits(pTrait.traits_to_remove);
		}
		if (pRemoveOpposites)
		{
			removeOppositeTraits(pTrait);
		}
		else if (hasOppositeTrait(pTrait))
		{
			return false;
		}
		_traits.Add(pTrait);
		return true;
	}

	public bool removeTrait(ActorTrait pTrait)
	{
		return _traits.Remove(pTrait);
	}

	public void removeTraits(ICollection<ActorTrait> pTraits)
	{
		foreach (ActorTrait pTrait in pTraits)
		{
			_traits.Remove(pTrait);
		}
	}

	private void removeOppositeTraits(ActorTrait pTrait)
	{
		if (pTrait.hasOppositeTraits())
		{
			removeTraits(pTrait.opposite_traits);
		}
	}

	public void sortTraits(IReadOnlyCollection<ActorTrait> pTraits)
	{
		if (!_traits.SetEquals(pTraits))
		{
			return;
		}
		_traits.Clear();
		foreach (ActorTrait pTrait in pTraits)
		{
			_traits.Add(pTrait);
		}
	}

	public void traitModifiedEvent()
	{
	}

	public void fillTraitAssetsFromStringList(IEnumerable<string> pList)
	{
		_traits.Clear();
		if (pList == null)
		{
			return;
		}
		foreach (string p in pList)
		{
			ActorTrait actorTrait = AssetManager.traits.get(p);
			if (actorTrait != null)
			{
				_traits.Add(actorTrait);
			}
		}
	}

	public ActorAsset getActorAsset()
	{
		return _asset;
	}

	public void setSubspecies(Subspecies pSubspecies)
	{
		_subspecies = pSubspecies;
	}
}
