public class CityZoneHelper
{
	public CityZoneGrowth city_growth;

	public CityZoneAbandon city_abandon;

	public CityPlaceFinder city_place_finder;

	public CityZoneHelper()
	{
		city_growth = new CityZoneGrowth();
		city_abandon = new CityZoneAbandon();
		city_place_finder = new CityPlaceFinder();
	}

	public void update(float pElapsed)
	{
		city_abandon.checkCities();
	}

	public void clear()
	{
		city_abandon.clearAll();
		city_growth.clearAll();
		city_place_finder.clearAll();
	}
}
