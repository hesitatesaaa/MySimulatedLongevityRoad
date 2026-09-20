using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoTesterBot : BaseMapObject
{
	internal string debugString = "";

	internal bool active;

	internal AiSystemTester ai;

	internal float wait;

	internal int beh_year_target;

	internal WorldTile beh_tile_target;

	internal string beh_asset_target;

	internal string active_tester = "";

	private Image _icon;

	public Image icon
	{
		get
		{
			if ((Object)(object)_icon == (Object)null)
			{
				_icon = ((Component)((Component)this).transform.Find("Icon")).GetComponent<Image>();
			}
			return _icon;
		}
	}

	internal void clearWorld()
	{
		beh_tile_target = null;
		beh_asset_target = null;
		beh_year_target = 0;
		ai?.restartJob();
	}

	internal override void create()
	{
		if (ai != null)
		{
			ai.reset();
			ai = null;
		}
		base.create();
		ai = new AiSystemTester(this);
		ai.next_job_delegate = AssetManager.tester_jobs.getNextJob;
		DebugConfig.createTool("Auto Tester", 150, 0);
		startAutoTester();
	}

	internal void create(string pJob)
	{
		if (ai != null)
		{
			ai.reset();
			ai = null;
		}
		base.create();
		active_tester = pJob;
		List<string> tJobs = new List<string> { pJob };
		ai = new AiSystemTester(this);
		ai.next_job_delegate = delegate
		{
			if (tJobs.Count == 0)
			{
				active = false;
				((Component)this).gameObject.SetActive(false);
				return (string)null;
			}
			return tJobs.Pop();
		};
		startAutoTester();
	}

	public override void update(float pElapsed)
	{
		if (active)
		{
			base.update(pElapsed);
			if (wait > 0f)
			{
				wait -= pElapsed;
			}
			else
			{
				ai.update();
			}
		}
	}

	private void updateButton()
	{
		if (active)
		{
			icon.sprite = SpriteTextureLoader.getSprite("ui/Icons/iconPause");
			WorldTip.instance.showToolbarText("Auto tester running");
		}
		else
		{
			icon.sprite = SpriteTextureLoader.getSprite("ui/Icons/iconPlay");
			WorldTip.instance.showToolbarText("Auto tester paused");
		}
	}

	public void startAutoTester()
	{
		active = true;
		updateButton();
	}

	public void stopAutoTester()
	{
		active = false;
		updateButton();
	}

	public void toggleAutoTester()
	{
		if (ai == null)
		{
			create();
		}
		active = !active;
		updateButton();
	}
}
