namespace MySimulatedLongevityRoad.Modules;

internal abstract class MclslModuleBase
{
    internal virtual string Name => GetType().Name;
    internal virtual int Order => 100;
    internal virtual bool RunsWhenCoreDisabled => false;
    internal virtual bool HasLoadRecovery => false;
    internal virtual bool HasAnnualStep => false;
    internal virtual void Init() { }
    internal virtual void OnWorldLoaded(int year) { }
    internal virtual void TickRealtime() { }
    internal virtual void TickFrame(int frameCounter) { }
    internal virtual void TickAnnual(int year) { }
    internal virtual void TickLoadRecovery(int year) { }
    internal virtual void PrepareForSave() { }
    internal virtual void Clear() { }
}
