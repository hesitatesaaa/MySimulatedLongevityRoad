using System.Collections.Generic;
using UnityEngine;

public class DebugMessageFly : MonoBehaviour
{
	private List<string> listString = new List<string>();

	public Transform originTransform;

	private TextMesh textMesh;

	private void Awake()
	{
		textMesh = ((Component)this).GetComponent<TextMesh>();
	}

	public void addString(string pText)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		if (textMesh.color.a < 0.3f)
		{
			listString.Clear();
		}
		else if (listString.Count > 20)
		{
			listString.RemoveAt(0);
		}
		listString.Add(pText);
		Vector3 localPosition = default(Vector3);
		((Vector3)(ref localPosition))._002Ector(originTransform.localPosition.x, originTransform.localPosition.y);
		((Component)this).transform.localPosition = localPosition;
		string text = "";
		foreach (string item in listString)
		{
			text = text + item + "\n";
		}
		textMesh.text = text;
		Color color = textMesh.color;
		color.a = 1f;
		textMesh.color = color;
	}

	public void moveUp()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = ((Component)this).transform.localPosition;
		localPosition.y += 3f;
		((Component)this).transform.localPosition = localPosition;
	}

	private void Update()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localScale = ((Component)this).transform.localScale;
		localScale.x += 2f * Time.deltaTime;
		if (localScale.x > 1f)
		{
			localScale.x = 1f;
		}
		((Component)this).transform.localScale = localScale;
		Vector3 localPosition = ((Component)this).transform.localPosition;
		localPosition.y += 0.5f * Time.deltaTime;
		((Component)this).transform.localPosition = localPosition;
		Color color = textMesh.color;
		color.a -= 0.3f * Time.deltaTime;
		textMesh.color = color;
		if (color.a <= 0f)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			DebugMessage.instance.list.Remove(this);
		}
	}
}
