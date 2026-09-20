using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

internal class WindowKeyboardHighlighter : MonoBehaviour
{
	private static List<InputField> inputFields = new List<InputField>();

	private bool rescan;

	private int noInputs;

	private bool anyFocused;

	private void OnEnable()
	{
		if (!TouchScreenKeyboard.isSupported)
		{
			Object.Destroy((Object)(object)((Component)this).GetComponent<WindowKeyboardHighlighter>());
		}
		else
		{
			findInputFields();
		}
	}

	private void findInputFields()
	{
		noInputs = 0;
		rescan = false;
		inputFields.Clear();
		Transform[] componentsInChildren = ((Component)((Component)this).transform).GetComponentsInChildren<Transform>();
		foreach (Transform val in componentsInChildren)
		{
			if (((Component)(object)val).HasComponent<InputField>())
			{
				inputFields.Add(((Component)val).GetComponent<InputField>());
			}
		}
	}

	private void OnDisable()
	{
		inputFields.Clear();
	}

	private void up(InputField pInput)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		int num = Screen.height / 4 * 3;
		if (!(((Component)pInput).transform.position.y >= (float)num))
		{
			Vector3 localPosition = ((Component)this).gameObject.transform.localPosition;
			localPosition.y += 10f;
			((Component)this).transform.localPosition = localPosition;
		}
	}

	private void down()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (!(((Component)this).gameObject.transform.localPosition.y <= 10f))
		{
			Vector3 localPosition = ((Component)this).gameObject.transform.localPosition;
			localPosition.y -= 5f;
			((Component)this).transform.localPosition = localPosition;
		}
	}

	private void Update()
	{
		anyFocused = false;
		if (inputFields.Count == 0)
		{
			noInputs++;
		}
		foreach (InputField inputField in inputFields)
		{
			if (!((Component)inputField).gameObject.activeInHierarchy)
			{
				rescan = true;
			}
			if (inputField.isFocused)
			{
				up(inputField);
				anyFocused = true;
			}
		}
		if (!anyFocused)
		{
			down();
		}
		if (rescan || noInputs > 60)
		{
			findInputFields();
		}
	}
}
