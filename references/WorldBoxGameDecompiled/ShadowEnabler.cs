using UnityEngine;
using UnityEngine.UI;

public class ShadowEnabler : MonoBehaviour
{
	public Shadow[] shadowObjects = (Shadow[])(object)new Shadow[0];

	private bool isEnabled;

	private void Awake()
	{
		shadowObjects = ((Component)this).GetComponentsInChildren<Shadow>(true);
	}

	private void Update()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		bool flag = ((Component)this).transform.localScale.y == 1f;
		if (isEnabled != flag)
		{
			isEnabled = flag;
			toggle();
		}
	}

	private void OnDisable()
	{
		isEnabled = false;
		toggle();
	}

	private void OnEnable()
	{
		isEnabled = false;
		toggle();
	}

	private void toggle()
	{
		for (int i = 0; i < shadowObjects.Length; i++)
		{
			Shadow val = shadowObjects[i];
			if (!((Object)(object)val == (Object)null))
			{
				((Behaviour)val).enabled = isEnabled;
			}
		}
	}
}
