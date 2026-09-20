using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class CanvasNotch : MonoBehaviour
{
	private bool screenChangeVarsInitialized;

	private bool ranFirstTime;

	private ScreenOrientation lastOrientation;

	private Vector2 lastResolution;

	private Rect lastSafeArea;

	private Rect lastCanvasRect;

	private RectTransform safeAreaTransform;

	private Canvas _canvas;

	private void Awake()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		_canvas = ((Component)((Component)this).gameObject.transform).GetComponentInParent<Canvas>();
		safeAreaTransform = ((Component)this).GetComponent<RectTransform>();
		if (!screenChangeVarsInitialized)
		{
			lastOrientation = Screen.orientation;
			lastResolution.x = Screen.width;
			lastResolution.y = Screen.height;
			lastSafeArea = Screen.safeArea;
			screenChangeVarsInitialized = true;
		}
	}

	private void Start()
	{
		ApplySafeArea();
	}

	private void Update()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		if (Application.isMobilePlatform && Screen.orientation != lastOrientation)
		{
			OrientationChanged();
		}
		if (Screen.safeArea != lastSafeArea)
		{
			SafeAreaChanged();
		}
		if ((Object)(object)_canvas != (Object)null && _canvas.pixelRect != lastCanvasRect)
		{
			CanvasChanged();
		}
		if ((float)Screen.width != lastResolution.x || (float)Screen.height != lastResolution.y)
		{
			ResolutionChanged();
		}
		if (!ranFirstTime)
		{
			ApplySafeArea();
		}
	}

	private void ApplySafeArea()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_canvas == (Object)null) && !((Object)(object)safeAreaTransform == (Object)null))
		{
			ranFirstTime = true;
			Rect safeArea = Screen.safeArea;
			Rect val = default(Rect);
			((Rect)(ref val))._002Ector(0f, 0f, (float)Screen.width, (float)Screen.height);
			Vector2 val2 = ((Rect)(ref safeArea)).min - ((Rect)(ref val)).min;
			Vector2 val3 = ((Rect)(ref safeArea)).max - ((Rect)(ref val)).max;
			((Rect)(ref safeArea)).min = ((Rect)(ref safeArea)).min - val3;
			((Rect)(ref safeArea)).max = ((Rect)(ref safeArea)).max - val2;
			Vector2 position = ((Rect)(ref safeArea)).position;
			Vector2 anchorMax = ((Rect)(ref safeArea)).position + ((Rect)(ref safeArea)).size;
			ref float x = ref position.x;
			float num = x;
			Rect pixelRect = _canvas.pixelRect;
			x = num / ((Rect)(ref pixelRect)).width;
			ref float y = ref position.y;
			float num2 = y;
			pixelRect = _canvas.pixelRect;
			y = num2 / ((Rect)(ref pixelRect)).height;
			ref float x2 = ref anchorMax.x;
			float num3 = x2;
			pixelRect = _canvas.pixelRect;
			x2 = num3 / ((Rect)(ref pixelRect)).width;
			ref float y2 = ref anchorMax.y;
			float num4 = y2;
			pixelRect = _canvas.pixelRect;
			y2 = num4 / ((Rect)(ref pixelRect)).height;
			safeAreaTransform.anchorMin = position;
			safeAreaTransform.anchorMax = anchorMax;
		}
	}

	private void OrientationChanged()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		lastOrientation = Screen.orientation;
		lastResolution.x = Screen.width;
		lastResolution.y = Screen.height;
		ApplySafeArea();
	}

	private void ResolutionChanged()
	{
		lastResolution.x = Screen.width;
		lastResolution.y = Screen.height;
		ApplySafeArea();
	}

	private void SafeAreaChanged()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		lastSafeArea = Screen.safeArea;
		ApplySafeArea();
	}

	private void CanvasChanged()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		lastCanvasRect = _canvas.pixelRect;
		ApplySafeArea();
	}

	private void debugConsole()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<string, Rect> dictionary = new Dictionary<string, Rect>();
		Debug.Log((object)("amount of cutouts: " + Screen.cutouts.Length));
		dictionary["screen"] = new Rect(0f, 0f, (float)Screen.width, (float)Screen.height);
		dictionary["safearea"] = Screen.safeArea;
		Rect val;
		foreach (string key in dictionary.Keys)
		{
			string[] obj = new string[10] { "[o] ", key, ": x:", null, null, null, null, null, null, null };
			val = dictionary[key];
			obj[3] = ((Rect)(ref val)).x.ToString();
			obj[4] = ", y:";
			val = dictionary[key];
			obj[5] = ((Rect)(ref val)).y.ToString();
			obj[6] = ", w:";
			val = dictionary[key];
			obj[7] = ((Rect)(ref val)).width.ToString();
			obj[8] = ", h:";
			val = dictionary[key];
			obj[9] = ((Rect)(ref val)).height.ToString();
			Debug.Log((object)string.Concat(obj));
		}
		if ((Object)(object)_canvas == (Object)null)
		{
			Debug.Log((object)"canvas not ready");
			return;
		}
		foreach (string key2 in dictionary.Keys)
		{
			string[] obj2 = new string[10] { "[c] ", key2, ": x:", null, null, null, null, null, null, null };
			val = dictionary[key2];
			obj2[3] = (((Rect)(ref val)).x / _canvas.scaleFactor).ToString();
			obj2[4] = ", y:";
			val = dictionary[key2];
			obj2[5] = (((Rect)(ref val)).y / _canvas.scaleFactor).ToString();
			obj2[6] = ", w:";
			val = dictionary[key2];
			obj2[7] = (((Rect)(ref val)).width / _canvas.scaleFactor).ToString();
			obj2[8] = ", h:";
			val = dictionary[key2];
			obj2[9] = (((Rect)(ref val)).height / _canvas.scaleFactor).ToString();
			Debug.Log((object)string.Concat(obj2));
		}
	}

	public CanvasNotch()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		lastOrientation = (ScreenOrientation)5;
		lastResolution = Vector2.zero;
		lastSafeArea = Rect.zero;
		lastCanvasRect = Rect.zero;
		((MonoBehaviour)this)._002Ector();
	}
}
