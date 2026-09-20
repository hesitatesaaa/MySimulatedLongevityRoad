using System;
using UnityEngine;
using UnityEngine.UI;

public class LongTextLoader : MonoBehaviour
{
	public TextAsset textAsset;

	protected Text m_text;

	private void Start()
	{
		m_text = ((Component)this).GetComponent<Text>();
		create();
		finish();
	}

	private void finish()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		RectTransform component = ((Component)m_text).GetComponent<RectTransform>();
		component.sizeDelta = new Vector2(component.sizeDelta.x, m_text.preferredHeight + 10f);
		RectTransform component2 = ((Component)((Component)this).transform.parent).GetComponent<RectTransform>();
		component2.sizeDelta = new Vector2(component2.sizeDelta.x, component.sizeDelta.y);
		float num = 0f - ((Component)component2).transform.localPosition.y;
		((Component)((Transform)component2).parent).GetComponent<RectTransform>().sizeDelta = new Vector2(0f, component.sizeDelta.y + 20f + num);
	}

	public virtual void create()
	{
		try
		{
			m_text.text = textAsset.text;
		}
		catch (Exception)
		{
			Debug.LogError((object)"LongTextLoader: Text File is too long");
		}
	}
}
