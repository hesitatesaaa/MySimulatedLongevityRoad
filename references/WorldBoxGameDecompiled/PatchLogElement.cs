using UnityEngine;
using UnityEngine.UI;

public class PatchLogElement : MonoBehaviour
{
	public PatchLogTitle title;

	public Text date;

	public Text date_ago;

	public Image background;

	public GameObject texts;

	public bool _folded;

	public void fold()
	{
		_folded = true;
		title.setFolded();
		texts.gameObject.SetActive(false);
	}

	public void unfold()
	{
		_folded = false;
		title.setUnfolded();
		texts.gameObject.SetActive(true);
	}

	public void toggleState()
	{
		if (_folded)
		{
			unfold();
		}
		else
		{
			fold();
		}
	}
}
