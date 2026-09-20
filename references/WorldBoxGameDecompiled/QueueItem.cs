using System;
using UnityEngine;

[Serializable]
public class QueueItem
{
	public object timestamp;

	public string salt;

	public string version;

	public string identifier;

	public string language;

	public string platform;

	public int progress;

	public QueueItem()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		salt = RequestHelper.salt;
		version = Application.version;
		identifier = Application.identifier;
		language = LocalizedTextManager.instance.language;
		platform = ((object)Application.platform/*cast due to constrained. prefix*/).ToString();
		base._002Ector();
	}
}
