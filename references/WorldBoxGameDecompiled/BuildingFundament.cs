using System;

[Serializable]
public class BuildingFundament
{
	public readonly int left;

	public readonly int right;

	public readonly int top;

	public readonly int bottom;

	public readonly int width;

	public readonly int height;

	public BuildingFundament(int pLeft, int pRight, int pTop, int pBottom)
	{
		left = pLeft;
		right = pRight;
		top = pTop;
		bottom = pBottom;
		width = right + left + 1;
		height = top + bottom + 1;
	}
}
