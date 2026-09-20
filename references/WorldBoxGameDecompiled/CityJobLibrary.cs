public class CityJobLibrary : AssetLibrary<JobCityAsset>
{
	public override void init()
	{
		base.init();
		add(new JobCityAsset
		{
			id = "city"
		});
		t.addTask("check_army");
		t.addTask("wait1");
		t.addTask("do_checks");
		t.addTask("wait1");
		t.addTask("border_shrink");
		t.addTask("wait1");
		t.addTask("produce_boat");
		t.addTask("wait1");
		t.addTask("supply_kingdom_cities");
		t.addTask("wait1");
		t.addTask("produce_resources");
		t.addTask("wait1");
		t.addTask("check_farms");
	}
}
