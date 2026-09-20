using UnityEngine;

public class GraphyCaller : MonoBehaviour
{
	private int clicked;

	public void click()
	{
		clicked++;
		if (clicked > 10)
		{
			bool flag = DebugConfig.isOn(DebugOption.DebugButton);
			DebugConfig.setOption(DebugOption.DebugButton, !flag);
			DebugConfig.instance.debugButton.SetActive(!flag);
		}
	}

	public void clickConsole()
	{
		clicked++;
		if (clicked > 10)
		{
			World.world.console.Show();
		}
	}

	private void OnEnable()
	{
		clicked = 0;
	}
}
