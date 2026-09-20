using System;

[Serializable]
public class RegisterQueue : QueueItem
{
	public string username;

	public string password;

	public string email;

	public string reason;

	public string error;

	public string status;
}
