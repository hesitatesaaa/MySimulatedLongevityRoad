using System;
using UnityEngine;
using UnityEngine.UI;

public class WorldAgeButton : BaseWorldAgeElement
{
	[SerializeField]
	private Image _selected;

	protected override void prepare()
	{
		base.prepare();
		DraggableLayoutElement draggableLayoutElement = default(DraggableLayoutElement);
		if (((Component)this).TryGetComponent<DraggableLayoutElement>(ref draggableLayoutElement))
		{
			DraggableLayoutElement draggableLayoutElement2 = draggableLayoutElement;
			draggableLayoutElement2.start_being_dragged = (Action<DraggableLayoutElement>)Delegate.Combine(draggableLayoutElement2.start_being_dragged, new Action<DraggableLayoutElement>(onStartDrag));
		}
	}

	private void onStartDrag(DraggableLayoutElement pOriginalElement)
	{
		((Behaviour)_selected).enabled = false;
	}

	public void toggleSelectedButton(bool pState)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_selected != (Object)null)
		{
			((Graphic)_selected).color = asset.pie_selection_color;
			((Behaviour)_selected).enabled = pState;
		}
	}
}
