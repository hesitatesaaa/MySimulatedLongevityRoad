using UnityEngine;

public class QuantumSpriteCacheData
{
	public Sprite[] sprites;

	public Vector3[] scales;

	public Vector3[] shadow_scales;

	public Vector3[] positions;

	public Material[] materials;

	public Vector3[] rotations;

	public bool[] flip_x_states;

	public Color[] colors;

	public int[] indexes;

	public int[] indexes_2;

	public QuantumSpriteCacheData(int pCapacity)
	{
		checkSize(pCapacity);
	}

	public void checkSize(int pTargetSize)
	{
		if (positions == null || positions.Length < pTargetSize)
		{
			positions = Toolbox.checkArraySize(positions, pTargetSize);
			scales = Toolbox.checkArraySize(scales, pTargetSize);
			shadow_scales = Toolbox.checkArraySize(shadow_scales, pTargetSize);
			rotations = Toolbox.checkArraySize(rotations, pTargetSize);
			sprites = Toolbox.checkArraySize(sprites, pTargetSize);
			materials = Toolbox.checkArraySize(materials, pTargetSize);
			flip_x_states = Toolbox.checkArraySize(flip_x_states, pTargetSize);
			colors = Toolbox.checkArraySize(colors, pTargetSize);
			indexes = Toolbox.checkArraySize(indexes, pTargetSize);
			indexes_2 = Toolbox.checkArraySize(indexes_2, pTargetSize);
		}
	}
}
