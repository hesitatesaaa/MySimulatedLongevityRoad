using UnityEngine;
using UnityEngine.UI;

public class MaskAutoEnabler : MonoBehaviour
{
	private void Awake()
	{
		((Behaviour)((Component)this).GetComponent<Mask>()).enabled = true;
		((Behaviour)((Component)this).GetComponent<Image>()).enabled = true;
	}
}
