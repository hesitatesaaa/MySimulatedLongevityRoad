// Minimal stubs so Data/MclslEventCatalog.cs compiles outside Unity.
// Only the referenced record type is touched; the tested helpers never use it.
namespace MySimulatedLongevityRoad.Data
{
    internal sealed class MclslRunEventRecord
    {
        public string Category { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public int Importance { get; set; }
    }
}
