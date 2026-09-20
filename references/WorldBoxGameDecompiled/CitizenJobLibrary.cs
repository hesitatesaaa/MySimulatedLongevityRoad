using System.Collections.Generic;

public class CitizenJobLibrary : AssetLibrary<CitizenJobAsset>
{
	public List<CitizenJobAsset> list_priority_normal;

	public List<CitizenJobAsset> list_priority_high;

	public List<CitizenJobAsset> list_priority_high_food;

	public static CitizenJobAsset builder;

	public static CitizenJobAsset gatherer_bushes;

	public static CitizenJobAsset gatherer_herbs;

	public static CitizenJobAsset gatherer_honey;

	public static CitizenJobAsset farmer;

	public static CitizenJobAsset hunter;

	public static CitizenJobAsset woodcutter;

	public static CitizenJobAsset miner;

	public static CitizenJobAsset miner_deposit;

	public static CitizenJobAsset road_builder;

	public static CitizenJobAsset cleaner;

	public static CitizenJobAsset manure_cleaner;

	public static CitizenJobAsset attacker;

	public override void init()
	{
		base.init();
		builder = add(new CitizenJobAsset
		{
			id = "builder",
			priority = 9,
			debug_option = DebugOption.CitizenJobBuilder,
			path_icon = "ui/Icons/citizen_jobs/iconCitizenJobBuilder"
		});
		gatherer_bushes = add(new CitizenJobAsset
		{
			id = "gatherer_bushes",
			priority_no_food = 10,
			debug_option = DebugOption.CitizenJobGathererBushes,
			path_icon = "ui/Icons/citizen_jobs/iconCitizenJobGathererBushes"
		});
		gatherer_herbs = add(new CitizenJobAsset
		{
			id = "gatherer_herbs",
			priority_no_food = 10,
			debug_option = DebugOption.CitizenJobGathererHerbs,
			path_icon = "ui/Icons/citizen_jobs/iconCitizenJobGathererHerbs"
		});
		gatherer_honey = add(new CitizenJobAsset
		{
			id = "gatherer_honey",
			priority_no_food = 10,
			debug_option = DebugOption.CitizenJobGathererHoney,
			path_icon = "ui/Icons/citizen_jobs/iconCitizenJobGathererHoney"
		});
		farmer = add(new CitizenJobAsset
		{
			id = "farmer",
			ok_for_king = false,
			ok_for_leader = false,
			debug_option = DebugOption.CitizenJobFarmer,
			path_icon = "ui/Icons/citizen_jobs/iconCitizenJobFarmer"
		});
		hunter = add(new CitizenJobAsset
		{
			id = "hunter",
			debug_option = DebugOption.CitizenJobHunter,
			path_icon = "ui/Icons/citizen_jobs/iconCitizenJobHunter"
		});
		woodcutter = add(new CitizenJobAsset
		{
			id = "woodcutter",
			debug_option = DebugOption.CitizenJobWoodcutter,
			path_icon = "ui/Icons/citizen_jobs/iconCitizenJobWoodcutter"
		});
		miner = add(new CitizenJobAsset
		{
			id = "miner",
			ok_for_king = false,
			ok_for_leader = false,
			debug_option = DebugOption.CitizenJobMiner,
			path_icon = "ui/Icons/citizen_jobs/iconCitizenJobMiner"
		});
		miner_deposit = add(new CitizenJobAsset
		{
			id = "miner_deposit",
			ok_for_king = false,
			ok_for_leader = false,
			debug_option = DebugOption.CitizenJobMinerDeposit,
			path_icon = "ui/Icons/citizen_jobs/iconCitizenJobMinerDeposit"
		});
		road_builder = add(new CitizenJobAsset
		{
			id = "road_builder",
			debug_option = DebugOption.CitizenJobRoadBuilder,
			path_icon = "ui/Icons/citizen_jobs/iconCitizenJobRoadBuilder"
		});
		cleaner = add(new CitizenJobAsset
		{
			id = "cleaner",
			debug_option = DebugOption.CitizenJobCleaner,
			path_icon = "ui/Icons/citizen_jobs/iconCitizenJobCleaner"
		});
		manure_cleaner = add(new CitizenJobAsset
		{
			id = "manure_cleaner",
			debug_option = DebugOption.CitizenJobManureCleaner,
			path_icon = "ui/Icons/citizen_jobs/iconCitizenJobManureCleaner"
		});
		attacker = add(new CitizenJobAsset
		{
			id = "attacker",
			debug_option = DebugOption.CitizenJobAttacker,
			path_icon = "ui/Icons/citizen_jobs/iconCitizenJobAttacker",
			common_job = false
		});
	}

	public override void post_init()
	{
		base.post_init();
		foreach (CitizenJobAsset item in list)
		{
			if (item.common_job)
			{
				item.unit_job_default = item.id;
			}
		}
	}

	public override void linkAssets()
	{
		base.linkAssets();
		list_priority_normal = new List<CitizenJobAsset>();
		list_priority_high = new List<CitizenJobAsset>();
		list_priority_high_food = new List<CitizenJobAsset>();
		foreach (CitizenJobAsset item in list)
		{
			if (item.common_job)
			{
				if (item.priority_no_food > 0)
				{
					list_priority_high_food.Add(item);
				}
				if (item.priority > 0)
				{
					list_priority_high.Add(item);
				}
				else
				{
					list_priority_normal.Add(item);
				}
			}
		}
	}
}
