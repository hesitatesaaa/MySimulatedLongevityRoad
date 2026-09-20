using System;
using System.Collections.Generic;
using System.Globalization;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NeuronsOverview : UnitElement, IInitializePotentialDragHandler, IEventSystemHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	private const float DRAGGING_SMOOTHING_TIME = 0.1f;

	private const float ROTATION_BOUNDS = 0.7f;

	private const float ROTATION_BOUNDS_MARGIN = 1.05f;

	private const float DRAG_SPEED = 0.46f;

	private const float DRAG_ROTATE_SPEED = 0.005f;

	private const float MIN_NEURON_CURSOR_DISTANCE = 40f;

	private const float RADIUS_NEURONS = 70f;

	private const float NEURON_SCALE_MIN = 0.8f;

	private const float NEURON_SCALE_MAX = 1.5f;

	private const float BASE_AXON_DISTANCE = 250f;

	private const float DISTANCE_SCALING_FACTOR = 1.5f;

	[SerializeField]
	private NeuronElement _prefab_neuron;

	[SerializeField]
	private NerveImpulseElement _prefab_nerve_impulse;

	[SerializeField]
	private AxonElement _prefab_axon;

	[SerializeField]
	private RectTransform _parent_axons;

	[SerializeField]
	private RectTransform _parent_nerve_impulses;

	[SerializeField]
	private RectTransform _parent_neurons;

	[SerializeField]
	private GameObject _mind_main;

	[SerializeField]
	private UnitTextManager _text_phrases;

	private ObjectPoolGenericMono<NeuronElement> _pool_neurons;

	private ObjectPoolGenericMono<NerveImpulseElement> _pool_impulses;

	private ObjectPoolGenericMono<AxonElement> _pool_axons;

	private List<NeuronElement> _neurons;

	private NeuronElement _last_activated_neuron;

	private List<NerveImpulseElement> _active_impulses;

	private Color _color_neuron_disabled_front;

	private Color _color_neuron_disabled_back;

	private Color _color_neuron_back;

	private Color _color_neuron_front;

	private Color _color_axon_default;

	private Color _color_axon_default_center;

	private Color _color_light_axon;

	private Color _neuron_highlighted;

	private float _offset_target_x;

	private float _offset_target_y;

	private bool _is_dragging;

	private Vector2 _last_mouse_delta;

	private float _offset_x;

	private float _offset_y;

	private int _decision_counter;

	private DecisionAsset[] _decision_assets;

	public static NeuronsOverview instance;

	private NeuronElement _active_neuron;

	private NeuronElement _latest_touched_neuron;

	private bool _all_state;

	private void Start()
	{
		instance = this;
	}

	protected override void Awake()
	{
		base.Awake();
		_pool_neurons = new ObjectPoolGenericMono<NeuronElement>(_prefab_neuron, (Transform)(object)_parent_neurons);
		_pool_impulses = new ObjectPoolGenericMono<NerveImpulseElement>(_prefab_nerve_impulse, (Transform)(object)_parent_nerve_impulses);
		_pool_axons = new ObjectPoolGenericMono<AxonElement>(_prefab_axon, (Transform)(object)_parent_axons);
	}

	public void OnInitializePotentialDrag(PointerEventData eventData)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		eventData.useDragThreshold = false;
		_last_mouse_delta = Vector2.zero;
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		_offset_x = (_offset_target_x = 0f);
		_offset_y = (_offset_target_y = 0f);
		clearHighlight();
		Tooltip.hideTooltipNow();
	}

	private void highlightAllAxons(float pLight)
	{
		foreach (AxonElement item in _pool_axons.getListTotal())
		{
			if (!(item.mod_light > pLight))
			{
				item.mod_light = pLight;
			}
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		_is_dragging = true;
		Vector2 delta = eventData.delta;
		if (((Vector2)(ref delta)).magnitude > ((Vector2)(ref _last_mouse_delta)).magnitude)
		{
			highlightAllAxons(0.35f);
		}
		_last_mouse_delta = delta;
		_offset_x = (0f - delta.y) * 0.46f;
		_offset_y = delta.x * 0.46f;
		updateNeuronsVisual();
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		_is_dragging = false;
		Vector2 delta = eventData.delta;
		_offset_target_x += (0f - delta.y) * 0.005f;
		_offset_target_y += delta.x * 0.005f;
		if (Mathf.Abs(_offset_target_x) > 0.7f || Mathf.Abs(_offset_target_y) > 0.7f)
		{
			if (Mathf.Abs(_offset_target_x) > Mathf.Abs(_offset_target_y))
			{
				_offset_target_y = _offset_target_y / Mathf.Abs(_offset_target_x) * 0.7f;
			}
			else
			{
				_offset_target_x = _offset_target_x / Mathf.Abs(_offset_target_y) * 0.7f;
			}
		}
		_offset_target_x = Mathf.Clamp(_offset_target_x, -0.7f, 0.7f);
		_offset_target_y = Mathf.Clamp(_offset_target_y, -0.7f, 0.7f);
		highlightAllAxons(1f);
		fireImpulsesEverywhere();
	}

	private void fireImpulsesEverywhere()
	{
		for (int i = 0; i < _neurons.Count; i++)
		{
			NeuronElement pNeuron = _neurons[i];
			fireImpulseWaveFromHere(pNeuron, 2);
		}
	}

	private void highlightNeuron(NeuronElement pHighlighted = null)
	{
		foreach (NeuronElement neuron in _neurons)
		{
			if (!((Object)(object)neuron == (Object)(object)pHighlighted) && neuron.highlighted)
			{
				neuron.highlighted = false;
				Tooltip.hideTooltipNow();
			}
		}
		pHighlighted?.setHighlighted();
	}

	private NeuronElement getClosestNeuronToCursor()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		NeuronElement result = null;
		float num = float.MaxValue;
		Vector2 val = Vector2.op_Implicit(Input.mousePosition);
		foreach (NeuronElement neuron in _neurons)
		{
			Vector2 val2 = Vector2.op_Implicit(((Component)neuron).transform.position);
			float num2 = Vector2.Distance(val, val2);
			if (!(num2 > 40f))
			{
				if ((Object)(object)neuron == (Object)(object)_active_neuron)
				{
					return neuron;
				}
				if (num2 < num)
				{
					num = num2;
					result = neuron;
				}
			}
		}
		return result;
	}

	private void prepareAxons()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		int count = _neurons.Count;
		float num = 250f / Mathf.Sqrt((float)count) * 1.5f;
		for (int i = 0; i < _neurons.Count - 1; i++)
		{
			NeuronElement neuronElement = _neurons[i];
			if (neuronElement.isCenter())
			{
				continue;
			}
			for (int j = i + 1; j < _neurons.Count; j++)
			{
				NeuronElement neuronElement2 = _neurons[j];
				if (!neuronElement2.isCenter() && !(Vector3.Distance(((Component)neuronElement).transform.localPosition, ((Component)neuronElement2).transform.localPosition) > num))
				{
					makeAxon(neuronElement, neuronElement2);
				}
			}
		}
	}

	private AxonElement makeAxon(NeuronElement pNeuron1, NeuronElement pNeuron2)
	{
		AxonElement next = _pool_axons.getNext();
		next.neuron_1 = pNeuron1;
		next.neuron_2 = pNeuron2;
		pNeuron1.addConnection(pNeuron2, next);
		pNeuron2.addConnection(pNeuron1, next);
		return next;
	}

	private void checkActorDecisions()
	{
		DecisionHelper.runSimulationForMindTab(actor);
		_decision_counter = DecisionHelper.decision_system.getCounter();
		_decision_assets = DecisionHelper.decision_system.getActions();
	}

	private void updateNeuronsVisual()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		Quaternion val = Quaternion.Euler(_offset_x, _offset_y, 0f);
		foreach (NeuronElement neuron in _neurons)
		{
			neuron.updateColorsAndTooltip();
			Vector3 localPosition = val * ((Component)neuron).transform.localPosition;
			((Component)neuron).transform.localPosition = localPosition;
			calculateNeuronDepth(neuron, 70f);
			updateNeuronColorAndScale(neuron);
		}
		sortNeuronsByDepth();
	}

	private void updateNeuronImpulseAutoSpawn()
	{
		foreach (NeuronElement neuron in _neurons)
		{
			neuron.updateSpawnTimer();
		}
	}

	private void sortNeuronsByDepth()
	{
		foreach (NeuronElement neuron in _neurons)
		{
			((Component)neuron).transform.SetAsLastSibling();
		}
		_neurons.Sort((NeuronElement a, NeuronElement b) => a.render_depth.CompareTo(b.render_depth));
	}

	private void calculateNeuronDepth(NeuronElement pNeuronElement, float pRadius)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		float z = ((Component)pNeuronElement).transform.localPosition.z;
		float render_depth = Mathf.InverseLerp(0f - pRadius, pRadius, z);
		pNeuronElement.render_depth = render_depth;
	}

	private void updateNeuronColorAndScale(NeuronElement pElement)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		if (!pElement.isDecisionEnabled())
		{
			Color color = Color.Lerp(_color_neuron_disabled_back, _color_neuron_disabled_front, pElement.render_depth);
			pElement.setColor(color);
		}
		else if (pElement.highlighted)
		{
			Color color2 = Color.Lerp(_color_neuron_back, _neuron_highlighted, pElement.render_depth);
			pElement.setColor(color2);
		}
		else
		{
			Color color3 = Color.Lerp(_color_neuron_back, _color_neuron_front, pElement.render_depth);
			pElement.setColor(color3);
		}
		float num = Mathf.Lerp(0.8f, 1.5f, pElement.render_depth);
		num *= pElement.scale_mod_spawn * pElement.bonus_scale;
		((Component)pElement).transform.localScale = new Vector3(num, num, num);
	}

	private void updateAxonPositions()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		foreach (AxonElement item in _pool_axons.getListTotal())
		{
			item.update();
			float num = 1f;
			NeuronElement neuron_ = item.neuron_1;
			NeuronElement neuron_2 = item.neuron_2;
			if (neuron_.highlighted || neuron_2.highlighted)
			{
				num = 6f;
			}
			Color val = _color_axon_default;
			if (item.axon_center)
			{
				val = _color_axon_default_center;
				num = 7f;
			}
			if (item.mod_light > 0f)
			{
				Color color = Color.Lerp(val, _color_light_axon, item.mod_light);
				((Graphic)item.image).color = color;
			}
			else
			{
				((Graphic)item.image).color = val;
			}
			Vector2 val2 = Vector2.op_Implicit(((Component)neuron_).transform.localPosition);
			Vector2 val3 = Vector2.op_Implicit(((Component)neuron_2).transform.localPosition);
			Vector2 val4 = (val2 + val3) / 2f;
			((Component)item).transform.localPosition = Vector2.op_Implicit(val4);
			float num2 = Vector3.Distance(Vector2.op_Implicit(val2), Vector2.op_Implicit(val3));
			((Component)item).transform.localScale = new Vector3(num2, num, 1f);
			Vector3 val5 = Vector2.op_Implicit(val3 - val2);
			float num3 = Mathf.Atan2(val5.y, val5.x) * 57.29578f;
			((Component)item).transform.rotation = Quaternion.Euler(0f, 0f, num3);
		}
	}

	private void smoothOffsets()
	{
		_offset_x = Mathf.Lerp(_offset_x, _offset_target_x, 0.1f);
		_offset_y = Mathf.Lerp(_offset_y, _offset_target_y, 0.1f);
	}

	internal void fireImpulseWaveFromHere(NeuronElement pNeuron, int pWaves = 4)
	{
		if (!pNeuron.isDecisionEnabled())
		{
			return;
		}
		foreach (NeuronElement connected_neuron in pNeuron.connected_neurons)
		{
			if (connected_neuron.isDecisionEnabled())
			{
				fireImpulse(pNeuron, connected_neuron, pWaves);
			}
		}
	}

	private void fireImpulseFrom(NeuronElement pPresynapticNeuron, int pWave, NeuronElement pIgnoreNeuron = null)
	{
		if (pPresynapticNeuron.connected_neurons.Count == 0)
		{
			return;
		}
		NeuronElement random;
		if ((Object)(object)pIgnoreNeuron == (Object)null)
		{
			random = pPresynapticNeuron.connected_neurons.GetRandom();
		}
		else
		{
			using ListPool<NeuronElement> listPool = new ListPool<NeuronElement>();
			foreach (NeuronElement connected_neuron in pPresynapticNeuron.connected_neurons)
			{
				if (!((Object)(object)pIgnoreNeuron == (Object)(object)connected_neuron))
				{
					listPool.Add(connected_neuron);
				}
			}
			if (listPool.Count == 0)
			{
				return;
			}
			random = listPool.GetRandom();
		}
		if (!((Object)(object)random == (Object)null))
		{
			fireImpulse(pPresynapticNeuron, random, pWave);
		}
	}

	internal void fireImpulse(NeuronElement pPresynapticNeuron, NeuronElement pPostsynapticNeuron, int pWave)
	{
		NerveImpulseElement next = _pool_impulses.getNext();
		next.energize(pPresynapticNeuron, pPostsynapticNeuron, pWave);
		pPresynapticNeuron.spawnImpulseFromHere();
		_active_impulses.Add(next);
	}

	private void updateImpulses()
	{
		for (int num = _active_impulses.Count - 1; num >= 0; num--)
		{
			NerveImpulseElement nerveImpulseElement = _active_impulses[num];
			ImpulseReachResult impulseReachResult = nerveImpulseElement.moveTowardsNextNeuron();
			NeuronElement postsynaptic_neuron = nerveImpulseElement.postsynaptic_neuron;
			NeuronElement presynaptic_neuron = nerveImpulseElement.presynaptic_neuron;
			switch (impulseReachResult)
			{
			case ImpulseReachResult.Done:
				postsynaptic_neuron?.receiveImpulse();
				_active_impulses.RemoveAt(num);
				_pool_impulses.release(nerveImpulseElement);
				break;
			case ImpulseReachResult.Split:
				postsynaptic_neuron?.receiveImpulse();
				fireImpulseFrom(postsynaptic_neuron, nerveImpulseElement.wave, presynaptic_neuron);
				break;
			}
		}
	}

	private void Update()
	{
		if (!_is_dragging)
		{
			smoothOffsets();
			if (InputHelpers.mouseSupported)
			{
				_active_neuron = getHighlightedNeuron();
				highlightNeuron(_active_neuron);
			}
			if (InputHelpers.mouseSupported || (Object)(object)_latest_touched_neuron == (Object)null || !Tooltip.isShowingFor(_latest_touched_neuron))
			{
				updateNeuronsVisual();
			}
		}
		updateNeuronImpulseAutoSpawn();
		updateAxonPositions();
		updateImpulseSpawn();
		updateImpulses();
	}

	private NeuronElement getHighlightedNeuron()
	{
		if (_is_dragging)
		{
			return null;
		}
		if (_offset_x > 1.05f || _offset_x < -1.05f)
		{
			return null;
		}
		if (_offset_y > 1.05f || _offset_y < -1.05f)
		{
			return null;
		}
		return getClosestNeuronToCursor();
	}

	internal void startNewWhat()
	{
		_text_phrases.startNewWhat();
	}

	private void updateImpulseSpawn()
	{
		foreach (NeuronElement neuron in _neurons)
		{
			if (neuron.hasDecisionSet() && neuron.readyToSpawnImpulse())
			{
				fireImpulseFrom(neuron, 1);
			}
		}
	}

	private void initStartPositions()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _decision_counter; i++)
		{
			DecisionAsset pAsset = _decision_assets[i];
			NeuronElement next = _pool_neurons.getNext();
			next.setupDecisionAndActor(pAsset, actor);
			Vector3 positionOnSphere = getPositionOnSphere(i, _decision_counter);
			((Component)next).transform.localPosition = positionOnSphere;
			_neurons.Add(next);
		}
		updateNeuronsVisual();
	}

	private void loadLastDecisionForCenter()
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		NeuronElement neuronElement = null;
		string lastDecisionForMindOverview = actor.getLastDecisionForMindOverview();
		if (!string.IsNullOrEmpty(lastDecisionForMindOverview))
		{
			foreach (NeuronElement neuron in _neurons)
			{
				if (neuron.decision.id == lastDecisionForMindOverview)
				{
					neuronElement = neuron;
					break;
				}
			}
		}
		_last_activated_neuron = _pool_neurons.getNext();
		((Component)_last_activated_neuron).transform.localPosition = Vector3.zero;
		_last_activated_neuron.image.sprite = SpriteTextureLoader.getSprite("ui/icons/iconBrain");
		_last_activated_neuron.bonus_scale = 1.5f;
		_last_activated_neuron.setCenter(pState: true);
		_last_activated_neuron.actor = actor;
		_neurons.Add(_last_activated_neuron);
		if ((Object)(object)neuronElement != (Object)null)
		{
			makeAxon(neuronElement, _last_activated_neuron).axon_center = true;
		}
	}

	private Vector3 getPositionOnSphere(int pNeuronIndex, int pTotalNeurons)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Acos(1f - (float)(2 * (pNeuronIndex + 1)) / (float)pTotalNeurons);
		float num2 = MathF.PI * (1f + Mathf.Sqrt(5f)) * (float)pNeuronIndex;
		float num3 = 70f * Mathf.Cos(num2) * Mathf.Sin(num);
		float num4 = 70f * Mathf.Sin(num2) * Mathf.Sin(num);
		float num5 = 70f * Mathf.Cos(num);
		return new Vector3(num3, num4, num5);
	}

	private void clearMind()
	{
		foreach (NeuronElement neuron in _neurons)
		{
			neuron.clear();
		}
		_active_impulses.Clear();
		_pool_axons.clear();
		_pool_neurons.clear();
		_pool_impulses.clear();
		_neurons.Clear();
		foreach (AxonElement item in _pool_axons.getListTotal())
		{
			item.clear();
		}
	}

	protected override void OnEnable()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		ShortcutExtensions.DOKill((Component)(object)_mind_main.transform, false);
		_mind_main.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
		TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(_mind_main.transform, 1f, 0.6f), (Ease)27);
		checkActorDecisions();
		clearMind();
		initStartPositions();
		loadLastDecisionForCenter();
		prepareAxons();
		_is_dragging = false;
	}

	internal bool isDragging()
	{
		return _is_dragging;
	}

	internal void clearHighlight()
	{
		if (!((Object)(object)_active_neuron == (Object)null))
		{
			_active_neuron.highlighted = false;
			_active_neuron = null;
		}
	}

	public static void debugTool(DebugTool pTool)
	{
		instance?.debug(pTool);
	}

	public void debug(DebugTool pTool)
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		pTool.setText("offset_target_x:", getFloat(_offset_target_x), 0f, pShowBar: false, 0L);
		pTool.setText("offset_target_y:", getFloat(_offset_target_y), 0f, pShowBar: false, 0L);
		pTool.setSeparator();
		pTool.setText("offset_x:", getFloat(_offset_x), 0f, pShowBar: false, 0L);
		pTool.setText("offset_y:", getFloat(_offset_y), 0f, pShowBar: false, 0L);
		pTool.setSeparator();
		Quaternion val = Quaternion.Euler(_offset_x, _offset_y, 0f);
		pTool.setText("combined_rotation.x:", getFloat(val.x), 0f, pShowBar: false, 0L);
		pTool.setText("combined_rotation.y:", getFloat(val.y), 0f, pShowBar: false, 0L);
		pTool.setText("combined_rotation.z:", getFloat(val.z), 0f, pShowBar: false, 0L);
		pTool.setText("combined_rotation.w:", getFloat(val.w), 0f, pShowBar: false, 0L);
		pTool.setSeparator();
		pTool.setText("is_dragging:", _is_dragging, 0f, pShowBar: false, 0L);
		pTool.setText("last_mouse_delta:", _last_mouse_delta, 0f, pShowBar: false, 0L);
		pTool.setSeparator();
		pTool.setText("decisions:", _decision_counter, 0f, pShowBar: false, 0L);
		pTool.setSeparator();
		pTool.setText("neuron selected:", _active_neuron, 0f, pShowBar: false, 0L);
	}

	public static string getFloat(float pFloat)
	{
		if (pFloat < 0.001f && pFloat > -0.001f)
		{
			return pFloat.ToString("F6", CultureInfo.InvariantCulture);
		}
		if (pFloat > 0f)
		{
			return "<color=#75D53A>" + pFloat.ToString("F6", CultureInfo.InvariantCulture) + "</color>";
		}
		return "<color=#DB2920>" + pFloat.ToString("F6", CultureInfo.InvariantCulture) + "</color>";
	}

	public void setLatestTouched(NeuronElement pNeuron)
	{
		_latest_touched_neuron = pNeuron;
	}

	public void switchAllNeurons()
	{
		if (!isAnyEnabled())
		{
			_all_state = true;
		}
		else if (isAllEnabled())
		{
			_all_state = false;
		}
		else
		{
			_all_state = !_all_state;
		}
		foreach (NeuronElement neuron in _neurons)
		{
			if (neuron.hasDecisionSet())
			{
				actor.setDecisionState(neuron.decision.decision_index, _all_state);
			}
		}
		fireImpulsesEverywhere();
	}

	public bool getAllState()
	{
		return _all_state;
	}

	private bool isAnyEnabled()
	{
		foreach (NeuronElement neuron in _neurons)
		{
			if (neuron.hasDecisionSet() && actor.isDecisionEnabled(neuron.decision.decision_index))
			{
				return true;
			}
		}
		return false;
	}

	private bool isAllEnabled()
	{
		foreach (NeuronElement neuron in _neurons)
		{
			if (neuron.hasDecisionSet() && !actor.isDecisionEnabled(neuron.decision.decision_index))
			{
				return false;
			}
		}
		return true;
	}

	public NeuronsOverview()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		_neurons = new List<NeuronElement>();
		_active_impulses = new List<NerveImpulseElement>();
		_color_neuron_disabled_front = Toolbox.makeColor("#111111");
		_color_neuron_disabled_back = Toolbox.makeColor("#111111", 0.3f);
		_color_neuron_back = Toolbox.makeColor("#A9A9A9", 0.3f);
		_color_neuron_front = Toolbox.makeColor("#DDDDDD");
		_color_axon_default = Toolbox.makeColor("#FFFFFF", 0.1f);
		_color_axon_default_center = Toolbox.makeColor("#FF6666", 0.1f);
		_color_light_axon = Toolbox.makeColor("#3AFFFF", 0.54f);
		_neuron_highlighted = Toolbox.makeColor("#FFFFFF");
		_offset_target_x = -0.015f;
		_offset_target_y = 0.07f;
		_all_state = true;
		base._002Ector();
	}
}
