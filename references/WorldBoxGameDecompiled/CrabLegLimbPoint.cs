using UnityEngine;

public class CrabLegLimbPoint : MonoBehaviour
{
	private void Start()
	{
		((Renderer)((Component)this).GetComponent<SpriteRenderer>()).enabled = false;
	}
}
