using UnityEngine;

public class QuantumSpriteWithText : QuantumSprite
{
	public TextMesh text;

	public void initText()
	{
		Transform obj = ((Component)this).transform.Find("Text");
		text = ((obj != null) ? ((Component)obj).GetComponent<TextMesh>() : null);
	}
}
