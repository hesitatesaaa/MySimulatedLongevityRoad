using UnityEngine;

namespace LayoutGroupExt;

[AddComponentMenu("Layout/Horizontal Layout Group ( Extended )", 150)]
public class HorizontalLayoutGroupExtended : HorizontalOrVerticalLayoutGroupExtended
{
	protected HorizontalLayoutGroupExtended()
	{
	}

	public override void CalculateLayoutInputHorizontal()
	{
		base.CalculateLayoutInputHorizontal();
		CalcAlongAxis(0, isVertical: false);
	}

	public override void CalculateLayoutInputVertical()
	{
		CalcAlongAxis(1, isVertical: false);
	}

	public override void SetLayoutHorizontal()
	{
		SetChildrenAlongAxis(0, isVertical: false);
	}

	public override void SetLayoutVertical()
	{
		SetChildrenAlongAxis(1, isVertical: false);
	}
}
