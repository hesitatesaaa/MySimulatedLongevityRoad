using System.Collections.Generic;
using UnityEngine;

public class BookManager : CoreSystemManager<Book, BookData>
{
	public const string COVER_PATH = "books/book_covers/";

	public const string ICON_PATH = "books/book_icons/";

	private static Sprite[] _cached_covers;

	public BookManager()
	{
		type_id = "book";
	}

	public Book generateNewBook(Actor pActor)
	{
		City city = pActor.getCity();
		Building buildingWithBookSlot = city.getBuildingWithBookSlot();
		if (buildingWithBookSlot == null)
		{
			return null;
		}
		Book book = newBook(pActor);
		if (book == null)
		{
			return null;
		}
		World.world.game_stats.data.booksWritten++;
		World.world.map_stats.booksWritten++;
		pActor.changeHappiness("wrote_book");
		buildingWithBookSlot.addBook(book);
		city.setStatusDirty();
		return book;
	}

	public string getNewCoverPath()
	{
		if (_cached_covers == null)
		{
			_cached_covers = SpriteTextureLoader.getSpriteList("books/book_covers/");
		}
		return ((Object)_cached_covers.GetRandom()).name;
	}

	private BookTypeAsset getPossibleBookType(Actor pActor)
	{
		using ListPool<BookTypeAsset> listPool = new ListPool<BookTypeAsset>(AssetManager.book_types.list.Count * 5);
		for (int i = 0; i < AssetManager.book_types.list.Count; i++)
		{
			BookTypeAsset bookTypeAsset = AssetManager.book_types.list[i];
			if (bookTypeAsset.requirement_check == null || bookTypeAsset.requirement_check(pActor, bookTypeAsset))
			{
				int num = bookTypeAsset.writing_rate;
				if (bookTypeAsset.rate_calc != null)
				{
					num = bookTypeAsset.rate_calc(pActor, bookTypeAsset);
				}
				num = Mathf.Min(num, 10);
				for (int j = 0; j < num; j++)
				{
					listPool.Add(bookTypeAsset);
				}
			}
		}
		if (listPool.Count == 0)
		{
			return null;
		}
		return listPool.GetRandom();
	}

	public Book newBook(Actor pActor)
	{
		BookTypeAsset possibleBookType = getPossibleBookType(pActor);
		if (possibleBookType == null)
		{
			return null;
		}
		Book book = newObject();
		ActorTrait bookTrait = getBookTrait(pActor);
		LanguageTrait pTraitLanguage = pActor.language?.getTraitForBook();
		ReligionTrait pTraitReligion = pActor.religion?.getTraitForBook();
		CultureTrait pTraitCulture = pActor.culture?.getTraitForBook();
		book.newBook(pActor, possibleBookType, bookTrait, pTraitCulture, pTraitLanguage, pTraitReligion);
		return book;
	}

	private ActorTrait getBookTrait(Actor pActor)
	{
		IReadOnlyCollection<ActorTrait> traits = pActor.getTraits();
		using ListPool<ActorTrait> listPool = new ListPool<ActorTrait>(traits.Count);
		foreach (ActorTrait item in traits)
		{
			if (item.group_id == "mind")
			{
				listPool.Add(item);
			}
		}
		if (listPool.Count == 0)
		{
			return null;
		}
		return listPool.GetRandom();
	}

	public void copyBook(Book pBook)
	{
	}

	public void burnBook(Book pBook)
	{
		pBook.getLanguage()?.books.setDirty();
		pBook.getCulture()?.books.setDirty();
		pBook.getReligion()?.books.setDirty();
		removeObject(pBook);
	}

	public override void removeObject(Book pObject)
	{
		World.world.game_stats.data.booksBurnt++;
		World.world.map_stats.booksBurnt++;
		base.removeObject(pObject);
	}
}
