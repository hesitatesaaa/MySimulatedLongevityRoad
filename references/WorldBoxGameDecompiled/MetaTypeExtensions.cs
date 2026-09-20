using UnityEngine;

public static class MetaTypeExtensions
{
	public static MetaTypeAsset getAsset(this MetaType pType)
	{
		return AssetManager.meta_type_library.getAsset(pType);
	}

	public static bool isNone(this MetaType pType)
	{
		return pType == MetaType.None;
	}

	public static int getZoneState(this MetaType pType)
	{
		return AssetManager.meta_type_library.getAsset(pType).getZoneOptionState();
	}

	public static string AsString(this MetaType pType)
	{
		switch (pType)
		{
		case MetaType.World:
			return "world";
		case MetaType.Subspecies:
			return "subspecies";
		case MetaType.Family:
			return "family";
		case MetaType.Army:
			return "army";
		case MetaType.Language:
			return "language";
		case MetaType.Culture:
			return "culture";
		case MetaType.Religion:
			return "religion";
		case MetaType.Clan:
			return "clan";
		case MetaType.City:
			return "city";
		case MetaType.Kingdom:
			return "kingdom";
		case MetaType.Alliance:
			return "alliance";
		case MetaType.War:
			return "war";
		case MetaType.Plot:
			return "plot";
		case MetaType.Unit:
			return "unit";
		case MetaType.Building:
			return "building";
		case MetaType.Item:
			return "item";
		case MetaType.Special:
			return "special";
		case MetaType.None:
			return "none";
		default:
			Debug.LogError((object)("MetaTypeExtensions.AsString missing option for : " + pType));
			return pType.ToString().ToLower();
		}
	}
}
