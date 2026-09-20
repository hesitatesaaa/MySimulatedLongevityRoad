using System.Collections.Generic;

public class SpellHolder
{
	private List<SpellAsset> _spells = new List<SpellAsset>();

	private HashSet<SpellAsset> _spells_hashset = new HashSet<SpellAsset>();

	public IReadOnlyList<SpellAsset> spells => _spells;

	public bool hasAny()
	{
		return _spells.Count > 0;
	}

	public SpellAsset getRandomSpell()
	{
		return _spells.GetRandom();
	}

	public bool hasSpell(SpellAsset pSpell)
	{
		return _spells_hashset.Contains(pSpell);
	}

	public void reset()
	{
		_spells.Clear();
		_spells_hashset.Clear();
	}

	public void mergeWith(SpellHolder pListSpells)
	{
		mergeWith(pListSpells.spells);
	}

	public void mergeWith(IReadOnlyList<SpellAsset> pSpells)
	{
		_spells.AddRange(pSpells);
		_spells_hashset.UnionWith(pSpells);
	}

	public void mergeWith(List<string> pSpellIDs)
	{
		foreach (string pSpellID in pSpellIDs)
		{
			SpellAsset spellAsset = AssetManager.spells.get(pSpellID);
			if (spellAsset != null)
			{
				addSpell(spellAsset);
			}
		}
	}

	public void addSpell(SpellAsset pSpell)
	{
		_spells.Add(pSpell);
		_spells_hashset.Add(pSpell);
	}

	public IReadOnlyCollection<SpellAsset> getSpellsHashset()
	{
		return _spells_hashset;
	}
}
