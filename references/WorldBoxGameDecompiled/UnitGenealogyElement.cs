using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UnitGenealogyElement : UnitElement
{
	private const int AVATARS_LIMIT_PER_UNFOLD = 128;

	private const int AVATARS_LIMIT_INITIAL = 16;

	public const float COUNT_ANIMATION_STEP_TIME = 0.025f;

	private const float COUNT_ANIMATION_LENGTH = 0.5f;

	public const float COUNT_ANIMATION_STEPS = 20f;

	public UiUnitAvatarElement prefab_avatar;

	[SerializeField]
	private UnfoldButton _prefab_unfolder;

	[SerializeField]
	private Image _sex_icon;

	[SerializeField]
	private Sprite _default_genealogy_icon;

	private UnfoldButton _siblings_unfolder;

	private UnfoldButton _children_unfolder;

	private ObjectPoolGenericMono<UiUnitAvatarElement> _pool_grandparents;

	private ObjectPoolGenericMono<UiUnitAvatarElement> _pool_parents;

	private ObjectPoolGenericMono<UiUnitAvatarElement> _pool_siblings;

	private ObjectPoolGenericMono<UiUnitAvatarElement> _pool_children;

	public Transform transform_siblings;

	public Transform transform_children;

	public Transform transform_parents;

	public Transform transform_grandparents;

	protected override void Awake()
	{
		_pool_siblings = new ObjectPoolGenericMono<UiUnitAvatarElement>(prefab_avatar, transform_siblings);
		_pool_children = new ObjectPoolGenericMono<UiUnitAvatarElement>(prefab_avatar, transform_children);
		_pool_grandparents = new ObjectPoolGenericMono<UiUnitAvatarElement>(prefab_avatar, transform_grandparents);
		_pool_parents = new ObjectPoolGenericMono<UiUnitAvatarElement>(prefab_avatar, transform_parents);
		_siblings_unfolder = Object.Instantiate<UnfoldButton>(_prefab_unfolder, transform_siblings);
		_siblings_unfolder.setCallback(delegate
		{
			((MonoBehaviour)this).StartCoroutine(loadSiblings(pUnfold: true));
		});
		_children_unfolder = Object.Instantiate<UnfoldButton>(_prefab_unfolder, transform_children);
		_children_unfolder.setCallback(delegate
		{
			((MonoBehaviour)this).StartCoroutine(loadChildren(pUnfold: true));
		});
		base.Awake();
	}

	protected override IEnumerator showContent()
	{
		if (actor.asset.inspect_sex)
		{
			if (actor.isSexMale())
			{
				_sex_icon.sprite = SpriteTextureLoader.getSprite("ui/icons/IconMale");
			}
			else
			{
				_sex_icon.sprite = SpriteTextureLoader.getSprite("ui/icons/IconFemale");
			}
		}
		else
		{
			_sex_icon.sprite = _default_genealogy_icon;
		}
		yield return loadParents();
		yield return loadChildren();
		yield return loadSiblings();
		yield return loadGrandParents();
	}

	protected override void clear()
	{
		_pool_siblings.clear();
		_pool_children.clear();
		_pool_grandparents.clear();
		_pool_parents.clear();
		((Component)prefab_avatar).gameObject.SetActive(false);
		((Component)_siblings_unfolder).gameObject.SetActive(false);
		_siblings_unfolder.clear();
		((Component)_children_unfolder).gameObject.SetActive(false);
		_children_unfolder.clear();
		base.clear();
	}

	private IEnumerator loadParents()
	{
		using ListPool<Actor> tParents = new ListPool<Actor>(actor.getParents());
		track_objects.AddRange(tParents);
		foreach (ref Actor item in tParents)
		{
			Actor current = item;
			yield return showAvatar(current, _pool_parents);
		}
	}

	private IEnumerator loadGrandParents()
	{
		using ListPool<Actor> tGrandParents = new ListPool<Actor>();
		foreach (Actor parent in actor.getParents())
		{
			tGrandParents.AddRange(parent.getParents());
		}
		track_objects.AddRange(tGrandParents);
		foreach (ref Actor item in tGrandParents)
		{
			Actor current2 = item;
			yield return showAvatar(current2, _pool_grandparents);
		}
	}

	private IEnumerator loadChildren(bool pUnfold = false)
	{
		yield return unfold(childrenCheck, _pool_children, _children_unfolder, transform_children, pUnfold);
	}

	private IEnumerator loadSiblings(bool pUnfold = false)
	{
		yield return unfold(siblingsCheck, _pool_siblings, _siblings_unfolder, transform_siblings, pUnfold);
	}

	private IEnumerator unfold(UnfoldCheck pCheckAction, ObjectPoolGenericMono<UiUnitAvatarElement> pPool, UnfoldButton pFoldButton, Transform pParent, bool pUnfold = false)
	{
		((Component)pFoldButton).gameObject.SetActive(false);
		int num = (pUnfold ? 128 : 16);
		int num2 = 0;
		int tLeft = 0;
		int tLatestShown = 0;
		using ListPool<Actor> tFiltered = new ListPool<Actor>();
		foreach (Actor unit in World.world.units)
		{
			if (unit.isRekt() || actor == unit || !pCheckAction(unit))
			{
				continue;
			}
			num2++;
			track_objects.Add(unit);
			if (pFoldButton.offset == 0 || num2 >= pFoldButton.offset)
			{
				if (num2 - pFoldButton.offset >= num)
				{
					tLeft++;
					continue;
				}
				tLatestShown = num2;
				tFiltered.Add(unit);
			}
		}
		foreach (ref Actor item in tFiltered)
		{
			Actor current2 = item;
			yield return showAvatar(current2, pPool);
		}
		if (tLeft > 0)
		{
			((Component)pFoldButton).transform.SetSiblingIndex(pParent.childCount - 1);
			((Component)pFoldButton).gameObject.SetActive(true);
			pFoldButton.setData(tLeft, tLatestShown);
			((MonoBehaviour)this).StartCoroutine(counter(tLeft, pFoldButton));
		}
		else
		{
			((Component)pFoldButton).gameObject.SetActive(false);
		}
	}

	private IEnumerator counter(int pLeft, UnfoldButton pButton)
	{
		float tPerStep = (float)pLeft / 20f;
		for (float i = 0f; i < (float)(pLeft + 1); i += tPerStep)
		{
			string text = "+" + Mathf.Floor(i);
			pButton.setText(text);
			yield return (object)new WaitForSecondsRealtime(0.025f);
		}
	}

	private bool childrenCheck(Actor pActor)
	{
		return pActor.isChildOf(actor);
	}

	private bool siblingsCheck(Actor pActor)
	{
		return hasSameParent(pActor, actor);
	}

	private bool hasSameParent(Actor pActor, Actor pOther)
	{
		long parent_id_ = pOther.data.parent_id_1;
		long parent_id_2 = pOther.data.parent_id_2;
		long parent_id_3 = pActor.data.parent_id_1;
		if (parent_id_3.hasValue())
		{
			if (parent_id_3 == parent_id_)
			{
				return true;
			}
			if (parent_id_3 == parent_id_2)
			{
				return true;
			}
		}
		parent_id_3 = pActor.data.parent_id_2;
		if (parent_id_3.hasValue())
		{
			if (parent_id_3 == parent_id_)
			{
				return true;
			}
			if (parent_id_3 == parent_id_2)
			{
				return true;
			}
		}
		return false;
	}

	private IEnumerator showAvatar(Actor pActor, ObjectPoolGenericMono<UiUnitAvatarElement> pPool)
	{
		if (!pActor.isRekt())
		{
			yield return (object)new WaitForSecondsRealtime(0.025f);
			if (!pActor.isRekt())
			{
				pPool.getNext().show(pActor);
			}
		}
	}
}
