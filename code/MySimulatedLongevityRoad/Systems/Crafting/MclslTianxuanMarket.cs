using System;
using System.Collections.Generic;
using System.Linq;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslTianxuanMarket
{
    private const int MaxMarketListings = 1000;
    private const int MaxActivityRecords = 256;
    private static List<MclslMarketListing> Listings => MclslWorldRunRepository.Current.TianxuanListings ??=
        new List<MclslMarketListing>();
    private static List<MclslMarketActivity> Activities => MclslWorldRunRepository.Current.TianxuanActivities ??=
        new List<MclslMarketActivity>();

    private static MclslWorldRunState _indexedRun;
    private static List<MclslMarketListing> _indexedListings;
    private static readonly Dictionary<string, MclslMarketListing> ById = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, List<MclslMarketListing>> ByItem = new(StringComparer.Ordinal);
    private static readonly List<string> DesiredItemIdsScratch = new(8);
    private static MclslMarketListing[] _snapshot;
    private static MclslMarketActivity[] _listingActivitySnapshot;
    private static MclslMarketActivity[] _purchaseActivitySnapshot;

    internal static void ClearRuntime()
    {
        _indexedRun = null;
        _indexedListings = null;
        ById.Clear();
        ByItem.Clear();
        _snapshot = null;
        InvalidateActivitySnapshots();
    }

    private static void EnsureIndex()
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        List<MclslMarketListing> listings = run.TianxuanListings ??= new();
        if (ReferenceEquals(run, _indexedRun) && ReferenceEquals(listings, _indexedListings)
            && listings.Count == ById.Count) return;
        ById.Clear();
        ByItem.Clear();
        foreach (MclslMarketListing listing in listings)
        {
            if (listing?.Item == null || string.IsNullOrEmpty(listing.ListingId)) continue;
            ById[listing.ListingId] = listing;
            if (!ByItem.TryGetValue(listing.Item.ItemId, out List<MclslMarketListing> matches))
                ByItem[listing.Item.ItemId] = matches = new();
            matches.Add(listing);
        }
        _indexedRun = run;
        _indexedListings = listings;
        _snapshot = null;
    }

    private static void AddListing(MclslMarketListing listing)
    {
        EnsureIndex();
        Listings.Add(listing);
        ById[listing.ListingId] = listing;
        if (!ByItem.TryGetValue(listing.Item.ItemId, out List<MclslMarketListing> matches))
            ByItem[listing.Item.ItemId] = matches = new();
        matches.Add(listing);
        _snapshot = null;
    }

    private static bool RemoveListing(MclslMarketListing listing)
    {
        EnsureIndex();
        if (!Listings.Remove(listing)) return false;
        ById.Remove(listing.ListingId);
        if (ByItem.TryGetValue(listing.Item.ItemId, out List<MclslMarketListing> matches))
        {
            matches.Remove(listing);
            if (matches.Count == 0) ByItem.Remove(listing.Item.ItemId);
        }
        _snapshot = null;
        return true;
    }

    internal static IReadOnlyList<MclslMarketListing> Snapshot()
    {
        EnsureIndex();
        return _snapshot ??= Listings.ToArray();
    }

    internal static IReadOnlyList<MclslMarketActivity> ListingActivitySnapshot()
    {
        RefreshActivitySnapshots();
        return _listingActivitySnapshot;
    }

    internal static IReadOnlyList<MclslMarketActivity> PurchaseActivitySnapshot()
    {
        RefreshActivitySnapshots();
        return _purchaseActivitySnapshot;
    }

    private static void RefreshActivitySnapshots()
    {
        if (_listingActivitySnapshot != null && _purchaseActivitySnapshot != null) return;
        List<MclslMarketActivity> activities = Activities;
        _listingActivitySnapshot = activities.Where(x => x != null && x.Action == "Listed")
            .OrderByDescending(x => x.Year).Take(MaxActivityRecords).ToArray();
        _purchaseActivitySnapshot = activities.Where(x => x != null && x.Action == "Purchased")
            .OrderByDescending(x => x.Year).Take(MaxActivityRecords).ToArray();
    }

    private static void InvalidateActivitySnapshots()
    {
        _listingActivitySnapshot = null;
        _purchaseActivitySnapshot = null;
    }

    private static void RecordActivity(string action, MclslItemDefinition item, Actor actor, Actor other,
        int price, int year)
    {
        Activities.Add(new MclslMarketActivity
        {
            Action = action,
            ItemId = item.Id,
            ActorId = MclslActorAccessor.Id(actor),
            ActorName = SafeName(actor),
            OtherActorId = MclslActorAccessor.Id(other),
            OtherActorName = other == null ? string.Empty : SafeName(other),
            Count = 1,
            Price = price,
            Year = year
        });
        if (Activities.Count > MaxActivityRecords)
            Activities.RemoveRange(0, Activities.Count - MaxActivityRecords);
        InvalidateActivitySnapshots();
    }

    private static string SafeName(Actor actor)
    {
        try { return MclslActorAccessor.DisplayName(actor); }
        catch { return "无名修士"; }
    }

    internal static bool TryListSurplus(Actor actor, int year)
    {
        if (!MclslActorAccessor.Alive(actor) || Listings.Count >= MaxMarketListings) return false;
        if (string.IsNullOrEmpty(MclslActorAccessor.GetString(actor, MclslActorDataKeys.QiankunBag))) return false;
        MclslBagState bag = MclslBagSystem.Read(actor);
        int bestArtifact = 0;
        foreach (MclslOwnedItem owned in bag.Items)
        {
            MclslItemDefinition definition = MclslItemCatalog.Get(owned.ItemId);
            if (definition?.Category == "Artifact") bestArtifact = Math.Max(bestArtifact, definition.Grade);
        }
        for (int i = 0; i < bag.Items.Count; i++)
        {
            MclslOwnedItem owned = bag.Items[i];
            MclslItemDefinition item = MclslItemCatalog.Get(owned.ItemId);
            if (item == null) continue;
            int reserve = Reserve(actor, item, bestArtifact);
            if (MclslBagSystem.Count(bag, item.Id) <= reserve) continue;
            MclslOwnedItem offer = new() { ItemId = item.Id, Count = 1, Durability = owned.Durability,
                InstanceId = owned.InstanceId };
            if (item.Category == "Artifact") bag.Items.Remove(owned);
            else if (!MclslBagSystem.Remove(bag, item.Id)) continue;
            MclslBagSystem.Write(actor, bag);
            AddListing(new MclslMarketListing { ListingId = Guid.NewGuid().ToString("N"), SellerId = MclslActorAccessor.Id(actor),
                Item = offer, Price = item.Price, Year = year });
            RecordActivity("Listed", item, actor, null, item.Price, year);
            MclslWorldArchiveStore.MarkDirty();
            return true;
        }
        return false;
    }

    internal static bool TryBuy(Actor buyer, string listingId, int year = -1)
    {
        if (!MclslActorAccessor.Alive(buyer) || string.IsNullOrEmpty(listingId)) return false;
        EnsureIndex();
        if (!ById.TryGetValue(listingId, out MclslMarketListing listing)) return false;
        MclslItemDefinition item = MclslItemCatalog.Get(listing?.Item?.ItemId);
        if (item == null) return false;
        long buyerId = MclslActorAccessor.Id(buyer);
        if (buyerId == listing.SellerId || listing.Price < 0) return false;
        int funds = MclslActorAccessor.GetInt(buyer, MclslActorDataKeys.Contribution);
        if (funds < listing.Price) return false;
        if (!MclslActorRegistry.ResolveKnownOrWorld(listing.SellerId, out Actor seller) || !MclslActorAccessor.Alive(seller))
        {
            RemoveListing(listing);
            MclslWorldArchiveStore.MarkDirty();
            return false;
        }
        int sellerFunds = MclslActorAccessor.GetInt(seller, MclslActorDataKeys.Contribution);
        if (sellerFunds > 999999 - listing.Price) return false;
        // A listing is removed before settlement on the single game thread, preventing a second claim.
        if (!RemoveListing(listing)) return false;
        MclslBagState bag = MclslBagSystem.Read(buyer);
        if (item.Category == "Artifact") bag.Items.Add(listing.Item);
        else MclslBagSystem.Add(bag, listing.Item.ItemId);
        MclslBagSystem.Write(buyer, bag);
        MclslActorAccessor.Set(buyer, MclslActorDataKeys.Contribution, funds - listing.Price);
        MclslActorAccessor.Set(seller, MclslActorDataKeys.Contribution, sellerFunds + listing.Price);
        if (item.Category == "Artifact") MclslArtifactSystem.TryEquipBest(buyer);
        RecordActivity("Purchased", item, buyer, seller, listing.Price, year < 0 ? MclslRuntime.CurrentYear() : year);
        MclslWorldArchiveStore.MarkDirty();
        return true;
    }

    internal static void TryBuyNeeded(Actor actor, int year)
    {
        if (!MclslActorAccessor.Alive(actor)) return;
        EnsureIndex();
        if (Listings.Count == 0) return;
        MclslBagState bag = MclslBagSystem.Peek(actor);
        long actorId = MclslActorAccessor.Id(actor);
        List<string> desired = DesiredItemIdsScratch;
        desired.Clear();
        try
        {
            BuildNeeds(actor, bag, desired);
            foreach (string itemId in desired)
            {
                if (!ByItem.TryGetValue(itemId, out List<MclslMarketListing> offers)) continue;
                MclslMarketListing best = null;
                int funds = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Contribution);
                for (int i = 0; i < offers.Count; i++)
                {
                    MclslMarketListing offer = offers[i];
                    if (offer.SellerId == actorId || offer.Price > funds) continue;
                    if (best == null || offer.Price < best.Price || (offer.Price == best.Price && offer.Year < best.Year)) best = offer;
                }
                if (best != null && TryBuy(actor, best.ListingId, year)) return; // bounded to one purchase per yearly actor pass
            }
        }
        finally { desired.Clear(); }
    }

    private static void BuildNeeds(Actor actor, MclslBagState bag, List<string> desired)
    {
        string profession = MclslActorAccessor.GetString(actor, MclslActorDataKeys.Profession);
        int professionGrade = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ProfessionGrade);
        if (profession == MclslProfessionSystem.Alchemist || profession == MclslProfessionSystem.TalismanMaker)
        {
            if (professionGrade == 0)
                AddIfMissing(desired, bag, profession == MclslProfessionSystem.Alchemist ? "A08" : "F01");
            else
            {
                string category = profession == MclslProfessionSystem.Alchemist ? "Pill" : "Talisman";
                MclslItemDefinition recipe = MclslItemCatalog.All.FirstOrDefault(x => x.Category == category && x.Grade == professionGrade);
                if (recipe != null)
                {
                    AddIfMissing(desired, bag, recipe.IngredientA);
                    AddIfMissing(desired, bag, recipe.IngredientB);
                }
            }
        }

        int realm = MclslRealmIds.Index(MclslActorAccessor.Realm(actor));
        int maxGrade = Math.Clamp(realm + 1, 1, 4);
        string breakthrough = MclslActorAccessor.Realm(actor) switch
        {
            MclslRealmIds.LianQi => "D001", MclslRealmIds.ZhuJi => "D002",
            MclslRealmIds.JinDan => "D003", MclslRealmIds.YuanYing => "D012", _ => string.Empty
        };
        if (breakthrough.Length > 0 && MclslItemCatalog.Get(breakthrough).Grade <= maxGrade)
            AddIfMissing(desired, bag, breakthrough);

        if (MclslActorAccessor.GetInt(actor, "mclsl.v020.taishang_taken") == 0 && maxGrade >= 2)
            AddIfMissing(desired, bag, "D004");
        if (realm >= 0 && realm <= MclslRealmIds.Index(MclslRealmIds.HuaShen) && maxGrade >= 3)
            AddIfMissing(desired, bag, "D005");
        if (!actor.hasMaxHealth())
            AddIfMissing(desired, bag, maxGrade >= 3 ? "D007" : "D006");
        if (actor.equipment?.weapon != null && actor.equipment.weapon.isEmpty()
            && !bag.Items.Any(x => MclslItemCatalog.Get(x.ItemId)?.Category == "Artifact"))
            AddBestGradeItems(desired, maxGrade, "Artifact");
        if (!bag.Items.Any(x => MclslItemCatalog.Get(x.ItemId)?.Category == "Talisman"))
            AddBestGradeItems(desired, maxGrade, "Talisman");
    }

    private static void AddIfMissing(List<string> desired, MclslBagState bag, string itemId)
    {
        if (!string.IsNullOrEmpty(itemId) && MclslItemCatalog.Get(itemId) != null
            && MclslBagSystem.Count(bag, itemId) == 0 && !desired.Contains(itemId)) desired.Add(itemId);
    }

    private static void AddBestGradeItems(List<string> desired, int maxGrade, string category)
    {
        for (int grade = maxGrade; grade >= 1; grade--)
        {
            foreach (MclslItemDefinition item in MclslItemCatalog.All)
                if (item.Category == category && item.Grade == grade && !desired.Contains(item.Id)) desired.Add(item.Id);
        }
    }

    internal static void PruneDeadSellers()
    {
        int removed = Listings.RemoveAll(x => x?.Item == null || MclslItemCatalog.Get(x.Item.ItemId) == null
            || !MclslActorRegistry.ResolveKnownOrWorld(x.SellerId, out Actor seller) || !MclslActorAccessor.Alive(seller));
        if (removed > 0)
        {
            _indexedRun = null;
            _snapshot = null;
            MclslWorldArchiveStore.MarkDirty();
        }
    }

    private static int Reserve(Actor actor, MclslItemDefinition item, int bestArtifact)
    {
        if (item.Category == "Pill" || item.Category == "Talisman") return 1;
        if (item.Category == "Artifact") return item.Grade == bestArtifact ? 1 : 0;
        string profession = MclslActorAccessor.GetString(actor, MclslActorDataKeys.Profession);
        int grade = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ProfessionGrade);
        if (grade == 0)
        {
            string practiceMaterial = profession == MclslProfessionSystem.Alchemist ? "A08"
                : profession == MclslProfessionSystem.TalismanMaker ? "F01" : string.Empty;
            return item.Id == practiceMaterial ? 1 : 0;
        }
        string category = profession == MclslProfessionSystem.Alchemist ? "Pill" : profession == MclslProfessionSystem.TalismanMaker ? "Talisman" : string.Empty;
        return category.Length > 0 && MclslItemCatalog.All.Any(x => x.Category == category && x.Grade == grade
            && (x.IngredientA == item.Id || x.IngredientB == item.Id)) ? 1 : 0;
    }
}
