public class MetaRepresentationLibrary : AssetLibrary<MetaRepresentationAsset>
{
	public override void init()
	{
		base.init();
		add(new MetaRepresentationAsset
		{
			id = "alliance",
			meta_type = MetaType.Alliance,
			title_locale = "statistics_breakdown_alliances",
			icon_getter = (IMetaObject _) => "iconAlliance",
			check_has_meta = (Actor pActor) => pActor.hasKingdom() && pActor.kingdom.isCiv() && pActor.kingdom.hasAlliance(),
			meta_getter = (Actor pActor) => pActor.kingdom.getAlliance(),
			meta_getter_total = (Actor pActor) => new ListPool<IMetaObject> { pActor.kingdom.getAlliance() },
			world_units_getter = () => World.world.units.units_only_civ,
			general_icon_path = "iconAlliance",
			show_species_icon = false
		});
		add(new MetaRepresentationAsset
		{
			id = "army",
			meta_type = MetaType.Army,
			title_locale = "statistics_breakdown_armies",
			icon_getter = (IMetaObject _) => "iconArmy",
			check_has_meta = (Actor pActor) => pActor.hasArmy(),
			meta_getter = (Actor pActor) => pActor.city.getArmy(),
			meta_getter_total = (Actor pActor) => new ListPool<IMetaObject> { pActor.city.getArmy() },
			world_units_getter = () => World.world.units.units_only_civ,
			general_icon_path = "iconArmy"
		});
		add(new MetaRepresentationAsset
		{
			id = "city",
			meta_type = MetaType.City,
			title_locale = "statistics_breakdown_cities",
			icon_getter = (IMetaObject _) => "iconCity",
			check_has_meta = (Actor pActor) => pActor.hasCity(),
			meta_getter = (Actor pActor) => pActor.city,
			meta_getter_total = (Actor pActor) => new ListPool<IMetaObject> { pActor.city },
			world_units_getter = () => World.world.units.units_only_civ,
			general_icon_path = "iconCity"
		});
		add(new MetaRepresentationAsset
		{
			id = "clan",
			meta_type = MetaType.Clan,
			title_locale = "statistics_breakdown_clans",
			icon_getter = (IMetaObject _) => "iconClan",
			check_has_meta = (Actor pActor) => pActor.hasClan(),
			meta_getter = (Actor pActor) => pActor.clan,
			meta_getter_total = (Actor pActor) => new ListPool<IMetaObject> { pActor.clan },
			world_units_getter = () => World.world.units.units_only_civ,
			general_icon_path = "iconClan"
		});
		add(new MetaRepresentationAsset
		{
			id = "culture",
			meta_type = MetaType.Culture,
			title_locale = "statistics_breakdown_cultures",
			icon_getter = (IMetaObject _) => "iconCulture",
			check_has_meta = (Actor pActor) => pActor.hasCulture(),
			meta_getter = (Actor pActor) => pActor.culture,
			meta_getter_total = (Actor pActor) => new ListPool<IMetaObject> { pActor.culture },
			world_units_getter = () => World.world.units.units_only_civ,
			general_icon_path = "iconCulture",
			show_none_percent = true
		});
		add(new MetaRepresentationAsset
		{
			id = "kingdom",
			meta_type = MetaType.Kingdom,
			title_locale = "statistics_breakdown_kingdoms",
			icon_getter = (IMetaObject _) => "iconKingdom",
			check_has_meta = (Actor pActor) => pActor.hasKingdom() && pActor.kingdom.isCiv(),
			meta_getter = (Actor pActor) => pActor.kingdom,
			meta_getter_total = (Actor pActor) => new ListPool<IMetaObject> { pActor.kingdom },
			world_units_getter = () => World.world.units.units_only_civ,
			general_icon_path = "iconKingdom"
		});
		add(new MetaRepresentationAsset
		{
			id = "language",
			meta_type = MetaType.Language,
			title_locale = "statistics_breakdown_languages",
			icon_getter = (IMetaObject _) => "iconLanguage",
			check_has_meta = (Actor pActor) => pActor.hasLanguage(),
			meta_getter = (Actor pActor) => pActor.language,
			meta_getter_total = (Actor pActor) => new ListPool<IMetaObject> { pActor.language },
			world_units_getter = () => World.world.units.units_only_civ,
			general_icon_path = "iconLanguage",
			show_none_percent = true
		});
		add(new MetaRepresentationAsset
		{
			id = "religion",
			meta_type = MetaType.Religion,
			title_locale = "statistics_breakdown_religions",
			icon_getter = (IMetaObject _) => "iconReligion",
			check_has_meta = (Actor pActor) => pActor.hasReligion(),
			meta_getter = (Actor pActor) => pActor.religion,
			meta_getter_total = (Actor pActor) => new ListPool<IMetaObject> { pActor.religion },
			world_units_getter = () => World.world.units.units_only_civ,
			general_icon_path = "iconReligion",
			show_none_percent = true
		});
		add(new MetaRepresentationAsset
		{
			id = "subspecies",
			meta_type = MetaType.Subspecies,
			title_locale = "statistics_breakdown_subspecies",
			icon_getter = (IMetaObject pMeta) => pMeta.getActorAsset().icon,
			check_has_meta = (Actor pActor) => pActor.hasSubspecies(),
			meta_getter = (Actor pActor) => pActor.subspecies,
			meta_getter_total = (Actor pActor) => new ListPool<IMetaObject> { pActor.subspecies },
			world_units_getter = () => World.world.units.units_only_alive,
			general_icon_path = "iconSubspecies",
			show_species_icon = false,
			show_none_percent_for_total = false
		});
		add(new MetaRepresentationAsset
		{
			id = "war",
			meta_type = MetaType.War,
			title_locale = "statistics_breakdown_wars",
			icon_getter = (IMetaObject _) => "iconWar",
			check_has_meta = delegate(Actor pActor)
			{
				if (!pActor.hasKingdom() || !pActor.kingdom.isCiv())
				{
					return false;
				}
				foreach (War war in pActor.kingdom.getWars())
				{
					if (!war.hasEnded())
					{
						return true;
					}
				}
				return false;
			},
			meta_getter_total = (Actor pActor) => new ListPool<IMetaObject>(pActor.kingdom.getWars()),
			world_units_getter = () => World.world.units.units_only_civ,
			general_icon_path = "iconWar",
			show_species_icon = false
		});
	}

	public MetaRepresentationAsset getAsset(MetaType pType)
	{
		return get(pType.AsString());
	}

	public bool hasAsset(MetaType pType)
	{
		return has(pType.AsString());
	}

	public override void editorDiagnosticLocales()
	{
		base.editorDiagnosticLocales();
		foreach (MetaRepresentationAsset item in list)
		{
			checkLocale(item, item.title_locale);
		}
	}
}
