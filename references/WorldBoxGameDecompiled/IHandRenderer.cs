using UnityEngine;

public interface IHandRenderer
{
	bool is_colored { get; }

	bool is_animated { get; }

	Sprite[] getSprites();

	string getID()
	{
		return (this as Asset).id;
	}
}
