using UnityEngine;
using UnityEngine.UI;

public class AdButtonTimer : MonoBehaviour
{
	internal static AdButtonTimer instance;

	public Text timer;

	public Button button;

	public Image icon;

	private double adTimer;

	private Color transparent;

	private int tRecalc;

	private void Awake()
	{
		instance = this;
		adTimer = 10.0;
	}

	internal static void setAdTimer()
	{
		if (PlayerConfig.instance != null)
		{
			double nextAdTimestamp = PlayerConfig.instance.data.nextAdTimestamp;
			nextAdTimestamp -= Epoch.Current();
			instance.adTimer = nextAdTimestamp;
			if (instance.adTimer < 0.0 || PlayerConfig.instance.data.nextAdTimestamp == -1.0)
			{
				instance.adTimer = -1.0;
			}
		}
	}

	private void OnEnable()
	{
		setAdTimer();
		updateButton();
	}

	private void Update()
	{
		if (Config.hasPremium)
		{
			((Component)this).gameObject.SetActive(false);
			return;
		}
		if (adTimer > 0.0)
		{
			adTimer -= Time.deltaTime;
		}
		updateButton();
	}

	private void updateButton()
	{
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		if (tRecalc > 0)
		{
			tRecalc--;
		}
		else
		{
			tRecalc = 10;
			setAdTimer();
		}
		if (adTimer > 0.0)
		{
			((Component)timer).gameObject.SetActive(true);
			timer.text = Toolbox.formatTimer((float)adTimer);
			((Graphic)icon).color = transparent;
		}
		else
		{
			((Component)timer).gameObject.SetActive(false);
			((Graphic)icon).color = Color.white;
		}
	}

	public AdButtonTimer()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		transparent = new Color(1f, 1f, 1f, 0.3f);
		((MonoBehaviour)this)._002Ector();
	}
}
