using UnityEngine;
using UnityEngine.UI;

public class UiDebugWindow : TabbedWindow
{
	public Mask mask;

	public ActorDebugAssetsComponent assets_actor;

	public BuildingDebugAssetsComponent assets_building;

	private void OnEnable()
	{
		showInfo();
	}

	public void showInfo()
	{
		((Behaviour)mask).enabled = true;
		AchievementLibrary.god_mode.check();
		clear();
	}

	private void clear()
	{
	}

	public void clickConsole()
	{
		if (Config.game_loaded)
		{
			World.world.console.Show();
		}
	}

	public void clickNewDebugWindow()
	{
		DebugConfig.createTool("Game Info");
	}

	public void clickNewDebugWindowBench()
	{
		DebugConfig.createTool("Benchmark All");
	}

	public void clickRandomKingdomColor()
	{
		AssetsDebugManager.newKingdomColors();
		if (((Component)assets_actor).gameObject.activeSelf)
		{
			assets_actor.refresh();
		}
		if (((Component)assets_building).gameObject.activeSelf)
		{
			assets_building.refresh();
		}
	}

	public void clickRandomSkinColor()
	{
		AssetsDebugManager.newSkinColors();
		assets_actor.refresh();
	}

	public void clickChangeSex()
	{
		AssetsDebugManager.changeSex();
		assets_actor.refresh();
	}

	public void showDebugAvatarsWindow()
	{
		ScrollWindow.showWindow("debug_avatars");
	}
}
