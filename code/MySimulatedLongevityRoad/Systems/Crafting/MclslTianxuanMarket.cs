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
    private const int MaxStackListingCount = 20;
    internal const string ContributionCurrency = "Contribution";
    internal const string SpiritStoneCurrency = "SpiritStone";
    private static List<MclslMarketListing> Listings => MclslWorldRunRepository.Current.TianxuanListings ??=
        new List<MclslMarketListing>();
    private static List<MclslMarketActivity> Activities => MclslWorldRunRepository.Current.TianxuanActivities ??=
        new List<MclslMarketActivity>();

    private static MclslWorldRunState _indexedRun;
    private static List<MclslMarketListing> _indexedListings;
    private static readonly Dictionary<string, MclslMarketListing> ById = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, List<MclslMarketListing>> ByItem = new(StringComparer.Ordinal);
    private static readonly List<string> DesiredItemIdsScratch = new(8);
    private static readonly List<string> CraftMaterialNeedsScratch = new(8);
    private static readonly List<string> ExpansionMaterialNeedsScratch = new(24);
    private static readonly MclslItemDefinition[] ExpansionMaterials = MclslItemCatalog.All
        .Where(IsExpansionMaterial).ToArray();
    private static readonly string[] ExtraCombatNeeds = { "F021", "F022", "F023", "F024", "F025", "F026", "F027" };
    private static readonly string[] ManaNeeds = { "D013", "D017", "D020", "F020" };
    private static readonly List<string> ReservePlanScratch = new();
    private static readonly Dictionary<string, int> CraftReserveScratch = new(StringComparer.Ordinal);
    private static MclslMarketListing[] _snapshot;
    private static MclslMarketActivity[] _listingActivitySnapshot;
    private static MclslMarketActivity[] _purchaseActivitySnapshot;
    private static int _revision;
    internal static int ActiveListingCount => MclslWorldRunRepository.Current?.TianxuanListings?.Count ?? 0;
    internal static int Revision { get { EnsureIndex(); return _revision; } }

    internal static void ClearRuntime()
    {
        _indexedRun = null;
        _indexedListings = null;
        ById.Clear();
        ByItem.Clear();
        _snapshot = null;
        InvalidateActivitySnapshots();
        _revision++;
    }

    private static void EnsureIndex()
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        List<MclslMarketListing> listings = run.TianxuanListings ??= new();
        if (ReferenceEquals(run, _indexedRun) && ReferenceEquals(listings, _indexedListings)
            && listings.Count == ById.Count) return;
        ById.Clear();
        ByItem.Clear();
        HashSet<string> seenIds = new(StringComparer.Ordinal);
        bool repaired = false;
        for (int i = listings.Count - 1; i >= 0; i--)
        {
            MclslMarketListing listing = listings[i];
            MclslItemDefinition canonical = MclslItemCatalog.Get(listing?.Item?.ItemId);
            if (listing?.Item == null || string.IsNullOrWhiteSpace(listing.ListingId)
                || listing.Item.Count < 1 || canonical == null || !seenIds.Add(listing.ListingId))
            {
                listings.RemoveAt(i);
                repaired = true;
                continue;
            }
        }
        foreach (MclslMarketListing listing in listings)
        {
            MclslItemDefinition canonical = MclslItemCatalog.Get(listing.Item.ItemId);
            listing.Item.ItemId = canonical.Id;
            if (listing.ContributionUnitPrice <= 0) listing.ContributionUnitPrice = Math.Max(1, listing.Price > 0 ? listing.Price : canonical.Price);
            if (listing.SpiritStoneUnitPrice <= 0) listing.SpiritStoneUnitPrice = SpiritStonePrice(canonical, listing.ContributionUnitPrice);
            listing.Price = listing.ContributionUnitPrice;
            ById[listing.ListingId] = listing;
            if (!ByItem.TryGetValue(listing.Item.ItemId, out List<MclslMarketListing> matches))
                ByItem[listing.Item.ItemId] = matches = new();
            matches.Add(listing);
        }
        _indexedRun = run;
        _indexedListings = listings;
        _snapshot = null;
        _revision++;
        if (repaired)
        {
            MclslWorldArchiveStore.MarkDirty();
            MclslDiagnostics.Error("market-invalid-listings", "天玄镜已清理旧档中无效或重复的挂单。");
        }
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
        _revision++;
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
        _revision++;
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

    internal static int SpiritStonePrice(MclslItemDefinition item, int contributionPrice)
    {
        int tier = item.MaterialTier != MclslMaterialTier.None
            ? (int)item.MaterialTier : Math.Clamp(item.Grade, 0, 4);
        int factor = tier switch { 0 => 10, 1 => 15, 2 => 20, 3 => 25, _ => 30 };
        return Math.Max(1, (int)Math.Min(999999L, ((long)Math.Max(1, contributionPrice) * factor + 9) / 10));
    }

    private static void RecordActivity(string action, MclslItemDefinition item, Actor actor, Actor other,
        int count, int unitPrice, int totalPrice, string currency, int year)
    {
        Activities.Add(new MclslMarketActivity
        {
            Action = action,
            ItemId = item.Id,
            ActorId = MclslActorAccessor.Id(actor),
            ActorName = SafeName(actor),
            OtherActorId = MclslActorAccessor.Id(other),
            OtherActorName = other == null ? string.Empty : SafeName(other),
            Count = count,
            Price = unitPrice,
            Currency = currency,
            UnitPrice = unitPrice,
            TotalPrice = totalPrice,
            Year = year
        });
        if (Activities.Count > MaxActivityRecords)
            Activities.RemoveRange(0, Activities.Count - MaxActivityRecords);
        InvalidateActivitySnapshots();
        _revision++;
    }

    private static string SafeName(Actor actor)
    {
        try { return MclslActorAccessor.DisplayName(actor); }
        catch { return "无名修士"; }
    }

    internal static bool TryListSurplus(Actor actor, int year)
    {
        if (!MclslActorAccessor.Alive(actor) || MclslBagSystem.IsLocked(actor)) return false;
        bool listedArtifact = MclslArtifactSystem.TryListLowerGradeArtifact(actor, year);
        if (Listings.Count >= MaxMarketListings) return listedArtifact;
        if (string.IsNullOrEmpty(MclslActorAccessor.GetString(actor, MclslActorDataKeys.QiankunBag))) return listedArtifact;
        MclslBagState bag = MclslBagSystem.Read(actor);
        MclslConsumableInventoryPolicy.BuildReservePlan(actor, ReservePlanScratch);
        MclslProfessionSystem.BuildCraftMaterialReserves(actor, bag, CraftReserveScratch);
        int itemCount = bag.Items.Count;
        int start = itemCount == 0 ? 0 : (int)((unchecked((ulong)MclslActorAccessor.Id(actor) + (uint)year)) % (uint)itemCount);
        for (int offset = 0; offset < itemCount; offset++)
        {
            int i = (start + offset) % itemCount;
            MclslOwnedItem owned = bag.Items[i];
            MclslItemDefinition item = MclslItemCatalog.Get(owned.ItemId);
            if (item == null) continue;
            if (item.Category == "Artifact") continue; // Individual artifacts use the equipment surplus path.
            if (owned.AcquiredYear >= 0 && year <= owned.AcquiredYear) continue;
            int reserve = MclslConsumableInventoryPolicy.ReserveCount(actor, item, ReservePlanScratch, CraftReserveScratch);
            if (MclslBagSystem.Count(bag, item.Id) <= reserve) continue;
            int count = Math.Min(MaxStackListingCount, Math.Min(owned.Count, MclslBagSystem.Count(bag, item.Id) - reserve));
            if (count <= 0) continue;
            MclslOwnedItem offer = new() { ItemId = item.Id, Count = count, Durability = owned.Durability,
                InstanceId = owned.InstanceId, AcquiredYear = owned.AcquiredYear };
            owned.Count -= count;
            if (owned.Count <= 0) bag.Items.RemoveAt(i);
            MclslBagSystem.Write(actor, bag);
            AddListing(new MclslMarketListing { ListingId = Guid.NewGuid().ToString("N"), SellerId = MclslActorAccessor.Id(actor),
                Item = offer, Price = item.Price, ContributionUnitPrice = item.Price,
                SpiritStoneUnitPrice = SpiritStonePrice(item, item.Price), Year = year });
            RecordActivity("Listed", item, actor, null, count, item.Price, count * item.Price, ContributionCurrency, year);
            MclslWorldArchiveStore.MarkDirty();
            return true;
        }
        return listedArtifact;
    }

    internal static bool TryListArtifactSurplus(Actor actor, MclslOwnedItem owned, int year, bool deferWhenFull = true)
    {
        MclslItemDefinition item = MclslItemCatalog.Get(owned?.ItemId);
        if (!MclslActorAccessor.Alive(actor) || item?.Category != "Artifact" || item.Id is "B080" or "B081" || string.IsNullOrWhiteSpace(owned.InstanceId)) return false;
        EnsureIndex();
        if (Listings.Any(x => x?.Item?.InstanceId == owned.InstanceId)
            || MclslWorldRunRepository.Current.PendingArtifactListings?.Any(x => x?.Item?.InstanceId == owned.InstanceId) == true)
            return false;
        if (!deferWhenFull && Listings.Count >= MaxMarketListings) return false;
        MclslMarketListing listing = new()
        {
            ListingId = Guid.NewGuid().ToString("N"),
            SellerId = MclslActorAccessor.Id(actor),
            Item = new MclslOwnedItem
            {
                ItemId = item.Id, InstanceId = owned.InstanceId, Count = 1,
                Durability = Math.Clamp(owned.Durability, 1, 100), AcquiredYear = owned.AcquiredYear
            },
            Price = item.Price,
            ContributionUnitPrice = item.Price,
            SpiritStoneUnitPrice = SpiritStonePrice(item, item.Price),
            Year = Math.Max(0, year)
        };
        if (Listings.Count < MaxMarketListings) AddListing(listing);
        else
        {
            MclslWorldRunState run = MclslWorldRunRepository.Current;
            run.PendingArtifactListings ??= new Queue<MclslMarketListing>();
            run.PendingArtifactListings.Enqueue(listing);
        }
        RecordActivity("Listed", item, actor, null, 1, item.Price, item.Price, ContributionCurrency, year);
        MclslWorldArchiveStore.MarkDirty();
        return true;
    }

    internal static int PublishPendingArtifactListings(int limit)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        Queue<MclslMarketListing> pending = run.PendingArtifactListings ??= new Queue<MclslMarketListing>();
        int published = 0;
        int inspected = 0;
        int quota = Math.Max(0, limit);
        while (inspected < quota && Listings.Count < MaxMarketListings && pending.Count > 0)
        {
            MclslMarketListing listing = pending.Dequeue();
            inspected++;
            if (listing?.Item == null || MclslItemCatalog.Get(listing.Item.ItemId)?.Category != "Artifact"
                || !MclslActorRegistry.ResolveKnownOrWorld(listing.SellerId, out Actor seller)
                || !MclslActorAccessor.Alive(seller)) continue;
            AddListing(listing);
            published++;
        }
        if (inspected > 0) MclslWorldArchiveStore.MarkDirty();
        return published;
    }

    internal static bool TryBuy(Actor buyer, string listingId, int year = -1, int requestedCount = 1, string currency = null)
    {
        if (!MclslActorAccessor.Alive(buyer) || MclslBagSystem.IsLocked(buyer)
            || string.IsNullOrEmpty(listingId) || requestedCount < 1 || requestedCount > MaxStackListingCount) return false;
        int acquiredYear = year < 0 ? Math.Max(1, MclslRuntime.CurrentYear()) : year;
        if (acquiredYear <= 0 || PurchasesInYear(buyer, acquiredYear) >= MaxAnnualPurchases) return false;
        EnsureIndex();
        if (!ById.TryGetValue(listingId, out MclslMarketListing listing)) return false;
        MclslItemDefinition item = MclslItemCatalog.Get(listing?.Item?.ItemId);
        if (item == null) return false;
        long buyerId = MclslActorAccessor.Id(buyer);
        if (buyerId == listing.SellerId || listing.Item.Count < requestedCount
            || item.Category == "Artifact" && requestedCount != 1) return false;
        int contributionUnit = listing.ContributionUnitPrice > 0 ? listing.ContributionUnitPrice : Math.Max(1, listing.Price);
        int stoneUnit = listing.SpiritStoneUnitPrice > 0 ? listing.SpiritStoneUnitPrice : SpiritStonePrice(item, contributionUnit);
        int contributionFunds = MclslActorAccessor.GetInt(buyer, MclslActorDataKeys.Contribution);
        int stoneFunds = MclslActorAccessor.GetInt(buyer, MclslActorDataKeys.SpiritStones);
        bool canContribution = (long)contributionUnit * requestedCount <= contributionFunds;
        bool canStones = (long)stoneUnit * requestedCount <= stoneFunds;
        if (currency == null)
        {
            if (!canContribution && !canStones) return false;
            currency = canContribution && (!canStones || (double)contributionUnit / Math.Max(1, contributionFunds)
                <= (double)stoneUnit / Math.Max(1, stoneFunds)) ? ContributionCurrency : SpiritStoneCurrency;
        }
        if (currency != ContributionCurrency && currency != SpiritStoneCurrency) return false;
        if (currency == ContributionCurrency && !canContribution || currency == SpiritStoneCurrency && !canStones) return false;
        int unitPrice = currency == ContributionCurrency ? contributionUnit : stoneUnit;
        int totalPrice = checked(unitPrice * requestedCount);
        MclslBagState bag = MclslBagSystem.Read(buyer);
        if (item.Category == "Artifact" && MclslBagSystem.Count(bag, item.Id) > 0) return false;
        if (!MclslActorRegistry.ResolveKnownOrWorld(listing.SellerId, out Actor seller) || !MclslActorAccessor.Alive(seller))
        {
            RemoveListing(listing);
            MclslWorldArchiveStore.MarkDirty();
            return false;
        }
        string fundsKey = currency == ContributionCurrency ? MclslActorDataKeys.Contribution : MclslActorDataKeys.SpiritStones;
        int sellerFunds = MclslActorAccessor.GetInt(seller, fundsKey);
        if (sellerFunds > 999999 - totalPrice) return false;
        // The game thread settles one order at a time. A partial purchase leaves the order indexed.
        if (requestedCount == listing.Item.Count && !RemoveListing(listing)) return false;
        if (requestedCount < listing.Item.Count) { listing.Item.Count -= requestedCount; _snapshot = null; _revision++; }
        if (item.Category == "Artifact")
            bag.Items.Add(new MclslOwnedItem { ItemId = item.Id, InstanceId = listing.Item.InstanceId,
                Durability = listing.Item.Durability, Count = 1, AcquiredYear = acquiredYear });
        else MclslBagSystem.Add(bag, item.Id, requestedCount, acquiredYear: acquiredYear);
        MclslBagSystem.Write(buyer, bag);
        MclslRecipeKnowledge.LearnFromOwnedItem(buyer, item.Id);
        MclslActorAccessor.Set(buyer, fundsKey, (currency == ContributionCurrency ? contributionFunds : stoneFunds) - totalPrice);
        MclslActorAccessor.Set(seller, fundsKey, sellerFunds + totalPrice);
        if (item.Category == "Artifact") MclslArtifactSystem.OnArtifactAcquired(buyer);
        int purchasedBeforeSettlement = PurchasesInYear(buyer, acquiredYear);
        MclslActorAccessor.Set(buyer, MclslActorDataKeys.MarketPurchaseYear, acquiredYear);
        MclslActorAccessor.Set(buyer, MclslActorDataKeys.MarketPurchaseCount,
            Math.Min(MaxAnnualPurchases, purchasedBeforeSettlement + 1));
        RecordActivity("Purchased", item, buyer, seller, requestedCount, unitPrice, totalPrice, currency, acquiredYear);
        MclslWorldRunRepository.AddItemAcquisitionEvent(
            acquiredYear, buyer, item.Id, requestedCount, "天玄镜交易");
        MclslWorldArchiveStore.MarkDirty();
        return true;
    }

    internal const int MaxAnnualPurchases = 6;

    internal static int PurchasesInYear(Actor actor, int year)
    {
        if (actor?.data == null || year <= 0
            || MclslActorAccessor.GetInt(actor, MclslActorDataKeys.MarketPurchaseYear) != year) return 0;
        return Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.MarketPurchaseCount), 0, MaxAnnualPurchases);
    }

    internal static bool TryBuyNeeded(Actor actor, int year)
    {
        if (!MclslActorAccessor.Alive(actor) || PurchasesInYear(actor, year) >= MaxAnnualPurchases) return false;
        EnsureIndex();
        if (Listings.Count == 0) return false;
        MclslBagState bag = MclslBagSystem.Peek(actor);
        long actorId = MclslActorAccessor.Id(actor);
        int purchaseOrdinal = PurchasesInYear(actor, year);
        string profession = MclslProfessionSystem.FromTrait(actor);
        if (string.IsNullOrWhiteSpace(profession))
            profession = MclslActorAccessor.GetString(actor, MclslActorDataKeys.Profession, string.Empty);
        bool hasProfession = profession is MclslProfessionSystem.Alchemist
            or MclslProfessionSystem.Refiner or MclslProfessionSystem.TalismanMaker;
        List<string> finished = DesiredItemIdsScratch;
        finished.Clear();
        CraftMaterialNeedsScratch.Clear();
        ExpansionMaterialNeedsScratch.Clear();
        try
        {
            if (hasProfession)
            {
                BuildCraftMaterialNeeds(actor, bag, CraftMaterialNeedsScratch, year);
                BuildExpansionMaterialNeeds(actor, bag, profession, ExpansionMaterialNeedsScratch, year);
            }
            MclslConsumableInventoryPolicy.BuildReservePlan(actor, ReservePlanScratch);
            BuildFinishedGoodsNeeds(actor, bag, finished);
            AddSpellScrollNeed(actor, bag, finished);

            // Re-evaluate needs after every purchase and rotate the first lane.
            // Each scheduler pass settles at most one order.
            int firstLane = hasProfession ? (int)(unchecked((ulong)actorId + (uint)year + (uint)purchaseOrdinal) % 3UL) : 2;
            for (int pass = 0; pass < (hasProfession ? 3 : 1); pass++)
            {
                int lane = (firstLane + pass) % 3;
                List<string> needs = lane == 0 ? CraftMaterialNeedsScratch
                    : lane == 1 ? ExpansionMaterialNeedsScratch : finished;
                if (TryBuyFromNeeds(actor, actorId, year, purchaseOrdinal, needs)) return true;
            }
            return false;
        }
        finally
        {
            finished.Clear();
            CraftMaterialNeedsScratch.Clear();
            ExpansionMaterialNeedsScratch.Clear();
        }
    }

    private static bool TryBuyFromNeeds(Actor actor, long actorId, int year, int purchaseOrdinal, List<string> needs)
    {
        if (needs.Count == 0) return false;
        int start = (int)(unchecked((ulong)actorId + (uint)year + (uint)purchaseOrdinal) % (uint)needs.Count);
        int contributionFunds = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Contribution);
        int stoneFunds = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.SpiritStones);
        for (int offset = 0; offset < needs.Count; offset++)
        {
            string itemId = needs[(start + offset) % needs.Count];
            if (!ByItem.TryGetValue(itemId, out List<MclslMarketListing> offers))
            {
                LogExpansionMaterialSkip(itemId, "无挂单", actor, year);
                continue;
            }
            MclslMarketListing best = null;
            int bestPrice = int.MaxValue;
            bool otherSeller = false;
            for (int i = 0; i < offers.Count; i++)
            {
                MclslMarketListing offer = offers[i];
                if (offer.SellerId == actorId || offer.Item?.Count < 1) continue;
                otherSeller = true;
                MclslItemDefinition offerItem = MclslItemCatalog.Get(offer.Item.ItemId);
                int contributionPrice = offer.ContributionUnitPrice > 0 ? offer.ContributionUnitPrice : offer.Price;
                int stonePrice = offer.SpiritStoneUnitPrice > 0 ? offer.SpiritStoneUnitPrice : SpiritStonePrice(offerItem, contributionPrice);
                if (contributionPrice > contributionFunds && stonePrice > stoneFunds) continue;
                if (best == null || contributionPrice < bestPrice || (contributionPrice == bestPrice && offer.Year < best.Year))
                { best = offer; bestPrice = contributionPrice; }
            }
            if (best == null)
            {
                LogExpansionMaterialSkip(itemId, otherSeller ? "余额不足" : "无有效卖方", actor, year);
                continue;
            }
            if (!TryBuy(actor, best.ListingId, year))
            {
                LogExpansionMaterialSkip(itemId, "交割未完成", actor, year);
                continue;
            }
            if (itemId.EndsWith("_SCROLL", StringComparison.Ordinal))
                MclslSpellSystem.TryStudyScroll(actor, itemId);
            return true;
        }
        return false;
    }

    private static void BuildCraftMaterialNeeds(Actor actor, MclslBagState bag, List<string> desired, int year)
    {
        int grade = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ProfessionGrade);
        (string first, string second) = MclslProfessionSystem.RequiredCraftMaterialPair(actor, year, bag);
        AddRecipeMaterialNeeds(desired, bag, first, second, grade);
        List<MclslItemDefinition> recipes = MclslProfessionSystem.KnownCraftRecipes(actor, bag);
        if (recipes.Count > 0)
        {
            int start = (int)(unchecked((ulong)MclslActorAccessor.Id(actor) + (uint)year) % (uint)recipes.Count);
            for (int offset = 0; offset < recipes.Count; offset++)
            {
                MclslItemDefinition recipe = recipes[(start + offset) % recipes.Count];
                AddRecipeMaterialNeeds(desired, bag, recipe.IngredientA, recipe.IngredientB, grade);
            }
        }
    }

    private static void BuildExpansionMaterialNeeds(Actor actor, MclslBagState bag, string profession,
        List<string> desired, int year)
    {
        int grade = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ProfessionGrade);
        if (grade <= 0) return;
        foreach (MclslItemDefinition material in ExpansionMaterials)
        {
            bool matches = profession == MclslProfessionSystem.Alchemist && material.Category is "Plant" or "SpiritObject"
                || profession == MclslProfessionSystem.TalismanMaker && material.Category == "TalismanMaterial"
                || profession == MclslProfessionSystem.Refiner && material.Category == "Material";
            if (!matches) continue;
            if (!MclslProfessionSystem.IsProfessionMaterial(profession, material, grade))
            {
                LogExpansionMaterialSkip(material.Id, "职业品阶不足", actor, year);
                continue;
            }
            if (MclslBagSystem.Count(bag, material.Id) > 0)
            {
                LogExpansionMaterialSkip(material.Id, "需求已满足", actor, year);
                continue;
            }
            desired.Add(material.Id);
        }
    }

    private static bool IsExpansionMaterial(MclslItemDefinition item)
    {
        string id = item.Id;
        return item.MaterialTier != MclslMaterialTier.None
            && (id.Length == 3 && id.StartsWith("A1", StringComparison.Ordinal)
                || id.Length == 4 && id.StartsWith("F01", StringComparison.Ordinal)
                || item.Category == "Material" && id.StartsWith("M", StringComparison.Ordinal));
    }

    private static void LogExpansionMaterialSkip(string itemId, string reason, Actor actor, int year)
    {
        if (!MclslDiagnostics.Enabled || !IsExpansionMaterialId(itemId)) return;
        MclslDiagnostics.Throttle("market-material-" + itemId + "-" + reason,
            UnityEngine.Time.frameCount, 3600,
            "天玄镜未购入 " + itemId + "：" + reason + "；修士=" + MclslActorAccessor.Id(actor)
            + "；职业品阶=" + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ProfessionGrade)
            + "；年份=" + year);
    }

    private static bool IsExpansionMaterialId(string itemId)
    {
        MclslItemDefinition item = MclslItemCatalog.Get(itemId);
        return item != null && IsExpansionMaterial(item);
    }

    private static void AddRecipeMaterialNeeds(List<string> desired, MclslBagState bag, string first, string second, int grade)
    {
        if (string.Equals(first, second, StringComparison.Ordinal)) AddMissingCount(desired, bag, first, 2, grade);
        else
        {
            AddMissingCount(desired, bag, first, 1, grade);
            AddMissingCount(desired, bag, second, 1, grade);
        }
    }

    private static void AddSpellScrollNeed(Actor actor, MclslBagState bag, List<string> desired)
    {
        int realm = MclslRealmIds.Index(MclslActorAccessor.Realm(actor));
        if (realm < 0) return;
        string[] roots = MclslSpiritualRootSystem.RootAttributes(actor);
        for (int pass = 0; pass < 2; pass++)
        foreach (MclslSpellDefinition spell in MclslSpellSystem.All)
        {
            if ((Array.IndexOf(roots, spell.Law) >= 0) != (pass == 0)) continue;
            if (spell.MinRealm > realm || MclslSpellSystem.Knows(actor, spell.Id)
                || MclslBagSystem.Count(bag, spell.Id + "_SCROLL") > 0) continue;
            desired.Add(spell.Id + "_SCROLL");
        }
    }

    private static void BuildFinishedGoodsNeeds(Actor actor, MclslBagState bag, List<string> desired)
    {
        string breakthroughPill = MclslConsumableInventoryPolicy.BreakthroughPillForRealm(MclslActorAccessor.Realm(actor));
        AddIfMissing(desired, bag, breakthroughPill);

        bool injured = IsInjured(actor);
        string healingPill = string.Empty;
        foreach (string itemId in ReservePlanScratch)
        {
            if (itemId is "D006" or "D007" or "D010")
            {
                healingPill = itemId;
                break;
            }
        }
        if (injured) AddIfMissing(desired, bag, healingPill);
        if (injured && MclslConsumableInventoryPolicy.IsCurrentGradeUseful(actor, MclslItemCatalog.Get("D019")))
            AddIfMissing(desired, bag, "D019");

        foreach (string itemId in ReservePlanScratch)
        {
            MclslItemDefinition item = MclslItemCatalog.Get(itemId);
            if (item?.Category != "Pill" || itemId == breakthroughPill
                || itemId is "D006" or "D007" or "D010") continue;
            if (itemId == "D004" && MclslActorAccessor.GetInt(actor, "mclsl.v020.taishang_taken") != 0) continue;
            AddIfMissing(desired, bag, itemId);
        }

        int artifactGradeCap = Math.Clamp(MclslRealmIds.Index(MclslActorAccessor.Realm(actor)) + 1, 1, 4);
        foreach (string listedId in ByItem.Keys)
        {
            MclslItemDefinition artifact = MclslItemCatalog.Get(listedId);
            if (artifact?.Category != "Artifact" || artifact.Grade > artifactGradeCap
                || artifact.Id is "B080" or "B081") continue;
            if (MclslArtifactSystem.NeedsSuitableArtifact(actor, bag, artifact.Id))
                AddIfMissing(desired, bag, artifact.Id);
        }

        if (HasLiveCombatTarget(actor))
        {
            for (int i = 0; i < MclslConsumableInventoryPolicy.CombatTalismanCount; i++)
            {
                string talismanId = MclslConsumableInventoryPolicy.CombatTalismanAt(i);
                if (bag?.Items?.Any(owned => string.Equals(owned?.ItemId, talismanId, StringComparison.Ordinal)) == true
                    || actor.hasStatus("mclsl_item_" + talismanId)) continue;
                AddIfMissing(desired, bag, talismanId);
                break;
            }
        }

        // Newly added consumables were craftable and listable, but had no AI
        // demand unless they happened to be selected by the small reserve list.
        bool manaLow = MclslSpellSystem.CurrentMana(actor) < MclslSpellSystem.MaxMana(actor) / 2;
        if (manaLow)
        {
            foreach (string id in ManaNeeds)
                if (MclslConsumableInventoryPolicy.IsCurrentGradeUseful(actor, MclslItemCatalog.Get(id)))
                    AddIfMissing(desired, bag, id);
        }
        if (injured && MclslConsumableInventoryPolicy.IsCurrentGradeUseful(actor, MclslItemCatalog.Get("D015")))
            AddIfMissing(desired, bag, "D015");
        if ((MclslConsumableInventoryPolicy.HasControlStatus(actor) || HasLiveCombatTarget(actor))
            && MclslConsumableInventoryPolicy.IsCurrentGradeUseful(actor, MclslItemCatalog.Get("D014")))
            AddIfMissing(desired, bag, "D014");
        if (MclslSpellSystem.CurrentMana(actor) < MclslSpellSystem.MaxMana(actor) * 3 / 4
            && !actor.hasStatus("mclsl_item_D016")
            && MclslConsumableInventoryPolicy.IsCurrentGradeUseful(actor, MclslItemCatalog.Get("D016")))
            AddIfMissing(desired, bag, "D016");
        if (HasLiveCombatTarget(actor))
            foreach (string id in ExtraCombatNeeds)
            {
                MclslItemDefinition candidate = MclslItemCatalog.Get(id);
                if (MclslConsumableInventoryPolicy.IsCurrentGradeUseful(actor, candidate)) AddIfMissing(desired, bag, id);
            }

        // Keep non-combat elixirs such as cures, cultivation supplements and
        // training talismans available only when the shared reserve policy says
        // they are currently useful.
        foreach (string itemId in ReservePlanScratch)
        {
            MclslItemDefinition item = MclslItemCatalog.Get(itemId);
            if (item == null || item.Category == "SpiritObject" || item.Category == "Material"
                || item.Category == "Artifact" || item.Id is "D006" or "D007" or "D010"
                || item.Id == breakthroughPill || item.Category == "Talisman" && !string.Equals(item.Id, "F007", StringComparison.Ordinal))
                continue;
            AddIfMissing(desired, bag, item.Id);
        }
    }

    private static bool IsInjured(Actor actor)
    {
        if (actor == null) return false;
        try
        {
            float maximum = actor.getMaxHealth();
            return maximum > 0f && actor.getHealth() < maximum * 0.80f;
        }
        catch { return false; }
    }

    private static bool HasLiveCombatTarget(Actor actor)
    {
        if (!MclslActorAccessor.Alive(actor)) return false;
        try { return actor.has_attack_target && actor.attack_target != null && actor.attack_target.isAlive(); }
        catch { return false; }
    }

    private static void AddIfMissing(List<string> desired, MclslBagState bag, string itemId)
    {
        if (!string.IsNullOrEmpty(itemId) && MclslItemCatalog.Get(itemId) != null
            && MclslBagSystem.Count(bag, itemId) == 0 && !desired.Contains(itemId)) desired.Add(itemId);
    }

    private static void AddMissingCount(List<string> desired, MclslBagState bag, string itemId,
        int requiredCount, int professionGrade)
    {
        if (string.IsNullOrEmpty(itemId) || requiredCount <= 0) return;
        MclslItemDefinition item = MclslItemCatalog.Get(itemId);
        if (item == null) return;
        bool practiceMaterial = professionGrade == 0 && (itemId is "A08" or "F01");
        if (!practiceMaterial && item.MaterialTier != MclslMaterialTier.None
            && (int)item.MaterialTier > professionGrade) return;
        int pending = desired.Count(id => string.Equals(id, itemId, StringComparison.Ordinal));
        int missing = requiredCount - MclslBagSystem.Count(bag, itemId) - pending;
        for (int i = 0; i < missing; i++) desired.Add(itemId);
    }

    internal static void PruneDeadSellers()
    {
        int removed = Listings.RemoveAll(x => x?.Item == null || MclslItemCatalog.Get(x.Item.ItemId) == null
            || !MclslActorRegistry.ResolveKnownOrWorld(x.SellerId, out Actor seller) || !MclslActorAccessor.Alive(seller));
        if (removed > 0)
        {
            _indexedRun = null;
            _snapshot = null;
            _revision++;
            MclslWorldArchiveStore.MarkDirty();
        }
    }

}
