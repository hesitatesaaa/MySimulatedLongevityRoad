namespace MySimulatedLongevityRoad.Core;

internal enum MclslWorldInitializationStatus { Waiting, Initializing, Ready, Faulted }

// Failed initialization may have committed gameplay changes. Only a new world
// load/clear boundary may authorize another attempt, never the next Update.
internal struct MclslWorldInitialization
{
    internal MclslWorldInitializationStatus Status { get; private set; }
    internal bool NeedsAttempt => Status == MclslWorldInitializationStatus.Waiting;
    internal bool IsReady => Status == MclslWorldInitializationStatus.Ready;
    internal bool TryBegin()
    {
        if (!NeedsAttempt) return false;
        Status = MclslWorldInitializationStatus.Initializing;
        return true;
    }
    internal void Complete() { if (Status == MclslWorldInitializationStatus.Initializing) Status = MclslWorldInitializationStatus.Ready; }
    internal void Fail() => Status = MclslWorldInitializationStatus.Faulted;
    internal void Reset() => Status = MclslWorldInitializationStatus.Waiting;
}
