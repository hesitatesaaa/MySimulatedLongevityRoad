using UnityEngine;
using UnityEngine.UI;

public class PatchLogTitle : MonoBehaviour
{
	[SerializeField]
	private Image _background;

	[SerializeField]
	private Sprite _bg_folded;

	[SerializeField]
	private Sprite _bg_unfolded;

	public Image icon_left;

	public Image icon_right;

	public Text title;

	public void setUnfolded()
	{
		_background.sprite = _bg_unfolded;
	}

	public void setFolded()
	{
		_background.sprite = _bg_folded;
	}
}
