using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class IconClickRotator : MonoBehaviour
{
	private Quaternion _startRotation;

	private Coroutine _rotationRoutine;

	private void Awake()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		((UnityEvent)((Component)this).gameObject.AddOrGetComponent<Button>().onClick).AddListener(new UnityAction(click));
		((Component)this).gameObject.AddOrGetComponent<ScrollableButton>();
		_startRotation = ((Component)this).transform.rotation;
	}

	private void click()
	{
		startRandomRotation();
	}

	private void startRandomRotation()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (_rotationRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(_rotationRoutine);
		}
		float num = Random.Range(-180f, 180f);
		Quaternion targetRotation = Quaternion.Euler(0f, 0f, num);
		_rotationRoutine = ((MonoBehaviour)this).StartCoroutine(RotateTo(targetRotation, 0.2f));
	}

	private IEnumerator RotateTo(Quaternion targetRotation, float duration)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		float time = 0f;
		Quaternion initialRotation = ((Component)this).transform.rotation;
		while (time < duration)
		{
			((Component)this).transform.rotation = Quaternion.Lerp(initialRotation, targetRotation, time / duration);
			time += Time.deltaTime;
			yield return null;
		}
		((Component)this).transform.rotation = targetRotation;
	}
}
