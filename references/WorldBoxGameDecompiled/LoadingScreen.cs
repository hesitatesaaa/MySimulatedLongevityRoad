using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
	public delegate void TransitionAction();

	public Image background;

	public CanvasGroup canvasGroup;

	public Text percents;

	public LocalizedText topText;

	public LocalizedText tipText;

	public Image bar;

	public Image mask;

	private AsyncOperation asyncLoad;

	private bool appearDone;

	public bool inGameScreen;

	internal bool modeIn;

	public TransitionAction action;

	private float outTimer;

	public Canvas canvas;

	public Text loadingHelperText;

	private static int _last_tip = -1;

	private static int _max_tip = 0;

	private float lastBgWidth;

	private float lastBgHeight;

	private float lastCScale;

	public bool debugg;

	private void setupBg()
	{
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		float num = Screen.width;
		float num2 = Screen.height;
		if (lastBgHeight != num2 || lastBgWidth != num || canvas.scaleFactor != lastCScale)
		{
			lastBgWidth = num;
			lastBgHeight = num2;
			lastCScale = canvas.scaleFactor;
			float num3 = (float)((Graphic)background).mainTexture.width * canvas.scaleFactor;
			float num4 = (float)((Graphic)background).mainTexture.height * canvas.scaleFactor;
			float num5 = (float)Screen.width / num3;
			float num6 = (float)Screen.height / num4;
			if (num5 > num6)
			{
				((Component)background).transform.localScale = new Vector3(num5, num5, 1f);
			}
			else
			{
				((Component)background).transform.localScale = new Vector3(num6, num6, 1f);
			}
		}
	}

	private void Awake()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		InitLibraries.initMainLibs();
		Config.enableAutoRotation(pValue: false);
		((Component)this).transform.localPosition = default(Vector3);
		if (inGameScreen)
		{
			outTimer = 0.3f;
			canvasGroup.alpha = 1f;
			appearDone = true;
			((Component)bar).transform.localScale = new Vector3(1f, 1f, 1f);
		}
		else
		{
			canvasGroup.alpha = 0f;
			((Component)bar).transform.localScale = new Vector3(0f, 1f, 1f);
		}
	}

	private void startAction()
	{
		ScrollWindow.hideAllEvent(pWithAnimation: false);
		modeIn = false;
		if (Config.isMobile && !Config.hasPremium)
		{
			Debug.Log((object)("PremiumElementsChecker.goodForInterstitialAd(): " + PremiumElementsChecker.goodForInterstitialAd()));
			if (PremiumElementsChecker.goodForInterstitialAd())
			{
				if (PlayInterstitialAd.instance.isReady())
				{
					PlayInterstitialAd.instance.showAd();
					PremiumElementsChecker.setInterstitialAdTimer();
				}
				else
				{
					PlayInterstitialAd.instance.initAds();
				}
			}
		}
		action();
	}

	internal void startTransition(TransitionAction pAction)
	{
		Config.enableAutoRotation(pValue: false);
		action = pAction;
		((Component)bar).gameObject.SetActive(false);
		((Component)percents).gameObject.SetActive(false);
		((Component)topText).gameObject.SetActive(false);
		((Component)tipText).gameObject.SetActive(false);
		((Component)mask).gameObject.SetActive(false);
		((Component)this).gameObject.SetActive(true);
		canvasGroup.alpha = 0f;
		modeIn = true;
	}

	private void OnEnable()
	{
		string key = "loading_screen_" + Randy.randomInt(1, 22);
		topText.key = key;
		tipText.key = getTipID();
		topText.updateText();
		tipText.updateText();
		((Component)topText).gameObject.SetActive(true);
		((Component)tipText).gameObject.SetActive(true);
	}

	internal static string getTipID()
	{
		if (_max_tip == 0)
		{
			for (int i = 0; i < 1000 && LocalizedTextManager.stringExists(getTip(i)); i++)
			{
				_max_tip = i;
			}
		}
		int num = Randy.randomInt(0, _max_tip + 1);
		if (num == _last_tip)
		{
			return getTipID();
		}
		_last_tip = num;
		return getTip(num);
	}

	internal static string getTip(int pTip)
	{
		string pString = pTip.ToString();
		return "tip" + Toolbox.fillLeft(pString, 3, '0');
	}

	private void Update()
	{
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		if (!string.IsNullOrEmpty(SmoothLoader.latest_called_id))
		{
			loadingHelperText.text = SmoothLoader.latest_called_id + ":" + SmoothLoader.latest_time;
		}
		else
		{
			loadingHelperText.text = "";
		}
		if (inGameScreen)
		{
			if (modeIn)
			{
				if (canvasGroup.alpha >= 1f)
				{
					startAction();
				}
				CanvasGroup obj = canvasGroup;
				obj.alpha += Time.deltaTime * 2f;
				return;
			}
			if (outTimer > 0f)
			{
				outTimer -= Time.deltaTime;
				return;
			}
			if (canvasGroup.alpha <= 0f)
			{
				Config.enableAutoRotation(pValue: true);
				((Component)this).gameObject.SetActive(false);
			}
			if (!SmoothLoader.isLoading())
			{
				CanvasGroup obj2 = canvasGroup;
				obj2.alpha -= Time.fixedDeltaTime * 2f;
			}
			return;
		}
		if (!appearDone)
		{
			CanvasGroup obj3 = canvasGroup;
			obj3.alpha += Time.deltaTime;
			if (!(canvasGroup.alpha >= 1f))
			{
				return;
			}
			appearDone = true;
			((MonoBehaviour)this).StartCoroutine(LoadGame());
		}
		float num = ((Component)bar).transform.localScale.x;
		if (((Component)bar).transform.localScale.x < asyncLoad.progress)
		{
			num = ((Component)bar).transform.localScale.x + Time.deltaTime * 2f;
			if (num > asyncLoad.progress)
			{
				num = asyncLoad.progress;
			}
			((Component)bar).transform.localScale = new Vector3(num, 1f, 1f);
		}
		percents.text = Mathf.CeilToInt(asyncLoad.progress * 100f) + " %";
		if (num >= 0.9f)
		{
			if (!asyncLoad.allowSceneActivation)
			{
				Analytics.LogEvent("preloading_done");
			}
			asyncLoad.allowSceneActivation = true;
		}
	}

	private IEnumerator LoadGame()
	{
		asyncLoad = SceneManager.LoadSceneAsync("World");
		asyncLoad.allowSceneActivation = false;
		while (!asyncLoad.isDone)
		{
			yield return null;
		}
	}
}
