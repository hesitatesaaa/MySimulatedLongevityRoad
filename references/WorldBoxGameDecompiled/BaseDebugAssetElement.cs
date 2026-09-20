using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BaseDebugAssetElement<TAsset> : MonoBehaviour where TAsset : Asset
{
	public static TAsset selected_asset;

	internal TAsset asset;

	public Sprite no_animation;

	public Button asset_button;

	public Text title;

	public Text stats_description;

	public Text stats_values;

	internal RectTransform rect_transform;

	private void Awake()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		rect_transform = ((Component)this).GetComponent<RectTransform>();
		((UnityEvent)asset_button.onClick).AddListener(new UnityAction(showAssetWindow));
		asset_button.OnHover((UnityAction)delegate
		{
			selected_asset = asset;
		});
	}

	public virtual void setData(TAsset pAsset)
	{
		asset = pAsset;
		title.text = asset.id;
		initAnimations();
		initStats();
	}

	protected virtual void initAnimations()
	{
		throw new NotImplementedException();
	}

	public virtual void update()
	{
		throw new NotImplementedException();
	}

	public virtual void stopAnimations()
	{
		throw new NotImplementedException();
	}

	public virtual void startAnimations()
	{
		throw new NotImplementedException();
	}

	protected virtual void initStats()
	{
		stats_description.text = "";
		stats_values.text = "";
	}

	protected void showStat(string pID, object pValue)
	{
		Text obj = stats_description;
		obj.text = obj.text + LocalizedTextManager.getText(pID) + "\n";
		Text obj2 = stats_values;
		obj2.text = obj2.text + pValue?.ToString() + "\n";
	}

	protected virtual void showAssetWindow()
	{
		selected_asset = asset;
	}
}
