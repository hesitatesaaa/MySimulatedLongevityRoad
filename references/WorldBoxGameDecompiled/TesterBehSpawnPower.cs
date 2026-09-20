using UnityEngine;
using ai.behaviours;

public class TesterBehSpawnPower : BehaviourActionTester
{
	protected string _power;

	public TesterBehSpawnPower(string pPower = null)
	{
		_power = pPower;
	}

	public override BehResult execute(AutoTesterBot pObject)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		string power = _power;
		int num = Randy.randomInt(0, MapBox.width);
		int num2 = Randy.randomInt(0, MapBox.height);
		if (!AssetManager.powers.dict.ContainsKey(power))
		{
			Debug.LogError((object)("TESTER ERROR... " + power));
			return BehResult.Continue;
		}
		GodPower pPower = AssetManager.powers.get(power);
		string current_brush = Config.current_brush;
		Config.current_brush = Brush.getRandom();
		pObject.debugString = "rand_power_" + power;
		BehaviourActionBase<AutoTesterBot>.world.player_control.clickedFinal(new Vector2Int(num, num2), pPower);
		Config.current_brush = current_brush;
		return base.execute(pObject);
	}
}
