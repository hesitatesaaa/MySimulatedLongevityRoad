using System.IO;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using Newtonsoft.Json;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslReincarnationProfileStore
{
    private static MclslReincarnationProfile _current;
    private static bool _preserveBackup;
    private static string SavePath => Path.Combine(MclslWorldContext.StorageRoot, "ReincarnationProfile.json");
    internal static MclslReincarnationProfile Current { get { EnsureLoaded(); return _current; } }

    internal static void EnsureLoaded()
    {
        if (_current != null) return;
        var result = MclslAtomicFile.Read(SavePath, json =>
        {
            var profile = JsonConvert.DeserializeObject<MclslReincarnationProfile>(json);
            if (profile == null || profile.Version != MclslSaveVersions.ReincarnationProfile)
                throw new InvalidDataException("还真档案格式不属于0.8.0。");
            return profile;
        }, out MclslReincarnationProfile loaded, out string failure);
        if (result == MclslArchiveReadResult.Invalid) throw new InvalidDataException(failure);
        var current = loaded ?? new MclslReincarnationProfile();
        current.KnownKnowledgeIds ??= new();
        current.KnownTechniqueIds ??= new();
        current.KnownTimelineAnchorIds ??= new();
        current.CarrySlotLimit = MclslRuntimeSettings.CarrySlotLimit;
        _preserveBackup = result == MclslArchiveReadResult.Backup;
        _current = current;
    }

    internal static void Flush()
    {
        EnsureLoaded();
        MclslAtomicFile.Write(SavePath, JsonConvert.SerializeObject(_current, Formatting.Indented), _preserveBackup);
        _preserveBackup = false;
    }
}
