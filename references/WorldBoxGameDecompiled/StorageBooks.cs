using System;
using System.Collections.Generic;

[Serializable]
public class StorageBooks : IDisposable
{
	public List<long> list_books = new List<long>();

	public void Dispose()
	{
		list_books.Clear();
		list_books = null;
	}

	public void addBook(Book pBook)
	{
		list_books.Add(pBook.id);
	}

	public bool hasAny()
	{
		return list_books.Count > 0;
	}

	public int totalBooks()
	{
		return list_books.Count;
	}
}
