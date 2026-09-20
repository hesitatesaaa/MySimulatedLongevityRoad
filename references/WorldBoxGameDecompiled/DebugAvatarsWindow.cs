using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugAvatarsWindow : MonoBehaviour
{
	private static readonly bool _test_mutations = false;

	private static readonly bool _test_eggs = true;

	private static readonly bool _test_hand_items = false;

	private static readonly bool _test_statuses = false;

	[SerializeField]
	private Transform _avatars_parent;

	[SerializeField]
	private UnitAvatarLoader _avatar_prefab;

	[SerializeField]
	private Image _autotest_button_icon;

	[SerializeField]
	private Sprite _sprite_play;

	[SerializeField]
	private Sprite _sprite_pause;

	private ObjectPoolGenericMono<UnitAvatarLoader> _avatars;

	private List<SubspeciesTrait> _pool_mutations = new List<SubspeciesTrait>();

	private List<SubspeciesTrait> _pool_eggs = new List<SubspeciesTrait>();

	private List<PhenotypeAsset> _pool_phenotype = new List<PhenotypeAsset>();

	private List<AvatarCombineHandItem> _pool_hand_renderers = new List<AvatarCombineHandItem>();

	private List<StatusAsset> _pool_statuses = new List<StatusAsset>();

	private AvatarsCombineDataContainer _combine_data = new AvatarsCombineDataContainer();

	private HashSet<string> _statuses = new HashSet<string>();

	private HashSet<long> _check_collisions = new HashSet<long>();

	private bool _autotest_state;

	private Coroutine _autotest_routine;

	private void Awake()
	{
		init();
	}

	private void init()
	{
		_avatars = new ObjectPoolGenericMono<UnitAvatarLoader>(_avatar_prefab, _avatars_parent);
		preparePools();
	}

	private void OnEnable()
	{
		showAvatars();
	}

	private void OnDisable()
	{
		clear();
	}

	private void clear()
	{
		_avatars.clear();
	}

	private void showAvatars()
	{
		foreach (ActorAsset item in AssetManager.actor_library.list)
		{
			if (item.has_override_sprite || !item.has_sprite_renderer)
			{
				continue;
			}
			SubspeciesTrait randomMutation = getRandomMutation();
			bool randomIsAdult = getRandomIsAdult();
			ActorSex randomSex = getRandomSex();
			ColorAsset random = AssetManager.kingdom_colors_library.list.GetRandom();
			bool randomIsUnconscious = getRandomIsUnconscious();
			bool pIsLying = randomIsUnconscious || getRandomIsLying();
			bool randomIsHovering = getRandomIsHovering();
			bool pIsTouchingLiquid = getRandomIsTouchingLiquid() && !randomIsHovering;
			bool randomIsImmovable = getRandomIsImmovable();
			AvatarCombineHandItem randomItemPath = getRandomItemPath();
			List<string> randomStatuses = getRandomStatuses(out var pStopIdleAnimation);
			PhenotypeAsset randomPhenotype = getRandomPhenotype();
			int randomPhenotypeShade = Actor.getRandomPhenotypeShade();
			SubspeciesTrait randomEgg = getRandomEgg();
			bool pIsEgg = !randomIsAdult && randomEgg != null;
			ActorTextureSubAsset texture_asset;
			if (randomMutation != null)
			{
				texture_asset = randomMutation.texture_asset;
				BaseStats base_stats_meta = randomMutation.base_stats_meta;
				if (!base_stats_meta.isEmpty() && base_stats_meta.hasTag("always_idle_animation"))
				{
					pStopIdleAnimation = false;
				}
			}
			else
			{
				texture_asset = item.texture_asset;
			}
			DynamicActorSpriteCreatorUI.getContainerForUI(item, randomIsAdult, texture_asset, randomMutation, pIsEgg, randomEgg);
			ActorAvatarData actorAvatarData = new ActorAvatarData();
			actorAvatarData.setData(item, randomMutation, randomSex, Randy.randomInt(0, int.MaxValue), -1, null, randomPhenotype.phenotype_index, randomPhenotypeShade, random, pIsEgg, pIsKing: false, pIsWarrior: false, pIsWise: false, randomEgg, randomIsAdult, pIsLying, pIsTouchingLiquid, pIsInsideBoat: false, randomIsHovering, randomIsImmovable, randomIsUnconscious, pStopIdleAnimation, randomItemPath?.hand_renderer, 1, randomStatuses, null);
			_avatars.getNext().load(actorAvatarData);
		}
	}

	private void preparePools()
	{
		foreach (SubspeciesTrait item2 in AssetManager.subspecies_traits.list)
		{
			if (item2.is_mutation_skin)
			{
				_pool_mutations.Add(item2);
			}
			if (item2.phenotype_egg)
			{
				_pool_eggs.Add(item2);
			}
			if (item2.phenotype_skin)
			{
				PhenotypeAsset item = AssetManager.phenotype_library.get(item2.id_phenotype);
				_pool_phenotype.Add(item);
			}
		}
		foreach (EquipmentAsset item3 in AssetManager.items.pot_weapon_assets_all)
		{
			_pool_hand_renderers.Add(new AvatarCombineHandItem(item3));
		}
		foreach (ResourceAsset item4 in AssetManager.resources.list)
		{
			_pool_hand_renderers.Add(new AvatarCombineHandItem(item4));
		}
		foreach (UnitHandToolAsset item5 in AssetManager.unit_hand_tools.list)
		{
			_pool_hand_renderers.Add(new AvatarCombineHandItem(item5));
		}
		foreach (StatusAsset item6 in AssetManager.status.list)
		{
			if (item6.need_visual_render)
			{
				_pool_statuses.Add(item6);
			}
		}
	}

	private SubspeciesTrait getRandomMutation()
	{
		if (Randy.randomChance(0.75f))
		{
			return null;
		}
		return _pool_mutations.GetRandom();
	}

	private SubspeciesTrait getRandomEgg()
	{
		if (Randy.randomChance(0.9f))
		{
			return null;
		}
		return _pool_eggs.GetRandom();
	}

	private PhenotypeAsset getRandomPhenotype()
	{
		return _pool_phenotype.GetRandom();
	}

	private ActorSex getRandomSex()
	{
		if (Randy.randomChance(0.5f))
		{
			return ActorSex.Male;
		}
		return ActorSex.Female;
	}

	private bool getRandomIsAdult()
	{
		return Randy.randomBool();
	}

	private bool getRandomIsLying()
	{
		return Randy.randomChance(0.2f);
	}

	private bool getRandomIsTouchingLiquid()
	{
		return Randy.randomBool();
	}

	private bool getRandomIsHovering()
	{
		return Randy.randomChance(0.2f);
	}

	private bool getRandomIsImmovable()
	{
		return Randy.randomChance(0.2f);
	}

	private bool getRandomIsUnconscious()
	{
		return Randy.randomChance(0.2f);
	}

	private AvatarCombineHandItem getRandomItemPath()
	{
		if (Randy.randomChance(0.4f))
		{
			return null;
		}
		return _pool_hand_renderers.GetRandom();
	}

	private List<string> getRandomStatuses(out bool pStopIdleAnimation)
	{
		pStopIdleAnimation = false;
		List<string> list = new List<string>();
		foreach (StatusAsset item in AssetManager.status.list)
		{
			if (item.need_visual_render && !Randy.randomChance(0.95f))
			{
				if (item.base_stats.hasTag("stop_idle_animation"))
				{
					pStopIdleAnimation = true;
				}
				list.Add(item.id);
			}
		}
		return list;
	}

	public void toggleAutotest()
	{
		_autotest_state = !_autotest_state;
		if (_autotest_state)
		{
			_autotest_button_icon.sprite = _sprite_pause;
			_autotest_routine = ((MonoBehaviour)this).StartCoroutine(autotestRoutine());
		}
		else
		{
			_autotest_button_icon.sprite = _sprite_play;
			((MonoBehaviour)this).StopCoroutine(_autotest_routine);
		}
	}

	private T getFromPool<T>(List<T> pPool, int pGlobalIndex, string pId) where T : class
	{
		int listIndex = _combine_data.getListIndex(pGlobalIndex, pId);
		if (pPool.Count - 1 < listIndex)
		{
			return null;
		}
		return pPool[listIndex];
	}

	private bool getBool(int pGlobalIndex, string pId)
	{
		return _combine_data.getListIndex(pGlobalIndex, pId) == 1;
	}

	private IEnumerator autotestRoutine()
	{
		_combine_data.clear();
		_statuses.Clear();
		_check_collisions.Clear();
		_combine_data.add("tAdult", 2);
		_combine_data.add("tTouchingLiquid", 2);
		_combine_data.add("tLying", 2);
		_combine_data.add("tImmovable", 2);
		_combine_data.add("tUnconscious", 2);
		_combine_data.add("tSex", 2);
		if (_test_mutations)
		{
			_combine_data.add("_pool_mutations", _pool_mutations.Count);
		}
		if (_test_eggs)
		{
			_combine_data.add("_pool_eggs", _pool_eggs.Count);
		}
		if (_test_hand_items)
		{
			_combine_data.add("_pool_hand_renderers", _pool_hand_renderers.Count);
		}
		if (_test_statuses)
		{
			_combine_data.add("_pool_statuses", _pool_statuses.Count);
		}
		int tTotal = _combine_data.totalCombinations();
		for (int i = 0; i < tTotal; i++)
		{
			bool flag = getBool(i, "tAdult");
			bool flag2 = getBool(i, "tTouchingLiquid");
			bool flag3 = getBool(i, "tLying");
			bool flag4 = getBool(i, "tImmovable");
			bool flag5 = getBool(i, "tUnconscious");
			ActorSex actorSex = ((!getBool(i, "tSex")) ? ActorSex.Female : ActorSex.Male);
			bool flag6 = false;
			bool flag7 = false;
			long num = (flag ? 1 : 2) + (flag2 ? 1 : 2) * 10 + (flag3 ? 1 : 2) * 100 + (flag4 ? 1 : 2) * 1000 + (flag5 ? 1 : 2) * 10000 + ((actorSex == ActorSex.Male) ? 1 : 2) * 100000 + (flag6 ? 1 : 2) * 1000000;
			SubspeciesTrait subspeciesTrait = null;
			if (_test_mutations)
			{
				subspeciesTrait = getFromPool(_pool_mutations, i, "_pool_mutations");
				num += _pool_mutations.IndexOf(subspeciesTrait) * 100000000;
				BaseStats base_stats_meta = subspeciesTrait.base_stats_meta;
				if (!base_stats_meta.isEmpty() && base_stats_meta.hasTag("always_idle_animation"))
				{
					flag7 = true;
				}
			}
			SubspeciesTrait subspeciesTrait2 = null;
			if (subspeciesTrait == null && _test_eggs)
			{
				subspeciesTrait2 = getFromPool(_pool_eggs, i, "_pool_eggs");
				num += _pool_eggs.IndexOf(subspeciesTrait2) * 10000000000L;
			}
			bool flag8 = subspeciesTrait2 != null;
			IHandRenderer handRenderer;
			if (!flag8 && _test_hand_items)
			{
				AvatarCombineHandItem fromPool = getFromPool(_pool_hand_renderers, i, "_pool_hand_renderers");
				num += _pool_hand_renderers.IndexOf(fromPool) * 10000000000000L;
				handRenderer = fromPool.hand_renderer;
			}
			else
			{
				handRenderer = null;
				num += _pool_hand_renderers.Count * 10000000000000L;
			}
			StatusAsset statusAsset = null;
			if (_test_statuses)
			{
				statusAsset = getFromPool(_pool_statuses, i, "_pool_statuses");
				num += _pool_statuses.IndexOf(statusAsset) * 10000000000000000L;
			}
			int num2 = 1;
			foreach (UnitAvatarLoader item in _avatars.getListTotal())
			{
				_statuses.Clear();
				StatusAsset statusAsset2 = ((_test_statuses && Randy.randomBool()) ? _pool_statuses.GetRandom() : null);
				StatusAsset statusAsset3 = ((_test_statuses && Randy.randomBool()) ? _pool_statuses.GetRandom() : null);
				if (statusAsset != null)
				{
					_statuses.Add(statusAsset.id);
					if (statusAsset.base_stats.hasTag("stop_idle_animation"))
					{
						flag6 = true;
					}
				}
				if (statusAsset2 != null)
				{
					_statuses.Add(statusAsset2.id);
					if (statusAsset2.base_stats.hasTag("stop_idle_animation"))
					{
						flag6 = true;
					}
				}
				if (statusAsset3 != null)
				{
					_statuses.Add(statusAsset3.id);
					if (statusAsset3.base_stats.hasTag("stop_idle_animation"))
					{
						flag6 = true;
					}
				}
				num2++;
				ActorAvatarData data = item.getData();
				ActorAsset asset = data.asset;
				ActorTextureSubAsset pTextureAsset = ((subspeciesTrait == null) ? asset.texture_asset : subspeciesTrait.texture_asset);
				DynamicActorSpriteCreatorUI.getContainerForUI(asset, flag, pTextureAsset, subspeciesTrait, flag8, subspeciesTrait2);
				if (flag7)
				{
					flag6 = false;
				}
				ActorAvatarData actorAvatarData = new ActorAvatarData();
				actorAvatarData.setData(data.asset, subspeciesTrait, actorSex, Randy.randomInt(0, int.MaxValue), -1, null, data.phenotype_index, data.phenotype_skin_shade, data.kingdom_color, flag8, pIsKing: false, pIsWarrior: false, pIsWise: false, subspeciesTrait2, flag, flag3, flag2, pIsInsideBoat: false, data.is_hovering, flag4, flag5, flag6, handRenderer, num2, _statuses, null);
				item.load(actorAvatarData);
			}
			_check_collisions.Add(num);
			Debug.Log((object)string.Format("tested: {0}/{1}, hashset: {2}/{3} adult: {4}, liquid: {5}, lying: {6}, immovable: {7}, uncon: {8}, sex: {9}, mut: {10}, egg: {11}, item: {12}, status: {13}", i + 1, tTotal, _check_collisions.Count, tTotal, flag, flag2, flag3, flag4, flag5, actorSex, subspeciesTrait?.id ?? "null", subspeciesTrait2?.id ?? "null", handRenderer, statusAsset?.id ?? "null"));
			yield return null;
		}
	}
}
