using UnityEngine;
using UnityEngine.UI;
using db;

public static class WorldLogMessageExtensions
{
	public static void clear(this WorldLogMessage pMessage)
	{
		pMessage.unit = null;
	}

	public static void add(this WorldLogMessage pMessage)
	{
		HistoryHud.instance.newHistory(pMessage);
		DBInserter.insertLog(pMessage);
	}

	public static string getFormatedText(this WorldLogMessage pMessage, Text pTextField)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		WorldLogAsset asset = pMessage.getAsset();
		string localeID;
		if (asset.random_ids > 0)
		{
			int pIndex = pMessage.timestamp % asset.random_ids + 1;
			localeID = asset.getLocaleID(pIndex);
		}
		else
		{
			localeID = asset.getLocaleID();
		}
		string pText = LocalizedTextManager.getText(localeID);
		if (asset.text_replacer != null)
		{
			asset.text_replacer(pMessage, ref pText);
		}
		((Graphic)pTextField).color = asset.color;
		return pText;
	}

	public static bool followLocation(this WorldLogMessage pMessage)
	{
		if (pMessage.hasFollowLocation())
		{
			WorldLog.locationFollow(pMessage.unit);
			return true;
		}
		return false;
	}

	public static void jumpToLocation(this WorldLogMessage pMessage)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (!pMessage.followLocation())
		{
			Vector3 pPoint = pMessage.getLocation();
			if (pPoint != Vector3.zero && Toolbox.inMapBorder(ref pPoint))
			{
				WorldLog.locationJump(pPoint);
			}
		}
	}

	public static bool hasLocation(this WorldLogMessage pMessage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return pMessage.getLocation() != Vector3.zero;
	}

	public static bool hasFollowLocation(this WorldLogMessage pMessage)
	{
		if (pMessage.unit != null && pMessage.unit.isAlive())
		{
			return true;
		}
		return false;
	}

	public static Vector3 getLocation(this WorldLogMessage pMessage)
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (pMessage.unit != null && pMessage.unit.isAlive())
		{
			return Vector2.op_Implicit(pMessage.unit.current_position);
		}
		if (pMessage.x != -1 && pMessage.y != -1)
		{
			Vector2 pPoint = pMessage.location;
			if (Toolbox.inMapBorder(ref pPoint))
			{
				return Vector2.op_Implicit(pMessage.location);
			}
		}
		return Vector3.zero;
	}

	public static WorldLogAsset getAsset(this WorldLogMessage pMessage)
	{
		return AssetManager.world_log_library.get(pMessage.asset_id);
	}

	public static string getSpecial(this WorldLogMessage pMessage, int pSpecialId)
	{
		return pSpecialId switch
		{
			1 => Toolbox.coloredText(pMessage.special1, pMessage.color_special_1), 
			2 => Toolbox.coloredText(pMessage.special2, pMessage.color_special_2), 
			3 => Toolbox.coloredText(pMessage.special3, pMessage.color_special_3), 
			_ => "", 
		};
	}
}
