using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class NeuronElement : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IInitializePotentialDragHandler
{
	[SerializeField]
	public Image image;

	private const float SCALE_HIGHLIGHTED = 1.6f;

	private const float SCALE_SPAWN_IMPULSE = 1.5f;

	private const float SCALE_RECEIVE_IMPULSE_INCREASE = 0.1f;

	private const float SCALE_NORMAL = 1f;

	internal float render_depth;

	internal bool highlighted;

	internal List<NeuronElement> connected_neurons = new List<NeuronElement>();

	internal float scale_mod_spawn = 1f;

	internal float bonus_scale = 1f;

	internal DecisionAsset decision;

	internal Actor actor;

	private float _spawn_interval;

	private float _spawn_timer;

	private List<AxonElement> _axons = new List<AxonElement>();

	private NeuronsOverview _neurons_overview;

	private TooltipData _tooltip_data;

	private bool _center;

	private void Start()
	{
		_neurons_overview = ((Component)this).gameObject.GetComponentInParent<NeuronsOverview>();
		initClick();
		initTooltip();
	}

	protected void initClick()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		Button val = default(Button);
		if (((Component)this).TryGetComponent<Button>(ref val))
		{
			((UnityEvent)val.onClick).AddListener(new UnityAction(setPressed));
		}
	}

	protected void initTooltip()
	{
		TipButton tipButton = default(TipButton);
		if (((Component)this).TryGetComponent<TipButton>(ref tipButton))
		{
			Object.Destroy((Object)(object)tipButton);
		}
	}

	private void showTooltip()
	{
		if (isCenter())
		{
			Tooltip.show(this, "tip", new TooltipData
			{
				tip_name = "toggle_all_neurons",
				tip_description = "toggle_all_neurons_description"
			});
		}
		else if (hasDecisionSet())
		{
			_tooltip_data = new TooltipData
			{
				neuron = this,
				tooltip_scale = Mathf.Lerp(0.4f, 1f, render_depth)
			};
			Tooltip.show(this, "neuron", _tooltip_data);
		}
	}

	public void setupDecisionAndActor(DecisionAsset pAsset, Actor pActor)
	{
		decision = pAsset;
		actor = pActor;
		image.sprite = decision.getSprite();
		_spawn_interval = decision.cooldown;
		_spawn_timer = Randy.randomFloat(0f, _spawn_interval);
	}

	public void updateColorsAndTooltip()
	{
		if (!highlighted)
		{
			scale_mod_spawn -= Time.deltaTime * 2f;
			scale_mod_spawn = Mathf.Max(1f, scale_mod_spawn);
		}
		else if (hasDecisionSet() && Tooltip.isShowingFor(this))
		{
			_tooltip_data.tooltip_scale = Mathf.Lerp(0.4f, 1f, render_depth);
		}
	}

	public void updateSpawnTimer()
	{
		if (_spawn_timer > 0f && isDecisionEnabled())
		{
			_spawn_timer -= Time.deltaTime;
		}
	}

	public void spawnImpulseFromHere()
	{
		scale_mod_spawn = Math.Max(1.5f, scale_mod_spawn);
		_spawn_timer = _spawn_interval;
		foreach (AxonElement axon in _axons)
		{
			axon.mod_light = 1f;
		}
	}

	public bool isDecisionEnabled()
	{
		if (!hasDecisionSet())
		{
			return true;
		}
		return actor.isDecisionEnabled(decision.decision_index);
	}

	public void setHighlighted()
	{
		if (!highlighted)
		{
			highlighted = true;
			scale_mod_spawn = 1.6f;
			showTooltip();
		}
	}

	public void setPressed()
	{
		if (_neurons_overview.isDragging() || (!hasDecisionSet() && !isCenter()))
		{
			return;
		}
		_neurons_overview.setLatestTouched(this);
		if (!InputHelpers.mouseSupported)
		{
			if (!Tooltip.isShowingFor(this))
			{
				showTooltip();
				return;
			}
		}
		else
		{
			showTooltip();
		}
		scale_mod_spawn = 1.6f;
		if (isCenter())
		{
			centerBrainClick();
		}
		else
		{
			actor.switchDecisionState(decision.decision_index);
		}
		actor.makeConfused(-1f, pColorEffect: true);
		_neurons_overview.fireImpulseWaveFromHere(this);
		_neurons_overview.startNewWhat();
		actor.updateStats();
		((Component)this).GetComponentInParent<StatsWindow>().updateStats();
		AchievementLibrary.mindless_husk.check(actor);
	}

	private void centerBrainClick()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		_neurons_overview.switchAllNeurons();
		if (_neurons_overview.getAllState())
		{
			((Graphic)image).color = Color32.op_Implicit(Toolbox.color_clear);
		}
		else
		{
			((Graphic)image).color = Toolbox.color_white;
		}
	}

	public void receiveImpulse()
	{
		scale_mod_spawn += 0.1f;
		scale_mod_spawn = Mathf.Min(1.5f, scale_mod_spawn);
	}

	public bool readyToSpawnImpulse()
	{
		if (!isDecisionEnabled())
		{
			return false;
		}
		return _spawn_timer <= 0f;
	}

	public int getSimulatedTimer()
	{
		return (int)_spawn_timer;
	}

	public void clear()
	{
		decision = null;
		actor = null;
		connected_neurons.Clear();
		_axons.Clear();
		_center = false;
	}

	public void setColor(Color pColor)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)image).color = pColor;
	}

	public bool hasDecisionSet()
	{
		return decision != null;
	}

	public void addConnection(NeuronElement pConnection, AxonElement pAxon)
	{
		connected_neurons.Add(pConnection);
		_axons.Add(pAxon);
	}

	public void setCenter(bool pState)
	{
		_center = pState;
	}

	public bool isCenter()
	{
		return _center;
	}

	public void OnInitializePotentialDrag(PointerEventData pEventData)
	{
		NeuronsOverview neurons_overview = _neurons_overview;
		if (neurons_overview != null)
		{
			((Component)neurons_overview).SendMessage("OnInitializePotentialDrag", (object)pEventData);
		}
	}

	public void OnBeginDrag(PointerEventData pEventData)
	{
		NeuronsOverview neurons_overview = _neurons_overview;
		if (neurons_overview != null)
		{
			((Component)neurons_overview).SendMessage("OnBeginDrag", (object)pEventData);
		}
	}

	public void OnDrag(PointerEventData pEventData)
	{
		NeuronsOverview neurons_overview = _neurons_overview;
		if (neurons_overview != null)
		{
			((Component)neurons_overview).SendMessage("OnDrag", (object)pEventData);
		}
	}

	public void OnEndDrag(PointerEventData pEventData)
	{
		NeuronsOverview neurons_overview = _neurons_overview;
		if (neurons_overview != null)
		{
			((Component)neurons_overview).SendMessage("OnEndDrag", (object)pEventData);
		}
	}
}
