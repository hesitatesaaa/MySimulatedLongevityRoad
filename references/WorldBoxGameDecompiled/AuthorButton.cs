using UnityEngine;

public class AuthorButton : MonoBehaviour
{
	public string authorId;

	private void Awake()
	{
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	public void showWorldNetAuthorListWindow()
	{
	}
}
