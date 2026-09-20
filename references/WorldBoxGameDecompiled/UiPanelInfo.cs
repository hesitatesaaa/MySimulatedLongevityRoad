using UnityEngine;
using UnityEngine.UI;

public class UiPanelInfo : MonoBehaviour
{
	public GameObject population;

	public GameObject beasts;

	public GameObject deaths;

	public GameObject infected;

	public GameObject buildings;

	public GameObject vegetations;

	private float interval = 0.2f;

	private float timer;

	private Text population_name;

	private Text population_value;

	private Text beasts_name;

	private Text beasts_value;

	private Text deaths_name;

	private Text deaths_value;

	private Text infected_name;

	private Text infected_value;

	private Text buildings_name;

	private Text buildings_value;

	private Text vegetation_name;

	private Text vegetation_value;

	private bool lastRTL;

	private void OnEnable()
	{
		population_name = ((Component)population.transform.Find("Name")).GetComponent<Text>();
		population_value = ((Component)population.transform.Find("Value")).GetComponent<Text>();
		beasts_name = ((Component)beasts.transform.Find("Name")).GetComponent<Text>();
		beasts_value = ((Component)beasts.transform.Find("Value")).GetComponent<Text>();
		infected_name = ((Component)infected.transform.Find("Name")).GetComponent<Text>();
		infected_value = ((Component)infected.transform.Find("Value")).GetComponent<Text>();
		deaths_name = ((Component)deaths.transform.Find("Name")).GetComponent<Text>();
		deaths_value = ((Component)deaths.transform.Find("Value")).GetComponent<Text>();
		buildings_name = ((Component)buildings.transform.Find("Name")).GetComponent<Text>();
		buildings_value = ((Component)buildings.transform.Find("Value")).GetComponent<Text>();
		vegetation_name = ((Component)vegetations.transform.Find("Name")).GetComponent<Text>();
		vegetation_value = ((Component)vegetations.transform.Find("Value")).GetComponent<Text>();
	}

	private void Update()
	{
		if (!Config.game_loaded || (Object)(object)World.world == (Object)null || World.world.map_stats == null || (Object)(object)World.world.game_stats == (Object)null)
		{
			return;
		}
		if (timer > 0f)
		{
			timer -= Time.deltaTime;
			return;
		}
		timer = interval;
		if (LocalizedTextManager.current_language.isRTL() != lastRTL)
		{
			lastRTL = LocalizedTextManager.current_language.isRTL();
			if (lastRTL)
			{
				population_value.alignment = (TextAnchor)3;
				beasts_value.alignment = (TextAnchor)3;
				infected_value.alignment = (TextAnchor)3;
				deaths_value.alignment = (TextAnchor)3;
				buildings_value.alignment = (TextAnchor)3;
				vegetation_value.alignment = (TextAnchor)3;
				population_name.alignment = (TextAnchor)5;
				beasts_name.alignment = (TextAnchor)5;
				infected_name.alignment = (TextAnchor)5;
				deaths_name.alignment = (TextAnchor)5;
				buildings_name.alignment = (TextAnchor)5;
				vegetation_name.alignment = (TextAnchor)5;
			}
			else
			{
				population_value.alignment = (TextAnchor)5;
				beasts_value.alignment = (TextAnchor)5;
				infected_value.alignment = (TextAnchor)5;
				deaths_value.alignment = (TextAnchor)5;
				buildings_value.alignment = (TextAnchor)5;
				vegetation_value.alignment = (TextAnchor)5;
				population_name.alignment = (TextAnchor)3;
				beasts_name.alignment = (TextAnchor)3;
				infected_name.alignment = (TextAnchor)3;
				deaths_name.alignment = (TextAnchor)3;
				buildings_name.alignment = (TextAnchor)3;
				vegetation_name.alignment = (TextAnchor)3;
			}
		}
		population_value.text = World.world.getCivWorldPopulation().ToString() ?? "";
		beasts_value.text = World.world.map_stats.current_mobs.ToString() ?? "";
		infected_value.text = World.world.map_stats.current_infected.ToString() ?? "";
		deaths_value.text = World.world.map_stats.deaths.ToString() ?? "";
		buildings_value.text = World.world.map_stats.current_houses.ToString() ?? "";
		vegetation_value.text = World.world.map_stats.current_vegetation.ToString() ?? "";
		((Component)population_value).GetComponent<LocalizedText>().checkSpecialLanguages();
		((Component)beasts_value).GetComponent<LocalizedText>().checkSpecialLanguages();
		((Component)infected_value).GetComponent<LocalizedText>().checkSpecialLanguages();
		((Component)deaths_value).GetComponent<LocalizedText>().checkSpecialLanguages();
		((Component)buildings_value).GetComponent<LocalizedText>().checkSpecialLanguages();
		((Component)vegetation_value).GetComponent<LocalizedText>().checkSpecialLanguages();
	}
}
