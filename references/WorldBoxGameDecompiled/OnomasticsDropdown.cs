using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class OnomasticsDropdown : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private OnomasticsTab _onomastics_tab;

	private Button _button;

	private Dropdown _dropdown;

	private List<string> _options;

	internal static string current_template;

	internal static int current_template_index;

	private void Start()
	{
		_dropdown = ((Component)this).GetComponent<Dropdown>();
		createDropdownOptions();
		((UnityEvent<int>)(object)_dropdown.onValueChanged).AddListener((UnityAction<int>)dropdownValueChanged);
	}

	private void createDropdownOptions()
	{
		_dropdown.ClearOptions();
		_options = new List<string>();
		_options.Add("");
		foreach (NameGeneratorAsset item in AssetManager.name_generator.list)
		{
			if (item.onomastics_templates.Count < 1)
			{
				_options.Add("<color=red>" + item.id + "</color>");
				continue;
			}
			if (item.onomastics_templates.Count < 2)
			{
				_options.Add(item.id);
				continue;
			}
			for (int i = 0; i < item.onomastics_templates.Count; i++)
			{
				_options.Add(item.id + "#" + i);
			}
		}
		_options.Sort((string a, string b) => Toolbox.removeRichTextTags(a).CompareTo(Toolbox.removeRichTextTags(b)));
		_dropdown.AddOptions(_options);
	}

	private void dropdownValueChanged(int pOption)
	{
		if (pOption < 0 || pOption >= _dropdown.options.Count)
		{
			return;
		}
		string text = _dropdown.options[pOption].text;
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		text = Toolbox.removeRichTextTags(text);
		int result = 0;
		if (text.Contains('#'))
		{
			string[] array = text.Split('#');
			text = array[0];
			if (!int.TryParse(array[1], out result))
			{
				return;
			}
		}
		NameGeneratorAsset nameGeneratorAsset = AssetManager.name_generator.get(text);
		if (nameGeneratorAsset != null)
		{
			current_template = text;
			current_template_index = result;
			string pTemplate = null;
			if (result >= 0 && result < nameGeneratorAsset.onomastics_templates.Count)
			{
				pTemplate = nameGeneratorAsset.onomastics_templates[result];
			}
			_onomastics_tab.loadTemplate(pTemplate);
		}
	}

	public void OnPointerClick(PointerEventData pEventData)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Invalid comparison between Unknown and I4
		if ((Object)(object)((BaseEventData)pEventData).selectedObject == (Object)null || (Object)(object)((BaseEventData)pEventData).selectedObject.GetComponentInChildren<Scrollbar>() != (Object)null || !((UIBehaviour)_dropdown).IsActive() || !((Selectable)_dropdown).IsInteractable())
		{
			return;
		}
		ScrollRect componentInChildren = ((Component)this).gameObject.GetComponentInChildren<ScrollRect>();
		Scrollbar val = ((componentInChildren != null) ? componentInChildren.verticalScrollbar : null);
		if (_options.Count > 1 && (Object)(object)val != (Object)null)
		{
			if ((int)val.direction == 3)
			{
				val.value = Mathf.Max(0.001f, (float)_dropdown.value / (float)(_options.Count - 1));
			}
			else
			{
				val.value = Mathf.Max(0.001f, 1f - (float)_dropdown.value / (float)(_options.Count - 1));
			}
		}
	}
}
