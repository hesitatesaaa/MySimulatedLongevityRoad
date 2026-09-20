using System.Collections.Generic;

public interface IBooksWindow
{
	List<long> getBooks();

	bool hasBooks()
	{
		return getBooks().Count > 0;
	}
}
