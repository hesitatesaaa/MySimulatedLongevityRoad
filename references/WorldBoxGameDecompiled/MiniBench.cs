using System;
using System.Diagnostics;
using UnityEngine;

public readonly struct MiniBench : IDisposable
{
	private readonly string _id;

	private readonly Stopwatch _sw;

	private readonly long _dont_show_below_ms;

	private const string _COLOR_WARN = "<color=yellow>";

	private const string _COLOR_SLOW = "<color=red>";

	private static int MAX_LOG_LENGTH = 38;

	public MiniBench(string pID)
	{
		_id = pID;
		_sw = new Stopwatch();
		_sw.Start();
		_dont_show_below_ms = 0L;
	}

	public MiniBench(string pID, long pDontShowBelowMs)
	{
		_id = pID;
		_sw = new Stopwatch();
		_sw.Start();
		_dont_show_below_ms = pDontShowBelowMs;
	}

	public void Dispose()
	{
		_sw.Stop();
		long elapsedMilliseconds = _sw.ElapsedMilliseconds;
		if (elapsedMilliseconds >= _dont_show_below_ms)
		{
			string text = ((elapsedMilliseconds > 999) ? "<color=red>" : ((elapsedMilliseconds <= 499) ? "" : "<color=yellow>"));
			string text2 = text;
			string text3 = ((elapsedMilliseconds > 499) ? "</color>" : "");
			double totalSeconds = _sw.Elapsed.TotalSeconds;
			if (_id.Length + 2 > MAX_LOG_LENGTH)
			{
				MAX_LOG_LENGTH = _id.Length + 2;
			}
			Debug.Log((object)(Toolbox.fillRight("[" + _id + "]", MAX_LOG_LENGTH) + " = " + text2 + totalSeconds.ToString("F6") + text3));
		}
	}
}
