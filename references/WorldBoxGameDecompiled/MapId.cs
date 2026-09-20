using UnityEngine;
using UnityEngine.UI;

public class MapId : MonoBehaviour
{
	public Button continueButton;

	public InputField mapIdText;

	public Text statusText;

	public static string mapId;

	public static Map map;

	public Sprite buttonOn;

	public Sprite buttonOff;

	public static string formattedMapId
	{
		get
		{
			if (!string.IsNullOrEmpty(mapId) && mapId.Length == 12)
			{
				return "WB-" + mapId.Substring(0, 4) + "-" + mapId.Substring(4, 4) + "-" + mapId.Substring(8, 4);
			}
			return mapId;
		}
	}
}
