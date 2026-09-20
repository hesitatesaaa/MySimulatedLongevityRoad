using System.Collections.Generic;
using UnityEngine;

public class NameplateLibrary : AssetLibrary<NameplateAsset>
{
	public readonly Dictionary<MetaType, NameplateAsset> map_modes_nameplates = new Dictionary<MetaType, NameplateAsset>();

	private NameplateAsset _plate_kingdom;

	private NameplateAsset _plate_city;

	private const int OFFSET_UNIT_Y = -2;

	public override void init()
	{
		base.init();
		add(new NameplateAsset
		{
			id = "plate_subspecies",
			path_sprite = "ui/nameplates/nameplate_subspecies",
			padding_left = 11,
			padding_right = 13,
			banner_only_mode_scale = 1.8f,
			map_mode = MetaType.Subspecies,
			overlap_for_fluid_mode = true,
			action_main = actionSubspecies
		});
		add(new NameplateAsset
		{
			id = "plate_army",
			path_sprite = "ui/nameplates/nameplate_army",
			padding_left = 26,
			padding_right = 18,
			padding_top = -2,
			banner_only_mode_scale = 1.5f,
			map_mode = MetaType.Army,
			action_main = actionArmy
		});
		add(new NameplateAsset
		{
			id = "plate_family",
			path_sprite = "ui/nameplates/nameplate_family",
			padding_left = 11,
			padding_right = 13,
			banner_only_mode_scale = 1.5f,
			map_mode = MetaType.Family,
			overlap_for_fluid_mode = true,
			action_main = actionFamily
		});
		add(new NameplateAsset
		{
			id = "plate_religion",
			path_sprite = "ui/nameplates/nameplate_religion",
			padding_left = 11,
			padding_right = 13,
			map_mode = MetaType.Religion,
			action_main = actionReligion
		});
		add(new NameplateAsset
		{
			id = "plate_culture",
			path_sprite = "ui/nameplates/nameplate_culture",
			padding_left = 11,
			padding_right = 13,
			map_mode = MetaType.Culture,
			action_main = actionCulture
		});
		add(new NameplateAsset
		{
			id = "plate_language",
			path_sprite = "ui/nameplates/nameplate_language",
			padding_left = 11,
			padding_right = 13,
			map_mode = MetaType.Language,
			action_main = actionLanguage
		});
		add(new NameplateAsset
		{
			id = "plate_alliance",
			path_sprite = "ui/nameplates/nameplate_alliance",
			map_mode = MetaType.Alliance,
			banner_only_mode_scale = 3f,
			padding_left = 14,
			padding_top = 2,
			action_main = actionAlliance
		});
		_plate_kingdom = add(new NameplateAsset
		{
			id = "plate_kingdom",
			path_sprite = "ui/nameplates/nameplate_kingdom",
			padding_left = 26,
			padding_right = 26,
			padding_top = -2,
			banner_only_mode_scale = 2.5f,
			map_mode = MetaType.Kingdom,
			action_main = actionKingdom
		});
		_plate_city = add(new NameplateAsset
		{
			id = "plate_city",
			path_sprite = "ui/nameplates/nameplate_city",
			map_mode = MetaType.City,
			banner_only_mode_scale = 2.5f,
			padding_left = 6,
			padding_right = 7,
			padding_top = -2,
			action_main = actionCity
		});
		add(new NameplateAsset
		{
			id = "plate_clan",
			path_sprite = "ui/nameplates/nameplate_clan",
			map_mode = MetaType.Clan,
			padding_left = 17,
			padding_right = 24,
			action_main = actionClan
		});
	}

	private bool isWithinCamera(Vector2 pVector)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return World.world.move_camera.isWithinCameraViewNotPowerBar(pVector);
	}

	public override NameplateAsset add(NameplateAsset pAsset)
	{
		map_modes_nameplates.Add(pAsset.map_mode, pAsset);
		return base.add(pAsset);
	}

	private void actionAlliance(NameplateManager pManager, NameplateAsset pAsset)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		foreach (Alliance alliance in World.world.alliances)
		{
			City city = null;
			Kingdom kingdom = null;
			foreach (Kingdom item in alliance.kingdoms_hashset)
			{
				if (item.hasCapital() && isWithinCamera(item.capital.city_center) && (kingdom == null || kingdom.power < item.power))
				{
					kingdom = item;
				}
			}
			if (kingdom != null && kingdom.hasCapital())
			{
				city = kingdom.capital;
			}
			if (city != null)
			{
				pManager.prepareNext(pAsset, alliance).showTextAlliance(alliance, city);
			}
		}
		foreach (Kingdom kingdom2 in World.world.kingdoms)
		{
			if (kingdom2.hasCapital() && !kingdom2.hasAlliance() && isWithinCamera(kingdom2.capital.city_center))
			{
				if (num >= pAsset.max_nameplate_count)
				{
					break;
				}
				num++;
				pManager.prepareNext(_plate_kingdom, kingdom2).showTextKingdom(kingdom2, kingdom2.capital.city_center);
			}
		}
	}

	private void actionReligion(NameplateManager pManager, NameplateAsset pAsset)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		switch (MetaTypeLibrary.religion.getZoneOptionState())
		{
		case 0:
		{
			foreach (Kingdom kingdom in World.world.kingdoms)
			{
				if (kingdom.hasReligion() && kingdom.hasCapital() && isWithinCamera(kingdom.capital.city_center))
				{
					pManager.prepareNext(pAsset, kingdom.religion).showTextReligion(kingdom.religion, Vector2.op_Implicit(kingdom.capital.city_center));
				}
			}
			return;
		}
		case 1:
		{
			foreach (City city in World.world.cities)
			{
				if (city.hasReligion())
				{
					Religion religion = city.getReligion();
					if (isWithinCamera(city.city_center))
					{
						pManager.prepareNext(pAsset, religion).showTextReligion(religion, Vector2.op_Implicit(city.city_center));
					}
				}
			}
			return;
		}
		}
		foreach (Religion religion2 in World.world.religions)
		{
			if (num >= pAsset.max_nameplate_count)
			{
				break;
			}
			if (getPositionForMeta(religion2, out var pPosition))
			{
				pManager.prepareNext(pAsset, religion2).showTextReligion(religion2, pPosition);
				num++;
			}
		}
	}

	private void actionLanguage(NameplateManager pManager, NameplateAsset pAsset)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		switch (MetaTypeLibrary.language.getZoneOptionState())
		{
		case 0:
		{
			foreach (Kingdom kingdom in World.world.kingdoms)
			{
				if (kingdom.hasLanguage() && kingdom.hasCapital() && isWithinCamera(kingdom.capital.city_center))
				{
					pManager.prepareNext(pAsset, kingdom.language).showTextLanguage(kingdom.language, Vector2.op_Implicit(kingdom.capital.city_center));
				}
			}
			return;
		}
		case 1:
		{
			foreach (City city in World.world.cities)
			{
				if (city.hasLanguage())
				{
					Language language = city.getLanguage();
					if (isWithinCamera(city.city_center))
					{
						pManager.prepareNext(pAsset, language).showTextLanguage(language, Vector2.op_Implicit(city.city_center));
					}
				}
			}
			return;
		}
		}
		foreach (Language language2 in World.world.languages)
		{
			if (num >= pAsset.max_nameplate_count)
			{
				break;
			}
			if (getPositionForMeta(language2, out var pPosition))
			{
				pManager.prepareNext(pAsset, language2).showTextLanguage(language2, pPosition);
				num++;
			}
		}
	}

	private void actionCulture(NameplateManager pManager, NameplateAsset pAsset)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		switch (MetaTypeLibrary.culture.getZoneOptionState())
		{
		case 0:
		{
			foreach (Kingdom kingdom in World.world.kingdoms)
			{
				if (kingdom.hasCulture() && kingdom.hasCapital() && isWithinCamera(kingdom.capital.city_center))
				{
					pManager.prepareNext(pAsset, kingdom.culture).showTextCulture(kingdom.culture, Vector2.op_Implicit(kingdom.capital.city_center));
				}
			}
			return;
		}
		case 1:
		{
			foreach (City city in World.world.cities)
			{
				if (city.hasCulture())
				{
					Culture culture = city.getCulture();
					if (isWithinCamera(city.city_center))
					{
						pManager.prepareNext(pAsset, culture).showTextCulture(culture, Vector2.op_Implicit(city.city_center));
					}
				}
			}
			return;
		}
		}
		foreach (Culture culture2 in World.world.cultures)
		{
			if (num >= pAsset.max_nameplate_count)
			{
				break;
			}
			if (getPositionForMeta(culture2, out var pPosition))
			{
				pManager.prepareNext(pAsset, culture2).showTextCulture(culture2, pPosition);
				num++;
			}
		}
	}

	private void actionCity(NameplateManager pManager, NameplateAsset pAsset)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		using ListPool<City> listPool = new ListPool<City>(World.world.cities.list);
		listPool.Sort(sortByMembers);
		if (MetaTypeLibrary.city.getZoneOptionState() == 0)
		{
			foreach (ref City item in listPool)
			{
				City current = item;
				if (num >= pAsset.max_nameplate_count)
				{
					break;
				}
				if (isWithinCamera(current.city_center))
				{
					pManager.prepareNext(_plate_city, current).showTextCity(current, current.city_center);
					num++;
				}
			}
			return;
		}
		foreach (City city in World.world.cities)
		{
			if (num >= pAsset.max_nameplate_count)
			{
				break;
			}
			Actor pForceActor = null;
			if (city.hasLeader() && !city.leader.isRekt() && city.leader.is_visible)
			{
				pForceActor = city.leader;
			}
			if (getPositionForMeta(city, out var pPosition, pForceActor))
			{
				pManager.prepareNext(pAsset, city).showTextCity(city, Vector2.op_Implicit(pPosition));
				num++;
			}
		}
	}

	private void actionKingdom(NameplateManager pManager, NameplateAsset pAsset)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		if (MetaTypeLibrary.kingdom.getZoneOptionState() == 0)
		{
			foreach (Kingdom kingdom in World.world.kingdoms)
			{
				if (kingdom.hasCapital() && isWithinCamera(kingdom.capital.city_center))
				{
					pManager.prepareNext(pAsset, kingdom).showTextKingdom(kingdom, kingdom.capital.city_center);
				}
			}
			return;
		}
		foreach (Kingdom kingdom2 in World.world.kingdoms)
		{
			if (num >= pAsset.max_nameplate_count)
			{
				break;
			}
			Actor pForceActor = null;
			if (kingdom2.hasKing() && !kingdom2.king.isRekt() && kingdom2.king.is_visible)
			{
				pForceActor = kingdom2.king;
			}
			if (getPositionForMeta(kingdom2, out var pPosition, pForceActor))
			{
				pManager.prepareNext(pAsset, kingdom2).showTextKingdom(kingdom2, Vector2.op_Implicit(pPosition));
				num++;
			}
		}
	}

	private void actionSubspecies(NameplateManager pManager, NameplateAsset pAsset)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		switch (MetaTypeLibrary.subspecies.getZoneOptionState())
		{
		case 0:
		{
			foreach (Kingdom kingdom in World.world.kingdoms)
			{
				Subspecies mainSubspecies2 = kingdom.getMainSubspecies();
				if (!mainSubspecies2.isRekt() && kingdom.hasCapital() && isWithinCamera(kingdom.capital.city_center))
				{
					pManager.prepareNext(pAsset, mainSubspecies2).showTextSubspecies(mainSubspecies2, Vector2.op_Implicit(kingdom.capital.city_center));
				}
			}
			return;
		}
		case 1:
		{
			foreach (City city in World.world.cities)
			{
				Subspecies mainSubspecies = city.getMainSubspecies();
				if (!mainSubspecies.isRekt() && isWithinCamera(city.city_center))
				{
					pManager.prepareNext(pAsset, mainSubspecies).showTextSubspecies(mainSubspecies, Vector2.op_Implicit(city.city_center));
				}
			}
			return;
		}
		}
		foreach (Subspecies subspecy in World.world.subspecies)
		{
			if (num >= pAsset.max_nameplate_count)
			{
				break;
			}
			if (getPositionForMeta(subspecy, out var pPosition))
			{
				pManager.prepareNext(pAsset, subspecy).showTextSubspecies(subspecy, pPosition);
				num++;
			}
		}
	}

	private bool getPositionForMeta(IMetaObject pMetaObject, out Vector3 pPosition, Actor pForceActor = null)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (!pMetaObject.isAlive() || !pMetaObject.hasUnits())
		{
			pPosition = Vector3.zero;
			return false;
		}
		Actor actor = pForceActor;
		if (actor == null)
		{
			actor = pMetaObject.getOldestVisibleUnitForNameplatesCached();
		}
		if (actor == null)
		{
			pPosition = Vector3.zero;
			return false;
		}
		Vector3 val = Vector2.op_Implicit(actor.current_position);
		val.y += actor.getHeight();
		val.y += -2f;
		pPosition = val;
		return true;
	}

	private int sortByMembers(IMetaObject pObject1, IMetaObject pObject2)
	{
		int num = pObject2.isFavorite().CompareTo(pObject1.isFavorite());
		if (num != 0)
		{
			return num;
		}
		int num2 = pObject2.isSelected().CompareTo(pObject1.isSelected());
		if (num2 != 0)
		{
			return num2;
		}
		return pObject2.countUnits().CompareTo(pObject1.countUnits());
	}

	private void actionArmy(NameplateManager pManager, NameplateAsset pAsset)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		MetaTypeLibrary.army.getZoneOptionState();
		using ListPool<Army> listPool = new ListPool<Army>(World.world.armies.list);
		listPool.Sort(sortByMembers);
		int num = 0;
		foreach (ref Army item in listPool)
		{
			Army current = item;
			if (num >= pAsset.max_nameplate_count)
			{
				break;
			}
			Actor pForceActor = null;
			if (current.hasCaptain())
			{
				pForceActor = current.getCaptain();
			}
			if (getPositionForMeta(current, out var pPosition, pForceActor))
			{
				pManager.prepareNext(pAsset, current).showTextArmy(current, pPosition);
				num++;
			}
		}
	}

	private void actionFamily(NameplateManager pManager, NameplateAsset pAsset)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		switch (MetaTypeLibrary.family.getZoneOptionState())
		{
		case 0:
		{
			foreach (Kingdom kingdom in World.world.kingdoms)
			{
				if (kingdom.hasCapital() && kingdom.hasKing() && kingdom.king.hasFamily() && isWithinCamera(kingdom.capital.city_center))
				{
					pManager.prepareNext(pAsset, kingdom.king.family).showTextFamily(kingdom.king.family, Vector2.op_Implicit(kingdom.capital.city_center));
				}
			}
			return;
		}
		case 1:
		{
			foreach (City city in World.world.cities)
			{
				if (city.hasLeader() && city.leader.hasFamily())
				{
					Family family = city.leader.family;
					if (family != null && isWithinCamera(city.city_center))
					{
						pManager.prepareNext(pAsset, family).showTextFamily(family, Vector2.op_Implicit(city.city_center));
					}
				}
			}
			return;
		}
		}
		using ListPool<Family> listPool = new ListPool<Family>(World.world.families.list);
		listPool.Sort(sortByMembers);
		foreach (ref Family item in listPool)
		{
			Family current3 = item;
			if (num >= pAsset.max_nameplate_count)
			{
				break;
			}
			if (getPositionForMeta(current3, out var pPosition))
			{
				pManager.prepareNext(pAsset, current3).showTextFamily(current3, pPosition);
				num++;
			}
		}
	}

	private void actionClan(NameplateManager pManager, NameplateAsset pAsset)
	{
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		switch (MetaTypeLibrary.clan.getZoneOptionState())
		{
		case 0:
		{
			foreach (Kingdom kingdom in World.world.kingdoms)
			{
				if (kingdom.hasCapital() && kingdom.hasKing() && kingdom.king.hasClan() && isWithinCamera(kingdom.capital.city_center))
				{
					pManager.prepareNext(pAsset, kingdom.king.clan).showTextClanCity(kingdom.king.clan, kingdom.capital);
				}
			}
			return;
		}
		case 1:
		{
			foreach (City city in World.world.cities)
			{
				Clan royalClan = city.getRoyalClan();
				if (royalClan != null && isWithinCamera(city.city_center))
				{
					pManager.prepareNext(pAsset, royalClan).showTextClanCity(royalClan, city);
				}
			}
			return;
		}
		}
		foreach (Clan clan in World.world.clans)
		{
			if (num >= pAsset.max_nameplate_count)
			{
				break;
			}
			if (getPositionForMeta(clan, out var pPosition))
			{
				pManager.prepareNext(pAsset, clan).showTextClanFluid(clan, pPosition);
				num++;
			}
		}
	}
}
