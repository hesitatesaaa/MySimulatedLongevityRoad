using System.Collections;
using UnityEngine;

public class BooksContainer : BooksElement
{
	private ObjectPoolGenericMono<CultureBookButton> _pool_books;

	private CultureBookButton _prefab_book;

	[SerializeField]
	private Transform _title;

	[SerializeField]
	private Transform _books_grid;

	protected override void Awake()
	{
		_prefab_book = Resources.Load<CultureBookButton>("ui/PrefabBook");
		_pool_books = new ObjectPoolGenericMono<CultureBookButton>(_prefab_book, _books_grid);
		base.Awake();
	}

	protected override IEnumerator showContent()
	{
		if (books.Count == 0)
		{
			yield break;
		}
		if ((Object)(object)_title != (Object)null)
		{
			((Component)_title).gameObject.SetActive(true);
		}
		((Component)_books_grid).gameObject.SetActive(true);
		using ListPool<long> tBooks = new ListPool<long>(books);
		foreach (ref long item in tBooks)
		{
			long tBookID = item;
			yield return (object)new WaitForSecondsRealtime(0.025f);
			loadBookButton(tBookID);
		}
	}

	public void loadBookButton(long pBookID)
	{
		_pool_books.getNext().load(pBookID);
	}

	protected override void clear()
	{
		_pool_books.clear();
		if ((Object)(object)_title != (Object)null)
		{
			((Component)_title).gameObject.SetActive(false);
		}
		((Component)_books_grid).gameObject.SetActive(false);
		base.clear();
	}

	protected override void clearInitial()
	{
		for (int i = 0; i < _books_grid.childCount; i++)
		{
			Object.Destroy((Object)(object)((Component)_books_grid.GetChild(i)).gameObject);
		}
		base.clearInitial();
	}
}
