using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class OnomasticsNameGenerator : MonoBehaviour
{
	private const int MAX_STRING_LENGTH = 250;

	private const int AMOUNT_NAME_EXAMPLES = 20;

	private const float TIMER_NAME_INTERVAL = 0.2f;

	private float _timer_name;

	private int _index_name;

	public Text text_name_examples;

	private string _male_color => ColorStyleLibrary.m.color_text_pumpkin;

	private string _female_color => ColorStyleLibrary.m.color_text_pumpkin_light;

	private string _separator_color => ColorStyleLibrary.m.color_text_grey_dark;

	protected void updateNameGeneration(OnomasticsData pData)
	{
		_timer_name += Time.deltaTime;
		if (!(_timer_name > 0.2f))
		{
			return;
		}
		_timer_name = 0f;
		if (_index_name >= 20 || Toolbox.removeRichTextTags(text_name_examples.text).Length >= 250)
		{
			return;
		}
		_index_name++;
		ActorSex actorSex = (Randy.randomBool() ? ActorSex.Female : ActorSex.Male);
		string text = pData.generateName(actorSex);
		if (text == "Rebr")
		{
			if (text_name_examples.text != string.Empty)
			{
				return;
			}
			actorSex = ((actorSex != ActorSex.Female) ? ActorSex.Female : ActorSex.Male);
			text = pData.generateName(actorSex);
		}
		if (!string.IsNullOrEmpty(text))
		{
			string text2 = text_name_examples.text;
			if (text2.Length > 0)
			{
				text2 += ", ";
			}
			text2 += Toolbox.coloredText(text, (actorSex == ActorSex.Male) ? _male_color : _female_color);
			text_name_examples.text = text2;
			textExamplesEffect(((Component)text_name_examples).gameObject.transform, 1f, 0.03f);
		}
	}

	private void textExamplesEffect(Transform pTransformTarget, float pDefaultScale = 1f, float pPower = 0.1f, float pDuration = 0.3f)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		ShortcutExtensions.DOKill((Component)(object)pTransformTarget, true);
		pTransformTarget.localScale = new Vector3(pDefaultScale, pDefaultScale, pDefaultScale);
		ShortcutExtensions.DOPunchScale(pTransformTarget, new Vector3(pPower, pPower, pPower), pDuration, 1, 1f);
	}

	protected void clickRegenerate()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		_index_name = 0;
		text_name_examples.text = string.Empty;
		((Graphic)text_name_examples).color = Toolbox.makeColor(_separator_color);
	}
}
