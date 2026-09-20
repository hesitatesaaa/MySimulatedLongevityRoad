using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldTip : MonoBehaviour
{
	public Transform transform_toolbar;

	public Transform transform_main;

	public Transform transform_positionTop;

	public static WorldTip instance;

	public Canvas canvas;

	public Text text;

	public TipStatus status;

	public CanvasGroup canvasGroup;

	private float timeout;

	private float scale = 1f;

	public static Dictionary<string, string> replacementDict;

	private void Awake()
	{
		status = TipStatus.Hidden;
		canvasGroup.alpha = 0f;
		instance = this;
	}

	public void show(string pText, bool pTranslate = true, string pPosition = "center", float pTime = 3f, string pColor = "#F3961F")
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)instance == (Object)null)
		{
			return;
		}
		((Graphic)instance.text).color = Toolbox.makeColor(pColor);
		if (pTranslate)
		{
			instance.text.text = LocalizedTextManager.getText(pText);
			if (replacementDict != null)
			{
				instance.text.text = replaceWords(instance.text.text);
			}
			((Component)instance.text).GetComponent<LocalizedText>().checkSpecialLanguages();
		}
		else
		{
			instance.text.text = pText;
		}
		updateTextWidth();
		((Component)this).transform.SetParent(transform_main);
		instance.startShow(pTime);
		if (pPosition == "center")
		{
			((Component)this).transform.position = Vector3.zero;
		}
		else if (pPosition == "top")
		{
			((Component)this).transform.position = transform_positionTop.position;
		}
		else
		{
			((Component)instance).transform.position = Input.mousePosition;
		}
	}

	public static void showNowCenter(string pText)
	{
		showNow(pText);
	}

	public static void showNowTop(string pText, bool pTranslate = true)
	{
		showNow(pText, pTranslate, "top");
	}

	public static void addWordReplacement(string key, string value)
	{
		if (replacementDict == null)
		{
			replacementDict = new Dictionary<string, string>();
		}
		replacementDict[key] = value;
	}

	public static string replaceWords(string text)
	{
		foreach (string key in replacementDict.Keys)
		{
			text = text.Replace(key, replacementDict[key]);
		}
		replacementDict = null;
		return text;
	}

	public static void showNow(string pText, bool pTranslate = true, string pPosition = "center", float pTime = 3f, string pColor = "#F3961F")
	{
		if (!((Object)(object)instance == (Object)null))
		{
			instance.show(pText, pTranslate, pPosition, pTime, pColor);
		}
	}

	public void showToolbarText(string pText)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		text.text = pText;
		LocalizedText localizedText = default(LocalizedText);
		if (((Component)text).TryGetComponent<LocalizedText>(ref localizedText))
		{
			localizedText.checkSpecialLanguages();
		}
		updateTextWidth();
		startShow();
		((Component)this).transform.position = transform_toolbar.position;
	}

	public void showToolbarText(GodPower pPower, bool pShowOnComputer = true)
	{
		if (pShowOnComputer || !Config.isComputer)
		{
			string localizedName;
			string localizedDescription;
			if (pPower.type == PowerActionType.PowerSpawnActor)
			{
				ActorAsset actorAsset = pPower.getActorAsset();
				localizedName = actorAsset.getLocalizedName();
				localizedDescription = actorAsset.getLocalizedDescription();
			}
			else
			{
				localizedName = LocalizedTextManager.getText(pPower.getLocaleID());
				localizedDescription = LocalizedTextManager.getText(pPower.getDescriptionID());
			}
			showToolbarText(localizedName, localizedDescription, pShowOnComputer);
		}
	}

	public void showToolbarText(string pTextMain, string pTextDescription, bool pShowOnComputer = true)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if (pShowOnComputer || !Config.isComputer)
		{
			text.text = pTextMain + "\n" + pTextDescription;
			LocalizedText localizedText = default(LocalizedText);
			if (((Component)text).TryGetComponent<LocalizedText>(ref localizedText))
			{
				localizedText.checkSpecialLanguages();
			}
			updateTextWidth();
			startShow();
			((Component)this).transform.position = transform_toolbar.position;
		}
	}

	public void setText(string pText, bool pAddSKip = false)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		text.text = LocalizedTextManager.getText(pText);
		((Component)text).GetComponent<LocalizedText>().checkSpecialLanguages();
		if (pAddSKip)
		{
			text.text = "\n" + text.text;
		}
		updateTextWidth();
		((Component)this).transform.position = transform_toolbar.position;
		startShow();
	}

	private void updateTextWidth()
	{
	}

	private void startShow(float pTime = 3f)
	{
		status = TipStatus.Shown;
		timeout = pTime;
		scale = 1.5f;
	}

	public static void hideNow()
	{
		if (!((Object)(object)instance == (Object)null) && ((Component)instance).gameObject.activeSelf)
		{
			instance.startHide();
		}
	}

	internal void startHide()
	{
		status = TipStatus.Hidden;
		timeout = 0f;
	}

	private void Update()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (scale > 1f)
		{
			scale -= Time.deltaTime * 3f;
			if (scale < 1f)
			{
				scale = 1f;
			}
			((Component)this).transform.localScale = new Vector3(scale, scale, 1f);
		}
		switch (status)
		{
		case TipStatus.Hidden:
			if (canvasGroup.alpha > 0f)
			{
				CanvasGroup obj2 = canvasGroup;
				obj2.alpha -= Time.deltaTime * 2f;
			}
			break;
		case TipStatus.Shown:
			if (canvasGroup.alpha < 1f)
			{
				CanvasGroup obj = canvasGroup;
				obj.alpha += Time.deltaTime * 3f;
			}
			if (canvasGroup.alpha == 1f)
			{
				timeout -= Time.deltaTime;
				if (timeout <= 0f)
				{
					startHide();
				}
			}
			break;
		}
	}
}
