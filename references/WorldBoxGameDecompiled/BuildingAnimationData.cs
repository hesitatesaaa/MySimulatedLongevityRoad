using UnityEngine;

public class BuildingAnimationData
{
	public bool animated;

	public ListPool<Sprite> list_spawn;

	public ListPool<Sprite> list_main;

	public ListPool<Sprite> list_main_disabled;

	public ListPool<Sprite> list_ruins;

	public ListPool<Sprite> list_special;

	public Sprite[] main;

	public Sprite[] main_disabled;

	public Sprite[] spawn;

	public Sprite[] ruins;

	public Sprite[] special;
}
