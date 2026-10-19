using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using Newtonsoft.Json;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

// Diagnostic-only exhaustive recipe and inventory integration, never shipped.
internal static class MclslBeastRecipeProbe
{
    private static readonly string Output=Environment.GetEnvironmentVariable("MCLSL_BEAST_RECIPE_PROBE");
    private static readonly List<object> Icons=new(),Recipes=new(),Uses=new();
    private static readonly List<string> Errors=new();
    private static Actor Artisan,Enemy;
    private static int Stage,Cursor,UseCursor;
    private static double MapAt=-1,Started;
    private static bool Done;
    private static readonly BindingFlags Flags=BindingFlags.Static|BindingFlags.NonPublic;
    private static readonly string[] Realms={"",MclslRealmIds.ZhuJi,MclslRealmIds.JinDan,MclslRealmIds.YuanYing,MclslRealmIds.HuaShen};
    private static readonly int[] Ages={0,30,100,200,600};
    internal static void Tick()
    {
        if(Done||string.IsNullOrEmpty(Output))return;
        try
        {
            Config.paused=true;
            if(World.world==null||SmoothLoader.isLoading()||Time.realtimeSinceStartup<90)return;
            if(MapAt<0){Started=Time.realtimeSinceStartup;MapAt=Started;World.world.generateNewMap();Write("fresh-map-requested");return;}
            if(Time.realtimeSinceStartup-MapAt<10)return;
            if(Stage==0){Artisan=Spawn("human",0);Enemy=Spawn("mclsl_beast_feiqiu",2);Stage=1;return;}
            if(Stage==1)
            {
                Artisan.setStatsDirty();Artisan.updateStats();Enemy.setStatsDirty();Enemy.updateStats();
                if(Artisan.getMaxHealth()<=0||Enemy.getMaxHealth()<=0)return;
                Artisan.data.age_overgrowth=100;Artisan.addTrait("gifts_6");
                MclslCultivationStateTransitions.TrySetCultivationSystem(Artisan,MclslCultivationSystemIds.NewLaw);
                MclslBeastWorld.Enabled=false;Stage=2;
            }
            if(Stage==2)
            {
                if(Cursor<MclslBeastContent.Items.Length)
                {
                    var item=MclslBeastContent.Items[Cursor++];var sprite=SpriteTextureLoader.getSprite(item.IconPath);
                    bool valid=sprite?.texture!=null&&sprite.texture.width==64&&sprite.texture.height==64;
                    Icons.Add(new {item.Id,valid,width=sprite?.texture?.width,height=sprite?.texture?.height});
                    if(!valid)Errors.Add("registered icon failed: "+item.Id);return;
                }
                Cursor=0;Stage=3;Write("all-111-registered-icons-loaded");
            }
            if(Stage==3)
            {
                if(Cursor<MclslBeastContent.Recipes.Length)
                {
                    var recipe=MclslBeastContent.Recipes[Cursor++];int year=Math.Max(1,MclslRuntime.CurrentYear())+Cursor;
                    Artisan.data.age_overgrowth=Ages[recipe.Grade];
                    MclslCultivationStateTransitions.TrySetRealm(Artisan,Realms[recipe.Grade],MclslRuntime.CurrentYear(),"recipe diagnostic");
                    typeof(MclslProfessionSystem).GetMethod("SyncTrait",Flags).Invoke(null,new object[]{Artisan,recipe.Profession});
                    MclslActorAccessor.Set(Artisan,MclslActorDataKeys.Profession,recipe.Profession);
                    MclslActorAccessor.Set(Artisan,MclslActorDataKeys.ProfessionGrade,recipe.Grade);
                    MclslActorAccessor.Set(Artisan,MclslActorDataKeys.KnownRecipes,"v1:"+recipe.ProductId);
                    MclslActorAccessor.Set(Artisan,MclslActorDataKeys.InventoryPlanYear,year);
                    MclslActorAccessor.Set(Artisan,MclslActorDataKeys.InventoryPlanRecipeId,recipe.ProductId);
                    MclslActorAccessor.Set(Artisan,MclslActorDataKeys.ProfessionLastCraftYear,year-1);
                    foreach(var slot in MclslArtifactSystem.EquipmentSlots)MclslArtifactSystem.TryUnequip(Artisan,slot);
                    var bag=new MclslBagState();
                    foreach(var ingredient in recipe.Ingredients)MclslBagSystem.Add(bag,ingredient.ItemId,ingredient.Count,acquiredYear:year-1);
                    string missing=recipe.Ingredients.Last().ItemId;MclslBagSystem.Remove(bag,missing,1);MclslBagSystem.Write(Artisan,bag);
                    var before=recipe.Ingredients.Select(x=>MclslBagSystem.Count(Artisan,x.ItemId)).ToArray();
                    var craft=typeof(MclslProfessionSystem).GetMethod("TryCraft",Flags);
                    bool insufficient=(bool)craft.Invoke(null,new object[]{Artisan,recipe.Profession,year});
                    bool atomic=!insufficient&&before.SequenceEqual(recipe.Ingredients.Select(x=>MclslBagSystem.Count(Artisan,x.ItemId)))&&MclslBagSystem.Count(Artisan,recipe.ProductId)==0;
                    bag=MclslBagSystem.Read(Artisan);MclslBagSystem.Add(bag,missing,1,acquiredYear:year-1);MclslBagSystem.Write(Artisan,bag);
                    bool crafted=(bool)craft.Invoke(null,new object[]{Artisan,recipe.Profession,year});
                    bool exact=recipe.Ingredients.All(x=>MclslBagSystem.Count(Artisan,x.ItemId)==0)&&MclslBagSystem.Count(Artisan,recipe.ProductId)==1;
                    var item=MclslItemCatalog.Get(recipe.ProductId);bool equipped=true;
                    if(item.Category=="Artifact")
                    {
                        var owned=MclslBagSystem.Read(Artisan).Items.FirstOrDefault(x=>x.ItemId==item.Id);
                        equipped=owned!=null&&MclslArtifactSystem.TryEquip(Artisan,owned.InstanceId,true)&&MclslArtifactSystem.EquippedArtifactId(Artisan,item.EquipmentSlot)==item.Id;
                    }
                    Recipes.Add(new {recipe.ProductId,recipe.Grade,recipe.Profession,insufficientAtomic=atomic,crafted,exactConsumed=exact,equipped,
                        age=Artisan.getAge(),realm=MclslActorAccessor.Realm(Artisan),actualGrade=MclslProfessionSystem.GetGrade(Artisan)});
                    if(!atomic||!crafted||!exact||!equipped)Errors.Add("recipe integration failed: "+recipe.ProductId);return;
                }
                Stage=4;Write("all-75-recipes-inspected");
            }
            if(Stage==4)
            {
                if(UseCursor<MclslBeastConsumables.All.Length)
                {
                    var definition=MclslBeastConsumables.All[UseCursor++];
                    Artisan.data.age_overgrowth=Ages[definition.Grade];
                    MclslCultivationStateTransitions.TrySetRealm(Artisan,Realms[definition.Grade],MclslRuntime.CurrentYear(),"consumable diagnostic");
                    MclslActorAccessor.Set(Artisan,"mclsl.beast.pill_due","");MclslActorAccessor.Set(Artisan,"mclsl.beast.talisman_due","");MclslActorAccessor.Set(Artisan,"mclsl.mana_restore.last_time","");
                    foreach(var status in Artisan.getStatusesIds().Where(x=>x.StartsWith("mclsl_beast_",StringComparison.Ordinal)).ToArray())Artisan.finishStatusEffect(status);
                    Artisan.setStatsDirty();Artisan.updateStats();Artisan.data.health=Math.Max(1,Artisan.getMaxHealth()/4);
                    MclslActorAccessor.Set(Artisan,MclslActorDataKeys.ManaInitialized,1);MclslActorAccessor.Set(Artisan,MclslActorDataKeys.ManaCurrent,MclslSpellSystem.MaxMana(Artisan)/4);
                    Artisan.setAttackTarget(Enemy);Enemy.setAttackTarget(Artisan);
                    var bag=new MclslBagState();MclslBagSystem.Add(bag,definition.Id,1,acquiredYear:MclslRuntime.CurrentYear());MclslBagSystem.Write(Artisan,bag);
                    bool used=MclslItemUseSystem.TryUse(Artisan,definition.Id),consumed=MclslBagSystem.Count(Artisan,definition.Id)==0;
                    Uses.Add(new {definition.Id,used,consumed});if(!used||!consumed)Errors.Add("consumable manual use failed: "+definition.Id);return;
                }
                Stage=50;Write("all-50-consumables-used");return;
            }
            if(Stage==50)
            {
                // The paused fresh map has not initialized its cached native
                // calendar year. Advance the real engine before creating a
                // positive previous-year inventory fixture.
                Config.paused=false;
                if(MclslRuntime.CurrentYear()<1)return;
                Stage=5;return;
            }
            if(Stage==5)
            {
                Artisan.data.age_overgrowth=Ages[1];MclslCultivationStateTransitions.TrySetRealm(Artisan,MclslRealmIds.ZhuJi,MclslRuntime.CurrentYear(),"automatic inventory diagnostic");
                Artisan.setStatsDirty();Artisan.updateStats();Artisan.data.health=Math.Max(1,Artisan.getMaxHealth()/4);
                MclslActorAccessor.Set(Artisan,"mclsl.beast.pill_due","");
                int year=MclslRuntime.CurrentYear();var bag=new MclslBagState();MclslBagSystem.Add(bag,"YD_common_1",1,acquiredYear:year);MclslBagSystem.Write(Artisan,bag);
                bool freshBlocked=!MclslItemUseSystem.TryUseBeastAutomatic(Artisan,"YD_common_1",MclslConsumableUseTrigger.Combat)&&MclslBagSystem.Count(Artisan,"YD_common_1")==1;
                bool gradeBlocked=!MclslConsumableInventoryPolicy.CanUse(Artisan,MclslItemCatalog.Get("YD_common_4"),year-1,year,MclslConsumableUseTrigger.Combat);
                bag=MclslBagSystem.Read(Artisan);MclslBagSystem.Add(bag,"YD_common_1",1,acquiredYear:year-1);MclslBagSystem.Write(Artisan,bag);
                bool matureUsed=MclslItemUseSystem.TryUseBeastAutomatic(Artisan,"YD_common_1",MclslConsumableUseTrigger.Combat)&&MclslBagSystem.Count(Artisan,"YD_common_1")==1;
                float speed=AssetManager.status.get("mclsl_equipped_artifact_YB_feiqiu")?.base_stats.get("multiplier_speed")??-1;
                float health=AssetManager.status.get("mclsl_equipped_artifact_YB_qilin")?.base_stats.get("multiplier_health")??-1;
                bool statMerge=Math.Abs(speed-0.2f)<0.00001f&&Math.Abs(health-0.4f)<0.00001f;
                Uses.Add(new {scenario="automatic-inventory-policy",worldYear=year,freshBlocked,gradeBlocked,matureUsed});Uses.Add(new {scenario="artifact-native-stat-merge",speed,health,passed=statMerge});
                if(!freshBlocked||!gradeBlocked||!matureUsed||!statMerge)Errors.Add("inventory age/grade or native artifact stat integration failed");
                Done=true;Write("recipe-probe-completed");Application.Quit();
            }
        }
        catch(Exception ex){Errors.Add(ex.ToString());Done=true;Write("recipe-probe-failed");Application.Quit();}
    }
    private static Actor Spawn(string id,int offset)
    {
        int n=0;for(int y=30;y<MapBox.height-4;y++)for(int x=30;x<MapBox.width-4;x++)
        {var tile=World.world.GetTileSimple(x,y);if(tile?.Type!=null&&!tile.Type.liquid&&n++>=offset)return World.world.units.createNewUnit(id,tile,false,0f,null,null,true,false,false,false);}
        throw new InvalidOperationException("No legal diagnostic tile");
    }
    private static void Write(string status)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Output));File.WriteAllText(Output,JsonConvert.SerializeObject(new {source="unity-runtime",kind="controlled-recipe-integration",status,
            capturedAtUtc=DateTime.UtcNow,unityVersion=Application.unityVersion,elapsedSeconds=Time.realtimeSinceStartup-Started,
            placedActorIds=new[]{MclslActorAccessor.Id(Artisan),MclslActorAccessor.Id(Enemy)},icons=Icons,recipes=Recipes,uses=Uses,errors=Errors,gameplayCasesPassed=false,
            fixture="Paused native engine; materials, recipe knowledge and professions explicitly seeded. Annual year arguments advance per recipe. Verifies production consumption, inventory, statuses and equipment; not natural profession acquisition or AI battle/performance."},Formatting.Indented));
        Debug.Log("[MCLSL recipe runtime] "+status);
    }
}
