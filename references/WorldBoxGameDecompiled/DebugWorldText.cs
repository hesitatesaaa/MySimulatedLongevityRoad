using FMOD.Studio;
using UnityEngine;
using life.taxi;

public class DebugWorldText : MonoBehaviour
{
	public TextMesh text_mesh;

	public TextMesh text_mesh_bg_clone;

	private string _color_sounds_attached = "#FF1F44";

	private string _color_sounds = "#607BFF";

	private string _color_actors = "#FF8F44";

	private string _color_building = "#00FFFF";

	private string _color_city = "#A0FF93";

	private string _color_kingdom = "#FF4242";

	private string cur_string;

	private string cur_color;

	public void create()
	{
		((Component)text_mesh_bg_clone).GetComponent<Renderer>().sortingLayerID = SortingLayer.NameToID("Debug");
		((Component)text_mesh_bg_clone).GetComponent<Renderer>().sortingOrder = 1;
		((Component)text_mesh).GetComponent<Renderer>().sortingLayerID = SortingLayer.NameToID("Debug");
		((Component)text_mesh).GetComponent<Renderer>().sortingOrder = 2;
	}

	private void prepare(string pID, string pColor, float pSize = 0.25f)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		text_mesh.color = Color.white;
		cur_string = pID;
		cur_color = "<color=" + pColor + ">";
		text_mesh_bg_clone.characterSize = pSize;
		text_mesh.characterSize = pSize;
	}

	private void add(string pTitle, object pText)
	{
		cur_string = cur_string + pTitle + ": " + cur_color + pText?.ToString() + "</color>\n";
	}

	public void setTextFmodSound(DebugMusicBoxData pData)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		setTextFmodSound(pData, Color.white);
	}

	public void setTextFmodSound(DebugMusicBoxData pData, Color pColor)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		float a = pData.timer / 3f;
		prepare("#fmod\n", _color_sounds, 0.5f);
		cur_string = "mb:" + pData.path;
		Color color = pColor;
		color.a = a;
		fin();
		text_mesh.color = color;
		text_mesh_bg_clone.color = color;
	}

	public void setTextFmodSound(EventInstance pInstance)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		EventDescription val = default(EventDescription);
		((EventInstance)(ref pInstance)).getDescription(ref val);
		string pText = default(string);
		((EventDescription)(ref val)).getPath(ref pText);
		prepare("#fmod\n", _color_sounds_attached, 0.5f);
		add("name", pText);
		fin();
	}

	public void setTextZone(TileZone pZone)
	{
		prepare("#zone\n", _color_actors, 0.5f);
		(string, int)[] debug_args = pZone.debug_args;
		for (int i = 0; i < debug_args.Length; i++)
		{
			(string, int) tuple = debug_args[i];
			add(tuple.Item1, tuple.Item2);
		}
		fin();
	}

	public void setTextBoat(Actor pActor)
	{
		Boat simpleComponent = pActor.getSimpleComponent<Boat>();
		TaxiRequest taxi_request = simpleComponent.taxi_request;
		if (simpleComponent.hasPassengers() || taxi_request != null)
		{
			prepare("#boat\n", _color_kingdom, 0.8f);
		}
		else
		{
			prepare("#boat\n", _color_actors, 0.4f);
		}
		if (pActor.ai.job != null)
		{
			add("job", pActor.ai.job.id + "(" + pActor.ai.task_index + "/" + pActor.ai.job.tasks.Count + ")");
		}
		if (pActor.hasTask())
		{
			string text = " [" + pActor.ai.action_index + "/" + pActor.ai.task?.list.Count + "]";
			add("task", pActor.ai.task.id + " " + text);
			string text2 = pActor.ai.action?.GetType().ToString();
			if (text2 != null)
			{
				text2 = text2.Replace("ai.behaviours.", "");
			}
			add("action", text2);
		}
		add("timer", simpleComponent.actor.timer_action);
		fin();
	}

	private void debugForce(Actor pActor)
	{
		add("force xy", pActor.velocity.x + "-" + pActor.velocity.y);
		add("force z", pActor.velocity.z);
		add("zPosition", pActor.position_height);
		add("force_speed", pActor.velocity_speed);
		add("under_force", pActor.under_forces);
		add("mass", pActor.stats["mass"]);
	}

	public void setTextActor(Actor pActor)
	{
		prepare("#unit\n", _color_actors, 0.2f);
		add("name", pActor.data.name);
		add("timer_action", pActor.timer_action);
		if (pActor.isCarryingResources())
		{
			add("inv.count", pActor.inventory.countResources());
			add("inv.render", pActor.inventory.getItemIDToRender());
		}
		add("stats", pActor.asset.id);
		add("id", pActor.data.id);
		add("alive", pActor.isAlive());
		add("health", pActor.getHealth() + "/" + pActor.getMaxHealth());
		add("traits", pActor.countTraits());
		if (pActor.hasAnyStatusEffect())
		{
			add("statuses", pActor.countStatusEffects());
		}
		if (pActor.ai.job != null)
		{
			add("job", pActor.ai.job.id + "(" + pActor.ai.task_index + "/" + pActor.ai.job.tasks.Count + ")");
		}
		if (pActor.hasTask())
		{
			add("task", pActor.ai.task.id);
			string text = pActor.ai.action?.GetType().ToString();
			if (text != null)
			{
				text = text.Replace("ai.behaviours.", "");
			}
			text = text + pActor.ai.action_index + "/" + pActor.ai.task?.list.Count;
			add("action", text);
		}
		fin();
	}

	public void setTextArmy(Army pArmy)
	{
		prepare("#army\n", _color_building, 0.3f);
		add("captain", pArmy.getCaptain().getName());
		add("id", pArmy.id);
		add("units", pArmy.countUnits());
		add("alive", pArmy.isAlive());
		if (pArmy.getCity().isAlive())
		{
			add("city", pArmy.getCity().name);
		}
		else
		{
			add("city", "DESTROYED, SHOULD BE NULL");
		}
		fin();
	}

	public void setTextBuilding(Building pObj)
	{
		prepare("#build\n", _color_building, 0.3f);
		add("objectID", pObj.data.id);
		add("state", pObj.data.state);
		add("animationState", pObj.animation_state);
		add("ownership", pObj.state_ownership);
		add("kingdom", pObj.kingdom.id);
		if (pObj.asset.hasHousingSlots())
		{
			add("housing", pObj.countResidents() + "/" + pObj.asset.housing_slots);
		}
		fin();
	}

	public void setTextCity(City pObj)
	{
		prepare("#city\n", _color_city, 1.5f);
		bool flag = false;
		string text = "";
		foreach (string key in pObj.buildings_dict_id.Keys)
		{
			if (flag)
			{
				break;
			}
			foreach (Building item in pObj.buildings_dict_id[key])
			{
				if (!item.isAlive())
				{
					flag = true;
					text += "dead,";
				}
				if (item.asset.id != key)
				{
					flag = true;
					text = text + "wrong stats " + item.asset.id;
				}
				if (flag)
				{
					break;
				}
			}
		}
		int num = 0;
		foreach (Actor unit in pObj.units)
		{
			if (unit.isTask("put_out_fire"))
			{
				num++;
			}
		}
		add("on_fire", pObj.isCityUnderDangerFire());
		add("danger", pObj.isInDanger());
		add("firemen", num);
		add("total", pObj.status.population + "/" + pObj.getPopulationMaximum());
		add("units", pObj.units.Count);
		add("buildings", pObj.buildings.Count);
		add("orders_psbl", pObj._debug_last_possible_build_orders);
		add("orders_no_res", pObj._debug_last_possible_build_orders_no_resources);
		add("order_last", pObj._debug_last_build_order_try);
		add("house_zone_limit", pObj.getHouseCurrent() + "/" + pObj.getHouseLimit());
		if (pObj.ai.job != null)
		{
			add("job", pObj.ai.job.id + "(" + pObj.ai.task_index + "/" + pObj.ai.job.tasks.Count + ")");
		}
		if (pObj.ai.task != null)
		{
			add("task", pObj.ai.task.id);
		}
		else
		{
			add("task", "-");
		}
		if (flag)
		{
			add("ERROR", text);
		}
		fin();
	}

	public void setTextCityTasks(City pCity)
	{
		prepare("#city_tasks\n", _color_city, 0.5f);
		add("trees:", pCity.tasks.trees);
		add("stone:", pCity.tasks.minerals);
		add("minerals:", pCity.tasks.minerals);
		add("bushes:", pCity.tasks.bushes);
		add("plants:", pCity.tasks.plants);
		add("hives:", pCity.tasks.hives);
		add("farm_fields:", pCity.tasks.farm_fields);
		add("wheats:", pCity.tasks.wheats);
		add("ruins:", pCity.tasks.ruins);
		add("poops:", pCity.tasks.poops);
		add("roads:", pCity.tasks.roads);
		add("fire:", pCity.tasks.fire);
		add("", "");
		int num = 0;
		int num2 = 0;
		foreach (CitizenJobAsset key in pCity.jobs.jobs.Keys)
		{
			int num3 = pCity.jobs.jobs[key];
			int num4 = 0;
			if (pCity.jobs.occupied.ContainsKey(key))
			{
				num4 = pCity.jobs.occupied[key];
			}
			num += num3;
			num2 += num4;
			add(key.id + ":", num4 + "/" + num3);
		}
		foreach (CitizenJobAsset key2 in pCity.jobs.occupied.Keys)
		{
			if (!pCity.jobs.jobs.ContainsKey(key2))
			{
				int num5 = pCity.jobs.occupied[key2];
				num2 += num5;
				add(key2.id + ":", num5 + "/" + 0);
			}
		}
		int num6 = 0;
		int num7 = 0;
		foreach (Actor unit in pCity.units)
		{
			if (unit.isAdult())
			{
				num6++;
			}
			if (unit.hasTask() && unit.citizen_job != null)
			{
				num7++;
			}
		}
		add("total:", num2 + "/" + num);
		add("pop|adults|workers:", pCity.units.Count + " | " + num6 + " | " + num7);
		fin();
	}

	public void setTextKingdom(Kingdom pObj)
	{
		prepare("#kingdom\n", _color_kingdom, 2f);
		add("total", pObj.getPopulationPeople() + "/" + pObj.getPopulationTotalPossible());
		add("units", pObj.units.Count);
		add("buildings", pObj.buildings.Count);
		add("timer_action", pObj.timer_action);
		add("timer_new_king", pObj.data.timer_new_king);
		if (pObj.ai.job != null)
		{
			add("job", pObj.ai.job.id + "(" + pObj.ai.task_index + "/" + pObj.ai.job.tasks.Count + ")");
		}
		if (pObj.ai.task != null)
		{
			add("task", pObj.ai.task.id);
		}
		else
		{
			add("task", "-");
		}
		fin();
	}

	private void fin()
	{
		text_mesh.text = cur_string;
		text_mesh_bg_clone.text = cur_string;
	}
}
