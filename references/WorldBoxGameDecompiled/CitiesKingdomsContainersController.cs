using UnityEngine;

public class CitiesKingdomsContainersController : MonoBehaviour
{
	[SerializeField]
	private CitiesBannersContainer _banners_cities;

	[SerializeField]
	private GameObject _line_cities;

	[SerializeField]
	private KingdomsBannersContainer _banners_kingdoms;

	[SerializeField]
	private GameObject _line_kingdoms;

	public void update(NanoObject pNano)
	{
		_banners_cities.update(pNano);
		_banners_kingdoms.update(pNano);
		IMetaObject obj = (IMetaObject)pNano;
		bool active = obj.hasCities();
		((Component)_banners_cities).gameObject.SetActive(active);
		_line_cities.SetActive(active);
		bool active2 = obj.hasKingdoms();
		((Component)_banners_kingdoms).gameObject.SetActive(active2);
		_line_kingdoms.SetActive(active2);
	}
}
