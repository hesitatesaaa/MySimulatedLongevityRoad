using UnityEngine;

public class ActorRenderData
{
	public Vector3[] positions;

	public Vector3[] scales;

	public Vector3[] rotations;

	public Color[] colors;

	public bool[] has_normal_render;

	public Sprite[] main_sprites;

	public Sprite[] main_sprite_colored;

	public Material[] materials;

	public bool[] flip_x_states;

	public bool[] shadows;

	public Vector3[] shadow_position;

	public Vector3[] shadow_scales;

	public Sprite[] shadow_sprites;

	public bool[] has_item;

	public Vector3[] item_scale;

	public Vector3[] item_pos;

	public Sprite[] item_sprites;

	public ActorRenderData(int pCapacity)
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
			colors = Toolbox.checkArraySize(colors, pTargetSize);
			has_normal_render = Toolbox.checkArraySize(has_normal_render, pTargetSize);
			main_sprites = Toolbox.checkArraySize(main_sprites, pTargetSize);
			main_sprite_colored = Toolbox.checkArraySize(main_sprite_colored, pTargetSize);
			materials = Toolbox.checkArraySize(materials, pTargetSize);
			flip_x_states = Toolbox.checkArraySize(flip_x_states, pTargetSize);
			shadows = Toolbox.checkArraySize(shadows, pTargetSize);
			shadow_sprites = Toolbox.checkArraySize(shadow_sprites, pTargetSize);
			shadow_position = Toolbox.checkArraySize(shadow_position, pTargetSize);
			shadow_scales = Toolbox.checkArraySize(shadow_scales, pTargetSize);
			has_item = Toolbox.checkArraySize(has_item, pTargetSize);
			item_scale = Toolbox.checkArraySize(item_scale, pTargetSize);
			item_pos = Toolbox.checkArraySize(item_pos, pTargetSize);
			item_sprites = Toolbox.checkArraySize(item_sprites, pTargetSize);
		}
	}
}
