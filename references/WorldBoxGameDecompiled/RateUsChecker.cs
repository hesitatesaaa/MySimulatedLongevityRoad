using UnityEngine;

public class RateUsChecker : MonoBehaviour
{
	public GameObject rateUs;

	public GameObject updateAvailable;

	private void OnEnable()
	{
		if (Config.game_loaded && (Object)(object)rateUs != (Object)null && (Object)(object)rateUs.gameObject != (Object)null)
		{
			rateUs.gameObject.SetActive(false);
		}
	}

	private void Update()
	{
		if (VersionCheck.isOutdated())
		{
			if ((Object)(object)rateUs != (Object)null && (Object)(object)rateUs.gameObject != (Object)null)
			{
				rateUs.gameObject.SetActive(false);
			}
			if ((Object)(object)updateAvailable != (Object)null && (Object)(object)updateAvailable.gameObject != (Object)null)
			{
				updateAvailable.gameObject.SetActive(true);
			}
		}
		else if ((Object)(object)updateAvailable != (Object)null && (Object)(object)updateAvailable.gameObject != (Object)null)
		{
			updateAvailable.gameObject.SetActive(false);
		}
	}
}
