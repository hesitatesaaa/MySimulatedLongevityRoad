namespace MySimulatedLongevityRoad.Core;

internal enum MclslModuleLoadStatus { Unloaded, Ready, Faulted }

/// <summary>Only a successfully loaded module may replace this world's saved data.</summary>
internal struct MclslModuleLoadState
{
    internal MclslModuleLoadStatus Status { get; private set; }
    private long _generation;
    internal bool CanSave => Status == MclslModuleLoadStatus.Ready
        && _generation == MclslWorldContext.Generation;
    internal void Ready() { _generation = MclslWorldContext.Generation; Status = MclslModuleLoadStatus.Ready; }
    internal void Fault() { _generation = MclslWorldContext.Generation; Status = MclslModuleLoadStatus.Faulted; }
    internal void Clear() { this = default; }
}
