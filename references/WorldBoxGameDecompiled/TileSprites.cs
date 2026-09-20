using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileSprites
{
	private List<Tile> _tiles = new List<Tile>();

	public Tile main => _tiles[0];

	public void addVariation(Sprite pSprite, string pID)
	{
		Tile val = ScriptableObject.CreateInstance<Tile>();
		((Object)val).name = pID;
		val.sprite = pSprite;
		_tiles.Add(val);
	}

	public Tile getRandom()
	{
		return _tiles.GetRandom();
	}

	public Tile getVariation(int pID)
	{
		return _tiles[pID];
	}
}
