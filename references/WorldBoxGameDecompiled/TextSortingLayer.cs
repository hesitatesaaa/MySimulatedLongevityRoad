using UnityEngine;

public class TextSortingLayer : MonoBehaviour
{
	private MeshRenderer meshRenderer;

	private void Start()
	{
		meshRenderer = ((Component)this).gameObject.GetComponent<MeshRenderer>();
		((Renderer)meshRenderer).sortingLayerID = SortingLayer.NameToID("MapOverlay");
		((Renderer)meshRenderer).sortingOrder = 200;
	}
}
