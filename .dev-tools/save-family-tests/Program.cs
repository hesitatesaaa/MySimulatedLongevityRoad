using System.Reflection;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using Newtonsoft.Json;

int assertions = 0;
void Check(bool condition, string message) { assertions++; if (!condition) throw new Exception(message); }
MclslWorldRunState Run(string id="test",string era="ancient_law") => new() {
    RunId=id, StartYear=100, EraOriginYear=100, AncientLawEndYear=1100,
    TransmissionEndYear=1110, NewLawStartYear=1110, CultivationEpoch=era,
    FiredHistoricalEvents=new(){"epoch_chuanfa_dao"}, PendingAncientCultivatorIds=new(){"17","18"}, TransmissionStepYear=1104
};
void Reset(MclslWorldRunState run=null) {
    MclslWorldArchiveStore.Clear(); MclslWorldRunRepository.ResetWorld(); MclslFamilySystem.Clear();
    World.world=new(); MclslBagSystem.Bags.Clear(); MclslBagSystem.Locked.Clear(); MclslTianxuanMarket.Escrow.Clear();
    MclslTianxuanMarket.Liquidations=0; UnityEngine.Time.unscaledTime=0; MclslWorldBootstrapLane.Completed=true;
    MclslTianxuanMarket.Listings.Clear(); MclslTianxuanMarket.FailRemove=false; MclslAnnualFrameBudget.Remaining=int.MaxValue;
    MclslWorldRunRepository.FailNormalize=false; MclslWorldRunRepository.ClearAwards();
    if(run!=null) { MclslWorldArchiveStore.Load(); MclslWorldRunRepository.ImportArchive(new(){CurrentRun=run}); }
}
Actor Person(long id,long family,bool cultivator=true) {
    var actor=new Actor{data=new(){id=id,family=family},name="族人"+id,realm=cultivator?MclslRealmIds.LianQi:""};
    World.world.units.actors[id]=actor; return actor;
}
void Sweep() { for(int i=0;i<50;i++) MclslFamilySystem.TickReconciliation(); }
void Issue(string account,long amount) => MclslEconomyCommands.Commit(100,new[]{new MclslEconomicOperation(MclslEconomicKind.Issue,MclslCurrency.SpiritStone,amount,toAccount:account)});
long Balance(string account)=>MclslWorldRunRepository.Current.Economy.Balance(account,MclslCurrency.SpiritStone);

foreach(string era in new[]{"ancient_law","transmission_transition","new_law"}) {
    Reset(Run(era,era)); MclslWorldArchiveStore.SaveNow();
    MclslWorldRunRepository.ResetWorld(); MclslWorldArchiveStore.Clear(); MclslWorldArchiveStore.Load();
    var restored=MclslWorldRunRepository.Current;
    Check(restored.RunId==era && restored.CultivationEpoch==era,"era roundtrip");
    Check(restored.AncientLawEndYear==1100 && restored.TransmissionStepYear==1104 && restored.PendingAncientCultivatorIds.SequenceEqual(new[]{"17","18"}),"timeline/queue roundtrip");
    Check(!MclslWorldArchiveStore.WriteLocked,"valid save is writable");
    MclslWorldArchiveStore.Load(); Check(ReferenceEquals(restored,MclslWorldRunRepository.Current),"repeat load is idempotent");
}
Reset();
World.world.map_stats.custom_data.set("mclsl.architecture.v1",JsonConvert.SerializeObject(new MclslWorldArchiveBundle{CurrentRun=Run("primary","new_law")}));
World.world.map_stats.custom_data.set("mclsl.architecture.v1.backup",JsonConvert.SerializeObject(new MclslWorldArchiveBundle{CurrentRun=Run("backup","ancient_law")}));
MclslWorldArchiveStore.Load(); Check(MclslWorldRunRepository.Current.RunId=="primary","primary stops before older backup");
Reset(); World.world.map_stats.custom_data.set("mclsl.architecture.v1","broken-json");
World.world.map_stats.custom_data.set("mclsl.architecture.v1.backup",JsonConvert.SerializeObject(new MclslWorldArchiveBundle{CurrentRun=Run("backup")}));
MclslWorldArchiveStore.Load(); Check(MclslWorldRunRepository.Current.RunId=="backup" && !MclslWorldArchiveStore.WriteLocked,"corrupt primary recovers backup");
Reset(); World.world.map_stats.custom_data.set("mclsl.architecture.v1","broken-json");
World.world.map_stats.custom_data.set("mclsl.architecture.v1.backup","broken-json");
MclslWorldArchiveStore.Load(); MclslWorldRunRepository.EnsureCurrentRun(2000); MclslWorldArchiveStore.SaveNow();
Check(MclslWorldArchiveStore.WriteLocked && MclslWorldRunRepository.Current.RunId=="","corrupt save cannot fabricate a world");
Check(World.world.map_stats.custom_data.values["mclsl.architecture.v1"]=="broken-json","corrupt archive is never overwritten");
Reset(Run("before")); MclslWorldRunRepository.FailNormalize=true;
try { MclslWorldRunRepository.ImportArchive(new(){CurrentRun=Run("bad")}); } catch { }
Check(MclslWorldRunRepository.Current.RunId=="before","failed candidate import is atomic"); MclslWorldRunRepository.FailNormalize=false;
Reset(Run()); MclslRuntimeSettings.TransmissionYear=9999;
MclslWorldRunRepository.Current.FiredHistoricalEvents.Clear(); MclslWorldEpochSystem.NormalizeForTest(MclslWorldRunRepository.Current);
Check(MclslWorldRunRepository.Current.AncientLawEndYear==1100,"saved era is frozen across configuration changes"); MclslRuntimeSettings.TransmissionYear=1000;
Console.WriteLine("PASS archive recovery, roundtrip and anchor-era contracts");

Reset(Run());
var mortal=Person(1,77,false); var founder=Person(2,77); var brother=Person(3,77); var other=Person(4,88);
var wild=Person(900,900);wild.asset.id="mclsl_beast_feiqiu";
MclslFamilySystem.Observe(wild);
Check(MclslFamilySystem.FamilyOf(wild)==null,"ordinary beasts do not enter human family system");
var namedBeast=Person(901,901);namedBeast.asset.id="mclsl_named_diyi";
MclslFamilySystem.Observe(namedBeast);
Check(MclslFamilySystem.FamilyOf(namedBeast)!=null,"named beasts retain character family behavior");
brother.data.parent_id_1=other.data.id;
MclslFamilySystem.Observe(founder); MclslFamilySystem.Observe(other); Sweep();
var family=MclslFamilySystem.FamilyOf(founder);
Check(MclslFamilySystem.FamilyOf(mortal)==family && MclslFamilySystem.FamilyOf(brother)==family,"same native genealogy includes mortals and overrides parent affiliation");
Check(family.Members.Count(x=>x.IsCultivator)==2 && family.Members.Count(x=>!x.IsCultivator)==1,"mortal/cultivator counts");
Issue(MclslEconomyCommands.Account(mortal),100); Issue(MclslEconomyCommands.Account(founder),100);
MclslFamilySystem.TickAnnual(100);
Check(Balance(MclslEconomyCommands.Account(mortal))==100,"mortal pays no cultivation dues");
Check(!MclslFamilySystem.SetHead(family,mortal.data.id) && !MclslFamilySystem.SetFocus(family,mortal.data.id),"mortal is not a cultivating head or aid target");
var bag=MclslBagSystem.Read(founder); MclslBagSystem.Add(bag,"A07",5);
MclslTianxuanMarket.Escrow[founder.data.id]=new(){new(){ItemId="A07",Count=3}};
founder.alive=false; MclslBagSystem.Locked.Add(founder.data.id); MclslFamilySystem.OnDeath(founder);
Check(!family.Members.Find(x=>x.ActorId==founder.data.id).Alive,"inheritance failure immediately marks death");
Check(!family.Members.Find(x=>x.ActorId==founder.data.id).DeathSettled,"failed estate remains retryable");
MclslBagSystem.Locked.Clear(); MclslFamilySystem.TickReconciliation();
long treasury=Balance("family/"+family.Id); int inheritedItems=family.Items.Sum(x=>x.Count);
MclslFamilySystem.OnDeath(founder);
Check(family.Members.Find(x=>x.ActorId==founder.data.id).DeathSettled && inheritedItems==8,"retry moves bag and seller escrow exactly once");
Check(Balance("family/"+family.Id)==treasury && family.Items.Sum(x=>x.Count)==8,"repeat death preserves assets");
brother.alive=false; MclslFamilySystem.OnDeath(brother); Check(!family.Extinct,"living mortal prevents extinction");
MclslFamilySystem.OnDeath(mortal); Check(family.Members.Find(x=>x.ActorId==mortal.data.id).Alive,"averted death cannot mark a living actor dead");
World.world.units.actors.Remove(mortal.data.id); UnityEngine.Time.unscaledTime=10; Sweep();
Check(family.Members.Find(x=>x.ActorId==mortal.data.id).Missing && !family.Extinct,"missing identity is not death or extinction");
World.world.units.actors[mortal.data.id]=mortal; UnityEngine.Time.unscaledTime=20; Sweep();
Check(family.Members.Find(x=>x.ActorId==mortal.data.id).Alive && !family.Members.Find(x=>x.ActorId==mortal.data.id).Missing,"returning mortal recovers status");
mortal.alive=false; MclslFamilySystem.OnDeath(mortal); Check(family.Extinct,"last confirmed death causes extinction");
Console.WriteLine("PASS family identity, mortal economics, death and estate retries");

var split=Run(); split.Families.Add(new(){Id="oldest",NativeFamilyId=77,FoundedYear=1,Members=new(){new(){ActorId=10}},Items=new(){new(){ItemId="A07",Count=2}},History=new(){"old"}});
split.Families.Add(new(){Id="later",NativeFamilyId=77,FoundedYear=2,Members=new(){new(){ActorId=11}},Items=new(){new(){ItemId="A07",Count=3}},History=new(){"later"}});
Reset(split); Person(10,77); Person(11,77); Issue("family/oldest",40); Issue("family/later",60); Sweep();
Check(MclslFamilySystem.Families.Count==1 && Balance("family/oldest")==100 && Balance("family/later")==0,"same-genealogy dedup conserves funds");
Check(split.Families[0].Items.Sum(x=>x.Count)==5 && split.Families[0].History.Contains("later"),"dedup preserves objects and history");
UnityEngine.Time.unscaledTime=10; Sweep(); Check(Balance("family/oldest")==100 && split.Families[0].Items.Sum(x=>x.Count)==5,"dedup repeats without double transfer");
Console.WriteLine("PASS duplicate-family conservation");

Reset(Run()); MclslWorldBootstrapLane.Completed=false;
var stale=new MclslFamilyRecord{Id="pending",Members=new(){new(){ActorId=999}}}; MclslWorldRunRepository.Current.Families.Add(stale);
Sweep(); Check(stale.Members[0].Alive && !stale.Members[0].Missing,"load bootstrap gates missing detection");
MclslWorldBootstrapLane.Completed=true;

var buildKey=typeof(MclslMaterialDiscovery).GetMethod("BuildSourceKey",BindingFlags.Static|BindingFlags.NonPublic)!;
var rollMethod=typeof(MclslMaterialDiscovery).GetMethod("StableRoll",BindingFlags.Static|BindingFlags.NonPublic)!;
var scaled=typeof(MclslMaterialDiscovery).GetMethod("ScaledChance",BindingFlags.Static|BindingFlags.NonPublic)!;
var boundaryActors=new Dictionary<int,Actor>();
for(long id=1;boundaryActors.Count<4 && id<300000;id++) {
    var a=new Actor{data=new(){id=id},realm=MclslRealmIds.LianQi};
    var key=(string)buildKey.Invoke(null,new object[]{"test",a,200,"annual","annual_activity"})!;
    int roll=(int)rollMethod.Invoke(null,new object[]{key,"A07"})!%10000;
    if(new[]{9,10,19,20}.Contains(roll))boundaryActors.TryAdd(roll,a);
}
Check(boundaryActors.Count==4,"found exact deterministic probability boundaries");
foreach(bool mountain in new[]{false,true}) foreach(var (roll,a) in boundaryActors) {
    MclslBagSystem.Bags.Remove(a.data.id); MclslWorldRunRepository.ClearAwards(); a.current_tile.Type.mountains=mountain;
    MclslMaterialDiscovery.TryAnnualActivity(a,200);
    Check(MclslBagSystem.Count(a,"A07")== (roll<(mountain?20:10)?1:0),"A07 10/10000 and 20/10000 exact boundary");
}
var capped=boundaryActors[9]; MclslBagSystem.Bags.Remove(capped.data.id); MclslWorldRunRepository.ClearAwards(); MclslBagSystem.Add(capped,"A07",8);
MclslMaterialDiscovery.TryAnnualActivity(capped,200); Check(MclslBagSystem.Count(capped,"A07")==8,"8-item acquisition ceiling");
Check((int)scaled.Invoke(null,new object[]{MclslMaterialTier.Huang,100})! == 25 && (int)scaled.Invoke(null,new object[]{MclslMaterialTier.Xuan,30})! == 6,"other materials keep prior thresholds");
Check(MclslEconomicPolicy.CanDiscardPublicStock(MclslItemCatalog.Get("A07")),"A07 enters market recycling");
foreach(string id in new[]{"A01","A02","A04","A05","A09","A12","A15","A17","A18","A19"}) Check(!MclslEconomicPolicy.CanDiscardPublicStock(MclslItemCatalog.Get(id)),"other spirit objects keep retention: "+id);
Check(!MclslEconomicPolicy.OrderExpired(109,100) && MclslEconomicPolicy.OrderExpired(110,100),"seller 10-year boundary");
Check(!MclslEconomicPolicy.PublicOrdinaryStockExpired(119,100) && MclslEconomicPolicy.PublicOrdinaryStockExpired(120,100),"public 20-year boundary");
Console.WriteLine("PASS acquisition boundaries, bag cap and selective market expiry");
Reset(Run());
var seller=Person(501,1);
MclslMarketListing Listing(string id,int count,int year,long owner=0,string itemId="A07")=>new(){ListingId=id,Year=year,SellerId=owner,Item=new(){ItemId=itemId,Count=count},ContributionUnitPrice=100};
var publicOld=Listing("public-old",1200,100); var publicRecent=Listing("public-recent",5,101);
var otherSpirit=Listing("other-spirit",10,1,0,"A01"); var sellerOld=Listing("seller-old",3,110,seller.data.id);
MclslTianxuanMarket.Listings.AddRange(new[]{publicOld,publicRecent,otherSpirit,sellerOld});
MclslTianxuanMarket.BeginAnnualPrune(120); MclslAnnualFrameBudget.Remaining=1;
Check(!MclslTianxuanMarket.TickAnnualPrune() && MclslTianxuanMarket.Listings.Count==3,"1200 public A07 units clear within annual operation budget");
Check(MclslTianxuanMarket.Listings.Contains(publicRecent) && MclslTianxuanMarket.Listings.Contains(otherSpirit),"recent A07 and other spirit stocks are retained");
MclslAnnualFrameBudget.Remaining=100;
Check(MclslTianxuanMarket.TickAnnualPrune(),"bounded prune resumes to completion");
Check(MclslBagSystem.Count(seller,"A07")==3 && !MclslTianxuanMarket.Listings.Contains(sellerOld),"zero recycling budget returns whole seller escrow");
Check(MclslWorldRunRepository.Current.JulingMarrowMetrics.GetValueOrDefault("市场/Destroyed")==1200 && MclslWorldRunRepository.Current.JulingMarrowMetrics.GetValueOrDefault("市场/Cancelled")==3,"clear and return counters preserve unit totals");
Reset(Run()); seller=Person(502,1);
var recycle=Listing("recycle",2,100,seller.data.id); MclslTianxuanMarket.Listings.Add(recycle);
MclslTianxuanMarket.BeginAnnualPrune(110); MclslWorldRunRepository.Current.AnnualBatch.MarketRecycleBudgetStones=80;
Check(MclslTianxuanMarket.TickAnnualPrune() && Balance(MclslEconomyCommands.Account(seller))==80,"budgeted seller recycle pays 40 percent reference price");
Check(recycle.SellerId==0 && recycle.Year==110 && recycle.Item.Count==2,"recycled listing becomes public stock with refreshed age");
MclslTianxuanMarket.BeginAnnualPrune(110); MclslTianxuanMarket.TickAnnualPrune();
Check(Balance(MclslEconomyCommands.Account(seller))==80,"same-year retry never pays twice");
MclslTianxuanMarket.BeginAnnualPrune(129); MclslTianxuanMarket.TickAnnualPrune(); Check(MclslTianxuanMarket.Listings.Contains(recycle),"recycled stock survives before 20 years");
MclslTianxuanMarket.BeginAnnualPrune(130); MclslTianxuanMarket.TickAnnualPrune(); Check(!MclslTianxuanMarket.Listings.Contains(recycle),"recycled stock clears at 20 years");
Reset(Run()); seller=Person(503,1); var rollback=Listing("rollback",2,100,seller.data.id); MclslTianxuanMarket.Listings.Add(rollback);
MclslTianxuanMarket.BeginAnnualPrune(110); MclslTianxuanMarket.FailRemove=true;
Check(!MclslTianxuanMarket.TickAnnualPrune() && MclslBagSystem.Count(seller,"A07")==0 && MclslTianxuanMarket.Listings.Contains(rollback),"failed escrow removal rolls back returned bag and retains cursor");
MclslTianxuanMarket.FailRemove=false; Check(MclslTianxuanMarket.TickAnnualPrune() && MclslBagSystem.Count(seller,"A07")==2,"failed return retries without duplicated units");
Console.WriteLine("PASS budgeted market prune, returns, rollback and recycle payments");
UnityEngine.Time.frameCount=100;
var tab=new MySimulatedLongevityRoad.UI.MclslBackpackTabElement(); tab.OnDisable();
Check(tab.Cleanups==0 && tab.Stops==2,"native disable and its virtual clear never synchronously recycle icons");
MySimulatedLongevityRoad.UI.MclslDeferredUiWork.Tick(); Check(tab.Cleanups==0,"same activation frame cannot recycle icons");
UnityEngine.Time.frameCount++; MySimulatedLongevityRoad.UI.MclslDeferredUiWork.Tick();
Check(tab.Cleanups==1,"persistent driver performs one cleanup in safe next frame");
MySimulatedLongevityRoad.UI.MclslDeferredUiWork.Tick(); Check(tab.Cleanups==1,"repeated driver tick cannot recycle twice");
tab.clear(); MySimulatedLongevityRoad.UI.MclslDeferredUiWork.Cancel(tab); UnityEngine.Time.frameCount++;
MySimulatedLongevityRoad.UI.MclslDeferredUiWork.Tick(); Check(tab.Cleanups==1,"reopening cancels stale queued cleanup");
Console.WriteLine("PASS native disable callback chain and deferred UI cleanup");
Console.WriteLine($"PASS {assertions} assertions (game/Unity services stubbed; no in-game acceptance implied).");
