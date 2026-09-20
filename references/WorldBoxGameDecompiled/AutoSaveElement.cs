using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using Humanizer;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class AutoSaveElement : MonoBehaviour, IPointerMoveHandler, IEventSystemHandler
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static UnityAction _003C_003E9__12_0;

		internal void _003CAwake_003Eb__12_0()
		{
			if (InputHelpers.mouseSupported)
			{
				Tooltip.hideTooltip();
			}
		}
	}

	[SerializeField]
	private Image _preview;

	[SerializeField]
	private Text _save_name;

	[SerializeField]
	private Text _save_time_ago;

	[SerializeField]
	private CountUpOnClick _kingdoms;

	[SerializeField]
	private CountUpOnClick _cities;

	[SerializeField]
	private CountUpOnClick _population;

	[SerializeField]
	private CountUpOnClick _mobs;

	[SerializeField]
	private CountUpOnClick _age;

	[SerializeField]
	private Button _button;

	[SerializeField]
	private GameObject _premium_icon;

	private string _world_path;

	private MapMetaData _meta_data;

	private void Awake()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		Button button = _button;
		object obj = _003C_003Ec._003C_003E9__12_0;
		if (obj == null)
		{
			UnityAction val = delegate
			{
				if (InputHelpers.mouseSupported)
				{
					Tooltip.hideTooltip();
				}
			};
			_003C_003Ec._003C_003E9__12_0 = val;
			obj = (object)val;
		}
		button.OnHoverOut((UnityAction)obj);
	}

	public void OnPointerMove(PointerEventData pData)
	{
		if (InputHelpers.mouseSupported && !Tooltip.anyActive())
		{
			tooltipAction();
		}
	}

	private void tooltipAction()
	{
		if (_meta_data != null && Config.tooltips_active)
		{
			_meta_data.temp_date_string = SaveManager.getMapCreationTime(_world_path);
			Tooltip.show(_button, "map_meta", new TooltipData
			{
				map_meta = _meta_data
			});
		}
	}

	public void load(AutoSaveData pData)
	{
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		_world_path = pData.path;
		string text = SaveManager.generatePngSmallPreviewPath(pData.path);
		if (!string.IsNullOrEmpty(text) && File.Exists(text))
		{
			byte[] array = File.ReadAllBytes(text);
			Texture2D val = new Texture2D(32, 32);
			((Texture)val).anisoLevel = 0;
			((Texture)val).filterMode = (FilterMode)0;
			if (ImageConversion.LoadImage(val, array))
			{
				Sprite sprite = Sprite.Create(val, new Rect(0f, 0f, 32f, 32f), new Vector2(0.5f, 0.5f));
				_preview.sprite = sprite;
			}
		}
		_meta_data = SaveManager.getMetaFor(pData.path);
		_save_name.text = _meta_data.mapStats.name;
		((Graphic)_save_name).color = _meta_data.mapStats.getArchitectMood().getColorText();
		_kingdoms.setValue(_meta_data.kingdoms);
		_cities.setValue(_meta_data.cities);
		_population.setValue(_meta_data.population);
		_mobs.setValue(_meta_data.mobs);
		_age.setValue(Date.getYear(_meta_data.mapStats.world_time));
		string text2 = "";
		string text3 = "";
		try
		{
			DateTime dateTime = Epoch.toDateTime(pData.timestamp);
			CultureInfo culture = LocalizedTextManager.getCulture();
			DateTime dateTime2 = DateTime.UtcNow.AddDays(7.0);
			if (dateTime.Year < 2017)
			{
				text2 = "GREG";
			}
			else if (dateTime > dateTime2)
			{
				text2 = "DREDD";
			}
			else if (LocalizedTextManager.cultureSupported())
			{
				DateTime dateTime3 = dateTime;
				CultureInfo cultureInfo = culture;
				text2 = DateHumanizeExtensions.Humanize(dateTime3, true, (DateTime?)null, cultureInfo);
			}
			else
			{
				string shortDatePattern = culture.DateTimeFormat.ShortDatePattern;
				text2 = dateTime.ToString(shortDatePattern, culture);
			}
		}
		catch (Exception ex)
		{
			Debug.Log((object)("failed with " + text3));
			Debug.LogError((object)ex);
		}
		_save_time_ago.text = text2;
		((Object)((Component)this).gameObject).name = "AutoSaveElement_" + pData.timestamp;
	}

	public void clickLoadAutoSave()
	{
		SaveManager.setCurrentPath(_world_path);
		ScrollWindow.showWindow("load_world");
	}

	private void OnDisable()
	{
		_meta_data = null;
		if ((Object)(object)_preview != (Object)null)
		{
			_preview.sprite = null;
		}
	}
}
