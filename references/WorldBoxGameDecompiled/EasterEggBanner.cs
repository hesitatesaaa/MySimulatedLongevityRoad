using UnityEngine;
using UnityEngine.UI;

public class EasterEggBanner : MonoBehaviour
{
	[SerializeField]
	private GameObject _container_with_elements;

	private float _cur_random_accumulation;

	private const float BASE_CHANCE = 0.1f;

	private const float ACCUMULATION_STEP = 0.01f;

	private bool? _dragging_item;

	public Image main_image;

	private void OnEnable()
	{
		nextChance();
	}

	private void nextChance()
	{
		bool flag = Randy.randomChance(0.1f + _cur_random_accumulation);
		if (!flag)
		{
			_cur_random_accumulation += 0.01f;
		}
		else
		{
			_cur_random_accumulation = 0f;
		}
		_container_with_elements.SetActive(flag);
	}

	private void clearChance()
	{
		_cur_random_accumulation = 0f;
		_container_with_elements.SetActive(false);
	}

	private void Update()
	{
		if (!_container_with_elements.activeSelf)
		{
			return;
		}
		bool flag = Config.isDraggingItem();
		if (flag != _dragging_item)
		{
			_dragging_item = flag;
			if (!flag)
			{
				clearChance();
			}
		}
	}
}
