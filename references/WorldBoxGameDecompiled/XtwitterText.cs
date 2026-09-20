using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class XtwitterText : MonoBehaviour
{
	private Text _text;

	private string[] _strings = new string[6] { "Twitter", "Xwitter", "??", "X?", "X??", "X???" };

	private int _index;

	private float _timer = 2f;

	private const int INTERVAL = 2;

	private Tweener _current_tween;

	private void Awake()
	{
		_text = ((Component)this).GetComponent<Text>();
	}

	private void Update()
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		_timer -= Time.deltaTime;
		if (_timer <= 0f)
		{
			_timer = 2f;
			_text.text = _strings[_index];
			_index = (_index + 1) % _strings.Length;
			ShortcutExtensions.DOPunchScale(((Component)this).transform, new Vector3(0.2f, 0.2f, 0.2f), 0.3f, 10, 1f);
		}
	}
}
