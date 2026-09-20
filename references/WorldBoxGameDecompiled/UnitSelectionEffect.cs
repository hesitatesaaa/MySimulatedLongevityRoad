using UnityEngine;

public class UnitSelectionEffect : BaseAnimatedObject
{
	public static Actor last_actor;

	private bool _is_visible = true;

	internal override void create()
	{
		base.create();
		((Component)this).transform.parent = ((Component)World.world).transform;
		((Object)((Component)this).transform).name = "unit_selector_effect";
	}

	public override void update(float pElapsed)
	{
		base.update(pElapsed);
		bool visible = visibleCheck();
		setVisible(visible);
	}

	public bool visibleCheck()
	{
		if (!MapBox.isRenderGameplay())
		{
			return false;
		}
		if (World.world.isAnyPowerSelected())
		{
			GodPower godPower = World.world.selected_buttons.selectedButton.godPower;
			if (!godPower.allow_unit_selection && !godPower.show_close_actor)
			{
				return false;
			}
		}
		if (World.world.isBusyWithUI())
		{
			return false;
		}
		Actor actor = World.world.getActorNearCursor();
		if (ControllableUnit.isControllingUnit())
		{
			actor = null;
		}
		setLastActor(actor);
		if (actor == null)
		{
			return false;
		}
		return true;
	}

	public void setVisible(bool pVisible)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (pVisible != _is_visible)
		{
			((Component)this).gameObject.SetActive(pVisible);
			if (!pVisible)
			{
				((Component)this).transform.position = Globals.POINT_IN_VOID;
				((Component)this).transform.localScale = Vector3.one;
				setLastActor(null);
			}
		}
		if (pVisible)
		{
			((Component)this).transform.position = Vector2.op_Implicit(last_actor.current_position);
			((Component)this).transform.localScale = last_actor.current_scale;
		}
		_is_visible = pVisible;
	}

	public static void setLastActor(Actor pActor)
	{
		last_actor = pActor;
	}
}
