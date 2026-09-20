using UnityEngine;
using UnityEngine.UI;

public class ColorToolElement : MonoBehaviour
{
	[Header("Edit Colors")]
	public Color colorMain;

	public Color colorMain2;

	public Color colorBanner;

	public Color colorText;

	[Header("Edit Asset Name / Id")]
	public string id;

	public bool favorite;

	[Header("Other Stuff")]
	[Space(30f)]
	public Image background;

	public Image icon;

	public Text text;

	public Image sprite_favorite;

	public Image borderInside;

	public Image borderOutside;

	[HideInInspector]
	public ColorAsset color_asset;

	public Image test_house;

	public Image test_face;

	public Sprite house_default_sprite;

	public Sprite face_default_sprite;

	public int debug_index;

	public void createKingdom(ColorAsset pColor)
	{
		color_asset = pColor;
	}

	public void createCulture(ColorAsset pColor)
	{
		color_asset = pColor;
		setColorsForObjects(pColor);
		saveColors(pColor);
	}

	public void createClans(ColorAsset pColor)
	{
		color_asset = pColor;
		string random = AssetManager.clan_banners_library.main.backgrounds.GetRandom();
		string random2 = AssetManager.clan_banners_library.main.icons.GetRandom();
		background.sprite = SpriteTextureLoader.getSprite(random);
		icon.sprite = SpriteTextureLoader.getSprite(random2);
		setColorsForObjects(pColor);
		saveColors(pColor);
	}

	private void setColorsForObjects(ColorAsset pColorAsset)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)borderInside).color = Color32.op_Implicit(pColorAsset.getColorBorderInsideAlpha32());
		((Graphic)borderOutside).color = pColorAsset.getColorMainSecond();
		((Graphic)background).color = pColorAsset.getColorMainSecond();
		((Graphic)icon).color = pColorAsset.getColorBanner();
		((Graphic)text).color = pColorAsset.getColorText();
		favorite = pColorAsset.favorite;
		id = pColorAsset.id;
		text.text = pColorAsset.id + " |  " + pColorAsset.index_id;
		debug_index = pColorAsset.index_id;
		if ((Object)(object)test_house != (Object)null && (Object)(object)house_default_sprite != (Object)null)
		{
			test_house.sprite = DynamicSpriteCreator.createNewSpriteForDebug(house_default_sprite, pColorAsset);
		}
		if ((Object)(object)test_face != (Object)null && (Object)(object)face_default_sprite != (Object)null)
		{
			test_face.sprite = DynamicSpriteCreator.createNewSpriteForDebug(face_default_sprite, pColorAsset);
		}
		if ((Object)(object)sprite_favorite != (Object)null)
		{
			((Component)sprite_favorite).gameObject.SetActive(favorite);
		}
	}

	private void OnValidate()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		if (color_asset != null)
		{
			color_asset.color_main = Toolbox.colorToHex(Color32.op_Implicit(colorMain), pAlpha: false);
			color_asset.color_main_2 = Toolbox.colorToHex(Color32.op_Implicit(colorMain2), pAlpha: false);
			color_asset.color_banner = Toolbox.colorToHex(Color32.op_Implicit(colorBanner), pAlpha: false);
			color_asset.color_text = Toolbox.colorToHex(Color32.op_Implicit(colorText), pAlpha: false);
			color_asset.id = id;
			color_asset.favorite = favorite;
			color_asset.setEditorColors(colorMain, colorMain2, colorBanner, colorText);
			setColorsForObjects(color_asset);
		}
	}

	private void saveColors(ColorAsset pColor)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		colorMain = pColor.getColorMain();
		colorMain2 = pColor.getColorMainSecond();
		colorBanner = pColor.getColorBanner();
		colorText = pColor.getColorText();
	}
}
