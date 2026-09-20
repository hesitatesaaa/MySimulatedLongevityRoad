using System;

public interface IWorldBoxAd
{
	Action adResetCallback { get; set; }

	Action adFailedCallback { get; set; }

	Action adFinishedCallback { get; set; }

	Action adStartedCallback { get; set; }

	Action<string> logger { get; set; }

	void Reset();

	void RequestAd();

	void KillAd();

	bool IsReady();

	void ShowAd();

	bool HasAd();

	bool IsInitialized();

	string GetProviderName();

	string GetColor();
}
