using UnityEngine;
using UnityEngine.UI;

public class AxonElement : MonoBehaviour
{
	public Image image;

	internal NeuronElement neuron_1;

	internal NeuronElement neuron_2;

	internal float mod_light = 1f;

	public bool axon_center;

	public void update()
	{
		mod_light -= Time.deltaTime * 2f;
		mod_light = Mathf.Max(0f, mod_light);
	}

	public void clear()
	{
		axon_center = false;
	}
}
