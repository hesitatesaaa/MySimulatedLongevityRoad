using System.Collections.Generic;
using UnityEngine;

public class AllianceManager : MetaSystemManager<Alliance, AllianceData>
{
	public Sprite[] _cached_banner_backgrounds;

	public Sprite[] _cached_banner_icons;

	private List<Alliance> _to_dissolve = new List<Alliance>();

	public AllianceManager()
	{
		type_id = "alliance";
	}

	public override void update(float pElapsed)
	{
		base.update(pElapsed);
		using (IEnumerator<Alliance> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Alliance current = enumerator.Current;
				current.clearCursorOver();
				if (!current.checkActive())
				{
					_to_dissolve.Add(current);
				}
				else
				{
					current.update();
				}
			}
		}
		foreach (Alliance item in _to_dissolve)
		{
			dissolveAlliance(item);
		}
		_to_dissolve.Clear();
	}

	public void dissolveAlliance(Alliance pAlliance)
	{
		World.world.game_stats.data.alliancesDissolved++;
		World.world.map_stats.alliancesDissolved++;
		WorldLog.logAllianceDisolved(pAlliance);
		pAlliance.dissolve();
		removeObject(pAlliance);
	}

	private void addTest()
	{
	}

	public bool forceAlliance(Kingdom pKingdom1, Kingdom pKingdom2)
	{
		Alliance alliance = pKingdom1.getAlliance();
		if (alliance == null)
		{
			alliance = pKingdom2.getAlliance();
		}
		bool result = false;
		if (alliance == null)
		{
			alliance = newAlliance(pKingdom1, pKingdom2);
			result = true;
		}
		else
		{
			alliance.join(pKingdom1, pRecalc: true, pForce: true);
			alliance.join(pKingdom2, pRecalc: true, pForce: true);
		}
		alliance.setType(AllianceType.Forced);
		return result;
	}

	public void useDiscordPower(Alliance pAlliance, City pCity)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Kingdom kingdom = pCity.kingdom;
		pAlliance.leave(kingdom);
		EffectsLibrary.highlightKingdomZones(kingdom, Color.white);
		if (pAlliance.kingdoms_hashset.Count == 0)
		{
			dissolveAlliance(pAlliance);
		}
	}

	public Alliance newAlliance(Kingdom pKingdom, Kingdom pKingdom2)
	{
		World.world.game_stats.data.alliancesMade++;
		World.world.map_stats.alliancesMade++;
		Alliance alliance = newObject();
		alliance.createNewAlliance();
		alliance.addFounders(pKingdom, pKingdom2);
		WorldLog.logAllianceCreated(alliance);
		return alliance;
	}

	public Sprite[] getBackgroundsList()
	{
		if (_cached_banner_backgrounds == null)
		{
			_cached_banner_backgrounds = SpriteTextureLoader.getSpriteList("alliances/backgrounds/");
		}
		return _cached_banner_backgrounds;
	}

	public Sprite[] getIconsList()
	{
		if (_cached_banner_icons == null)
		{
			_cached_banner_icons = SpriteTextureLoader.getSpriteList("alliances/icons/");
		}
		return _cached_banner_icons;
	}

	public bool anyAlliances()
	{
		return Count > 0;
	}

	public override void clear()
	{
		base.clear();
	}

	protected override void updateDirtyUnits()
	{
	}
}
