using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class InterestingPeopleTab : WindowMetaElementBase
{
	private const float TWEEN_DURATION = 0.15f;

	public InterestingPeopleElement most_kills;

	public InterestingPeopleElement most_children;

	public InterestingPeopleElement most_births;

	public InterestingPeopleElement oldest;

	public InterestingPeopleElement fastest;

	public InterestingPeopleElement strongest;

	public InterestingPeopleElement weakest;

	public InterestingPeopleElement smartest;

	public InterestingPeopleElement dumbest;

	public InterestingPeopleElement richest;

	public InterestingPeopleElement most_known;

	public InterestingPeopleElement biggest_level;

	public InterestingPeopleElement happiest;

	public InterestingPeopleElement saddest;

	public InterestingPeopleElement hungriest;

	public InterestingPeopleElement fullest;

	public InterestingPeopleElement youngest;

	public InterestingPeopleElement most_health;

	public InterestingPeopleElement lowest_health;

	private readonly List<Actor> _unit_most_kills = new List<Actor>();

	private readonly List<Actor> _unit_most_children = new List<Actor>();

	private readonly List<Actor> _unit_most_births = new List<Actor>();

	private readonly List<Actor> _unit_oldest = new List<Actor>();

	private readonly List<Actor> _unit_fastest = new List<Actor>();

	private readonly List<Actor> _unit_strongest = new List<Actor>();

	private readonly List<Actor> _unit_weakest = new List<Actor>();

	private readonly List<Actor> _unit_smartest = new List<Actor>();

	private readonly List<Actor> _unit_dumbest = new List<Actor>();

	private readonly List<Actor> _unit_richest = new List<Actor>();

	private readonly List<Actor> _unit_most_known = new List<Actor>();

	private readonly List<Actor> _unit_biggest_level = new List<Actor>();

	private readonly List<Actor> _unit_saddest = new List<Actor>();

	private readonly List<Actor> _unit_happiest = new List<Actor>();

	private readonly List<Actor> _unit_hungriest = new List<Actor>();

	private readonly List<Actor> _unit_fullest = new List<Actor>();

	private readonly List<Actor> _unit_youngest = new List<Actor>();

	private readonly List<Actor> _unit_most_health = new List<Actor>();

	private readonly List<Actor> _unit_lowest_health = new List<Actor>();

	private List<Actor>[] _all_unit_lists;

	private InterestingPeopleElement[] _all_elements;

	private IInterestingPeopleWindow _interesting_people_window;

	private List<Tweener> _tweeners = new List<Tweener>();

	protected override void Awake()
	{
		_interesting_people_window = ((Component)this).GetComponentInParent<IInterestingPeopleWindow>();
		_all_elements = new InterestingPeopleElement[19]
		{
			biggest_level, fastest, fullest, happiest, hungriest, most_births, most_children, most_kills, most_known, oldest,
			richest, saddest, smartest, dumbest, strongest, weakest, youngest, most_health, lowest_health
		};
		_all_unit_lists = new List<Actor>[19]
		{
			_unit_biggest_level, _unit_fastest, _unit_fullest, _unit_happiest, _unit_hungriest, _unit_most_births, _unit_most_children, _unit_most_kills, _unit_most_known, _unit_oldest,
			_unit_richest, _unit_saddest, _unit_smartest, _unit_dumbest, _unit_strongest, _unit_weakest, _unit_youngest, _unit_most_health, _unit_lowest_health
		};
		base.Awake();
	}

	protected override IEnumerator showContent()
	{
		IEnumerable<Actor> interestingUnitsList = _interesting_people_window.getInterestingUnitsList();
		return renderElements(interestingUnitsList);
	}

	private IEnumerator renderElements(IEnumerable<Actor> pList)
	{
		int tMaxKills = 1;
		int tMaxChildren = 1;
		int tMaxAge = 0;
		int tMinAge = int.MaxValue;
		int tMaxMoney = 1;
		int tMaxSpeed = 1;
		int tMaxDamage = 1;
		int tMinDamage = int.MaxValue;
		int tMaxIntelligence = 1;
		int tMinIntelligence = int.MaxValue;
		int num = 1;
		int tMaxLevel = 1;
		int tMinSad = -10;
		int tMaxHappy = 10;
		int tMinNutrition = 30;
		int tMaxNutrition = 60;
		int tMaxBirths = 1;
		int tMaxHealth = 1;
		int tMinHealth = int.MaxValue;
		using ListPool<Actor> tUnits = new ListPool<Actor>(pList);
		tUnits.RemoveAll((Actor tActor) => !tActor.isAlive() || tActor.asset.is_boat);
		tUnits.Sort(ListSorters.sortUnitByKills);
		tUnits.Sort(ListSorters.sortUnitByAgeOldFirst);
		foreach (ref Actor item in tUnits)
		{
			Actor current = item;
			if (current.data.kills > tMaxKills)
			{
				tMaxKills = current.data.kills;
				_unit_most_kills.Clear();
				_unit_most_kills.Add(current);
			}
			else if (current.data.kills == tMaxKills && _unit_most_kills.Count < 3)
			{
				_unit_most_kills.Add(current);
			}
			if (current.current_children_count > tMaxChildren)
			{
				tMaxChildren = current.current_children_count;
				_unit_most_children.Clear();
				_unit_most_children.Add(current);
			}
			else if (current.current_children_count == tMaxChildren && _unit_most_children.Count < 3)
			{
				_unit_most_children.Add(current);
			}
			if (current.data.births > tMaxBirths)
			{
				tMaxBirths = current.data.births;
				_unit_most_births.Clear();
				_unit_most_births.Add(current);
			}
			else if (current.data.births == tMaxBirths && _unit_most_births.Count < 3)
			{
				_unit_most_births.Add(current);
			}
			if (current.stats["speed"] > (float)tMaxSpeed)
			{
				tMaxSpeed = (int)current.stats["speed"];
				_unit_fastest.Clear();
				_unit_fastest.Add(current);
			}
			else if ((int)current.stats["speed"] == tMaxSpeed && _unit_fastest.Count < 3)
			{
				_unit_fastest.Add(current);
			}
			int health = current.getHealth();
			if (health > tMaxHealth)
			{
				tMaxHealth = health;
				_unit_most_health.Clear();
				_unit_most_health.Add(current);
			}
			else if (health == tMaxHealth && _unit_most_health.Count < 3)
			{
				_unit_most_health.Add(current);
			}
			if (health < tMinHealth)
			{
				tMinHealth = health;
				_unit_lowest_health.Clear();
				_unit_lowest_health.Add(current);
			}
			else if (health == tMinHealth && _unit_lowest_health.Count < 3)
			{
				_unit_lowest_health.Add(current);
			}
			int num2 = (int)current.stats["damage"];
			if (num2 > tMaxDamage)
			{
				tMaxDamage = num2;
				_unit_strongest.Clear();
				_unit_strongest.Add(current);
			}
			else if (num2 == tMaxDamage && _unit_strongest.Count < 3)
			{
				_unit_strongest.Add(current);
			}
			if (num2 < tMinDamage)
			{
				tMinDamage = num2;
				_unit_weakest.Clear();
				_unit_weakest.Add(current);
			}
			else if (num2 == tMinDamage && _unit_weakest.Count < 3)
			{
				_unit_weakest.Add(current);
			}
			int num3 = (int)current.stats["intelligence"];
			if (num3 > tMaxIntelligence)
			{
				tMaxIntelligence = num3;
				_unit_smartest.Clear();
				_unit_smartest.Add(current);
			}
			else if (num3 == tMaxIntelligence && _unit_smartest.Count < 3)
			{
				_unit_smartest.Add(current);
			}
			if (num3 < tMinIntelligence)
			{
				tMinIntelligence = num3;
				_unit_dumbest.Clear();
				_unit_dumbest.Add(current);
			}
			else if (num3 == tMinIntelligence && _unit_dumbest.Count < 3)
			{
				_unit_dumbest.Add(current);
			}
			if (current.money > tMaxMoney)
			{
				tMaxMoney = current.money;
				_unit_richest.Clear();
				_unit_richest.Add(current);
			}
			else if (current.money == tMaxMoney && _unit_richest.Count < 3)
			{
				_unit_richest.Add(current);
			}
			if (current.renown > num)
			{
				num = current.renown;
				_unit_most_known.Clear();
				_unit_most_known.Add(current);
			}
			else if (current.renown == num && _unit_most_known.Count < 3)
			{
				_unit_most_known.Add(current);
			}
			if (current.data.level > tMaxLevel)
			{
				tMaxLevel = current.data.level;
				_unit_biggest_level.Clear();
				_unit_biggest_level.Add(current);
			}
			else if (current.data.level == tMaxLevel && _unit_biggest_level.Count < 3)
			{
				_unit_biggest_level.Add(current);
			}
			if (current.hasEmotions())
			{
				int happiness = current.getHappiness();
				if (happiness > tMaxHappy)
				{
					tMaxHappy = happiness;
					_unit_happiest.Clear();
					_unit_happiest.Add(current);
				}
				else if (happiness == tMaxHappy && _unit_happiest.Count < 3)
				{
					_unit_happiest.Add(current);
				}
				if (happiness < tMinSad)
				{
					tMinSad = happiness;
					_unit_saddest.Clear();
					_unit_saddest.Add(current);
				}
				else if (happiness == tMinSad && _unit_saddest.Count < 3)
				{
					_unit_saddest.Add(current);
				}
			}
			int nutrition = current.data.nutrition;
			if (nutrition > tMaxNutrition)
			{
				tMaxNutrition = nutrition;
				_unit_fullest.Clear();
				_unit_fullest.Add(current);
			}
			else if (nutrition == tMaxNutrition && _unit_fullest.Count < 3)
			{
				_unit_fullest.Add(current);
			}
			if (nutrition < tMinNutrition)
			{
				tMinNutrition = nutrition;
				_unit_hungriest.Clear();
				_unit_hungriest.Add(current);
			}
			else if (nutrition == tMinNutrition && _unit_hungriest.Count < 3)
			{
				_unit_hungriest.Add(current);
			}
			int age = current.getAge();
			if (age > tMaxAge)
			{
				tMaxAge = age;
				_unit_oldest.Clear();
				_unit_oldest.Add(current);
			}
			else if (age == tMaxAge && _unit_oldest.Count < 3)
			{
				_unit_oldest.Add(current);
			}
			if (age < tMinAge)
			{
				tMinAge = age;
				_unit_youngest.Clear();
				_unit_youngest.Add(current);
			}
			else if (age == tMinAge && _unit_youngest.Count < 3)
			{
				_unit_youngest.Add(current);
			}
		}
		List<Actor>[] all_unit_lists = _all_unit_lists;
		foreach (List<Actor> collection in all_unit_lists)
		{
			track_objects.AddRange(collection);
		}
		yield return render(_unit_most_known, most_known, num);
		yield return render(_unit_biggest_level, biggest_level, tMaxLevel);
		yield return render(_unit_oldest, oldest, tMaxAge, 0);
		if (tMinAge != tMaxAge)
		{
			yield return render(_unit_youngest, youngest, tMinAge, 0);
		}
		yield return render(_unit_most_kills, most_kills, tMaxKills);
		yield return render(_unit_richest, richest, tMaxMoney);
		yield return render(_unit_most_children, most_children, tMaxChildren);
		yield return render(_unit_most_births, most_births, tMaxBirths);
		yield return render(_unit_happiest, happiest, tMaxHappy);
		yield return render(_unit_saddest, saddest, tMinSad, -1000);
		yield return render(_unit_hungriest, hungriest, tMinNutrition, 0);
		yield return render(_unit_fullest, fullest, tMaxNutrition);
		yield return render(_unit_smartest, smartest, tMaxIntelligence);
		yield return render(_unit_dumbest, dumbest, tMinIntelligence, -1000);
		yield return render(_unit_fastest, fastest, tMaxSpeed);
		yield return render(_unit_strongest, strongest, tMaxDamage);
		yield return render(_unit_weakest, weakest, tMinDamage, -1000);
		yield return render(_unit_most_health, most_health, tMaxHealth);
		if (tMinHealth != tMaxHealth)
		{
			yield return render(_unit_lowest_health, lowest_health, tMinHealth, 0);
		}
	}

	private IEnumerator render(List<Actor> pActor, InterestingPeopleElement pElement, int pValue, int pMinValue = 2)
	{
		if (pValue < pMinValue || pActor.Count == 0)
		{
			((Component)pElement).gameObject.SetActive(false);
			yield break;
		}
		((Component)pElement).gameObject.SetActive(true);
		foreach (Actor item in pActor)
		{
			if (item.isAlive())
			{
				pElement.show(item, pValue);
				yield return (object)new WaitForSecondsRealtime(0.025f);
			}
		}
	}

	private void finishTweens()
	{
		foreach (Tweener tweener in _tweeners)
		{
			TweenExtensions.Kill((Tween)(object)tweener, true);
		}
		_tweeners.Clear();
	}

	protected override void clear()
	{
		base.clear();
		finishTweens();
		InterestingPeopleElement[] all_elements = _all_elements;
		for (int i = 0; i < all_elements.Length; i++)
		{
			((Component)all_elements[i]).gameObject.SetActive(false);
		}
		List<Actor>[] all_unit_lists = _all_unit_lists;
		for (int i = 0; i < all_unit_lists.Length; i++)
		{
			all_unit_lists[i].Clear();
		}
	}
}
