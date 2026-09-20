using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class WindowMetaGeneric<TMetaObject, TData> : StatsWindow, IMetaWindow, IInterestingPeopleWindow, IMetaWithFamiliesWindow where TMetaObject : CoreSystemObject<TData> where TData : BaseSystemData
{
	[SerializeField]
	protected Image species_icon;

	[SerializeField]
	private Image _favorite_icon;

	protected NameInput _name_input;

	private BannerGeneric<TMetaObject, TData>[] _main_banners;

	internal string _initial_name;

	protected virtual TMetaObject meta_object
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public TMetaObject getMetaObject()
	{
		return meta_object;
	}

	public ICoreObject getCoreObject()
	{
		return meta_object;
	}

	protected override void create()
	{
		initMonoFields();
		initNameInput();
		initStuff();
		base.create();
	}

	private void initMonoFields()
	{
		Transform obj = ((Component)this).gameObject.transform.FindRecursive("NameInputElement");
		_name_input = ((obj != null) ? ((Component)obj).GetComponent<NameInput>() : null);
		_main_banners = ((Component)this).gameObject.transform.FindAllRecursive<BannerGeneric<TMetaObject, TData>>((Transform p) => ((Object)p).name == "Main Banner");
	}

	internal override bool checkCancelWindow()
	{
		if (meta_object == null)
		{
			return true;
		}
		if (!meta_object.isAlive())
		{
			return true;
		}
		return base.checkCancelWindow();
	}

	protected virtual void initStuff()
	{
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		startShowingWindow();
	}

	public virtual void startShowingWindow()
	{
		clear();
		loadBanners();
		loadNameInput();
		showTopPartInformation();
	}

	protected virtual void showTopPartInformation()
	{
		if ((Object)(object)species_icon != (Object)null)
		{
			species_icon.sprite = getActorIcon();
		}
		updateFavoriteIcon();
	}

	private void loadBanners()
	{
		BannerGeneric<TMetaObject, TData>[] main_banners = _main_banners;
		for (int i = 0; i < main_banners.Length; i++)
		{
			main_banners[i].load(meta_object);
		}
	}

	public void reloadBanner()
	{
		loadBanners();
	}

	protected virtual void loadNameInput()
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		((UnityEvent<string>)(object)_name_input.inputField.onEndEdit).AddListener((UnityAction<string>)delegate(string pString)
		{
			onNameChange(pString);
		});
		string text = (_initial_name = meta_object.data.name.Trim());
		_name_input.setText(text);
		ColorAsset color = meta_object.getColor();
		if (color != null)
		{
			((Graphic)_name_input.textField).color = color.getColorText();
		}
		else
		{
			((Graphic)_name_input.textField).color = Toolbox.color_white;
		}
		if (meta_object.data.custom_name)
		{
			_name_input.SetOutline();
		}
	}

	protected virtual void clear()
	{
	}

	protected virtual void OnDisable()
	{
		_name_input.inputField.DeactivateInputField();
	}

	protected virtual void initNameInput()
	{
	}

	protected virtual bool onNameChange(string pInput)
	{
		if (string.IsNullOrWhiteSpace(pInput))
		{
			return false;
		}
		if (meta_object.isRekt())
		{
			return false;
		}
		string text = pInput.Trim();
		if (_initial_name == text)
		{
			return false;
		}
		meta_object.data.custom_name = true;
		meta_object.setName(text);
		_initial_name = text;
		_name_input.SetOutline();
		return true;
	}

	public void pressFavorite()
	{
		if (meta_object != null)
		{
			meta_object.setFavorite(!meta_object.isFavorite());
			updateFavoriteIcon();
			refreshMetaList();
			SpriteSwitcher.checkAllStates();
			if (meta_object.isFavorite())
			{
				WorldTip.showNowTop(getTipFavorite());
			}
		}
	}

	private void updateFavoriteIcon()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_favorite_icon == (Object)null))
		{
			if (meta_object.isFavorite())
			{
				((Graphic)_favorite_icon).color = ColorStyleLibrary.m.favorite_selected;
			}
			else
			{
				((Graphic)_favorite_icon).color = ColorStyleLibrary.m.favorite_not_selected;
			}
		}
	}

	internal void tryShowPastNames()
	{
		List<NameEntry> past_names = meta_object.data.past_names;
		if (past_names != null && past_names.Count > 1)
		{
			showStatRow("past_names", meta_object.data.past_names?.Count ?? 1, MetaType.None, -1L, "iconVillages", "past_names", getTooltipPastNames);
		}
	}

	internal TooltipData getTooltipPastNames()
	{
		return new TooltipData
		{
			tip_name = "past_names",
			past_names = new ListPool<NameEntry>(meta_object.data.past_names),
			meta_type = meta_type
		};
	}

	protected string getTipFavorite()
	{
		return "favorited";
	}

	public virtual IEnumerable<Actor> getInterestingUnitsList()
	{
		return ((IMetaObject)meta_object).getUnits();
	}

	public virtual IEnumerable<Family> getFamilies()
	{
		return ((IMetaObject)meta_object).getFamilies();
	}

	public virtual bool hasFamilies()
	{
		return ((IMetaObject)meta_object).hasFamilies();
	}

	protected Sprite getActorIcon()
	{
		return (meta_object as IMetaObject)?.getSpriteIcon();
	}
}
