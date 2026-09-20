using UnityEngine;

namespace LayoutGroupExt;

[AddComponentMenu("Layout/Vertical Layout Group ( Extended )", 151)]
public class VerticalLayoutGroupExtended : HorizontalOrVerticalLayoutGroupExtended
{
	protected VerticalLayoutGroupExtended()
	{
	}

	public override void CalculateLayoutInputHorizontal()
	{
		base.CalculateLayoutInputHorizontal();
		CalcAlongAxis(0, isVertical: true);
	}

	public override void CalculateLayoutInputVertical()
	{
		CalcAlongAxis(1, isVertical: true);
	}

	public override void SetLayoutHorizontal()
	{
		SetChildrenAlongAxis(0, isVertical: true);
	}

	public override void SetLayoutVertical()
	{
		SetChildrenAlongAxis(1, isVertical: true);
	}
}
