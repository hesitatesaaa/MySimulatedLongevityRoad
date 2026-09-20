using System.Collections;
using System.Collections.Generic;
using LayoutGroupExt;
using UnityEngine;

public class TraitsContainer<TTrait, TTraitButton> : MonoBehaviour, ITraitsContainer<TTrait, TTraitButton> where TTrait : BaseTrait<TTrait> where TTraitButton : TraitButton<TTrait>
{
	[SerializeField]
	private TTraitButton _prefab_trait;

	[SerializeField]
	private Transform _regular_title;

	[SerializeField]
	private Transform _unlocked_title;

	[SerializeField]
	private Transform _grid;

	private LayoutGroupExtended _layout_grid;

	private ObjectPoolGenericMono<TTraitButton> _pool_traits;

	private ITraitWindow<TTrait, TTraitButton> _trait_window;

	private Dictionary<TTrait, TTraitButton> _traits = new Dictionary<TTrait, TTraitButton>();

	private void Awake()
	{
		_trait_window = ((Component)this).GetComponentInParent<ITraitWindow<TTrait, TTraitButton>>();
		_pool_traits = new ObjectPoolGenericMono<TTraitButton>(_prefab_trait, _grid);
		_layout_grid = ((Component)_grid).GetComponent<LayoutGroupExtended>();
		((Component)_grid).gameObject.AddOrGetComponent<TraitsGrid>().on_change = sortTraits;
	}

	private void OnEnable()
	{
		if ((Object)(object)_regular_title != (Object)null)
		{
			if (((Component)_unlocked_title).gameObject.activeSelf)
			{
				((Component)_regular_title).gameObject.SetActive(false);
			}
			else
			{
				((Component)_regular_title).gameObject.SetActive(true);
			}
		}
		((MonoBehaviour)this).StartCoroutine(loadActiveTraits());
	}

	private void OnDisable()
	{
		_traits.Clear();
		_pool_traits.clear();
	}

	public void reloadTraits(bool pAnimated)
	{
		((MonoBehaviour)this).StopAllCoroutines();
		((MonoBehaviour)this).StartCoroutine(loadActiveTraits(pAnimated));
	}

	protected IEnumerator loadActiveTraits(bool pAnimated = true)
	{
		using (ListPool<TTrait> listPool = new ListPool<TTrait>(_trait_window.getTraits()))
		{
			_traits.Clear();
			_pool_traits.clear();
			foreach (ref TTrait item in listPool)
			{
				TTrait current = item;
				loadActiveTrait(current);
			}
		}
		yield break;
	}

	private void loadActiveTrait(TTrait pTraitAsset)
	{
		TTraitButton next = _pool_traits.getNext();
		next.load(pTraitAsset);
		_traits[pTraitAsset] = next;
		AugmentationUnlockedAction pAction = _trait_window.getEditor().reloadButtons;
		next.removeElementUnlockedAction(pAction);
		next.addElementUnlockedAction(pAction);
		ITraitsEditor<TTrait> editor = _trait_window.getEditor();
		next.removeClickAction(editor.scrollToGroupStarter);
		next.addClickAction(editor.scrollToGroupStarter);
	}

	public void sortTraits()
	{
		using ListPool<TTrait> listPool = new ListPool<TTrait>(_traits.Keys);
		listPool.Sort((TTrait a, TTrait b) => ((Component)_traits[a]).transform.GetSiblingIndex().CompareTo(((Component)_traits[b]).transform.GetSiblingIndex()));
		_trait_window.sortTraits(listPool);
	}

	public ObjectPoolGenericMono<TTraitButton> getTraitPool()
	{
		return _pool_traits;
	}

	public IReadOnlyCollection<TTraitButton> getTraitButtons()
	{
		return _traits.Values;
	}
}
