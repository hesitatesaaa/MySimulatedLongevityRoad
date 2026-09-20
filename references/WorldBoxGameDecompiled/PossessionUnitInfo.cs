using UnityEngine;
using UnityEngine.UI;

public class PossessionUnitInfo : MonoBehaviour
{
	[SerializeField]
	private Text _name_field;

	[SerializeField]
	private Image _icon_species;

	[SerializeField]
	private Image _icon_sex;

	[SerializeField]
	private KingdomBanner _banner_kingdom;

	[SerializeField]
	private Text _text_age;

	[SerializeField]
	private Text _text_kills;

	[SerializeField]
	private Text _text_level;

	[SerializeField]
	private StatBar _bar_health;

	private void OnEnable()
	{
		Actor controllableUnit = ControllableUnit.getControllableUnit();
		if (controllableUnit != null)
		{
			showForUnit(controllableUnit);
		}
	}

	private void Update()
	{
		Actor controllableUnit = ControllableUnit.getControllableUnit();
		if (controllableUnit != null)
		{
			showForUnit(controllableUnit);
		}
	}

	private void showForUnit(Actor pActor)
	{
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		if (pActor.isSexMale())
		{
			_icon_sex.sprite = SpriteTextureLoader.getSprite("ui/icons/IconMale");
		}
		else
		{
			_icon_sex.sprite = SpriteTextureLoader.getSprite("ui/icons/IconFemale");
		}
		_icon_species.sprite = pActor.asset.getSpriteIcon();
		if (pActor.kingdom.isCiv())
		{
			((Component)_banner_kingdom).gameObject.SetActive(true);
			_banner_kingdom.load(pActor.kingdom);
		}
		else
		{
			((Component)_banner_kingdom).gameObject.SetActive(false);
		}
		float pVal = pActor.getHealth();
		float num = pActor.getMaxHealth();
		_bar_health.setBar(pVal, num, "/" + ((int)num).ToText(4), pReset: false, pFloat: false, pUpdateText: true, 0.25f);
		_name_field.text = pActor.getName();
		((Graphic)_name_field).color = pActor.kingdom.getColor().getColorText();
		_text_age.text = pActor.getAge().ToString();
		_text_kills.text = pActor.data.kills.ToString();
		_text_level.text = pActor.level.ToString();
	}
}
