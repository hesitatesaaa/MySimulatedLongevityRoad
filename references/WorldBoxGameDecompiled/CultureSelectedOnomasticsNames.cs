using UnityEngine;

public class CultureSelectedOnomasticsNames : OnomasticsNameGenerator
{
	[SerializeField]
	private GameObject _main_container;

	[SerializeField]
	private GameObject _separator;

	private Culture _culture;

	private string _last_template;

	private MetaType _meta_type => MetaType.Unit;

	public void load(Culture pCulture)
	{
		string templateString = getTemplateString(pCulture);
		if (_culture != pCulture || !(templateString == _last_template))
		{
			_culture = pCulture;
			_last_template = templateString;
			clickRegenerate();
		}
	}

	public void update()
	{
		bool flag = _culture.isRekt();
		_main_container.SetActive(!flag);
		_separator.SetActive(!flag);
		if (!flag)
		{
			OnomasticsData onomasticData = _culture.getOnomasticData(_meta_type);
			updateNameGeneration(onomasticData);
		}
	}

	public void click()
	{
		clickRegenerate();
	}

	private string getTemplateString(Culture pCulture)
	{
		return pCulture.getOnomasticData(_meta_type).getShortTemplate();
	}
}
