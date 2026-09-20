using System.Collections.Generic;

public class BooksHandler
{
	private readonly List<long> _books = new List<long>();

	private bool _books_dirty = true;

	private Culture _culture;

	private Language _language;

	private Religion _religion;

	public void setMeta(Culture pCulture = null, Language pLanguage = null, Religion pReligion = null)
	{
		_culture = pCulture;
		_language = pLanguage;
		_religion = pReligion;
		setDirty();
	}

	public List<long> getList()
	{
		checkBooks();
		return _books;
	}

	public int count()
	{
		return getList().Count;
	}

	public bool hasBooks()
	{
		return count() > 0;
	}

	public void setDirty()
	{
		_books_dirty = true;
	}

	private void checkBooks()
	{
		if (!_books_dirty)
		{
			return;
		}
		_books_dirty = false;
		_books.Clear();
		foreach (Book book in World.world.books)
		{
			if (_culture != null && book.getCulture() == _culture)
			{
				_books.Add(book.id);
			}
			else if (_language != null && book.getLanguage() == _language)
			{
				_books.Add(book.id);
			}
			else if (_religion != null && book.getReligion() == _religion)
			{
				_books.Add(book.id);
			}
		}
	}

	public void clear()
	{
		setDirty();
		_culture = null;
		_language = null;
		_religion = null;
	}
}
