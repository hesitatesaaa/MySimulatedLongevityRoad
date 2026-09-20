using System;
using UnityEngine;

public class LogHandlerInit : MonoBehaviour
{
	private void Awake()
	{
		try
		{
			LogHandler.init();
		}
		catch (Exception ex)
		{
			Debug.LogError((object)ex);
			throw;
		}
	}
}
