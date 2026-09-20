using UnityEngine;

public class WorldButton : MonoBehaviour
{
	public static WorldButton active_buttons;

	public WorldButton mainButtonObject;

	public WorldButton[] lesser_buttons;

	private Vector3 initial_pos;

	private void Start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		initial_pos = ((Component)this).transform.localPosition;
		if ((Object)(object)mainButtonObject != (Object)null)
		{
			hide();
		}
	}

	public void onClickMain()
	{
		if ((Object)(object)active_buttons != (Object)null && (Object)(object)active_buttons != (Object)(object)this)
		{
			active_buttons.hideChildren();
			active_buttons = null;
		}
		if (!((Component)lesser_buttons[0]).gameObject.activeSelf)
		{
			WorldButton[] array = lesser_buttons;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].activate();
			}
			active_buttons = this;
		}
		else
		{
			hideChildren();
		}
	}

	public void hideChildren()
	{
		WorldButton[] array = lesser_buttons;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].hide();
		}
	}

	public void hide()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).gameObject.SetActive(false);
		((Component)this).transform.localPosition = ((Component)mainButtonObject).transform.position;
	}

	public void activate()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).gameObject.SetActive(true);
		((Component)this).transform.localPosition = initial_pos;
	}
}
