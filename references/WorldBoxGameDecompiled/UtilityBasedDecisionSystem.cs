using System;
using System.Collections.Generic;

public class UtilityBasedDecisionSystem
{
	private const int MAX_POSSIBLE_DECISIONS = 1024;

	private readonly DecisionAsset[] _actions = new DecisionAsset[1024];

	private readonly float[] _factors = new float[1024];

	private readonly float[] _chances = new float[1024];

	public static Dictionary<string, int> debug_counter = new Dictionary<string, int>();

	private DecisionAsset[] _all_assets = new DecisionAsset[1024];

	private int _counter_all_assets;

	private int _counter_possible;

	private int _highest_priority;

	private readonly DecisionAsset[][] _priority_array = new DecisionAsset[Enum.GetValues(typeof(NeuroLayer)).Length][];

	private readonly int[] _priority_array_counters = new int[Enum.GetValues(typeof(NeuroLayer)).Length];

	private bool _do_priority_levels;

	public UtilityBasedDecisionSystem()
	{
		int i = 0;
		for (int num = _priority_array.Length; i < num; i++)
		{
			_priority_array[i] = new DecisionAsset[1024];
			_priority_array_counters[i] = 0;
		}
	}

	public DecisionAsset useOn(Actor pActor, bool pGameplay = true)
	{
		clear();
		ActorAsset asset = pActor.asset;
		if (pActor.isAbleToSkipPriorityLevels())
		{
			_do_priority_levels = Randy.randomChance(0.8f);
		}
		else
		{
			_do_priority_levels = true;
		}
		registerBasicDecisionLists(pActor, pGameplay);
		if (asset.hasDecisions())
		{
			registerDecisionArray(pActor, asset.getDecisions(), asset.decisions_counter, pGameplay);
		}
		if (pActor.decisions_counter > 0)
		{
			registerDecisionArray(pActor, pActor.decisions, pActor.decisions_counter, pGameplay);
		}
		calculateFactors(pActor);
		if (_counter_possible == 0)
		{
			return null;
		}
		if (!pGameplay)
		{
			calculateChances();
		}
		DecisionAsset decisionAsset = chooseBestAction();
		if (pGameplay)
		{
			pActor.setDecisionCooldown(decisionAsset);
		}
		return decisionAsset;
	}

	private void registerBasicDecisionLists(Actor pActor, bool pGameplay)
	{
		if (pActor.asset.is_boat)
		{
			return;
		}
		DecisionsLibrary decisions_library = AssetManager.decisions_library;
		if (pActor.isAnimal())
		{
			registerDecisionArray(pActor, decisions_library.list_only_animal, -1, pGameplay);
		}
		else if (pActor.isKingdomCiv())
		{
			registerDecisionArray(pActor, decisions_library.list_only_civ, -1, pGameplay);
			if (pActor.hasCity())
			{
				registerDecisionArray(pActor, decisions_library.list_only_city, -1, pGameplay);
			}
		}
		if (pActor.isBaby())
		{
			registerDecisionArray(pActor, decisions_library.list_only_children, -1, pGameplay);
		}
		registerDecisionArray(pActor, decisions_library.list_others, -1, pGameplay);
	}

	private void registerDecisionArray(Actor pActor, DecisionAsset[] pList, int pLength = -1, bool pGameplay = true)
	{
		if (pLength == -1)
		{
			pLength = pList.Length;
		}
		if (pGameplay)
		{
			registerDecisionArrayGameplay(pActor, pList, pLength);
		}
		else
		{
			registerDecisionArraySimulation(pActor, pList, pLength);
		}
	}

	private void registerDecisionArrayGameplay(Actor pActor, DecisionAsset[] pArray, int pLength)
	{
		NeuralLayerAsset[] layers_array = AssetManager.neural_layers.layers_array;
		DecisionChecks pChecks = new DecisionChecks(pActor);
		for (int i = 0; i < pLength; i++)
		{
			DecisionAsset decisionAsset = pArray[i];
			if ((_do_priority_levels && decisionAsset.priority_int_cached < _highest_priority) || pActor.isDecisionOnCooldown(decisionAsset.decision_index, decisionAsset.cooldown) || !pActor.isDecisionEnabled(decisionAsset.decision_index) || !decisionAsset.isPossible(ref pChecks))
			{
				continue;
			}
			if (decisionAsset.action_check_launch != null && !decisionAsset.action_check_launch(pActor))
			{
				if (decisionAsset.cooldown_on_launch_failure)
				{
					pActor.setDecisionCooldown(decisionAsset);
				}
				continue;
			}
			_all_assets[_counter_all_assets++] = decisionAsset;
			if (layers_array[decisionAsset.priority_int_cached].critical)
			{
				_do_priority_levels = true;
			}
			if (_do_priority_levels && decisionAsset.priority_int_cached > _highest_priority)
			{
				_highest_priority = decisionAsset.priority_int_cached;
			}
			int num = _priority_array_counters[decisionAsset.priority_int_cached];
			_priority_array[decisionAsset.priority_int_cached][num] = decisionAsset;
			_priority_array_counters[decisionAsset.priority_int_cached]++;
		}
	}

	private void calculateFactors(Actor pActor)
	{
		DecisionAsset[] pPriorityArray;
		int pLength;
		if (_do_priority_levels)
		{
			pPriorityArray = _priority_array[_highest_priority];
			pLength = _priority_array_counters[_highest_priority];
		}
		else
		{
			pPriorityArray = _all_assets;
			pLength = _counter_all_assets;
		}
		calculateFactorsFrom(pPriorityArray, pLength, pActor);
	}

	private void calculateFactorsFrom(DecisionAsset[] pPriorityArray, int pLength, Actor pActor)
	{
		DecisionAsset[] actions = _actions;
		float[] factors = _factors;
		for (int i = 0; i < pLength; i++)
		{
			DecisionAsset decisionAsset = pPriorityArray[i];
			float num = decisionAsset.weight;
			if (decisionAsset.has_weight_custom)
			{
				num = decisionAsset.weight_calculate_custom(pActor);
			}
			actions[_counter_possible] = decisionAsset;
			factors[_counter_possible] = num;
			_counter_possible++;
		}
	}

	private void registerDecisionArraySimulation(Actor pActor, DecisionAsset[] pArray, int pLength)
	{
		DecisionAsset[] actions = _actions;
		float[] factors = _factors;
		DecisionChecks pChecks = new DecisionChecks(pActor);
		for (int i = 0; i < pLength; i++)
		{
			DecisionAsset decisionAsset = pArray[i];
			if (decisionAsset.isPossible(ref pChecks))
			{
				float num = decisionAsset.weight;
				if (decisionAsset.has_weight_custom)
				{
					num = decisionAsset.weight_calculate_custom(pActor);
				}
				actions[_counter_possible] = decisionAsset;
				factors[_counter_possible] = num;
				_counter_possible++;
			}
		}
	}

	public void clear()
	{
		clearPriorityArray();
		_counter_possible = 0;
		_highest_priority = 0;
		_counter_all_assets = 0;
	}

	private void clearPriorityArray()
	{
		int i = 0;
		for (int num = _priority_array.Length; i < num; i++)
		{
			_priority_array[i].Clear();
			_priority_array_counters[i] = 0;
		}
	}

	private void calculateChances(float pRandomnessFactor = 1f)
	{
		float[] chances = _chances;
		float[] factors = _factors;
		int i = 0;
		for (int counter_possible = _counter_possible; i < counter_possible; i++)
		{
			float num = factors[i];
			float num2 = (float)Math.Pow(Math.E, num * pRandomnessFactor);
			chances[i] = num2;
		}
	}

	public DecisionAsset chooseBestAction(float pRandomnessFactor = 1f)
	{
		float[] chances = _chances;
		DecisionAsset[] actions = _actions;
		calculateChances(pRandomnessFactor);
		float num = Randy.random() * sum();
		float num2 = 0f;
		int i = 0;
		for (int counter_possible = _counter_possible; i < counter_possible; i++)
		{
			num2 += chances[i];
			if (num < num2)
			{
				return actions[i];
			}
		}
		if (_counter_possible <= 0)
		{
			return null;
		}
		return actions[_counter_possible - 1];
	}

	private float sum()
	{
		float[] chances = _chances;
		float num = 0f;
		int i = 0;
		for (int counter_possible = _counter_possible; i < counter_possible; i++)
		{
			num += chances[i];
		}
		return num;
	}

	public string getFactorString(DecisionAsset pAsset)
	{
		float num = _factors[pAsset.decision_index];
		return num.ToString("F3");
	}

	public string getChanceString(DecisionAsset pAsset)
	{
		float num = _chances[pAsset.decision_index];
		return num.ToString("F3");
	}

	public string getOrderString(DecisionAsset pAsset)
	{
		int i = 0;
		for (int counter_possible = _counter_possible; i < counter_possible; i++)
		{
			if (_actions[i] == pAsset)
			{
				return i + "/" + _counter_possible;
			}
		}
		return "??";
	}

	public void debug(Actor pActor, DebugTool pTool)
	{
		useOnDebug(pActor);
		int i = 0;
		for (int counter_possible = _counter_possible; i < counter_possible; i++)
		{
			DecisionAsset decisionAsset = _actions[i];
			float num = _factors[i];
			pTool.setText(decisionAsset.id, num.ToString("F3"), 0f, pShowBar: false, 0L);
		}
	}

	private void useOnDebug(Actor pActor)
	{
		ActorAsset asset = pActor.asset;
		clear();
		registerBasicDecisionLists(pActor, pGameplay: false);
		if (asset.hasDecisions())
		{
			registerDecisionArraySimulation(pActor, asset.getDecisions(), asset.decisions_counter);
		}
	}

	public int getCounter()
	{
		return _counter_possible;
	}

	public DecisionAsset[] getActions()
	{
		return _actions;
	}
}
