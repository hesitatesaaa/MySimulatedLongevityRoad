using UnityEngine;
using UnityEngine.UI;

public class NerveImpulseElement : MonoBehaviour
{
	public Image image;

	private const float SPEED_MIN = 1f;

	private const float SPEED_MAX = 3f;

	private const float SCALE_MIN = 0.6f;

	private const float SCALE_MAX = 1f;

	private float _speed_current;

	private float _move_timer;

	public NeuronElement presynaptic_neuron;

	public NeuronElement postsynaptic_neuron;

	private Color _color_back;

	private Color _color_front;

	public int wave;

	public void energize(NeuronElement pPresynapticNeuron, NeuronElement pPostsynapticNeuron, int pWave)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.localPosition = ((Component)pPresynapticNeuron).transform.localPosition;
		presynaptic_neuron = pPresynapticNeuron;
		postsynaptic_neuron = pPostsynapticNeuron;
		_move_timer = 0f;
		_speed_current = Randy.randomFloat(1f, 3f);
		wave = pWave;
	}

	public ImpulseReachResult moveTowardsNextNeuron()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)postsynaptic_neuron == (Object)null)
		{
			return ImpulseReachResult.Done;
		}
		_move_timer += _speed_current * Time.deltaTime;
		_move_timer = Mathf.Clamp01(_move_timer);
		Vector3 localPosition = ((Component)presynaptic_neuron).transform.localPosition;
		Vector3 localPosition2 = ((Component)postsynaptic_neuron).transform.localPosition;
		((Component)this).transform.localPosition = Vector3.Lerp(localPosition, localPosition2, _move_timer);
		updateImpulseColor();
		if (_move_timer >= 1f)
		{
			presynaptic_neuron = postsynaptic_neuron;
			postsynaptic_neuron = GetNextTargetNeuron();
			_move_timer = 0f;
			wave--;
			if (wave > 0)
			{
				return ImpulseReachResult.Split;
			}
			return ImpulseReachResult.Done;
		}
		return ImpulseReachResult.Move;
	}

	private NeuronElement GetNextTargetNeuron()
	{
		if (presynaptic_neuron.connected_neurons.Count == 0)
		{
			return null;
		}
		return presynaptic_neuron.connected_neurons.GetRandom();
	}

	private void updateImpulseColor()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Lerp(presynaptic_neuron.render_depth, postsynaptic_neuron.render_depth, _move_timer);
		Color val = Color.Lerp(_color_back, _color_front, num);
		if (((Graphic)image).color != val)
		{
			((Graphic)image).color = val;
		}
		float num2 = Mathf.Lerp(0.6f, 1f, num);
		if (((Component)this).transform.localScale.x != num2)
		{
			((Component)this).transform.localScale = new Vector3(num2, num2, num2);
		}
	}

	public NerveImpulseElement()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		_color_back = Toolbox.makeColor("#26A8A8", 0.5f);
		_color_front = Toolbox.makeColor("#3AFFFF", 0.7f);
		((MonoBehaviour)this)._002Ector();
	}
}
