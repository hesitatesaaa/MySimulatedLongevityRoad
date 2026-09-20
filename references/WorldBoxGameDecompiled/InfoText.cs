using UnityEngine;

public class InfoText : MonoBehaviour
{
	public TextMesh text;

	public TextMesh shadow;

	private void Start()
	{
		((Component)text).gameObject.GetComponent<Renderer>().sortingOrder = 1000;
		((Component)shadow).gameObject.GetComponent<Renderer>().sortingOrder = 999;
	}

	public void setText(string pText)
	{
		text.text = pText;
		shadow.text = pText;
	}
}
