using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.UnityConverters.Math;
using UnityEngine;

public abstract class BaseAssetLibrary
{
	private const string ERROR_COLOR_WHITE = "#FFFFFF";

	private const string ERROR_COLOR_RED = "#FF3232";

	private const string ERROR_COLOR_YELLOW = "#FFF832";

	private const string ERROR_COLOR_MAIN = "#D2B7FF";

	[JsonProperty(Order = -1)]
	public string id = "ASSET_LIBRARY";

	protected static int _latest_hash = 1;

	private static JsonSerializer _json_serializer_internal = null;

	private static JsonSerializer _json_serializer
	{
		get
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Expected O, but got Unknown
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Expected O, but got Unknown
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Expected O, but got Unknown
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Expected O, but got Unknown
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Expected O, but got Unknown
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Expected O, but got Unknown
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Expected O, but got Unknown
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Expected O, but got Unknown
			//IL_00cf: Expected O, but got Unknown
			if (_json_serializer_internal == null)
			{
				_json_serializer_internal = JsonSerializer.Create(new JsonSerializerSettings
				{
					DefaultValueHandling = (DefaultValueHandling)1,
					Formatting = (Formatting)1,
					ReferenceLoopHandling = (ReferenceLoopHandling)1,
					Culture = CultureInfo.InvariantCulture,
					ContractResolver = (IContractResolver)(object)new OrderedContractResolver(),
					Converters = { (JsonConverter)(object)new DelegateConverter() },
					Converters = { (JsonConverter)new StringEnumConverter() },
					Converters = { (JsonConverter)new Color32Converter() },
					Converters = { (JsonConverter)new ColorConverter() },
					Converters = { (JsonConverter)new Vector2Converter() },
					Converters = { (JsonConverter)new Vector2IntConverter() },
					Converters = { (JsonConverter)new Vector3Converter() },
					Converters = { (JsonConverter)new Vector3IntConverter() },
					Converters = { (JsonConverter)new Vector4Converter() }
				});
			}
			return _json_serializer_internal;
		}
	}

	public virtual int total_items => 0;

	public virtual void init()
	{
		_ = Application.version;
	}

	public void exportAssets()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected O, but got Unknown
		if (id.StartsWith("beh") || id.StartsWith("debug"))
		{
			return;
		}
		string text = "GenAssets/wbassets" + "/" + id + ".json";
		try
		{
			using FileStream stream = new FileStream(text, FileMode.Create, FileAccess.Write, FileShare.None);
			using StreamWriter streamWriter = new StreamWriter(stream)
			{
				NewLine = "\n"
			};
			JsonTextWriter val = new JsonTextWriter((TextWriter)streamWriter)
			{
				Formatting = (Formatting)1,
				Indentation = 4,
				IndentChar = ' '
			};
			try
			{
				_json_serializer.Serialize((JsonWriter)(object)val, (object)this);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("Failed to export assets to " + text + ": " + ex.Message));
		}
	}

	public void importAssets()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		string text = "GenAssets/wbassets" + "/" + id + ".json";
		if (!File.Exists(text))
		{
			Debug.LogError((object)("File not found: " + text));
			return;
		}
		using FileStream stream = new FileStream(text, FileMode.Open, FileAccess.Read, FileShare.Read);
		using StreamReader streamReader = new StreamReader(stream);
		JsonTextReader val = new JsonTextReader((TextReader)streamReader);
		try
		{
			_json_serializer.Populate((JsonReader)(object)val, (object)this);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public virtual void post_init()
	{
	}

	public virtual void linkAssets()
	{
	}

	public virtual void editorDiagnostic()
	{
		editorDiagnosticLocales();
	}

	public virtual void editorDiagnosticLocales()
	{
	}

	public virtual void checkLocale(Asset pAsset, string pLocaleID)
	{
		if (!string.IsNullOrEmpty(pLocaleID) && !LocalizedTextManager.stringExists(pLocaleID))
		{
			logAssetError("<e>" + pAsset.id + "</e>: Missing translation key", pLocaleID);
			AssetManager.missing_locale_keys.Add(pLocaleID);
		}
	}

	internal bool hasSpriteInResources(string pPath)
	{
		if ((Object)(object)SpriteTextureLoader.getSprite(pPath) == (Object)null)
		{
			return false;
		}
		return true;
	}

	internal bool hasSpriteInResourcesDebug(string pPath)
	{
		string text = Path.Combine(Path.Combine("Assets/Resources", pPath).Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
		if (File.Exists(text + ".png"))
		{
			return true;
		}
		if (Directory.Exists(text) && Directory.GetFiles(text, "*.png", SearchOption.TopDirectoryOnly).Length != 0)
		{
			return true;
		}
		return false;
	}

	protected void logErrorOpposites(string pMainTraitID, string pOppositeTraitID)
	{
		Debug.LogError((object)("<color=#FF3232>[" + pMainTraitID + "]</color> has opposite <color=#FFF832>[" + pOppositeTraitID + "]</color>, but <color=#FFF832>[" + pOppositeTraitID + "]</color> doesn't have opposite <color=#FF3232>[" + pMainTraitID + "]</color>"));
	}

	private static string formatLog(string pMessage, string pRightPart = null)
	{
		if (pMessage.Contains("<"))
		{
			pMessage = pMessage.Replace("<e>", "<b><color=#FFFFFF>");
			pMessage = pMessage.Replace("</e>", "</color></b>");
		}
		string text = "<color=#D2B7FF>" + pMessage.Trim() + "</color>";
		if (!string.IsNullOrEmpty(pRightPart))
		{
			text = text + " : <b><color=#FFF832>" + pRightPart.Trim() + "</color></b>";
		}
		return text;
	}

	public static void logAssetLog(string pMessage, string pRightPart = null)
	{
		Debug.Log((object)formatLog(pMessage, pRightPart));
	}

	public static void logAssetError(string pMessage, string pRightPart = null)
	{
		Debug.LogError((object)formatLog(pMessage, pRightPart));
	}

	public virtual IEnumerable<Asset> getList()
	{
		yield break;
	}
}
