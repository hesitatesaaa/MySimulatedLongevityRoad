using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PopulationPyramidRow : MonoBehaviour
{
	[SerializeField]
	private Image _left_icon;

	[SerializeField]
	private Image _right_icon;

	[SerializeField]
	private PopulationPyramidItem _left_item;

	[SerializeField]
	private PopulationPyramidItem _right_item;

	[SerializeField]
	private Text _text;

	private int _age_group_min;

	private int _age_group_max;

	private void Start()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		((UnityEvent)((Component)this).gameObject.AddOrGetComponent<Button>().onClick).AddListener(new UnityAction(animateBars));
		setupTooltip();
	}

	private void setupTooltip()
	{
		TipButton tipButton = default(TipButton);
		if (((Component)this).TryGetComponent<TipButton>(ref tipButton))
		{
			tipButton.setHoverAction(showTooltip, pAddAnimation: false);
		}
	}

	private void showTooltip()
	{
		CustomDataContainer<string> customDataContainer = new CustomDataContainer<string>();
		customDataContainer["age_range"] = _age_group_min + " - " + _age_group_max;
		CustomDataContainer<int> customDataContainer2 = new CustomDataContainer<int>();
		customDataContainer2["males"] = _left_item.getCount();
		customDataContainer2["females"] = _right_item.getCount();
		Tooltip.show(((Component)this).gameObject, "gender_data", new TooltipData
		{
			custom_data_string = customDataContainer,
			custom_data_int = customDataContainer2
		});
	}

	private void animateBars()
	{
		_left_item.animateBar();
		_right_item.animateBar();
	}

	internal void setAgeGroup(int pAgeGroup, int pAgeGroupMax)
	{
		_age_group_min = pAgeGroup;
		_age_group_max = pAgeGroupMax;
		_text.text = pAgeGroup.ToString();
		float num = 0.75f + (float)pAgeGroup / 400f;
		num = Mathf.Clamp(num, 0.75f, 1f);
		_left_item.setOpacity(num);
		_right_item.setOpacity(num);
	}

	internal void setColorTextBasedOnAmount(int pAmount)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (pAmount == 0)
		{
			((Graphic)_text).color = new Color(1f, 1f, 1f, 0.3f);
		}
		else
		{
			((Graphic)_text).color = new Color(1f, 1f, 1f, 1f);
		}
	}

	internal void setMaleCount(int pCount, int pMax)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		_left_item.setCount(pCount, pMax);
		if (pCount == 0)
		{
			((Graphic)_left_icon).color = new Color(1f, 1f, 1f, 0.3f);
		}
		else
		{
			((Graphic)_left_icon).color = new Color(1f, 1f, 1f, 1f);
		}
	}

	internal void setFemaleCount(int pCount, int pMax)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		_right_item.setCount(pCount, pMax);
		if (pCount == 0)
		{
			((Graphic)_right_icon).color = new Color(1f, 1f, 1f, 0.3f);
		}
		else
		{
			((Graphic)_right_icon).color = new Color(1f, 1f, 1f, 1f);
		}
	}
}
