using System;

namespace MySimulatedLongevityRoad.Interop;

/// <summary>
/// Public runtime marker for companion localization/name-pack mods.
/// Presence on disk is not enough: companion mods should read IsRuntimeLoaded.
/// </summary>
public static class MclslLocalizationRuntimeMarker
{
	public const int ApiVersion = 1;
	public const string ModId = "shiyue.worldbox.mod.MySimulatedLongevityRoad";
	public const string LocalizationProfile = "mclsl";
	public const string DisplayName = "我的模拟长生路";

	public static bool IsRuntimeLoaded { get; private set; }
	public static DateTime LoadedAtUtc { get; private set; }

	internal static void MarkLoaded()
	{
		IsRuntimeLoaded = true;
		LoadedAtUtc = DateTime.UtcNow;
	}
}
