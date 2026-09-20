using System.IO;
using UnityEngine;

public class ColorTool : MonoBehaviour
{
	public string colorString;

	public GameObject prefabKingdom;

	public GameObject prefabClan;

	public GameObject prefabCulture;

	public GameObject prefabAlliance;

	public Transform container;

	public string last_editor = "";

	private void resetCoords()
	{
	}

	public void InitKingdoms()
	{
		cleanup();
		last_editor = "kingdoms";
		KingdomColorsLibrary kingdomColorsLibrary = new KingdomColorsLibrary();
		kingdomColorsLibrary.init();
		kingdomColorsLibrary.post_init();
		foreach (ColorAsset item in kingdomColorsLibrary.list)
		{
			createColorToolElement(item, prefabKingdom, last_editor);
		}
	}

	public void InitCultures()
	{
		cleanup();
		last_editor = "cultures";
		CultureColorsLibrary cultureColorsLibrary = new CultureColorsLibrary();
		cultureColorsLibrary.init();
		cultureColorsLibrary.post_init();
		foreach (ColorAsset item in cultureColorsLibrary.list)
		{
			createColorToolElement(item, prefabCulture, last_editor);
		}
	}

	public void InitClans()
	{
		cleanup();
		last_editor = "clans";
		ClanColorsLibrary clanColorsLibrary = new ClanColorsLibrary();
		clanColorsLibrary.init();
		clanColorsLibrary.post_init();
		foreach (ColorAsset item in clanColorsLibrary.list)
		{
			createColorToolElement(item, prefabClan, last_editor);
		}
	}

	public void cleanup()
	{
		resetCoords();
		while (container.childCount > 0)
		{
			Object.DestroyImmediate((Object)(object)((Component)container.GetChild(0)).gameObject);
		}
	}

	private void createColorToolElement(ColorAsset pColor, GameObject pPrefab, string pWhat)
	{
		ColorToolElement component = Object.Instantiate<GameObject>(pPrefab, container).GetComponent<ColorToolElement>();
		if (last_editor == "kingdoms")
		{
			component.createKingdom(pColor);
		}
		else if (last_editor == "clans")
		{
			component.createClans(pColor);
		}
		else if (last_editor == "cultures")
		{
			component.createCulture(pColor);
		}
		((Object)((Component)component).transform).name = pColor.index_id + "-" + pColor.id;
		((Component)component).transform.SetSiblingIndex(pColor.index_id);
	}

	public void saveEditor()
	{
		if (last_editor == "kingdoms")
		{
			saveKingdoms();
		}
		else if (last_editor == "clans")
		{
			saveClans();
		}
		else if (last_editor == "cultures")
		{
			saveCultures();
		}
	}

	private void convertToolIntoAsset(ColorToolElement pTool, ColorAsset pAsset)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		pAsset.color_main = Toolbox.colorToHex(Color32.op_Implicit(pTool.colorMain), pAlpha: false);
		pAsset.color_main_2 = Toolbox.colorToHex(Color32.op_Implicit(pTool.colorMain2), pAlpha: false);
		pAsset.color_banner = Toolbox.colorToHex(Color32.op_Implicit(pTool.colorBanner), pAlpha: false);
		pAsset.color_text = Toolbox.colorToHex(Color32.op_Implicit(pTool.colorText), pAlpha: false);
		pAsset.id = pTool.id;
		pAsset.favorite = pTool.favorite;
	}

	private void saveKingdoms()
	{
		KingdomColorsLibrary pLibrary = new KingdomColorsLibrary();
		saveLib(pLibrary);
	}

	private void saveCultures()
	{
		CultureColorsLibrary pLibrary = new CultureColorsLibrary();
		saveLib(pLibrary);
	}

	private void saveClans()
	{
		ClanColorsLibrary pLibrary = new ClanColorsLibrary();
		saveLib(pLibrary);
	}

	private void saveLib(ColorLibrary pLibrary)
	{
		for (int i = 0; i < container.childCount; i++)
		{
			ColorToolElement component = ((Component)container.GetChild(i)).GetComponent<ColorToolElement>();
			ColorAsset colorAsset = new ColorAsset();
			convertToolIntoAsset(component, colorAsset);
			colorAsset.index_id = i;
			pLibrary.list.Add(colorAsset);
		}
		string contents = JsonUtility.ToJson((object)pLibrary, true);
		File.WriteAllText(pLibrary.getEditorPathForSave(), contents);
	}
}
