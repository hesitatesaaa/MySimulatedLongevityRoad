using UnityEngine;

public class SignoutButton : MonoBehaviour
{
	public void tryLogOut()
	{
		Auth.signOut();
		ScrollWindow.get("worldnet_logout").clickHide();
	}
}
