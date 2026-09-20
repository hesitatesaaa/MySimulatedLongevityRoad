using UnityEngine;
using UnityEngine.UI;

public class CultureBookButton : MonoBehaviour
{
	private Book _book;

	public Image cover;

	public Image icon;

	private bool _created;

	private void Start()
	{
		create();
	}

	private void create()
	{
		if (!_created)
		{
			_created = true;
			setupTooltip();
		}
	}

	public void setupTooltip()
	{
		TipButton tipButton = default(TipButton);
		if (((Component)this).TryGetComponent<TipButton>(ref tipButton))
		{
			tipButton.setHoverAction(showTooltip);
		}
	}

	internal void load(long pBookID)
	{
		Book pBook = World.world.books.get(pBookID);
		load(pBook);
	}

	internal void load(Book pBook)
	{
		_book = pBook;
		BookTypeAsset asset = _book.getAsset();
		string pPath = "books/book_icons/" + asset.path_icons + _book.data.path_icon;
		string pPath2 = "books/book_covers/" + _book.data.path_cover;
		Sprite sprite = SpriteTextureLoader.getSprite(pPath);
		Sprite sprite2 = SpriteTextureLoader.getSprite(pPath2);
		icon.sprite = sprite;
		cover.sprite = sprite2;
		((Object)((Component)this).gameObject).name = _book.getAsset().id;
	}

	private void showTooltip()
	{
		Tooltip.show(((Component)this).gameObject, "book", new TooltipData
		{
			book = _book
		});
	}
}
