using UnityEngine;
using UnityEngine.UI;

public class KnowledgeLibrary : AssetLibrary<KnowledgeAsset>
{
	private KnowledgeAsset units;

	private KnowledgeAsset items;

	private KnowledgeAsset genes;

	private KnowledgeAsset traits;

	private KnowledgeAsset subspecies_traits;

	private KnowledgeAsset culture_traits;

	private KnowledgeAsset language_traits;

	private KnowledgeAsset clan_traits;

	private KnowledgeAsset religion_traits;

	private KnowledgeAsset kingdom_traits;

	private KnowledgeAsset plots;

	public override void init()
	{
		base.init();
		units = add(new KnowledgeAsset
		{
			id = "units",
			path_icon = "ui/Icons/iconInterestingPeople",
			path_icon_easter_egg = "ui/Icons/iconBre",
			button_prefab_path = "ui/RunningIcon",
			window_id = "list_favorite_units",
			get_library = () => AssetManager.actor_library,
			get_asset_sprite = getActorSprite,
			load_button = delegate(Transform pButton, BaseUnlockableAsset pAsset)
			{
				RunningIcon component = ((Component)pButton).GetComponent<RunningIcon>();
				Sprite icon = units.get_asset_sprite(pAsset);
				component.setIcon(icon);
				checkButtonColor(component.getIconImage(), pAsset);
			},
			tip_button_loader = delegate(Transform pButton, BaseUnlockableAsset pAsset)
			{
				ActorAsset tAsset = pAsset as ActorAsset;
				((Component)pButton).GetComponent<TipButton>().setHoverAction(delegate
				{
					TooltipData pData = new TooltipData
					{
						actor_asset = tAsset
					};
					Tooltip.show(pButton, "unit_button", pData);
				});
			},
			show_tooltip = delegate(Transform pButton, BaseUnlockableAsset pAsset)
			{
				ActorAsset actor_asset = (ActorAsset)pAsset;
				TooltipData tooltipData = new TooltipData
				{
					actor_asset = actor_asset
				};
				Tooltip.show(pButton, "unit_button", tooltipData);
				return tooltipData;
			}
		});
		items = add(new KnowledgeAsset
		{
			id = "items",
			path_icon = "ui/Icons/iconEquipmentEditor",
			path_icon_easter_egg = "ui/Icons/civs/civ_armadillo",
			button_prefab_path = "ui/EquipmentButton",
			window_id = "equipment_rain_editor",
			get_library = () => AssetManager.items,
			get_asset_sprite = getAugmentationSprite,
			load_button = delegate(Transform pButton, BaseUnlockableAsset pAsset)
			{
				augmentationButtonLoader<EquipmentButton, EquipmentAsset>(pButton, pAsset);
			},
			show_tooltip = delegate(Transform pButton, BaseUnlockableAsset pAsset)
			{
				EquipmentAsset item_asset = (EquipmentAsset)pAsset;
				TooltipData tooltipData = new TooltipData
				{
					item_asset = item_asset
				};
				Tooltip.show(pButton, "equipment_in_editor", tooltipData);
				return tooltipData;
			}
		});
		genes = add(new KnowledgeAsset
		{
			id = "genes",
			path_icon = "ui/Icons/iconGene",
			path_icon_easter_egg = "ui/Icons/iconGreg",
			button_prefab_path = "ui/genetic_elements/GeneButton",
			window_id = "list_subspecies",
			get_library = () => AssetManager.gene_library,
			get_asset_sprite = getAugmentationSprite,
			load_button = delegate(Transform pButton, BaseUnlockableAsset pAsset)
			{
				augmentationButtonLoader<GeneButton, GeneAsset>(pButton, pAsset);
			},
			show_tooltip = delegate(Transform pButton, BaseUnlockableAsset pAsset)
			{
				GeneAsset gene = (GeneAsset)pAsset;
				TooltipData tooltipData = new TooltipData
				{
					gene = gene
				};
				Tooltip.show(pButton, "gene", tooltipData);
				return tooltipData;
			}
		});
		traits = add(new KnowledgeAsset
		{
			id = "traits",
			path_icon = "ui/Icons/iconEditTrait",
			path_icon_easter_egg = "ui/Icons/iconZombie",
			button_prefab_path = "ui/unit_window_elements/ActorTraitButton",
			window_id = "trait_rain_editor",
			get_library = () => AssetManager.traits,
			get_asset_sprite = getAugmentationSprite,
			load_button = delegate(Transform pButton, BaseUnlockableAsset pAsset)
			{
				traitButtonLoader<ActorTraitButton, ActorTrait>(pButton, pAsset);
			},
			show_tooltip = delegate(Transform pButton, BaseUnlockableAsset pAsset)
			{
				ActorTrait actorTrait = (ActorTrait)pAsset;
				TooltipData tooltipData = new TooltipData
				{
					trait = actorTrait,
					is_editor_augmentation_button = true
				};
				Tooltip.show(pButton, actorTrait.typed_id, tooltipData);
				return tooltipData;
			},
			click_icon_action = delegate(KnowledgeAsset pAsset)
			{
				Config.selected_trait_editor = PowerLibrary.traits_delta_rain_edit.id;
				ScrollWindow.showWindow(pAsset.window_id);
			}
		});
		subspecies_traits = add(new KnowledgeAsset
		{
			id = "subspecies_traits",
			path_icon = "ui/Icons/iconSpecies",
			path_icon_easter_egg = "ui/Icons/subspecies_traits/subspecies_trait_reproduction_budding",
			button_prefab_path = "ui/subspecies_window_elements/SubspeciesTraitButton",
			window_id = "list_subspecies",
			get_library = () => AssetManager.subspecies_traits,
			get_asset_sprite = getAugmentationSprite,
			load_button = delegate(Transform pButton, BaseUnlockableAsset pAsset)
			{
				traitButtonLoader<SubspeciesTraitButton, SubspeciesTrait>(pButton, pAsset);
			},
			show_tooltip = delegate(Transform pButton, BaseUnlockableAsset pAsset)
			{
				SubspeciesTrait subspeciesTrait = (SubspeciesTrait)pAsset;
				TooltipData tooltipData = new TooltipData
				{
					subspecies_trait = subspeciesTrait,
					is_editor_augmentation_button = true
				};
				Tooltip.show(pButton, subspeciesTrait.typed_id, tooltipData);
				return tooltipData;
			}
		});
		culture_traits = add(new KnowledgeAsset
		{
			id = "culture_traits",
			path_icon = "ui/Icons/iconCulture",
			path_icon_easter_egg = "ui/Icons/iconEvilMage",
			button_prefab_path = "ui/culture_window_elements/CultureTraitButton",
			window_id = "list_cultures",
			get_library = () => AssetManager.culture_traits,
			get_asset_sprite = getAugmentationSprite,
			load_button = delegate(Transform pButton, BaseUnlockableAsset pAsset)
			{
				traitButtonLoader<CultureTraitButton, CultureTrait>(pButton, pAsset);
			},
			show_tooltip = delegate(Transform pButton, BaseUnlockableAsset pAsset)
			{
				CultureTrait cultureTrait = (CultureTrait)pAsset;
				TooltipData tooltipData = new TooltipData
				{
					culture_trait = cultureTrait,
					is_editor_augmentation_button = true
				};
				Tooltip.show(pButton, cultureTrait.typed_id, tooltipData);
				return tooltipData;
			}
		});
		language_traits = add(new KnowledgeAsset
		{
			id = "language_traits",
			path_icon = "ui/Icons/iconLanguage",
			path_icon_easter_egg = "ui/Icons/iconShrug",
			button_prefab_path = "ui/language_window_elements/LanguageTraitButton",
			window_id = "list_languages",
			get_library = () => AssetManager.language_traits,
			get_asset_sprite = getAugmentationSprite,
			load_button = delegate(Transform pButton, BaseUnlockableAsset pAsset)
			{
				traitButtonLoader<LanguageTraitButton, LanguageTrait>(pButton, pAsset);
			},
			show_tooltip = delegate(Transform pButton, BaseUnlockableAsset pAsset)
			{
				LanguageTrait languageTrait = (LanguageTrait)pAsset;
				TooltipData tooltipData = new TooltipData
				{
					language_trait = languageTrait,
					is_editor_augmentation_button = true
				};
				Tooltip.show(pButton, languageTrait.typed_id, tooltipData);
				return tooltipData;
			}
		});
		clan_traits = add(new KnowledgeAsset
		{
			id = "clan_traits",
			path_icon = "ui/Icons/iconClan",
			path_icon_easter_egg = "ui/Icons/iconLivingPlants",
			button_prefab_path = "ui/clan_window_elements/ClanTraitButton",
			window_id = "list_clans",
			get_library = () => AssetManager.clan_traits,
			get_asset_sprite = getAugmentationSprite,
			load_button = delegate(Transform pButton, BaseUnlockableAsset pAsset)
			{
				traitButtonLoader<ClanTraitButton, ClanTrait>(pButton, pAsset);
			},
			show_tooltip = delegate(Transform pButton, BaseUnlockableAsset pAsset)
			{
				ClanTrait clanTrait = (ClanTrait)pAsset;
				TooltipData tooltipData = new TooltipData
				{
					clan_trait = clanTrait,
					is_editor_augmentation_button = true
				};
				Tooltip.show(pButton, clanTrait.typed_id, tooltipData);
				return tooltipData;
			}
		});
		religion_traits = add(new KnowledgeAsset
		{
			id = "religion_traits",
			path_icon = "ui/Icons/iconReligion",
			path_icon_easter_egg = "ui/Icons/iconWhiteMage",
			button_prefab_path = "ui/religion_window_elements/ReligionTraitButton",
			window_id = "list_religions",
			get_library = () => AssetManager.religion_traits,
			get_asset_sprite = getAugmentationSprite,
			load_button = delegate(Transform pButton, BaseUnlockableAsset pAsset)
			{
				traitButtonLoader<ReligionTraitButton, ReligionTrait>(pButton, pAsset);
			},
			show_tooltip = delegate(Transform pButton, BaseUnlockableAsset pAsset)
			{
				ReligionTrait religionTrait = (ReligionTrait)pAsset;
				TooltipData tooltipData = new TooltipData
				{
					religion_trait = religionTrait,
					is_editor_augmentation_button = true
				};
				Tooltip.show(pButton, religionTrait.typed_id, tooltipData);
				return tooltipData;
			}
		});
		kingdom_traits = add(new KnowledgeAsset
		{
			id = "kingdom_traits",
			show_in_knowledge_window = false,
			path_icon = "ui/Icons/iconKingdom",
			path_icon_easter_egg = "ui/Icons/iconWhiteMage",
			button_prefab_path = "ui/kingdom_window_elements/KingdomTraitButton",
			window_id = "list_kingdoms",
			get_library = () => AssetManager.kingdoms_traits,
			get_asset_sprite = getAugmentationSprite,
			load_button = delegate(Transform pButton, BaseUnlockableAsset pAsset)
			{
				traitButtonLoader<KingdomTraitButton, KingdomTrait>(pButton, pAsset);
			},
			show_tooltip = delegate(Transform pButton, BaseUnlockableAsset pAsset)
			{
				KingdomTrait kingdomTrait = (KingdomTrait)pAsset;
				TooltipData tooltipData = new TooltipData
				{
					kingdom_trait = kingdomTrait,
					is_editor_augmentation_button = true
				};
				Tooltip.show(pButton, kingdomTrait.typed_id, tooltipData);
				return tooltipData;
			}
		});
		plots = add(new KnowledgeAsset
		{
			id = "plots",
			path_icon = "ui/Icons/iconPlot",
			path_icon_easter_egg = "ui/Icons/actor_traits/iconGenius",
			button_prefab_path = "ui/PlotButton",
			window_id = "list_plots",
			get_library = () => AssetManager.plots_library,
			get_asset_sprite = getAugmentationSprite,
			load_button = delegate(Transform pButton, BaseUnlockableAsset pAsset)
			{
				augmentationButtonLoader<PlotButton, PlotAsset>(pButton, pAsset);
			},
			show_tooltip = delegate(Transform pButton, BaseUnlockableAsset pAsset)
			{
				PlotAsset plot_asset = (PlotAsset)pAsset;
				TooltipData tooltipData = new TooltipData
				{
					plot_asset = plot_asset
				};
				Tooltip.show(pButton, "plot_in_editor", tooltipData);
				return tooltipData;
			}
		});
	}

	private Sprite getActorSprite(BaseUnlockableAsset pAsset)
	{
		return ((ActorAsset)pAsset).getSpriteIcon();
	}

	private Sprite getAugmentationSprite(BaseUnlockableAsset pAsset)
	{
		return ((BaseAugmentationAsset)pAsset).getSprite();
	}

	private TButton augmentationButtonLoader<TButton, TAsset>(Transform pButton, BaseUnlockableAsset pAsset) where TButton : AugmentationButton<TAsset> where TAsset : BaseAugmentationAsset
	{
		TButton component = ((Component)pButton).GetComponent<TButton>();
		component.load(pAsset as TAsset);
		((Behaviour)component.locked_bg).enabled = false;
		checkButtonColor(component.image, pAsset);
		component.is_editor_button = true;
		return component;
	}

	private TButton traitButtonLoader<TButton, TAsset>(Transform pButton, BaseUnlockableAsset pAsset) where TButton : TraitButton<TAsset> where TAsset : BaseTrait<TAsset>
	{
		TButton val = augmentationButtonLoader<TButton, TAsset>(pButton, pAsset);
		checkButtonColor(val.image, pAsset);
		return val;
	}

	private void checkButtonColor(Image pImage, BaseUnlockableAsset pAsset)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (pAsset.isUnlockedByPlayer())
		{
			((Graphic)pImage).color = Toolbox.color_white;
		}
		else
		{
			((Graphic)pImage).color = Toolbox.color_black;
		}
	}

	public override void editorDiagnosticLocales()
	{
		base.editorDiagnosticLocales();
		foreach (KnowledgeAsset item in list)
		{
			checkLocale(item, item.getLocaleID());
		}
	}
}
