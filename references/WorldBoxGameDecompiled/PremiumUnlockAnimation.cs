using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class PremiumUnlockAnimation : MonoBehaviour
{
	public float time;

	public GameObject circleFX;

	public GameObject shineFX;

	public GameObject aye;

	private CanvasGroup canvasGroup;

	public float fadeDelay;

	private int index;

	public Vector3 scaleAdd;

	public static float scaleTime = 1f;

	public static float delayTime = 0.5f;

	private void Awake()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		aye.transform.localScale = new Vector3(1f, 0f, 1f);
	}

	private void Start()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		canvasGroup = shineFX.GetComponent<CanvasGroup>();
		circleFX.SetActive(true);
		TweenSettingsExtensions.SetLoops<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(circleFX.transform, Vector3.one, scaleTime), -1, (LoopType)1);
		TweenSettingsExtensions.SetDelay<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(aye.transform, Vector3.one, scaleTime), (Ease)24), delayTime);
	}

	private void Update()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		CanvasGroup obj = canvasGroup;
		obj.alpha += Time.deltaTime / fadeDelay;
		shineFX.transform.Rotate(new Vector3(0f, 0f, 1f));
	}

	public void clickClose()
	{
		circleFX.gameObject.SetActive(false);
		shineFX.gameObject.SetActive(false);
	}
}
