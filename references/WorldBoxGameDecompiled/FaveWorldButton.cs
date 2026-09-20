using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FaveWorldButton : MonoBehaviour
{
	public Image icon;

	private void Start()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		Button val = default(Button);
		if (((Component)this).TryGetComponent<Button>(ref val))
		{
			((UnityEvent)val.onClick).AddListener(new UnityAction(faveWorld));
		}
	}

	private void OnEnable()
	{
		updateFavoriteIconFor(SaveManager.currentSlot);
	}

	private void faveWorld()
	{
		int currentSlot = SaveManager.currentSlot;
		if (PlayerConfig.instance.data.favorite_world == currentSlot)
		{
			PlayerConfig.instance.data.favorite_world = -1;
		}
		else
		{
			PlayerConfig.instance.data.favorite_world = currentSlot;
		}
		PlayerConfig.saveData();
		updateFavoriteIconFor(currentSlot);
	}

	private void updateFavoriteIconFor(int pId)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (PlayerConfig.instance.data.favorite_world == pId)
		{
			((Graphic)icon).color = ColorStyleLibrary.m.favorite_selected;
		}
		else
		{
			((Graphic)icon).color = ColorStyleLibrary.m.favorite_not_selected;
		}
	}
}
