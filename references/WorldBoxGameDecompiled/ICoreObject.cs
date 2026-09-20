public interface ICoreObject
{
	string name { get; }

	long getID();

	int getAge();

	bool isAlive();

	bool isFavorite();

	void switchFavorite();
}
