using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
	public Sprite icon_default;

	public Sprite icon_bear;

	public Sprite icon_ivilizations;

	public Sprite icon_nuke;

	public Sprite icon_dragon;

	public Sprite icon_tornado;

	public Sprite icon_saveBox;

	public Sprite icon_customWorld;

	public Sprite icon_worldLaws;

	public Sprite icon_greyGoo;

	public Sprite icon_ufo;

	public Sprite icon_heart;

	public Sprite icon_finger;

	public GameObject bear;

	public GameObject centerObject;

	public GameObject brushSize;

	public GameObject adButton;

	public GameObject saveButton;

	public GameObject customMapButton;

	public GameObject worldRules;

	public GameObject tabDrawing;

	public GameObject tabCivs;

	public GameObject tabCreatures;

	public GameObject tabNature;

	public GameObject tabBombs;

	public GameObject tabOther;

	public GameObject settingsButton;

	public Text text;

	public Image attentionBox;

	public Text pressAnywhere;

	private int curPage;

	private List<TutorialPage> pages;

	private float waitTimer;

	private Color color_red;

	private Color color_white;

	private Color color_yellow;

	private Color color_yellow_transparent;

	private Tweener textTypeTween;

	private void create()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		pages = new List<TutorialPage>();
		color_red = Toolbox.makeColor("#FF3700");
		color_red.a = 0.4f;
		color_white = Toolbox.makeColor("#4AA5FF");
		color_white.a = 0.4f;
		color_yellow = Toolbox.makeColor("#FEFE00");
		color_yellow_transparent = Toolbox.makeColor("#FEFE00");
		color_yellow_transparent.a = 0f;
		add(new TutorialPage
		{
			text = "tut_page1",
			wait = 1f
		});
		add(new TutorialPage
		{
			text = "tut_page2",
			wait = 0.3f
		});
		add(new TutorialPage
		{
			text = "tut_page3_mobile",
			mobileOnly = true,
			centerImage = icon_finger,
			wait = 1f
		});
		add(new TutorialPage
		{
			text = "tut_page3_pc",
			pcOnly = true,
			wait = 1f
		});
		add(new TutorialPage
		{
			text = "tut_page4"
		});
		add(new TutorialPage
		{
			text = "tut_page5",
			object1 = saveButton,
			centerImage = icon_saveBox,
			wait = 1.5f
		});
		add(new TutorialPage
		{
			text = "tut_page6",
			object1 = customMapButton,
			centerImage = icon_customWorld,
			wait = 1.5f
		});
		add(new TutorialPage
		{
			text = "tut_page7",
			object1 = worldRules,
			centerImage = icon_worldLaws
		});
		add(new TutorialPage
		{
			text = "tut_page8",
			object1 = tabDrawing
		});
		add(new TutorialPage
		{
			text = "tut_page9",
			icon = "brush"
		});
		add(new TutorialPage
		{
			text = "tut_page10",
			object1 = tabCivs,
			centerImage = icon_ivilizations
		});
		add(new TutorialPage
		{
			text = "tut_page11",
			object1 = tabCreatures,
			centerImage = icon_dragon
		});
		add(new TutorialPage
		{
			text = "tut_page12",
			object1 = tabNature,
			centerImage = icon_tornado
		});
		add(new TutorialPage
		{
			text = "tut_page13",
			object1 = tabBombs,
			centerImage = icon_nuke
		});
		add(new TutorialPage
		{
			text = "tut_page14",
			object1 = tabOther,
			centerImage = icon_greyGoo
		});
		add(new TutorialPage
		{
			text = "tut_page15",
			centerImage = icon_ufo
		});
		add(new TutorialPage
		{
			text = "tut_page16",
			mobileOnly = true,
			icon = "reward",
			wait = 1.5f
		});
		add(new TutorialPage
		{
			text = "tut_page17",
			centerImage = icon_heart,
			wait = 0.5f
		});
	}

	public void startTutorial()
	{
		if (pages == null)
		{
			create();
		}
		((Component)this).gameObject.SetActive(true);
		curPage = -1;
		PowerButtonSelector.instance.unselectAll();
		PowerButtonSelector.instance.unselectTabs();
		((Component)attentionBox).gameObject.SetActive(false);
		nextPage();
	}

	private void nextPage()
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03be: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0444: Unknown result type (might be due to invalid IL or missing references)
		//IL_0468: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ee: Unknown result type (might be due to invalid IL or missing references)
		curPage++;
		if (curPage >= pages.Count)
		{
			endTutorial();
			return;
		}
		((Component)pressAnywhere).GetComponent<LocalizedText>().updateText();
		TutorialPage tutorialPage = pages[curPage];
		if (!Config.isMobile && tutorialPage.mobileOnly)
		{
			nextPage();
			return;
		}
		if (Config.isMobile && tutorialPage.pcOnly)
		{
			nextPage();
			return;
		}
		if ((Object)(object)tutorialPage.object1 == (Object)null)
		{
			((Component)attentionBox).gameObject.SetActive(false);
		}
		else
		{
			((Component)attentionBox).gameObject.SetActive(true);
			Vector3 position = tutorialPage.object1.transform.position;
			Vector2 sizeDelta = tutorialPage.object1.GetComponent<RectTransform>().sizeDelta;
			sizeDelta.x += 10f;
			sizeDelta.y += 10f;
			((Component)attentionBox).transform.position = position;
			((Graphic)attentionBox).rectTransform.sizeDelta = sizeDelta;
			ShortcutExtensions.DOKill((Component)(object)attentionBox, false);
			((Component)attentionBox).transform.localScale = new Vector3(0.5f, 0.5f);
			TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(((Component)attentionBox).transform, new Vector3(1f, 1f, 1f), 0.3f), (Ease)27);
			((Graphic)attentionBox).color = color_white;
		}
		if ((Object)(object)tutorialPage.centerImage == (Object)null)
		{
			tutorialPage.centerImage = icon_default;
		}
		centerObject.GetComponent<Image>().sprite = tutorialPage.centerImage;
		centerObject.gameObject.SetActive(false);
		adButton.gameObject.SetActive(false);
		brushSize.gameObject.SetActive(false);
		((Component)this.text).transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
		ShortcutExtensions.DOKill((Component)(object)this.text, false);
		this.text.text = "";
		string text = LocalizedTextManager.getText(tutorialPage.text);
		float num = text.Length / 25;
		if (num <= 1f)
		{
			num = 1f;
		}
		this.text.text = text;
		((Component)this.text).GetComponent<LocalizedText>().checkTextFont();
		((Component)this.text).GetComponent<LocalizedText>().checkSpecialLanguages();
		text = this.text.text;
		this.text.text = "";
		textTypeTween = (Tweener)(object)DOTweenModuleUI.DOText(this.text, text, num, false, (ScrambleMode)0, (string)null);
		TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(((Component)this.text).transform, new Vector3(1f, 1f, 1f), 0.3f), (Ease)26);
		waitTimer = tutorialPage.wait;
		if (canSkipTutorial())
		{
			waitTimer = 0f;
		}
		if (waitTimer > 0f)
		{
			((Component)pressAnywhere).gameObject.SetActive(false);
		}
		ShortcutExtensions.DOKill((Component)(object)bear.transform, false);
		bear.transform.localRotation = new Quaternion(0f, 0f, 0f, 0f);
		ShortcutExtensions.DOShakeRotation(bear.transform, num, 90f, 10, 90f, true, (ShakeRandomnessMode)0);
		if (tutorialPage.icon == "default")
		{
			centerObject.SetActive(true);
			ShortcutExtensions.DOKill((Component)(object)centerObject.transform, false);
			centerObject.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
			TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(centerObject.transform, new Vector3(1f, 1f, 1f), 0.5f), (Ease)26);
		}
		else if (tutorialPage.icon == "reward")
		{
			adButton.SetActive(true);
			ShortcutExtensions.DOKill((Component)(object)adButton.transform, false);
			adButton.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
			TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(adButton.transform, new Vector3(1f, 1f, 1f), 0.5f), (Ease)26);
		}
		else if (tutorialPage.icon == "brush")
		{
			brushSize.SetActive(true);
			ShortcutExtensions.DOKill((Component)(object)brushSize.transform, false);
			brushSize.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
			TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(brushSize.transform, new Vector3(1f, 1f, 1f), 0.5f), (Ease)26);
		}
	}

	internal void completeText()
	{
	}

	private bool canSkipTutorial()
	{
		return true;
	}

	public static void restartTutorial()
	{
		PlayerConfig.instance.data.tutorialFinished = false;
		PlayerConfig.saveData();
	}

	internal bool isActive()
	{
		return ((Component)this).gameObject.activeSelf;
	}

	internal void endTutorial()
	{
		((Component)this).gameObject.SetActive(false);
		PlayerConfig.instance.data.tutorialFinished = true;
		PlayerConfig.saveData();
		ScrollWindow.clearQueue();
	}

	private void LateUpdate()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		if (((Component)attentionBox).gameObject.activeSelf)
		{
			if (((Graphic)attentionBox).color == color_red)
			{
				DOTweenModuleUI.DOColor(attentionBox, color_white, 1f);
			}
			else if (((Graphic)attentionBox).color == color_white)
			{
				DOTweenModuleUI.DOColor(attentionBox, color_red, 1f);
			}
		}
		if (canSkipTutorial())
		{
			if (TweenExtensions.IsActive((Tween)(object)textTypeTween) && Input.GetMouseButtonUp(0))
			{
				TweenExtensions.Kill((Tween)(object)textTypeTween, true);
				return;
			}
		}
		else if (TweenExtensions.IsActive((Tween)(object)textTypeTween))
		{
			return;
		}
		if (waitTimer > 0f)
		{
			waitTimer -= Time.deltaTime;
			return;
		}
		if (!((Component)pressAnywhere).gameObject.activeSelf)
		{
			((Component)pressAnywhere).gameObject.SetActive(true);
			((Component)pressAnywhere).transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
			TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(((Component)pressAnywhere).transform, new Vector3(1f, 1f, 1f), 1f), (Ease)27);
			((Graphic)pressAnywhere).color = color_yellow_transparent;
			DOTweenModuleUI.DOColor(pressAnywhere, color_yellow, 1f);
		}
		if (Input.GetMouseButtonUp(0))
		{
			nextPage();
		}
	}

	private void add(TutorialPage pPage)
	{
		pages.Add(pPage);
	}
}
