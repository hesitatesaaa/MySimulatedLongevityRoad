using MySimulatedLongevityRoad.Data;
using System.Text.Json;

int checks=0;
void Check(bool passed,string description)
{ if(!passed)throw new InvalidOperationException(description);checks++; }
Check(MclslBeastContent.Rows.Length==21,"21 ordinary species");
Check(MclslBeastContent.Items.Length==111,"111 new independent item assets");
Check(MclslBeastContent.Recipes.Length==75,"75 new recipes");
Check(MclslBeastConsumables.All.Length==50,"all 25 pills and 25 talismans share use/purchase definitions");
var marketOffers=new[]{"YD_feiqiu","YD_dafeng","YD_kunpeng","YF_feiqiu","YM_feiqiu","YB_feiqiu"};
var needed=MclslBeastConsumables.SelectMarketNeeds(marketOffers,x=>x.Grade==1&&x.Kind=="pill",_=>0,id=>id=="YD_dafeng"?1:10);
Check(needed.SequenceEqual(new[]{"YD_dafeng"}),"same-effect purchase chooses one cheaper tier-valid actual offer");
needed=MclslBeastConsumables.SelectMarketNeeds(new[]{"YD_feiqiu"},x=>x.Grade==1&&x.Kind=="pill",id=>id=="YD_dafeng"?1:0,_=>1);
Check(needed.Length==0,"owned equivalent satisfies demand even when its ID is not on the market");
needed=MclslBeastConsumables.SelectMarketNeeds(new[]{"YD_feiqiu","YD_kunpeng"},x=>x.Kind=="pill",_=>0,_=>1);
Check(needed.SequenceEqual(new[]{"YD_kunpeng"}),"highest useful grade is preferred");
needed=MclslBeastConsumables.SelectMarketNeeds(new[]{"YD_feiqiu","YF_feiqiu"},_=>true,_=>0,_=>1);
Check(needed.Length==2,"pill and talisman needs stay separate");
Check(MclslBeastConsumables.SelectMarketNeeds(new[]{"YM_feiqiu","YB_feiqiu"},_=>true,_=>0,_=>1).Length==0,"materials and artifacts are excluded from consumable demand");
Check(MclslBeastConsumables.SelectMarketNeeds(Array.Empty<string>(),_=>true,_=>0,_=>1).Length==0,"empty market never manufactures supply");
Check(MclslBeastConsumables.SelectMarketNeeds(marketOffers,_=>false,_=>0,_=>1).Length==0,"unneeded or cooldown-blocked consumables produce no demand");
Check(MclslBeastContent.Rows.Select(x=>x.Species).Order().SequenceEqual(MclslBeastCatalog.All.Select(x=>x.Id).Order()),"every ordinary species covered");
Check(MclslBeastContent.Items.Select(x=>x.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count()==111,"unique item identities");
Check(MclslBeastContent.Items.Select(x=>x.IconPath).Distinct().Count()==111,"independent resource paths");
foreach(var artifact in MclslBeastContent.Items.Where(x=>x.Category=="Artifact"))
    Check(artifact.NativeStats.Select(x=>x.Id).Distinct().Count()==artifact.NativeStats.Length,"unique native stat keys: "+artifact.Id);
Check(Math.Abs(MclslItemCatalog.Get("YB_feiqiu").NativeStats.Single(x=>x.Id=="multiplier_speed").Value-0.20f)<0.00001f,"floating boots combine base and signature movement bonuses");
Check(Math.Abs(MclslItemCatalog.Get("YB_qilin").NativeStats.Single(x=>x.Id=="multiplier_health").Value-0.40f)<0.00001f,"qilin amulet combines base and signature health bonuses");
Check(MclslBeastContent.Rows.Single(x=>x.Species=="zhuque").MaterialId=="M25","reuse canonical fire feather");
Check(MclslItemCatalog.All.Count(x=>x.Name=="朱雀火羽")==1,"no duplicate fire feather");
Check(MclslItemCatalog.Get("M25").MaterialSources==MclslMaterialSource.BeastKill,"fire feather acquired through kills");
Check(MclslItemCatalog.All.Count(x=>x.MaterialTier!=MclslMaterialTier.None&&x.MaterialSources!=MclslMaterialSource.BeastKill)<=64,"discovery bitmask remains valid");
foreach(var recipe in MclslBeastContent.Recipes)
{
    var product=MclslItemCatalog.Get(recipe.ProductId);
    Check(product!=null&&product.Grade==recipe.Grade,"recipe grade "+recipe.Id);
    Check(MclslRecipeCatalog.Get(product.Id)==recipe,"single authoritative recipe "+recipe.Id);
    Check(recipe.Ingredients.All(x=>x.Count>0&&MclslItemCatalog.Get(x.ItemId)!=null),"ingredients exist "+recipe.Id);
    Check(recipe.Ingredients.All(x=>(int)MclslItemCatalog.Get(x.ItemId).MaterialTier<=recipe.Grade),"craftable ingredient grade "+recipe.Id);
    Check(recipe.Profession==(product.Category=="Pill"?"alchemist":product.Category=="Talisman"?"talisman":"refiner"),"profession mapping "+recipe.Id);
    Check(MclslRecipeCatalog.Describe(product.Id).Contains('×'),"quantities shown "+recipe.Id);
    Check(!string.IsNullOrWhiteSpace(product.EffectText),"concrete effect "+recipe.Id);
}
foreach(var row in MclslBeastContent.Rows)
{
    Check(MclslRecipeCatalog.Get(row.ArtifactId).Ingredients.Single(x=>x.ItemId==row.MaterialId).Count==2,"artifact signature count "+row.Species);
    Check(MclslRecipeCatalog.Get(row.ArtifactId).Ingredients.Single(x=>x.ItemId==MclslBeastContent.Common("bone",row.Grade)).Count==2,"artifact bone count "+row.Species);
    Check(MclslItemCatalog.Get(row.MaterialId).MaterialSources==MclslMaterialSource.BeastKill,"no annual signature supply "+row.Species);
    Check(MclslItemCatalog.NormalizeId("mclsl_artifact_"+row.ArtifactId.ToLowerInvariant())==row.ArtifactId,"native artifact canonical identity "+row.Species);
}
var duplicate=new MclslRecipeDefinition("test",1,"refiner",new("M01",2),new("M01",3),new("M02",1));
Check(duplicate.Ingredients.Length==2&&duplicate.Ingredients.Single(x=>x.ItemId=="M01").Count==5,"duplicate material quantities aggregated");
var legacy=MclslRecipeCatalog.Get("D001");
Check(legacy.Ingredients.Length==2&&legacy.Ingredients.All(x=>x.Count==1),"legacy recipe preserved");
var stock=new Dictionary<string,int>{{"YM_blood_2",1},{"YM_core_2",1},{"A14",1}};
var resolved=MclslRecipeCatalog.Get("YD_common_2").Resolve(id=>stock.GetValueOrDefault(id),id=>id);
Check(resolved!=null&&resolved.Single(x=>x.ItemId=="YM_blood_2").Count==1&&resolved.Single(x=>x.ItemId=="YM_core_2").Count==1,"mixed blood/core consumption");
Check(stock["YM_core_2"]==1,"preflight is nonmutating");
stock["A14"]=0;
Check(MclslRecipeCatalog.Get("YD_common_2").Resolve(id=>stock.GetValueOrDefault(id),id=>id)==null,"missing third ingredient rejects complete recipe");
var aliases=new MclslRecipeDefinition("alias",0,"refiner",new("R03",1),new("R04",1));
Check(aliases.Resolve(_=>1,_=>"common_metals")==null,"native aliases cannot spend one resource twice");
Check(aliases.Resolve(_=>2,_=>"common_metals").Single().Count==2,"native aliases aggregated for withdrawal");
int[] tiers={1,1,2,3,4,4,4};
for(int i=0;i<7;i++)Check(MclslBeastContent.MaterialGrade(i)==tiers[i],"death realm material tier "+i);
for(int ceiling=1;ceiling<=7;ceiling++)
{
    var observed=new int[ceiling];
    for(int roll=0;roll<MclslBeastRules.Weights.Take(ceiling).Sum();roll++)observed[MclslBeastRules.WeightedLevel(ceiling,roll)-1]++;
    Check(observed.SequenceEqual(MclslBeastRules.Weights.Take(ceiling)),"exact weighted distribution for unlocked ceiling "+ceiling);
    int count=MclslBeastRules.PlannedCount(ceiling,1,512,0,0);
    var roles=Enumerable.Range(0,count).Select(i=>MclslBeastRules.Role(i,count)).ToArray();
    Check(roles.Count(x=>x==2)==1,"one boss at level "+ceiling);
    Check(roles.Count(x=>x==1)==Math.Ceiling((count-1)*0.25),"elite composition at level "+ceiling);
}
Check(MclslBeastRules.PlannedCount(7,10,512,500,12)==0,"full reserved capacity rejects another wave");
Check(MclslBeastRules.PlannedCount(7,10,512,500,10)==2,"truncated capacity keeps only two slots");
Check(MclslBeastRules.Role(0,1)==2,"one-slot wave preserves boss");
Check(MclslBeastRules.NaturalChance(200000)==80000,"common regional arrival is reduced by 60 percent");
Check(MclslBeastRules.NaturalChance(1000000)==400000,"conditional guaranteed rare roll retains a 40 percent probability");
foreach(var beast in MclslBeastCatalog.All)
    Check(MclslBeastRules.NaturalChance(beast.ChancePerMillion)==beast.ChancePerMillion*0.4,"preserve species rarity ratios: "+beast.Id);
Check(MclslBeastRules.PlannedCount(7,10,4096,0,0,1048576,64)==64,"large-map maximum multiplier obeys single-wave cap");
Check(MclslBeastRules.PlannedCount(7,1,512,0,0,16384)==18,"small map reduces default wave size");
Check(MclslBeastRules.PlannedCount(7,1,512,0,0,262144)==56,"large map retains bounded scale");
Check(MclslBeastRules.PlannedCount(1,1,512,0,0,65536,1)==1,"one-unit setting keeps a single boss");
Check(MclslBeastRules.NaturalChance(-1)==0 && MclslBeastRules.NaturalChance(int.MaxValue)==400000,"natural probability input is bounded");
if(args.Length>0)
{
    string destination=Path.GetFullPath(args[0]);Directory.CreateDirectory(destination);
    var icons=MclslBeastContent.Items.Select(item=>new {id=item.Id,name=item.Name,category=item.Category,grade=item.Grade,tier=(int)item.MaterialTier,path="GameResources/"+item.IconPath+".png",
        prompt="Independent transparent 64x64 shaded pixel-art inventory icon for Chinese cultivation game 我的模拟长生路: "+item.Name+". Category "+item.Category+"; tier "+Math.Max(item.Grade,(int)item.MaterialTier)+". Match the provided existing material/pill/talisman/artifact reference. Unique silhouette and meaningful source motifs; fine dark outline, small highlights, no lettering, no border, no shared recolored base. Generate one asset only.",status="pending-imagegen"});
    var export=new {schemaVersion=1,targetVersion="0.6.0",icons,recipes=MclslBeastContent.Recipes.Select(x=>new {id=x.Id,product=x.ProductId,profession=x.Profession,grade=x.Grade,output=x.OutputCount,ingredients=x.Ingredients.Select(y=>new {id=y.ItemId,count=y.Count,alternatives=y.Alternatives})}),
        species=MclslBeastContent.Rows.Select(x=>new {id=x.Species,minRealm=x.MinRealm,grade=x.Grade,material=x.MaterialId,pill=x.PillId,talisman=x.TalismanId,artifact=x.ArtifactId})};
    string manifestPath=Path.Combine(destination,"beast-content.manifest.json");
    var exportNode=System.Text.Json.Nodes.JsonNode.Parse(JsonSerializer.Serialize(export)).AsObject();
    if(File.Exists(manifestPath))
    {
        var previous=System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(manifestPath));
        var previousIcons=previous?["icons"]?.AsArray().Where(x=>x?["id"]!=null).ToDictionary(x=>x["id"].GetValue<string>());
        foreach(var icon in exportNode["icons"].AsArray())
            if(previousIcons!=null&&previousIcons.TryGetValue(icon["id"].GetValue<string>(),out var old)
                &&new[]{"name","category","grade","tier","path","prompt"}.All(key=>System.Text.Json.Nodes.JsonNode.DeepEquals(icon[key],old[key])))
                foreach(string field in new[]{"status","review","provenance","sha256"})
                    if(old[field]!=null)icon[field]=old[field].DeepClone();
    }
    File.WriteAllText(manifestPath,exportNode.ToJsonString(new JsonSerializerOptions{WriteIndented=true}));
    var newIds=MclslBeastContent.Items.Select(x=>x.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
    var originalIcons=MclslItemCatalog.All.Where(x=>!newIds.Contains(x.Id)&&x.Category!="SpellScroll")
        .Select(x=>new {id=x.Id,name=x.Name,category=x.Category,path="GameResources/"+x.IconPath+".png"});
    File.WriteAllText(Path.Combine(destination,"beast-original-icons.manifest.json"),JsonSerializer.Serialize(originalIcons,new JsonSerializerOptions{WriteIndented=true}));
}
Console.WriteLine($"PASS: {checks} assertions against actual production catalog and recipe source. Offline logic checks only; no Unity or performance acceptance.");
