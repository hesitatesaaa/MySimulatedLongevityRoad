using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;

public enum ActorSex { Male, Female }
public class ActorData { public long id, family, ancestor_family, parent_id_1, parent_id_2; public ActorSex sex; }
public class Actor {
    public ActorAsset asset=new();
    public ActorData data = new(); public bool alive = true; public string name = "族人", realm = "";
    public City city; public WorldTile current_tile = new(); public Dictionary<string, float> stats = new() { ["damage"] = 10, ["health"] = 100 };
    public Dictionary<string, object> custom = new(); public float age = 30;
    public string getName() => name; public float getAge() => age;
}
public class City { public CityData data = new(); }
public class ActorAsset { public string id="human"; }
namespace MySimulatedLongevityRoad.Traits {
    internal static class MclslBeastActorRegistration {
        internal static bool IsOrdinary(Actor actor)=>MclslBeastCatalog.ForAsset(actor?.asset?.id)?.AssetPrefix=="mclsl_beast_";
    }
}
public class CityData { public long id; public string name = "城"; }
public class WorldTile { public TileType Type = new(); }
public class TileType { public string biome_id = ""; public bool mountains, liquid; }
public class UnitManager { public Dictionary<long, Actor> actors = new(); public Actor get(long id) => actors.GetValueOrDefault(id); }
public class SaveCustomData {
    public Dictionary<string, string> values = new();
    public void get(string key, out string value, string fallback) => value = values.GetValueOrDefault(key, fallback);
    public void set(string key, string value) => values[key] = value;
}
public class MapStats { public SaveCustomData custom_data = new(); }
public class MapBox { public static int current_world_seed_id = 42; public MapStats map_stats = new(); public UnitManager units = new(); }
public static class World { public static MapBox world = new(); }
namespace UnityEngine {
    public static class Time { public static float unscaledTime; public static int frameCount; }
    public class GameObject { public Transform transform=new(); public T GetComponent<T>() where T:new()=>new(); }
    public class Transform { public Transform Find(string path)=>null; public T GetComponent<T>() where T:new()=>new(); }
    public static class Debug { public static void Log(object s) {} public static void LogWarning(object s) {} public static void LogError(object s) {} }
}
namespace UnityEngine.UI { public class Button { public ButtonEvent onClick=new(); } public class ButtonEvent { public void RemoveAllListeners() {} } }
public class UnitElement { public virtual void clear() {} public virtual void OnDisable()=>clear(); }
namespace MySimulatedLongevityRoad.UI {
    internal sealed partial class MclslBackpackTabElement {
        private int _buildGeneration; private object _refresh; private bool _building; private long _revision;
        private readonly List<UnityEngine.GameObject> _icons=new(); private readonly List<int>[] _categories={new()};
        internal int Cleanups, Stops; internal void StopAllCoroutines()=>Stops++;
        private void ClearIcons()=>Cleanups++;
    }
}
namespace MySimulatedLongevityRoad.Core {
    internal static class MclslRuntime { internal static int Year = 100; internal static int CurrentYear() => Year; }
    internal static class MclslRuntimeSettings { internal static int TransmissionYear = 1000, CarrySlotLimit = 3; internal static bool ItemAcquisitionHistoryEnabled, RecordLowMaterialAcquisitionHistory; }
    internal static class MclslPerformanceProbe { internal static long Begin() => 0; internal static void End(string s, long l) {} internal static void RecordArchiveBytes(int a, int b) {} }
    internal static class MclslDiagnostics { internal static void Error(string a, string b) {} }
    internal static class MclslActorAccessor {
        internal static bool Alive(Actor a) => a?.data != null && a.alive;
        internal static long Id(Actor a) => a?.data?.id ?? 0;
        internal static bool IsCultivator(Actor a) => !string.IsNullOrEmpty(Realm(a));
        internal static string Realm(Actor a) => a?.realm ?? "";
        internal static string GetString(Actor a, string key, string fallback = "") => a?.custom.GetValueOrDefault(key)?.ToString() ?? fallback;
        internal static int GetInt(Actor a, string key, int fallback = 0) => a?.custom.GetValueOrDefault(key) is int n ? n : fallback;
        internal static long GetMoney(Actor a, string key, long fallback = 0) => MclslWorldRunRepository.Current.Economy.Balance(MclslEconomyCommands.Account(a), MclslCurrency.SpiritStone);
        internal static void Set(Actor a, string key, object value) => a.custom[key] = value;
        internal static string DisplayName(Actor a) => a.name;
        internal static void ApplyDisplayName(Actor a, string realm) {}
    }
}
namespace MySimulatedLongevityRoad.Data {
    internal static class MclslActorDataKeys { internal const string FamilyId="family", TechniqueId="technique", Aptitude="aptitude", SpiritStones="stones"; }
    internal static class MclslSaveVersions { internal const int WorldArchive = 24; }
    internal static class MclslRealmIds {
        internal const string LianQi="炼气", ZhuJi="筑基", JinDan="金丹", YuanYing="元婴", HuaShen="化神", HeDao="合道";
        internal static readonly string[] Ordered = { LianQi, ZhuJi, JinDan, YuanYing, HuaShen, HeDao };
        internal static int Index(string s) => Array.IndexOf(Ordered, s);
    }
    internal class MclslOwnedItem { public string ItemId="", InstanceId=""; public int Count, Durability=100, AcquiredYear; }
    internal class MclslMentorshipBook {}
    internal class MclslBagState { public int Version=1; public List<MclslOwnedItem> Items=new(); public Dictionary<string,string> EquippedArtifactSlots=new(); public List<MclslMentorshipBook> Books=new(); }
    internal class MclslAnnualBatchState {
        public int FamilySettlementYear, FamilyCursor, FamilyMemberCursor;
        public int MarketPruneYear, MarketPruneCursor; public long MarketRecycleSpentStones, MarketRecycleBudgetStones;
        public bool WorldBlocked; public List<string> MarketPruneListingIds=new();
    }
    internal class MclslMarketListing {
        public string ListingId=""; public long SellerId; public MclslOwnedItem Item=new();
        public int Price, ContributionUnitPrice, SpiritStoneUnitPrice, Year;
    }
    internal class MclslDeathRecord { public long ActorId; public int Year; }
    internal class MclslSectRuinRecord { public string Id="ruin"; public int Quality=4; }
    internal class MclslTechniqueLineageRecord { public string Id, Name, MaxRealm, SourceTechniqueId, State, Summary; public int FirstSeenYear; }
    internal class MclslTechniqueDefinition { public string Id, Name, MaxRealm; }
    internal static class MclslCultivationCatalog { internal static List<MclslTechniqueDefinition> Techniques=new(); }
    internal static class MclslHonorificNameCatalog { internal static string StableFamilySurname(string s,long id)=>"陆"; internal static string ApplyFamilySurname(long id,string s,string surname)=>s; }
    internal class MclslReincarnationProfile { public int CurrentCycle=1, CarrySlotLimit=3; public List<string> KnownKnowledgeIds=new(); }
    internal class MclslWorldArchiveBundle { public int Version {get;set;}=24; public MclslWorldRunState CurrentRun {get;set;}=new(); }
    internal class MclslWorldRunState {
        public string RunId {get;set;}=""; public int StartYear {get;set;} public int EraOriginYear {get;set;}
        public int AncientLawEndYear {get;set;} public int TransmissionEndYear {get;set;} public int NewLawStartYear {get;set;}
        public int TransmissionStepYear {get;set;} public int NewLawPioneerStartYear {get;set;}
        public int CycleNumber {get;set;} public int LastProcessedYear {get;set;} public int NextCaveBirthYear {get;set;}
        public bool UsesNativeKingdoms {get;set;} public string CultivationEpoch {get;set;}="ancient_law";
        public List<string> FiredHistoricalEvents {get;set;}=new(); public List<string> PendingAncientCultivatorIds {get;set;}=new();
        public List<string> InheritedKnowledgeIds {get;set;}=new(); public MclslEconomyState Economy {get;set;}=new();
        public List<MclslFamilyRecord> Families {get;set;}=new(); public MclslAnnualBatchState AnnualBatch {get;set;}=new();
        public List<MclslDeathRecord> DeathRecords {get;set;}=new(); public List<MclslTechniqueLineageRecord> TechniqueLineages {get;set;}=new();
        public int FamilyMembershipVersion {get;set;} public Dictionary<string,long> JulingMarrowMetrics {get;set;}=new();
    }
}
namespace MySimulatedLongevityRoad.Systems {
    internal static class MclslEconomyCommands {
        internal static string Account(Actor a)=>"actor/"+a.data.id;
        internal static MclslEconomicResult Commit(int year, IReadOnlyList<MclslEconomicOperation> ops,string source=null,long cursor=0)
            => MclslEconomicTransaction.Commit(MclslWorldRunRepository.Current.Economy,year,ops,source,cursor);
        internal static void NotifyWallet(Actor a) {}
        internal static void CloseDeadActorWallet(Actor a) {
            long stones=MclslWorldRunRepository.Current.Economy.Balance(Account(a),MclslCurrency.SpiritStone);
            Commit(100,new[]{new MclslEconomicOperation(MclslEconomicKind.Destroy,MclslCurrency.SpiritStone,stones,fromAccount:Account(a))},"close/"+Account(a),1);
        }
    }
    internal static class MclslBagSystem {
        internal static Dictionary<long,MclslBagState> Bags=new(); internal static HashSet<long> Locked=new();
        internal static bool IsLocked(Actor a)=>Locked.Contains(a.data.id);
        internal static MclslBagState Read(Actor a) { if(IsLocked(a)) throw new Exception("locked"); return Bags.TryGetValue(a.data.id,out var b)?b:(Bags[a.data.id]=new()); }
        internal static MclslBagState Peek(Actor a)=>Read(a);
        internal static int Count(Actor a,string id)=>Count(Read(a),id);
        internal static int Count(MclslBagState b,string id)=>b.Items.Where(x=>x.ItemId==id).Sum(x=>x.Count);
        internal static void Write(Actor a,MclslBagState b)=>Bags[a.data.id]=b;
        internal static MclslBagState Copy(MclslBagState b)=>new(){Items=b.Items.Select(MclslFamilyTransferPolicy.CopyItem).ToList()};
        internal static void Add(Actor a,string id,int count=1,int acquiredYear=-1)=>Add(Read(a),id,count,acquiredYear);
        internal static void Add(MclslBagState b,string id,int count=1,int acquiredYear=-1) {
            var held=b.Items.Find(x=>x.ItemId==id); if(held==null)b.Items.Add(new(){ItemId=id,Count=count});else held.Count+=count;
        }
    }
    internal static class MclslInventoryDataRules { internal static bool IsEquipped(MclslBagState b,MclslOwnedItem i)=>b.EquippedArtifactSlots.ContainsValue(i.InstanceId); internal static bool IsValid(MclslBagState b)=>true; }
    internal static partial class MclslTianxuanMarket {
        internal static Dictionary<long,List<MclslOwnedItem>> Escrow=new(); internal static int Liquidations;
        internal static List<MclslMarketListing> Listings=new(); private static Dictionary<string,MclslMarketListing> ById=new();
        private static object _snapshot; private static int _revision;
        private const string SpiritStoneCurrency="stones", ContributionCurrency="contribution";
        private static void EnsureIndex() => ById=Listings.ToDictionary(x=>x.ListingId);
        private static void UpdateMarketPrices(int year) {}
        private static int ReferenceStonePrice(MclslItemDefinition item)=>100;
        private static int MarketStonePrice(MclslItemDefinition item)=>100;
        private static int ContributionPrice(MclslItemDefinition item)=>100;
        internal static bool FailRemove;
        private static bool RemoveListing(MclslMarketListing listing) {
            if(FailRemove)return false; ById.Remove(listing.ListingId);return Listings.Remove(listing);
        }
        private static void RecordActivity(string action,MclslItemDefinition item,Actor actor,Actor other,int count,int unit,int total,string currency,int year) {
            if(item?.Id=="A07")MclslJulingMarrowMetrics.Record("市场/"+action,count);
        }
        internal static void ReleaseUnclaimedSellerEscrow(long id,int year) {}
        internal static void TransferSellerEscrowToFamily(long id,List<MclslOwnedItem> items,Action callback) {
            callback(); if(Escrow.Remove(id,out var held))items.AddRange(held);
        }
        internal static bool QueueEstateItems(List<MclslOwnedItem> items,int year,Func<bool> callback) { if(!callback())return false; Liquidations++;return true; }
    }
    internal static class MclslArtifactSystem { internal static void OnArtifactAcquired(Actor a) {} }
    internal static class MclslActorRegistry { internal static bool ResolveKnownOrWorld(long id,out Actor actor) { actor=World.world.units.get(id); return actor!=null; } internal static IReadOnlyList<Actor> Snapshot()=>World.world.units.actors.Values.ToArray(); }
    internal static class MclslWorldBootstrapLane { internal static bool Completed=true; }
    internal static class MclslFrameDeadline { internal static bool Expired; }
    internal static class MclslAnnualFrameBudget { internal static int Remaining=int.MaxValue; internal static bool TryConsumeOperation()=>Remaining-->0; }
    internal static class MclslEligibility { internal static bool CanCultivate(Actor a)=>a!=null; }
    internal static class MclslAnnouncementSystem { internal static void Enqueue(string a,string b,float c,int d) {} }
    internal static class MclslItemAcquisitionHistoryPolicy { internal static bool ShouldRecordMaterial(int t,bool a,bool b)=>false; }
    internal static class MclslReincarnationProfileStore { internal static MclslReincarnationProfile Current=new(); }
    internal static class MclslWorldRunRuntimeIndexes { internal static void Invalidate() {} }
    internal static class MclslNewLawPioneerSystem { internal const int PioneerLeadYears=20; }
    internal static partial class MclslWorldEpochSystem { internal const string AncientLawEpoch="ancient_law"; internal const int TransmissionDurationYears=10; internal static int AncientLawDurationYears=>Core.MclslRuntimeSettings.TransmissionYear; }
    internal static partial class MclslWorldRunRepository {
        private static MclslWorldRunState _current=new(),_ensuredRun,_normalizedRun; private static int _ensuredYear=-1;
        internal static MclslWorldRunState Current=>_current; internal static bool FailNormalize;
        internal static void ResetWorld() { _current=new(); InvalidateCurrentRunCaches(); }
        private static void Normalize() { if(FailNormalize)throw new Exception("bad collections"); MclslWorldEpochSystem.NormalizeForTest(_current); }
        private static void NormalizeCurrentRun()=>Normalize(); private static void EnsureAnchors() {} private static void EnsureAncientTechniqueSeeds(int y) {}
        private static void InvalidateCurrentRunCaches() {_ensuredRun=null;_normalizedRun=null;_ensuredYear=-1;}
        internal static void NotifyMapMarkerDataChanged() {} private static void InvalidateEventIndexes() {} private static void EnsureEventIndexes() {}
        internal static MclslWorldArchiveBundle ExportArchive()=>new(){CurrentRun=_current};
        private static HashSet<string> Awards=new(); internal static void ClearAwards()=>Awards.Clear();
        internal static bool TryClaimMaterialAwardEventKey(string s)=>Awards.Add(s);
        internal static void AddMaterialDiscoveryEvent(int y,string a,string b,Actor actor,string key) {}
        internal static void RecordAnnualFailure(int year,string system,string id,string method,int attempts,string state,string error) {}
    }
}
