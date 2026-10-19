using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Traits;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Profiling;

namespace MySimulatedLongevityRoad.Systems;

// Diagnostic-only source, injected into a separate test mod checkout. Not shipped.
internal static class MclslBeastRuntimeProbe
{
    private static readonly string Output=Environment.GetEnvironmentVariable("MCLSL_BEAST_RUNTIME_PROBE");
    private static readonly List<object> Morphs=new(),Atlases=new(),Waves=new(),Crafts=new(),Deaths=new(),Uses=new(),Trades=new();
    private static readonly List<string> Errors=new();
    private static readonly List<long> Placed=new(),Killed=new();
    private static int Stage,SpeciesCursor,AtlasCursor,WaveLevel=1,LootAttempts,CraftCursor;
    private static double MapAt=-1,Started;
    private static Actor Killer;
    private static Actor PendingSpecies,PendingVictim;
    private static double PendingAt;
    private static bool KillerReady;
    private static bool VictimHit;
    private static bool Done;
    private static Actor Seller,Buyer;
    private static long SavedKillerId;
    private static string SaveFolder,ExpectedBag,ExpectedRecipes;
    private static object DiskSave;
    private static bool MarketReady;
    private static int SavedWaveCursor;
    private static long[] SavedWaveActors;
    private static readonly string[] SpellIds={"S001","S004","S007","S008","S009","S010","S012","S013","S015","S016","S017","S019","S021","S022","S023","S026"};
    private static readonly string[] Products={"YD_feiqiu","YF_feiqiu","YB_feiqiu"};
    internal static void Tick()
    {
        if(Done||string.IsNullOrWhiteSpace(Output))return;
        try
        {
            Config.paused=Stage!=2&&Stage!=3;
            if(World.world==null||SmoothLoader.isLoading()||Time.realtimeSinceStartup<90)return;
            if(MapAt<0)
            {
                Started=Time.realtimeSinceStartup;MapAt=Started;
                if(Environment.GetEnvironmentVariable("MCLSL_BEAST_RUNTIME_PROBE_FOCUS")=="closure")Stage=20;
                World.world.generateNewMap();Write("fresh-map-requested");return;
            }
            if(Time.realtimeSinceStartup-MapAt<10)return;
            if(Stage==0)
            {
                if(Killer==null)
                {
                    Killer=Spawn("human",false,0);
                    PendingAt=Time.realtimeSinceStartup;return;
                }
                if(!KillerReady)
                {
                    Killer.setStatsDirty();Killer.updateStats();
                    if(Killer.getMaxHealth()<=0)
                    {if(Time.realtimeSinceStartup-PendingAt>10)throw new InvalidOperationException("Human native stats never initialized");return;}
                    MclslActorAccessor.Set(Killer,MclslActorDataKeys.CultivationSystem,MclslCultivationSystemIds.AncientLaw);
                    MclslCultivationStateTransitions.TrySetRealm(Killer,MclslRealmIds.ChangSheng,MclslRuntime.CurrentYear(),"runtime diagnostic");
                    Killer.setStatsDirty();Killer.updateStats();Killer.restoreHealthPercent(1);KillerReady=true;
                }
                if(SpeciesCursor<MclslBeastCatalog.All.Length)
                {
                    var definition=MclslBeastCatalog.All[SpeciesCursor];
                    if(PendingSpecies==null){PendingSpecies=Spawn(definition.BeastAssetId,definition.AnimalTemplate=="piranha",SpeciesCursor+1);PendingAt=Time.realtimeSinceStartup;return;}
                    PendingSpecies.setStatsDirty();PendingSpecies.updateStats();
                    if(PendingSpecies.getMaxHealth()<=0)
                    {if(Time.realtimeSinceStartup-PendingAt>10)throw new InvalidOperationException("Beast native stats never initialized: "+definition.Id);return;}
                    InspectSpecies(definition,PendingSpecies);PendingSpecies=null;SpeciesCursor++;return;
                }
                Stage=1;Write("21-species-morph-inspected");
            }
            if(Stage==1)
            {
                if(AtlasCursor<48)
                {
                    string id=SpellIds[AtlasCursor/3],phase=new[]{"cast","travel","hit"}[AtlasCursor%3];
                    int count=MclslSpellEffectFrames.Count(id,phase);var first=MclslSpellEffectFrames.Get(id,0,phase);
                    var last=MclslSpellEffectFrames.Get(id,count-1,phase);
                    bool valid=first!=null&&last!=null&&first.texture.width==768&&first.texture.height==((count+3)/4)*192
                        &&first.rect.width==192&&last.rect.height==192;
                    Atlases.Add(new {id,phase,frames=count,valid,width=first?.texture.width,height=first?.texture.height,firstRect=first?.rect.ToString(),lastRect=last?.rect.ToString()});
                    if(!valid)Errors.Add("stage atlas failed: "+id+"/"+phase);AtlasCursor++;return;
                }
                MclslBeastWorld.Enabled=false;MclslBeastWorld.CountMultiplier=1;Stage=2;Write("48-atlases-loaded-in-unity");
            }
            if(Stage==2)
            {
                if(!MclslBeastHabitatIndex.Refresh(MclslRuntime.CurrentYear(),false))return;
                if(WaveLevel<=7)
                {
                    if(MclslBeastWorld.Waves.Count<WaveLevel)
                        if(!MclslBeastWorld.Trigger(WaveLevel,MclslRuntime.CurrentYear()+WaveLevel)){Errors.Add("wave trigger failed: "+WaveLevel);WaveLevel++;return;}
                    var wave=MclslBeastWorld.Waves.Last();int before=wave.Cursor;
                    MclslBeastWorld.Tick();
                    if(wave.Cursor-before>4)Errors.Add("wave frame budget exceeded");
                    if(wave.Status=="生成中")return;
                    var actors=wave.Actors.Select(id=>{MclslActorRegistry.ResolveKnownOrWorld(id,out var actor);return actor;}).Where(x=>x!=null).ToArray();
                    int bosses=actors.Count(x=>MclslActorAccessor.GetInt(x,MclslBeastWorld.RankKey)==2);
                    bool valid=actors.All(x=>MclslBeastActorRegistration.IsOrdinary(x)&&!x.isSapient()
                        &&MclslRealmIds.Index(MclslActorAccessor.Realm(x))>=MclslBeastContent.BySpecies[MclslBeastCatalog.ForAsset(x.asset.id).Id].MinRealm);
                    Waves.Add(new {level=WaveLevel,wave.Id,wave.Theme,planned=wave.Count,spawned=wave.Actors.Count,resolvedNow=actors.Length,bossesAliveNow=bosses,valid,spawnedActorIds=wave.Actors.ToArray(),
                        units=actors.Select(x=>new {id=MclslActorAccessor.Id(x),asset=x.asset.id,realm=MclslActorAccessor.Realm(x),rank=MclslActorAccessor.GetInt(x,MclslBeastWorld.RankKey),x=x.current_tile.pos.x,y=x.current_tile.pos.y}).ToArray()});
                    if(!valid)Errors.Add("wave validation failed: "+WaveLevel);WaveLevel++;Write("wave-"+(WaveLevel-1)+"-spawned");return;
                }
                World.world.generateNewMap();MapAt=Time.realtimeSinceStartup;
                Killer=null;KillerReady=false;Stage=20;Write("fresh-map-for-loot-and-crafting");return;
            }
            if(Stage==20)
            {
                if(Killer==null){Killer=Spawn("human",false,0);PendingAt=Time.realtimeSinceStartup;return;}
                Killer.setStatsDirty();Killer.updateStats();
                if(Killer.getMaxHealth()<=0)return;
                Killer.data.age_overgrowth=100;
                Killer.addTrait("gifts_6");
                MclslActorAccessor.Set(Killer,MclslActorDataKeys.CultivationSystem,MclslCultivationSystemIds.AncientLaw);
                MclslCultivationStateTransitions.TrySetRealm(Killer,MclslRealmIds.JinDan,MclslRuntime.CurrentYear(),"runtime diagnostic");
                Killer.setStatsDirty();Killer.updateStats();Killer.restoreHealthPercent(1);Stage=3;return;
            }
            if(Stage==3)
            {
                if(PendingVictim!=null)
                {
                    if(VictimHit)
                    {
                        if(PendingVictim.isAlive()&&Time.realtimeSinceStartup-PendingAt<5)return;
                        bool finalCommitted=MclslActorAccessor.GetInt(PendingVictim,"mclsl.beast.loot_committed")==1;
                        long finalId=MclslActorAccessor.Id(PendingVictim);
                        Deaths.Add(new {victimId=finalId,aliveAfter=PendingVictim.isAlive(),deathRouteCommitted=finalCommitted});
                        if(finalCommitted)Killed.Add(finalId);
                        PendingVictim=null;VictimHit=false;return;
                    }
                    PendingVictim.setStatsDirty();PendingVictim.updateStats();
                    if(PendingVictim.getMaxHealth()<=0)
                    {if(Time.realtimeSinceStartup-PendingAt>10)throw new InvalidOperationException("Loot victim stats never initialized");return;}
                    long victimId=MclslActorAccessor.Id(PendingVictim);int healthBefore=PendingVictim.getHealth();
                    Killer.setAttackTarget(PendingVictim);PendingVictim.setAttackTarget(Killer);
                    MclslSpellSystem.DeliverDamage(Killer,PendingVictim,1000000000,AttackType.Divine);
                    VictimHit=true;PendingAt=Time.realtimeSinceStartup;return;
                }
                var bag=MclslBagSystem.Read(Killer);
                if((MclslBagSystem.Count(bag,"YM_feiqiu")<4||MclslBagSystem.Count(bag,"YM_bone_1")<2)&&LootAttempts<64)
                {
                    PendingVictim=Spawn("mclsl_beast_feiqiu",false,100+LootAttempts++);PendingAt=Time.realtimeSinceStartup;
                    return;
                }
                if(MclslBagSystem.Count(bag,"YM_feiqiu")<4||MclslBagSystem.Count(bag,"YM_bone_1")<2)
                    Errors.Add("real kill loot insufficient after 64 attempts");
                foreach(string auxiliary in new[]{"A08","F01","M01"})MclslBagSystem.Add(bag,auxiliary,1,acquiredYear:MclslRuntime.CurrentYear());
                MclslBagSystem.Write(Killer,bag);Stage=4;Write("manual-beast-real-kill-loot-observed");
            }
            if(Stage==4)
            {
                Config.paused=true;
                if(CraftCursor<Products.Length)
                {
                    string product=Products[CraftCursor];var recipe=MclslRecipeCatalog.Get(product);int year=MclslRuntime.CurrentYear()+CraftCursor+1;
                    MclslActorAccessor.Set(Killer,MclslActorDataKeys.Profession,recipe.Profession);
                    typeof(MclslProfessionSystem).GetMethod("SyncTrait",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{Killer,recipe.Profession});
                    MclslActorAccessor.Set(Killer,MclslActorDataKeys.ProfessionGrade,1);
                    MclslActorAccessor.Set(Killer,MclslActorDataKeys.KnownRecipes,"v1:"+product);
                    MclslActorAccessor.Set(Killer,MclslActorDataKeys.InventoryPlanYear,year);
                    MclslActorAccessor.Set(Killer,MclslActorDataKeys.InventoryPlanRecipeId,product);
                    MclslActorAccessor.Set(Killer,MclslActorDataKeys.ProfessionLastCraftYear,year-1);
                    var method=typeof(MclslProfessionSystem).GetMethod("TryCraft",BindingFlags.Static|BindingFlags.NonPublic);
                    bool crafted=(bool)method.Invoke(null,new object[]{Killer,recipe.Profession,year});
                    Crafts.Add(new {product,crafted,count=MclslBagSystem.Count(Killer,product),casterAlive=Killer.isAlive(),realm=MclslActorAccessor.Realm(Killer),grade=MclslProfessionSystem.GetGrade(Killer),
                        profession=MclslActorAccessor.GetString(Killer,MclslActorDataKeys.Profession),traitProfession=MclslProfessionSystem.FromTrait(Killer),known=MclslActorAccessor.GetString(Killer,MclslActorDataKeys.KnownRecipes),
                        ingredients=recipe.Ingredients.Select(x=>new {x.ItemId,x.Count,remaining=MclslBagSystem.Count(Killer,x.ItemId)}).ToArray()});
                    if(!crafted)Errors.Add("actual craft failed: "+product);CraftCursor++;return;
                }
                Stage=5;
            }
            if(Stage==5)
            {
                foreach(string id in Products.Take(2))
                {
                    int before=MclslBagSystem.Count(Killer,id);
                    bool used=MclslItemUseSystem.TryUse(Killer,id);
                    int after=MclslBagSystem.Count(Killer,id);
                    Uses.Add(new {id,used,before,after});
                    if(!used||after!=before-1)Errors.Add("consumable use failed: "+id);
                }
                var artifact=MclslBagSystem.Read(Killer).Items.FirstOrDefault(x=>x.ItemId==Products[2]);
                bool equipped=artifact!=null&&MclslArtifactSystem.TryEquip(Killer,artifact.InstanceId,true);
                Uses.Add(new {id=Products[2],equipped,slots=MclslBagSystem.Read(Killer).EquippedArtifactSlots});
                if(!equipped)Errors.Add("artifact equip failed");
                Stage=6;Write("real-kill-craft-use-equip-inspected");return;
            }
            if(Stage==6)
            {
                if(Seller==null){Seller=Spawn("human",false,50);Buyer=Spawn("human",false,70);return;}
                Seller.setStatsDirty();Seller.updateStats();Buyer.setStatsDirty();Buyer.updateStats();
                if(Seller.getMaxHealth()<=0||Buyer.getMaxHealth()<=0)return;
                int year=Math.Max(1,MclslRuntime.CurrentYear())+10;
                if(!MarketReady)
                {
                    var stock=MclslBagSystem.Read(Seller);
                    foreach(string id in Products.Concat(new[]{"YM_feiqiu"}))MclslBagSystem.Add(stock,id,5,acquiredYear:year-2);
                    MclslBagSystem.Write(Seller,stock);
                    MclslActorAccessor.Set(Buyer,MclslActorDataKeys.SpiritStones,1000000L);
                    MclslArtifactSystem.OnArtifactAcquired(Seller);MarketReady=true;
                }
                MclslTianxuanMarket.TryListSurplus(Seller,year);
                foreach(string id in Products.Concat(new[]{"YM_feiqiu"}))
                {
                    var listing=MclslTianxuanMarket.Snapshot().FirstOrDefault(x=>x.SellerId==MclslActorAccessor.Id(Seller)&&x.Item.ItemId==id);
                    long sellerBefore=MclslActorAccessor.GetMoney(Seller,MclslActorDataKeys.SpiritStones),buyerBefore=MclslActorAccessor.GetMoney(Buyer,MclslActorDataKeys.SpiritStones);
                    int carry=MclslActorAccessor.GetInt(Seller,MclslActorDataKeys.MarketFeeCarryStones);
                    int expectedFee=listing==null?0:MclslEconomicPolicy.SaleServiceFee(listing.SpiritStoneUnitPrice,carry,out _);
                    bool bought=listing!=null&&MclslTianxuanMarket.TryBuy(Buyer,listing.ListingId,year,1,"SpiritStone");
                    long paid=buyerBefore-MclslActorAccessor.GetMoney(Buyer,MclslActorDataKeys.SpiritStones),received=MclslActorAccessor.GetMoney(Seller,MclslActorDataKeys.SpiritStones)-sellerBefore;
                    Trades.Add(new {id,listed=listing!=null,bought,paid,received,expectedFee,buyerCount=MclslBagSystem.Count(Buyer,id),instance=listing?.Item?.InstanceId,durability=listing?.Item?.Durability});
                    if(!bought||paid<=0||paid-expectedFee!=received)Errors.Add("real market trade failed: "+id);
                }
                Stage=60;return;
            }
            if(Stage==60)
            {
                if(!MclslBeastHabitatIndex.Refresh(MclslRuntime.CurrentYear(),false))return;
                MclslBeastWorld.Enabled=false;
                if(!MclslBeastWorld.Trigger(1,MclslRuntime.CurrentYear()+20))throw new InvalidOperationException("Cannot create pending-wave save fixture");
                Config.paused=false;MclslBeastWorld.Tick();Config.paused=true;
                var pendingWave=MclslBeastWorld.Waves.Last();SavedWaveCursor=pendingWave.Cursor;SavedWaveActors=pendingWave.Actors.ToArray();
                SaveFolder=Path.Combine(Path.GetDirectoryName(Path.GetFullPath(Output)),Path.GetFileNameWithoutExtension(Output)+"-save");
                SavedKillerId=MclslActorAccessor.Id(Killer);ExpectedBag=MclslActorAccessor.GetString(Killer,MclslActorDataKeys.QiankunBag);
                ExpectedRecipes=MclslActorAccessor.GetString(Killer,MclslActorDataKeys.KnownRecipes);
                Directory.CreateDirectory(SaveFolder);SaveManager.saveWorldToDirectory(SaveFolder,true,true);
                Stage=7;MapAt=Time.realtimeSinceStartup;Write("real-map-saved-to-disk");World.world.save_manager.loadWorld(SaveFolder,false);return;
            }
            if(Stage==7)
            {
                bool actorRestored=MclslActorRegistry.ResolveKnownOrWorld(SavedKillerId,out Killer)&&Killer!=null;
                bool bagEqual=actorRestored&&ExpectedBag==MclslActorAccessor.GetString(Killer,MclslActorDataKeys.QiankunBag);
                bool recipesEqual=actorRestored&&ExpectedRecipes==MclslActorAccessor.GetString(Killer,MclslActorDataKeys.KnownRecipes);
                bool mapOnDisk=File.Exists(Path.Combine(SaveFolder,"map.wbox"));
                var wave=MclslBeastWorld.Waves.LastOrDefault();
                bool waveEqual=wave!=null&&wave.Cursor==SavedWaveCursor&&wave.Actors.SequenceEqual(SavedWaveActors);
                DiskSave=new {path=SaveFolder,mapOnDisk,actorRestored,bagEqual,recipesEqual,pendingWaveEqual=waveEqual,beforeCursor=SavedWaveCursor,afterCursor=wave?.Cursor,savedWaveActorIds=SavedWaveActors};
                if(!mapOnDisk||!actorRestored||!bagEqual||!recipesEqual||!waveEqual)Errors.Add("real disk save/load comparison failed");
                Stage=8;Write("disk-save-restored-pending-wave");return;
            }
            if(Stage==8)
            {
                Config.paused=false;MclslBeastWorld.Tick();
                var wave=MclslBeastWorld.Waves.Last();
                if(wave.Status=="生成中")return;
                bool resumed=wave.Actors.Count==24&&wave.Actors.Distinct().Count()==24&&SavedWaveActors.All(x=>wave.Actors.Contains(x));
                if(!resumed)Errors.Add("pending wave resume failed");
                DiskSave=new {saved=DiskSave,pendingWaveResumed=resumed,finalCursor=wave.Cursor,finalActorIds=wave.Actors.ToArray()};
                Done=true;Write("controlled-integration-completed");Application.Quit();
            }
        }
        catch(Exception ex){Errors.Add(ex.ToString());Done=true;Write("runtime-probe-failed");Application.Quit();}
    }
    private static Actor Spawn(string asset,bool water,int offset)
    {
        WorldTile selected=null;int candidate=0;
        for(int y=4;y<MapBox.height-4&&selected==null;y+=2)
            for(int x=4;x<MapBox.width-4;x+=2)
            {var tile=World.world.GetTile(x,y);if(tile?.Type!=null&&tile.Type.liquid==water&&candidate++>=offset){selected=tile;break;}}
        if(selected==null)throw new InvalidOperationException("No legal "+(water?"water":"land")+" placement tile");
        var actor=World.world.units.createNewUnit(asset,selected,false,0f,null,null,true,false,false,false);
        if(actor?.data==null)throw new InvalidOperationException("Actor placement failed: "+asset);
        Placed.Add(MclslActorAccessor.Id(actor));return actor;
    }
    private static void InspectSpecies(MclslBeastDefinition definition,Actor actor)
    {
        bool water=definition.AnimalTemplate=="piranha";
        long id=MclslActorAccessor.Id(actor);
        MclslBeastActorRegistration.EnsureCultivationSeed(actor);
        MclslBeastCultivation.SetRealm(actor,MclslRealmIds.LianQi,MclslRuntime.CurrentYear());
        bool animal=!actor.isSapient()&&!actor.asset.civ&&!actor.asset.auto_civ&&actor.city==null;
        actor.data.health=Math.Max(1,(int)(actor.getMaxHealth()*0.5f));float before=(float)actor.getHealth()/actor.getMaxHealth();
        actor.setAttackTarget(Killer);
        MclslBeastCultivation.SetRealm(actor,MclslRealmIds.YuanYing,MclslRuntime.CurrentYear());
        bool human=!actor.isSapient()&&!actor.asset.civ&&!actor.asset.auto_civ&&actor.city==null&&actor.asset.id==definition.AscendedAssetId;
        bool identity=MclslActorAccessor.Id(actor)==id&&actor.attack_target==Killer;
        float after=(float)actor.getHealth()/actor.getMaxHealth();
        MclslBeastCultivation.SetRealm(actor,MclslRealmIds.LianQi,MclslRuntime.CurrentYear());
        bool reverted=actor.asset.id==definition.BeastAssetId;
        Morphs.Add(new {species=definition.Id,actorId=id,animal,human,identity,reverted,before,after,healthRatioPreserved=before>0.4f&&after>0.4f&&Math.Abs(before-after)<0.02f,
            placementTile=new[]{actor.current_tile.pos.x,actor.current_tile.pos.y},terrainWater=actor.current_tile.Type.liquid});
        if(!animal||!human||!identity||!reverted||before<=0.4f||after<=0.4f||Math.Abs(before-after)>=0.02f)Errors.Add("morph failed: "+definition.Id);
    }
    private static void Write(string status)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(Output)));
        File.WriteAllText(Output,JsonConvert.SerializeObject(new {source="unity-runtime",kind="controlled-beast-integration-probe",status,
            capturedAtUtc=DateTime.UtcNow,unityVersion=Application.unityVersion,graphicsDevice=SystemInfo.graphicsDeviceName,
            elapsedSeconds=Time.realtimeSinceStartup-Started,pausedControlledExecution=false,simulationAdvancedBetweenSteps=true,saveTestScope="new-test-map-and-workspace-disk-save",
            auxiliaryMaterialsSeededByDiagnostic=new[]{"A08","F01","M01"},placedActorIds=Placed,killedActorIds=Killed,
            placedOrdinarySpeciesIds=Morphs.Count==21?MclslBeastCatalog.All.Select(x=>x.Id).ToArray():Array.Empty<string>(),morphs=Morphs,atlases=Atlases,waves=Waves,crafts=Crafts,uses=Uses,trades=Trades,diskSave=DiskSave,deaths=Deaths,lootAttempts=LootAttempts,errors=Errors,
            marketFixtureInventorySeeded=new[]{"YM_feiqiu","YD_feiqiu","YF_feiqiu","YB_feiqiu"},
            casterAlive=Killer?.isAlive(),casterRealm=Killer==null?null:MclslActorAccessor.Realm(Killer),casterBag=Killer==null?null:MclslBagSystem.Peek(Killer).Items,
            cpuUnityAllocatedBytes=Profiler.GetTotalAllocatedMemoryLong(),gpuMemoryBytes=(long?)null,
            gameplayCasesPassed=false,performance1005001000="pending",scopeLimitation="Controlled real-engine scenarios only. Independent marketplace fixture uses seeded stock. No full spell behavior, normal battle benchmark, long-run FPS, GPU usage or release certification."},Formatting.Indented));
        Debug.Log("[MCLSL beast runtime] "+status);
    }
}
