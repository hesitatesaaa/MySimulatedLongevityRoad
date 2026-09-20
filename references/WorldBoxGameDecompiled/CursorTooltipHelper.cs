using UnityEngine;

public static class CursorTooltipHelper
{
	private static float _timeout = 0f;

	private static float _timeout_interval = 0.2f;

	public static bool is_over_meta;

	public static void update()
	{
		if (!InputHelpers.mouseSupported)
		{
			return;
		}
		if (World.world.isBusyWithUI())
		{
			cancel();
			return;
		}
		if (isInputHappening())
		{
			cancel();
			return;
		}
		bool flag = false;
		flag = updateGameplayTooltip();
		if (!flag)
		{
			flag = updateMapTooltip();
		}
		if (!flag)
		{
			cancel();
		}
	}

	private static bool updateGameplayTooltip()
	{
		if (!PlayerConfig.optionBoolEnabled("tooltip_units"))
		{
			return false;
		}
		if (!MapBox.isRenderGameplay())
		{
			return false;
		}
		Actor last_actor = UnitSelectionEffect.last_actor;
		if (last_actor == null)
		{
			return false;
		}
		if (!last_actor.isAlive())
		{
			return false;
		}
		if (_timeout > 0f)
		{
			_timeout -= World.world.delta_time;
			return true;
		}
		string text = "actor";
		if (!HotkeyLibrary.many_mod.isHolding() || !showTooltipForSelectedMeta(last_actor))
		{
			if (last_actor.isKing())
			{
				text = "actor_king";
			}
			else if (last_actor.isCityLeader())
			{
				text = "actor_leader";
			}
			Tooltip.hideTooltip(last_actor, pOnlySimObjects: true, text);
			Tooltip.show(last_actor, text, new TooltipData
			{
				actor = last_actor,
				tooltip_scale = 0.7f,
				is_sim_tooltip = true,
				sound_allowed = false
			});
		}
		return true;
	}

	private static bool showTooltipForSelectedMeta(Actor pActor)
	{
		MetaType currentMapBorderMode = Zones.getCurrentMapBorderMode();
		TooltipData tooltipData = new TooltipData
		{
			tooltip_scale = 0.7f,
			is_sim_tooltip = true
		};
		object obj = null;
		string text;
		switch (currentMapBorderMode)
		{
		case MetaType.Alliance:
			if (!pActor.kingdom.hasAlliance())
			{
				return false;
			}
			text = "alliance";
			tooltipData.alliance = pActor.kingdom.getAlliance();
			obj = pActor.kingdom.getAlliance();
			break;
		case MetaType.Kingdom:
			if (!pActor.isKingdomCiv())
			{
				return false;
			}
			text = "kingdom";
			tooltipData.kingdom = pActor.kingdom;
			obj = pActor.kingdom;
			break;
		case MetaType.City:
			if (!pActor.hasCity())
			{
				return false;
			}
			text = "city";
			tooltipData.city = pActor.city;
			obj = pActor.city;
			break;
		case MetaType.Clan:
			if (!pActor.hasClan())
			{
				return false;
			}
			text = "clan";
			tooltipData.clan = pActor.clan;
			obj = pActor.clan;
			break;
		case MetaType.Culture:
			if (!pActor.hasCulture())
			{
				return false;
			}
			text = "culture";
			tooltipData.culture = pActor.culture;
			obj = pActor.culture;
			break;
		case MetaType.Family:
			if (!pActor.hasFamily())
			{
				return false;
			}
			text = "family";
			tooltipData.family = pActor.family;
			obj = pActor.family;
			break;
		case MetaType.Language:
			if (!pActor.hasLanguage())
			{
				return false;
			}
			text = "language";
			tooltipData.language = pActor.language;
			obj = pActor.language;
			break;
		case MetaType.Religion:
			if (!pActor.hasReligion())
			{
				return false;
			}
			text = "religion";
			tooltipData.religion = pActor.religion;
			obj = pActor.religion;
			break;
		case MetaType.Subspecies:
			if (!pActor.hasSubspecies())
			{
				return false;
			}
			text = "subspecies";
			tooltipData.subspecies = pActor.subspecies;
			obj = pActor.subspecies;
			break;
		default:
			return false;
		}
		Tooltip.hideTooltip(obj, pOnlySimObjects: true, text);
		Tooltip.show(obj, text, tooltipData);
		return true;
	}

	private static bool updateMapTooltip()
	{
		if (!PlayerConfig.optionBoolEnabled("tooltip_zones"))
		{
			return false;
		}
		if (!MapBox.isRenderMiniMap())
		{
			return false;
		}
		if (!Zones.showMapBorders())
		{
			return false;
		}
		if (_timeout > 0f)
		{
			_timeout -= World.world.delta_time;
			return true;
		}
		bool result = false;
		WorldTile mouseTilePosCachedFrame = World.world.getMouseTilePosCachedFrame();
		MetaTypeAsset cachedMapMetaAsset = World.world.getCachedMapMetaAsset();
		if (mouseTilePosCachedFrame != null && cachedMapMetaAsset != null)
		{
			result = cachedMapMetaAsset.check_cursor_tooltip(mouseTilePosCachedFrame.zone, cachedMapMetaAsset, cachedMapMetaAsset.getZoneOptionState());
		}
		return result;
	}

	private static void cancel()
	{
		Tooltip.hideTooltip(null, pOnlySimObjects: true, string.Empty);
		resetTimout();
	}

	private static bool isInputHappening()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
		{
			return true;
		}
		if (Input.mouseScrollDelta.y != 0f)
		{
			return true;
		}
		if (HotkeyLibrary.many_mod.isHolding())
		{
			return false;
		}
		return Input.anyKey;
	}

	private static void resetTimout()
	{
		_timeout = _timeout_interval;
	}
}
