using System.Collections.Generic;
using UnityEngine;

public class AnimationContainerUnit
{
	public bool child;

	internal readonly string id;

	internal readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

	internal readonly Dictionary<string, AnimationFrameData> dict_frame_data = new Dictionary<string, AnimationFrameData>();

	internal ActorAnimation idle;

	internal ActorAnimation walking;

	internal ActorAnimation swimming;

	public bool has_swimming;

	public bool has_idle;

	public bool has_walking;

	public bool render_heads_for_children;

	internal Sprite[] heads;

	internal Sprite[] heads_male;

	internal Sprite[] heads_female;

	public AnimationContainerUnit(string pTexturePath)
	{
		id = pTexturePath;
		Sprite[] spriteList = SpriteTextureLoader.getSpriteList(pTexturePath);
		foreach (Sprite val in spriteList)
		{
			sprites.Add(((Object)val).name, val);
		}
	}
}
