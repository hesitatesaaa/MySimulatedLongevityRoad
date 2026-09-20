namespace ai.behaviours;

public class BehaviourTaskCityLibrary : AssetLibrary<BehaviourTaskCity>
{
	public override void init()
	{
		base.init();
		BehaviourTaskCity obj = new BehaviourTaskCity
		{
			id = "nothing"
		};
		BehaviourTaskCity pAsset = obj;
		t = obj;
		add(pAsset);
		BehaviourTaskCity obj2 = new BehaviourTaskCity
		{
			id = "wait1"
		};
		pAsset = obj2;
		t = obj2;
		add(pAsset);
		t.addBeh(new CityBehRandomWait());
		BehaviourTaskCity obj3 = new BehaviourTaskCity
		{
			id = "wait5"
		};
		pAsset = obj3;
		t = obj3;
		add(pAsset);
		t.addBeh(new CityBehRandomWait(5f, 5f));
		BehaviourTaskCity obj4 = new BehaviourTaskCity
		{
			id = "random_wait_test"
		};
		pAsset = obj4;
		t = obj4;
		add(pAsset);
		t.addBeh(new CityBehRandomWait(5f, 10f));
		t.addBeh(new CityBehRandomWait(5f, 10f));
		t.addBeh(new CityBehRandomWait(5f, 10f));
		BehaviourTaskCity obj5 = new BehaviourTaskCity
		{
			id = "do_checks"
		};
		pAsset = obj5;
		t = obj5;
		add(pAsset);
		t.addBeh(new CityBehCheckLeader());
		t.addBeh(new CityBehRandomWait(0.1f));
		t.addBeh(new CityBehCheckAttackZone());
		t.addBeh(new CityBehRandomWait(0.1f));
		t.addBeh(new CityBehCheckCitizenTasks());
		t.addBeh(new CityBehRandomWait(0.1f));
		BehaviourTaskCity obj6 = new BehaviourTaskCity
		{
			id = "do_initial_load_check"
		};
		pAsset = obj6;
		t = obj6;
		add(pAsset);
		t.addBeh(new CityBehCheckCitizenTasks());
		t.addBeh(new CityBehCheckLoyalty());
		BehaviourTaskCity obj7 = new BehaviourTaskCity
		{
			id = "check_farms"
		};
		pAsset = obj7;
		t = obj7;
		add(pAsset);
		t.addBeh(new CityBehCheckFarms());
		BehaviourTaskCity obj8 = new BehaviourTaskCity
		{
			id = "check_loyalty",
			single_interval = 2f
		};
		pAsset = obj8;
		t = obj8;
		add(pAsset);
		t.addBeh(new CityBehCheckLoyalty());
		BehaviourTaskCity obj9 = new BehaviourTaskCity
		{
			id = "check_destruction",
			single_interval = 2f
		};
		pAsset = obj9;
		t = obj9;
		add(pAsset);
		t.addBeh(new CityBehCheckDestruction());
		BehaviourTaskCity obj10 = new BehaviourTaskCity
		{
			id = "produce_boat"
		};
		pAsset = obj10;
		t = obj10;
		add(pAsset);
		t.addBeh(new CityBehProduceBoat());
		BehaviourTaskCity obj11 = new BehaviourTaskCity
		{
			id = "border_shrink"
		};
		pAsset = obj11;
		t = obj11;
		add(pAsset);
		t.addBeh(new CityBehBorderShrink());
		BehaviourTaskCity obj12 = new BehaviourTaskCity
		{
			id = "build",
			single_interval = 0f
		};
		pAsset = obj12;
		t = obj12;
		add(pAsset);
		t.addBeh(new CityBehBuild());
		BehaviourTaskCity obj13 = new BehaviourTaskCity
		{
			id = "supply_kingdom_cities"
		};
		pAsset = obj13;
		t = obj13;
		add(pAsset);
		t.addBeh(new CityBehSupplyKingdomCities());
		BehaviourTaskCity obj14 = new BehaviourTaskCity
		{
			id = "produce_resources"
		};
		pAsset = obj14;
		t = obj14;
		add(pAsset);
		t.addBeh(new CityBehProduceResources());
		BehaviourTaskCity obj15 = new BehaviourTaskCity
		{
			id = "check_army"
		};
		pAsset = obj15;
		t = obj15;
		add(pAsset);
		t.addBeh(new CityBehCheckArmy());
	}

	public override void editorDiagnosticLocales()
	{
	}
}
