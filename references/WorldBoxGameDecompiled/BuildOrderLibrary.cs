public class BuildOrderLibrary : AssetLibrary<CityBuildOrderAsset>
{
	public static BuildOrder b;

	public override void init()
	{
		base.init();
		initCivsBasic();
		initCivsBasic2();
		initCivsAdvanced();
	}

	private void initCivsBasic()
	{
		add(new CityBuildOrderAsset
		{
			id = "build_order_basic"
		});
		t.addBuilding("order_bonfire", 1);
		t.addBuilding("order_stockpile", 1);
		t.addBuilding("order_hall_0", 1);
		b.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire");
		t.addBuilding("order_house_0", 0, 0, 0, pCheckFullVillage: false, pCheckHouseLimit: true);
		b.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire");
		t.addBuilding("order_watch_tower", 1, 30, 10);
		b.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire", "order_hall_0");
		t.addBuilding("order_temple", 1, 90, 20, pCheckFullVillage: false, pCheckHouseLimit: false, 20);
		b.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire", "order_hall_0", "order_statue");
		t.addBuilding("order_statue", 1, 70, 15);
		b.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_hall");
		t.addBuilding("order_well", 1, 20, 10);
		b.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_hall");
		t.addBuilding("order_mine", 1, 20, 10);
		b.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_bonfire", "type_hall");
		t.addBuilding("order_library", 1, 50, 15);
		b.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_bonfire", "type_hall");
		t.addBuilding("order_docks_0", 5, 0, 2);
		b.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire");
		t.addUpgrade("order_docks_0");
		b.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_docks_0");
		t.addBuilding("order_windmill_0", 1, 6, 5);
		b.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire");
		t.addBuilding("order_barracks", 1, 50, 16, pCheckFullVillage: false, pCheckHouseLimit: false, 20);
		b.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_bonfire", "type_hall");
		t.addBuilding("order_training_dummy", 3, 50, 16, pCheckFullVillage: false, pCheckHouseLimit: false, 20);
		b.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_barracks");
	}

	private void initCivsBasic2()
	{
		add(new CityBuildOrderAsset
		{
			id = "build_order_basic_2"
		});
		t.addBuilding("order_bonfire", 1);
		t.addBuilding("order_stockpile", 1);
		t.addBuilding("order_hall_0", 1);
		b.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire");
		t.addBuilding("order_house_0", 0, 0, 0, pCheckFullVillage: false, pCheckHouseLimit: true);
		b.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire");
		t.addBuilding("order_watch_tower", 1, 30, 10);
		b.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire", "order_hall_0");
		t.addBuilding("order_temple", 1, 90, 20, pCheckFullVillage: false, pCheckHouseLimit: false, 20);
		b.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire", "order_hall_0");
		t.addBuilding("order_mine", 1, 20, 10);
		b.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_bonfire", "type_hall");
		t.addBuilding("order_library", 1, 50, 15);
		b.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_bonfire", "type_hall");
		t.addBuilding("order_docks_0", 5, 0, 2);
		b.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire");
		t.addUpgrade("order_docks_0");
		b.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_docks_0");
		t.addBuilding("order_windmill_0", 1, 6, 5);
		b.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire");
		t.addBuilding("order_barracks", 1, 50, 16, pCheckFullVillage: false, pCheckHouseLimit: false, 20);
		b.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_bonfire", "type_hall");
		t.addBuilding("order_training_dummy", 3, 50, 16, pCheckFullVillage: false, pCheckHouseLimit: false, 20);
		b.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_barracks");
	}

	private void initCivsAdvanced()
	{
		add(new CityBuildOrderAsset
		{
			id = "build_order_advanced"
		});
		t.addBuilding("order_hall_0", 1);
		b.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire");
		t.addBuilding("order_bonfire", 1);
		t.addBuilding("order_stockpile", 1);
		t.addBuilding("order_house_0", 0, 0, 0, pCheckFullVillage: false, pCheckHouseLimit: true);
		b.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire");
		t.addBuilding("order_tent", 0, 0, 0, pCheckFullVillage: false, pCheckHouseLimit: true);
		b.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire");
		t.addUpgrade("order_tent");
		b.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_tent");
		t.addUpgrade("order_house_0");
		b.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_hall_0", "order_house_0");
		t.addUpgrade("order_house_1");
		b.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_hall_1", "order_house_1");
		t.addUpgrade("order_house_2");
		b.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_hall_1", "order_house_2");
		t.addUpgrade("order_house_3");
		b.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_hall_2", "order_house_3");
		t.addUpgrade("order_house_4");
		b.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_hall_2", "order_house_4");
		t.addUpgrade("order_hall_0", 0, 30, 8);
		b.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_house_1");
		t.addUpgrade("order_hall_1", 0, 100, 20);
		b.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_statue", "order_mine", "order_barracks");
		t.addBuilding("order_windmill_0", 1, 6, 5);
		b.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire");
		t.addUpgrade("order_windmill_0", 0, 40, 10);
		t.addBuilding("order_docks_0", 5, 0, 2);
		b.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire");
		t.addUpgrade("order_docks_0");
		b.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_docks_0");
		t.addBuilding("order_well", 1, 20, 10);
		b.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_hall");
		t.addBuilding("order_mine", 1, 20, 10);
		b.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire", "order_hall_0");
		t.addBuilding("order_barracks", 1, 50, 16, pCheckFullVillage: false, pCheckHouseLimit: false, 20);
		b.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_hall_1");
		t.addBuilding("order_training_dummy", 3, 50, 16, pCheckFullVillage: false, pCheckHouseLimit: false, 20);
		b.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_barracks");
		t.addBuilding("order_watch_tower", 1, 30, 10);
		b.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire", "order_hall_0");
		t.addBuilding("order_temple", 1, 90, 20, pCheckFullVillage: false, pCheckHouseLimit: false, 20);
		b.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire", "order_hall_1", "order_statue");
		t.addBuilding("order_statue", 1, 70, 15);
		b.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_hall_1");
		t.addBuilding("order_library", 1, 50, 15);
		b.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_bonfire", "type_hall");
		t.addBuilding("order_market", 1, 60, 15);
		b.requirements_types = AssetLibrary<CityBuildOrderAsset>.a<string>("type_bonfire", "type_hall");
	}

	public override void linkAssets()
	{
		base.linkAssets();
		foreach (CityBuildOrderAsset item in list)
		{
			item.prepareForAssetGeneration();
		}
	}
}
