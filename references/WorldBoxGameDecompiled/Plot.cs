using System;
using System.Collections.Generic;
using UnityEngine;

public class Plot : MetaObject<PlotData>
{
	public static bool DEBUG_PLOTS = false;

	private static Dictionary<int, Sprite> _cache_sprites;

	public City target_city;

	public Kingdom target_kingdom;

	public Alliance target_alliance;

	public Actor target_actor;

	public War target_war;

	private PlotAsset _plot_asset;

	private PlotState _state;

	private int _power;

	private static int _total_sprites = 7;

	private int _last_index;

	public float transition_animation;

	private float _transition_animation_speed;

	private Actor _plot_author;

	public float progress_target;

	public double last_update_progress;

	protected override MetaType meta_type => MetaType.Plot;

	public override BaseSystemManager manager => World.world.plots;

	public void newPlot(Actor pAuthor, PlotAsset pAsset, bool pForced)
	{
		data.plot_type_id = pAsset.id;
		_plot_asset = pAsset;
		string pName = _plot_asset.getLocaleID().Localize();
		setName(pName);
		data.forced = pForced;
		data.founder_name = pAuthor.getName();
		data.founder_id = pAuthor.getID();
		pAuthor.setPlot(this);
		_plot_author = pAuthor;
	}

	protected sealed override void setDefaultValues()
	{
		base.setDefaultValues();
		_state = PlotState.Active;
		_power = 0;
		_last_index = -1;
		transition_animation = 1f;
		_transition_animation_speed = 0.8f;
		progress_target = 0f;
		last_update_progress = 0.0;
	}

	private bool isState(PlotState pState)
	{
		return _state == pState;
	}

	public void finishPlot(PlotState pState, Actor pActor)
	{
		if (_state == pState)
		{
			return;
		}
		_state = pState;
		switch (_state)
		{
		case PlotState.Finished:
			World.world.game_stats.data.plotsSucceeded++;
			World.world.map_stats.plotsSucceeded++;
			if (pActor != null)
			{
				pActor.changeHappiness("just_finished_plot");
				pActor.addStatusEffect("recovery_plot");
			}
			break;
		case PlotState.Cancelled:
			World.world.game_stats.data.plotsForgotten++;
			World.world.map_stats.plotsForgotten++;
			pActor?.addStatusEffect("recovery_plot");
			break;
		}
		startRemovalAnimation();
		foreach (Actor unit in base.units)
		{
			unit.leavePlot();
		}
		startAnimation();
		data.progress_current = getProgressMax();
	}

	public PlotState getState()
	{
		return _state;
	}

	public override bool isReadyForRemoval()
	{
		return !isActive();
	}

	public override void loadData(PlotData pData)
	{
		base.loadData(pData);
		_plot_asset = AssetManager.plots_library.get(data.plot_type_id);
		target_city = World.world.cities.get(data.id_target_city);
		target_kingdom = World.world.kingdoms.get(data.id_target_kingdom);
		target_alliance = World.world.alliances.get(data.id_target_alliance);
		target_war = World.world.wars.get(data.id_target_war);
	}

	public void loadAuthors()
	{
		_plot_author = World.world.units.get(data.founder_id);
		target_actor = World.world.units.get(data.id_target_actor);
	}

	public override void save()
	{
		base.save();
		data.id_target_actor = target_actor?.data.id ?? (-1);
		data.id_target_city = target_city?.data.id ?? (-1);
		data.id_target_kingdom = target_kingdom?.data.id ?? (-1);
		data.id_target_alliance = target_alliance?.data.id ?? (-1);
		data.id_target_war = target_war?.data.id ?? (-1);
	}

	public void updateAnimations(float pElapsed)
	{
		if (transition_animation > 1f)
		{
			transition_animation -= Time.deltaTime * _transition_animation_speed;
			if (transition_animation < 1f)
			{
				transition_animation = 1f;
			}
		}
	}

	public override ColorAsset getColor()
	{
		Actor author = getAuthor();
		ColorAsset result = null;
		if (author != null)
		{
			result = author.kingdom.getColor();
		}
		return result;
	}

	public bool updateProgressTarget(Actor pActor, float pIntelligence)
	{
		last_update_progress = World.world.getCurWorldTime();
		progress_target += pIntelligence + 1f;
		if (progress_target > getProgressMax())
		{
			progress_target = getProgressMax();
		}
		float progressMod = getProgressMod();
		_transition_animation_speed = 1.5f * progressMod;
		if (transition_animation <= 1f)
		{
			startAnimation();
		}
		if (data.progress_current >= getProgressMax())
		{
			if (_plot_asset.action(pActor))
			{
				_plot_asset.post_action?.Invoke(pActor);
			}
			finishPlot(PlotState.Finished, pActor);
			return true;
		}
		return false;
	}

	public void updateProgress(float pElapsed)
	{
		if (data.progress_current < progress_target)
		{
			data.progress_current += pElapsed * 2f;
			if (data.progress_current > progress_target)
			{
				data.progress_current = progress_target;
			}
		}
	}

	public int getProgressPercentage()
	{
		float progress = getProgress();
		float progressMax = getProgressMax();
		if (progressMax == 0f)
		{
			return 0;
		}
		return (int)(progress / progressMax * 100f);
	}

	public float getProgressMod()
	{
		float progress = getProgress();
		float progressMax = getProgressMax();
		return progress / progressMax;
	}

	public bool checkInitiatorAndTargets()
	{
		if (_plot_asset.check_target_actor && !target_actor.isAlive())
		{
			return false;
		}
		if (_plot_asset.check_target_alliance && !target_alliance.isAlive())
		{
			return false;
		}
		if (_plot_asset.check_target_city && !target_city.isAlive())
		{
			return false;
		}
		if (_plot_asset.check_target_kingdom)
		{
			if (!target_kingdom.isAlive())
			{
				return false;
			}
			if (!target_kingdom.hasCities())
			{
				return false;
			}
		}
		if (_plot_asset.check_target_war && !target_war.isAlive())
		{
			return false;
		}
		return true;
	}

	public PlotAsset getAsset()
	{
		return _plot_asset;
	}

	public int getPower()
	{
		return _power;
	}

	public int getMaxSupporters()
	{
		return 15;
	}

	public int getSupporters()
	{
		return base.units.Count;
	}

	public float getProgressMax()
	{
		return _plot_asset.progress_needed;
	}

	public float getProgress()
	{
		return data.progress_current;
	}

	public void startAnimation()
	{
		transition_animation = 1.3f;
	}

	public void startRemovalAnimation()
	{
		foreach (Actor unit in base.units)
		{
			if (unit.isAlive())
			{
				World.world.stack_effects.plot_removals.Add(new PlotIconData
				{
					actor = unit,
					sprite = getSpritePath(),
					timestamp = World.world.getCurSessionTime()
				});
			}
		}
	}

	public Sprite getSprite()
	{
		return SpriteTextureLoader.getSprite(getSpritePath());
	}

	public string getSpritePath()
	{
		if (isFinished())
		{
			return "plots/icons/progress/plot_finished";
		}
		if (isCancelled())
		{
			return "plots/icons/progress/plot_cancelled";
		}
		return _plot_asset.path_icon;
	}

	public Sprite getSpriteIconProgress()
	{
		int num = (int)(getProgress() / getProgressMax() * (float)(_total_sprites + 1));
		if (num > _total_sprites)
		{
			num = _total_sprites;
		}
		if (_cache_sprites == null)
		{
			_cache_sprites = new Dictionary<int, Sprite>();
			for (int i = 0; i <= _total_sprites; i++)
			{
				_cache_sprites.Add(i, SpriteTextureLoader.getSprite("plots/speech_bubbles/speech_bubble_0" + i));
			}
		}
		Sprite result = _cache_sprites[num];
		if (num != _last_index)
		{
			_last_index = num;
			startAnimation();
		}
		return result;
	}

	public bool hasSupporter(Actor pActor)
	{
		if (base.units.Contains(pActor))
		{
			return true;
		}
		return false;
	}

	public bool isActive()
	{
		return isState(PlotState.Active);
	}

	public bool isCancelled()
	{
		return isState(PlotState.Cancelled);
	}

	public bool isFinished()
	{
		return isState(PlotState.Finished);
	}

	public bool isSameType(PlotAsset pAsset)
	{
		return pAsset == getAsset();
	}

	public Actor getAuthor()
	{
		_plot_author = World.world.units.get(data.founder_id);
		return _plot_author;
	}

	public override void clearLastYearStats()
	{
		throw new NotImplementedException(GetType().Name);
	}

	public override void increaseBirths()
	{
		throw new NotImplementedException(GetType().Name);
	}

	public override void increaseDeaths(AttackType pType)
	{
		throw new NotImplementedException(GetType().Name);
	}

	public override void Dispose()
	{
		target_city = null;
		target_kingdom = null;
		target_alliance = null;
		target_actor = null;
		target_war = null;
		_plot_asset = null;
		_plot_author = null;
		base.Dispose();
	}

	public bool isAuthorDead()
	{
		if (_plot_author != null)
		{
			return !_plot_author.isAlive();
		}
		return true;
	}
}
