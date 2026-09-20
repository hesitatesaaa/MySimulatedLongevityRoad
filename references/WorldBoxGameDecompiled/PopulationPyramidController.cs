using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopulationPyramidController : MonoBehaviour
{
	[SerializeField]
	private MetaType _meta_type = MetaType.Special;

	[SerializeField]
	private Transform _population_pyramid_container;

	[SerializeField]
	private PopulationPyramidRow _population_pyramid_row;

	private Dictionary<int, GenderCount> _age_data = new Dictionary<int, GenderCount>();

	private List<int> _age_groups = new List<int>(20);

	private int _max_amount;

	private ObjectPoolGenericMono<PopulationPyramidRow> _pool_rows;

	private int _max_life_span;

	private int _age_steps;

	private IEnumerable<Actor> _units;

	private void Awake()
	{
		if (_pool_rows == null)
		{
			for (int i = 0; i < _population_pyramid_container.childCount; i++)
			{
				Object.Destroy((Object)(object)((Component)_population_pyramid_container.GetChild(i)).gameObject);
			}
			_pool_rows = new ObjectPoolGenericMono<PopulationPyramidRow>(_population_pyramid_row, _population_pyramid_container);
		}
	}

	private void OnEnable()
	{
		load();
	}

	private void OnDisable()
	{
		clearBars();
	}

	internal void load()
	{
		calculateLifespan();
		calculateAgeData();
		calculateAgeGroups();
		((MonoBehaviour)this).StartCoroutine(showBars());
	}

	private void clearBars()
	{
		_units = null;
		_pool_rows.clear();
	}

	private void calculateLifespan()
	{
		IMetaObject metaObject = (IMetaObject)AssetManager.meta_type_library.getAsset(_meta_type).get_selected();
		_units = metaObject.getUnits();
		_max_life_span = metaObject.getMaxPossibleLifespan();
		foreach (Actor unit in _units)
		{
			if (unit.isAlive())
			{
				int age = unit.getAge();
				if (age > _max_life_span)
				{
					_max_life_span = age;
				}
			}
		}
		_max_life_span = Mathf.CeilToInt((float)_max_life_span / 20f) * 20;
		_max_life_span = Mathf.Clamp(_max_life_span, 40, 100);
		_age_steps = _max_life_span / 10;
	}

	private void calculateAgeData()
	{
		_max_amount = 0;
		_age_data.Clear();
		foreach (Actor unit in _units)
		{
			if (unit.isAlive())
			{
				int key = unit.getAge() / _age_steps;
				if (!_age_data.ContainsKey(key))
				{
					_age_data[key] = new GenderCount();
				}
				int num = 0;
				if (unit.isSexMale())
				{
					num = ++_age_data[key].males;
				}
				if (unit.isSexFemale())
				{
					num = ++_age_data[key].females;
				}
				if (num > _max_amount)
				{
					_max_amount = num;
				}
			}
		}
	}

	private void calculateAgeGroups()
	{
		_age_groups.Clear();
		int num = 10;
		foreach (int key in _age_data.Keys)
		{
			if (key > num)
			{
				num = key;
			}
		}
		while (num >= 0)
		{
			_age_groups.Add(num--);
		}
	}

	private IEnumerator showBars()
	{
		foreach (int age_group in _age_groups)
		{
			if (_age_data.ContainsKey(age_group) || age_group <= 10)
			{
				PopulationPyramidRow next = _pool_rows.getNext();
				next.setAgeGroup(age_group * _age_steps, (age_group + 1) * _age_steps - 1);
				if (_age_data.ContainsKey(age_group))
				{
					int colorTextBasedOnAmount = _age_data[age_group].males + _age_data[age_group].females;
					next.setMaleCount(_age_data[age_group].males, _max_amount);
					next.setFemaleCount(_age_data[age_group].females, _max_amount);
					next.setColorTextBasedOnAmount(colorTextBasedOnAmount);
				}
				else
				{
					next.setColorTextBasedOnAmount(0);
					next.setMaleCount(0, _max_amount);
					next.setFemaleCount(0, _max_amount);
				}
				yield return CoroutineHelper.wait_for_next_frame;
			}
		}
	}
}
