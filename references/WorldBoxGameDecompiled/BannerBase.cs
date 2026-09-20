using System;
using DG.Tweening;
using UnityEngine;

public abstract class BannerBase : MonoBehaviour, IBanner, IBaseMono, IRefreshElement
{
	private Sequence _sequence;

	protected virtual MetaType meta_type
	{
		get
		{
			throw new NotImplementedException(((object)this).GetType().Name);
		}
	}

	public MetaCustomizationAsset meta_asset => AssetManager.meta_customization_library.getAsset(meta_type);

	public MetaTypeAsset meta_type_asset => AssetManager.meta_type_library.getAsset(meta_type);

	internal int option_1
	{
		get
		{
			return meta_asset.option_1_get();
		}
		set
		{
			meta_asset.option_1_set(value);
		}
	}

	internal int option_2
	{
		get
		{
			return meta_asset.option_2_get();
		}
		set
		{
			meta_asset.option_2_set(value);
		}
	}

	internal int color
	{
		get
		{
			return meta_asset.color_get();
		}
		set
		{
			meta_asset.color_set(value);
		}
	}

	public virtual void load(NanoObject pObject)
	{
	}

	public virtual NanoObject GetNanoObject()
	{
		return null;
	}

	public void jump(float pSpeed = 0.1f, bool pSilent = false)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected O, but got Unknown
		float y = ((Component)this).transform.localPosition.y;
		TweenExtensions.Kill((Tween)(object)_sequence, false);
		_sequence = DOTween.Sequence();
		TweenSettingsExtensions.Append(_sequence, (Tween)(object)ShortcutExtensions.DOLocalMoveY(((Component)this).transform, y + 5f, pSpeed, false));
		TweenSettingsExtensions.Append(_sequence, (Tween)(object)ShortcutExtensions.DOLocalMoveY(((Component)this).transform, y, pSpeed, false));
		TweenSettingsExtensions.AppendCallback(_sequence, (TweenCallback)delegate
		{
			if (!pSilent)
			{
				SoundBox.click();
			}
		});
	}

	private void OnDisable()
	{
		TweenExtensions.Kill((Tween)(object)_sequence, false);
	}

	public string getName()
	{
		return GetNanoObject().name;
	}

	public virtual void showTooltip()
	{
		throw new NotImplementedException();
	}

	T IBaseMono.GetComponent<T>()
	{
		return ((Component)this).GetComponent<T>();
	}
}
