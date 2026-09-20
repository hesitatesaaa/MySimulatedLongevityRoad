using UnityEngine;

public class BuildingRenderData
{
	public Vector3[] positions;

	public Vector3[] scales;

	public Vector3[] rotations;

	public Sprite[] colored_sprites;

	public Sprite[] main_sprites;

	public Material[] materials;

	public bool[] flip_x_states;

	public Color[] colors;

	public bool[] shadows;

	public Sprite[] shadow_sprites;

	public BuildingRenderData(int pCapacity)
	{
		checkSize(pCapacity);
	}

	public void checkSize(int pTargetSize)
	{
		if (positions == null || positions.Length < pTargetSize)
		{
			positions = Toolbox.checkArraySize(positions, pTargetSize);
			scales = Toolbox.checkArraySize(scales, pTargetSize);
			rotations = Toolbox.checkArraySize(rotations, pTargetSize);
			colored_sprites = Toolbox.checkArraySize(colored_sprites, pTargetSize);
			main_sprites = Toolbox.checkArraySize(main_sprites, pTargetSize);
			materials = Toolbox.checkArraySize(materials, pTargetSize);
			flip_x_states = Toolbox.checkArraySize(flip_x_states, pTargetSize);
			colors = Toolbox.checkArraySize(colors, pTargetSize);
			shadows = Toolbox.checkArraySize(shadows, pTargetSize);
			shadow_sprites = Toolbox.checkArraySize(shadow_sprites, pTargetSize);
		}
	}
}
