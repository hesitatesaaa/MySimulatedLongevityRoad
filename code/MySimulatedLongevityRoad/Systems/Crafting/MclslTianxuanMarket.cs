using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal enum MclslMarketPurchaseStep : byte { Pending, Complete, Unavailable }

internal static class MclslTianxuanMarket
{
    private const int MaxMarketListings = 1000;
    private const int MaxPendingArtifactListings = 1024;
    internal static int PendingListingCount => MclslWorldRunRepository.Current?.PendingArtifactListings?.Count ?? 0;
    internal static bool HasDeferredArtifactCapacity
    {
        get
        {
            EnsureIndex();
            return Listings.Count < MaxMarketListings || PendingListingCount < MaxPendingArtifactListings;
        }
    }
    private const int MaxActivityRecords = 256;
    private const int MaxStackListingCount = 20;
    private static readonly int MaxPurchaseNeeds = MclslItemCatalog.All.Count() * 2;
    // Catalog and recipe definitions do not change between worlds. Derive the
    // reference table once; orders still store their own committed quote.
    private static readonly Dictionary<string, int> BaseReferencePrices = BuildBaseReferencePrices();
    internal const string ContributionCurrency = "Contribution";
    internal const string SpiritStoneCurrency = "SpiritStone";
    private static List<MclslMarketListing> Listings => MclslWorldRunRepository.Current.TianxuanListings ??=
        new List<MclslMarketListing>();
    private static List<MclslMarketActivity> Activities => MclslWorldRunRepository.Current.TianxuanActivities ??=
        new List<MclslMarketActivity>();

    private static MclslWorldRunState _indexedRun;
    private static List<MclslMarketListing> _indexedListings;
    private static Queue<MclslMarketListing> _indexedPending;
    private static int _indexedPendingCount;
    private static readonly Dictionary<string, MclslMarketListing> ById = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, List<MclslMarketListing>> ByItem = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, int> PendingByItem = new(StringComparer.Ordinal);
    private static readonly HashSet<string> ArtifactInstanceIds = new(StringComparer.Ordinal);
    private static readonly List<string> DesiredItemIdsScratch = new(8);
    private static readonly string[] ExtraCombatNeeds = { "F021", "F022", "F023", "F024", "F025", "F026", "F027" };
    private static readonly string[] ManaNeeds = { "D013", "D017", "D020", "F020" };
    private static readonly List<string> ReservePlanScratch = new();
    private static readonly Dictionary<string, int> CraftReserveScratch = new(StringComparer.Ordinal);
    private static readonly List<string> CraftKeysScratch = new();
    private static readonly List<MclslMarketPurchaseNeed> CraftNeedsScratch = new();
    private static readonly List<MclslMarketPurchaseNeed> FinishedNeedsScratch = new();
    private static readonly Dictionary<string, MclslMarketPurchaseNeed> CraftNeedsByItemScratch = new(StringComparer.Ordinal);
    private static readonly HashSet<string> FinishedIdsScratch = new(StringComparer.Ordinal);
    private static MclslMarketListing[] _snapshot;
    private static MclslMarketActivity[] _listingActivitySnapshot;
    private static MclslMarketActivity[] _purchaseActivitySnapshot;
    private static int _revision;
    internal static int ActiveListingCount => MclslWorldRunRepository.Current?.TianxuanListings?.Count ?? 0;
    internal static int PendingPurchaseBatchCount => MclslWorldRunRepository.Current?.PendingMarketPurchases?.Count ?? 0;
    internal static int Revision { get { EnsureIndex(); return _revision; } }

    internal static int ActiveItemCount(string itemId)
    {
        EnsureIndex();
        int count = PendingByItem.TryGetValue(itemId, out int pending) ? pending : 0;
        if (ByItem.TryGetValue(itemId, out List<MclslMarketListing> offers))
            foreach (MclslMarketListing offer in offers)
                count += Math.Max(0, offer?.Item?.Count ?? 0);
        return count;
    }

    internal static void TransferSellerEscrowToFamily(long sellerId, List<MclslOwnedItem> estate, Action commitFunds)
    {
        if (sellerId <= 0 || estate == null) return;
        EnsureIndex();
        MclslMarketListing[] active = Listings.Where(x => x?.SellerId == sellerId).ToArray();
        Queue<MclslMarketListing> pending = MclslWorldRunRepository.Current.PendingArtifactListings ??= new();
        MclslMarketListing[] pendingBefore = pending.ToArray();
        int estateBefore = estate.Count;
        List<MclslMarketListing> removed = new();
        try
        {
            foreach (MclslMarketListing listing in active)
                estate.Add(MclslFamilyTransferPolicy.CopyItem(listing.Item));
            foreach (MclslMarketListing listing in pendingBefore)
                if (listing?.SellerId == sellerId)
                    estate.Add(MclslFamilyTransferPolicy.CopyItem(listing.Item));
            foreach (MclslMarketListing listing in active)
            {
                if (!RemoveListing(listing)) throw new InvalidOperationException("遗产挂单在转移时失效");
                removed.Add(listing);
            }
            pending.Clear();
            foreach (MclslMarketListing listing in pendingBefore)
                if (listing?.SellerId != sellerId) pending.Enqueue(listing);
            _indexedPendingCount = -1;
            EnsureIndex();
            MclslWorldArchiveStore.MarkDirty();
            commitFunds();
        }
        catch
        {
            estate.RemoveRange(estateBefore, estate.Count - estateBefore);
            foreach (MclslMarketListing listing in removed)
                if (!ById.ContainsKey(listing.ListingId)) AddListing(listing);
            pending.Clear();
            foreach (MclslMarketListing listing in pendingBefore) pending.Enqueue(listing);
            _indexedPendingCount = -1;
            EnsureIndex();
            throw;
        }
    }

    internal static void ReleaseUnclaimedSellerEscrow(long sellerId, int year)
    {
        if (sellerId <= 0) return;
        EnsureIndex();
        bool changed = false;
        foreach (MclslMarketListing listing in Listings)
        {
            if (listing?.SellerId != sellerId) continue;
            listing.SellerId = 0;
            listing.Year = Math.Max(0, year);
            changed = true;
        }
        Queue<MclslMarketListing> pending = MclslWorldRunRepository.Current.PendingArtifactListings;
        if (pending != null)
            foreach (MclslMarketListing listing in pending)
            {
                if (listing?.SellerId != sellerId) continue;
                listing.SellerId = 0;
                listing.Year = Math.Max(0, year);
                changed = true;
            }
        if (!changed) return;
        _indexedPendingCount = -1;
        _snapshot = null;
        _revision++;
        MclslWorldArchiveStore.MarkDirty();
    }

    internal static void ClearRuntime()
    {
        _indexedRun = null;
        _indexedListings = null;
        _indexedPending = null;
        _indexedPendingCount = 0;
        ById.Clear();
        ByItem.Clear();
        PendingByItem.Clear();
        ArtifactInstanceIds.Clear();
        _snapshot = null;
        InvalidateActivitySnapshots();
        _revision++;
    }

    private static void EnsureIndex()
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        List<MclslMarketListing> listings = run.TianxuanListings ??= new();
        Queue<MclslMarketListing> pending = run.PendingArtifactListings ??= new();
        if (ReferenceEquals(run, _indexedRun) && ReferenceEquals(listings, _indexedListings)
            && ReferenceEquals(pending, _indexedPending) && listings.Count == ById.Count
            && pending.Count == _indexedPendingCount) return;
        ById.Clear();
        ByItem.Clear();
        PendingByItem.Clear();
        ArtifactInstanceIds.Clear();
        HashSet<string> seenIds = new(StringComparer.Ordinal);
        HashSet<string> seenInstances = new(StringComparer.Ordinal);
        bool repaired = false;
        for (int i = listings.Count - 1; i >= 0; i--)
        {
            MclslMarketListing listing = listings[i];
            MclslItemDefinition canonical = MclslItemCatalog.Get(listing?.Item?.ItemId);
            bool artifact = canonical?.Category == "Artifact";
            if (listing?.Item == null || string.IsNullOrWhiteSpace(listing.ListingId)
                || listing.Item.Count < 1 || canonical == null
                || artifact && (listing.Item.Count != 1 || string.IsNullOrWhiteSpace(listing.Item.InstanceId))
                || seenIds.Contains(listing.ListingId)
                || artifact && seenInstances.Contains(listing.Item.InstanceId))
            {
                listings.RemoveAt(i);
                repaired = true;
                continue;
            }
            seenIds.Add(listing.ListingId);
            if (artifact) seenInstances.Add(listing.Item.InstanceId);
        }
        foreach (MclslMarketListing listing in listings)
        {
            MclslItemDefinition canonical = MclslItemCatalog.Get(listing.Item.ItemId);
            listing.Item.ItemId = canonical.Id;
            if (listing.ContributionUnitPrice <= 0) listing.ContributionUnitPrice = ContributionPrice(canonical);
            if (listing.SpiritStoneUnitPrice <= 0) listing.SpiritStoneUnitPrice = MarketStonePrice(canonical);
            listing.Price = listing.ContributionUnitPrice;
            ById[listing.ListingId] = listing;
            if (canonical.Category == "Artifact") ArtifactInstanceIds.Add(listing.Item.InstanceId);
            if (!ByItem.TryGetValue(listing.Item.ItemId, out List<MclslMarketListing> matches))
                ByItem[listing.Item.ItemId] = matches = new();
            matches.Add(listing);
        }
        int pendingCount = pending.Count;
        int keptPending = 0;
        for (int i = 0; i < pendingCount; i++)
        {
            MclslMarketListing listing = pending.Dequeue();
            MclslItemDefinition item = MclslItemCatalog.Get(listing?.Item?.ItemId);
            bool artifact = item?.Category == "Artifact";
            if (item == null || listing.Item.Count < 1
                || artifact && (listing.Item.Count != 1 || string.IsNullOrWhiteSpace(listing.Item.InstanceId))
                || listing.SellerId != 0 && !artifact
                || string.IsNullOrWhiteSpace(listing.ListingId)
                || seenIds.Contains(listing.ListingId)
                || artifact && seenInstances.Contains(listing.Item.InstanceId)
                || listing.SellerId != 0 && keptPending >= MaxPendingArtifactListings)
            { repaired = true; continue; }
            seenIds.Add(listing.ListingId);
            if (artifact)
            {
                seenInstances.Add(listing.Item.InstanceId);
                ArtifactInstanceIds.Add(listing.Item.InstanceId);
            }
            listing.Item.ItemId = item.Id;
            pending.Enqueue(listing);
            PendingByItem.TryGetValue(listing.Item.ItemId, out int pendingStock);
            PendingByItem[listing.Item.ItemId] = pendingStock + listing.Item.Count;
            if (listing.SellerId != 0) keptPending++;
        }
        _indexedRun = run;
        _indexedListings = listings;
        _indexedPending = pending;
        _indexedPendingCount = pending.Count;
        _snapshot = null;
        _revision++;
        if (repaired)
        {
            MclslWorldArchiveStore.MarkDirty();
            MclslDiagnostics.Error("market-invalid-listings", "天玄镜已隔离无效或重复的挂单。");
        }
    }

    private static void AddListing(MclslMarketListing listing)
    {
        EnsureIndex();
        Listings.Add(listing);
        ById[listing.ListingId] = listing;
        if (MclslItemCatalog.Get(listing.Item.ItemId)?.Category == "Artifact")
            ArtifactInstanceIds.Add(listing.Item.InstanceId);
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
        if (MclslItemCatalog.Get(listing.Item?.ItemId)?.Category == "Artifact")
            ArtifactInstanceIds.Remove(listing.Item.InstanceId);
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
        return Math.Max(1, (int)Math.Min(999999L, (long)Math.Max(1, contributionPrice) * 2));
    }

    internal static int ReferenceStonePrice(MclslItemDefinition item)
    {
        if (item == null) return 1;
        return BaseReferencePrices.TryGetValue(item.Id, out int price) ? price : ComputeReferenceStonePrice(item);
    }

    internal static int MarketStonePrice(MclslItemDefinition item)
    {
        int reference = ReferenceStonePrice(item);
        if (item == null) return reference;
        Dictionary<string, MclslMarketPriceState> prices = MclslWorldRunRepository.Current?.MarketPrices;
        return prices != null && prices.TryGetValue(item.Id, out MclslMarketPriceState state)
            && state?.StonePrice > 0 ? state.StonePrice : reference;
    }

    private static Dictionary<string, int> BuildBaseReferencePrices()
    {
        Dictionary<string, int> prices = new(StringComparer.Ordinal);
        foreach (MclslItemDefinition definition in MclslItemCatalog.All)
            prices[definition.Id] = ComputeReferenceStonePrice(definition);
        return prices;
    }

    private static int ComputeReferenceStonePrice(MclslItemDefinition item)
    {
        if (item.Category == "SpellScroll"
            && item.Id.EndsWith("_SCROLL", StringComparison.Ordinal)
            && MclslSpellSystem.TryGet(item.Id.Substring(0, item.Id.Length - 7), out MclslSpellDefinition spell))
        {
            long actualRecipeCost = 0;
            foreach (KeyValuePair<string, int> ingredient in MclslSpellScrollCrafting.Recipe(spell))
                actualRecipeCost += (long)IngredientStonePrice(ingredient.Key) * ingredient.Value;
            return MclslEconomicPolicy.CraftedStonePrice(item.Id, actualRecipeCost);
        }
        int first = IngredientStonePrice(item.IngredientA);
        int second = IngredientStonePrice(item.IngredientB);
        if (first > 0 && second > 0 && item.Category is "Pill" or "Talisman" or "Artifact")
            return MclslEconomicPolicy.CraftedStonePrice(item.Id, (long)first + second);
        return MclslEconomicPolicy.ReferenceStonePrice(item.Price, first, second);
    }

    private static int IngredientStonePrice(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId)) return 0;
        MclslItemDefinition material = MclslItemCatalog.Get(itemId);
        return material == null ? itemId.StartsWith("R", StringComparison.Ordinal) ? 2 : 0
            : Math.Max(1, material.Price);
    }

    internal static int ContributionPrice(MclslItemDefinition item)
        => MclslEconomicPolicy.ContributionPrice(MarketStonePrice(item));

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
        RecordMarketFlow(action, item.Id, count, year);
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
        EnsureIndex();
        if (Listings.Count >= MaxMarketListings) return false;
        bool listedAny = MclslArtifactSystem.TryListLowerGradeArtifact(actor, year);
        if (Listings.Count >= MaxMarketListings) return listedAny;
        if (MclslBagSystem.Peek(actor).Items.Count == 0) return listedAny;
        MclslBagState bag = MclslBagSystem.Copy(MclslBagSystem.Read(actor));
        MclslConsumableInventoryPolicy.BuildReservePlan(actor, ReservePlanScratch);
        MclslProfessionSystem.BuildCraftMaterialReserves(actor, bag, CraftReserveScratch, year);
        MclslSpellScrollCrafting.AddMaterialReserves(actor, CraftReserveScratch);
        MclslOwnedItem[] candidates = bag.Items.ToArray();
        MclslBagState before = MclslBagSystem.Copy(bag);
        Dictionary<string, int> remainingCounts = new(StringComparer.Ordinal);
        foreach (MclslOwnedItem held in candidates)
            if (held != null && held.Count > 0)
            {
                remainingCounts.TryGetValue(held.ItemId, out int existing);
                remainingCounts[held.ItemId] = (int)Math.Min(int.MaxValue, (long)existing + held.Count);
            }
        List<(MclslMarketListing Listing, MclslItemDefinition Item, int Count, int ContributionPrice)> staged = new(4);
        int itemCount = candidates.Length;
        int start = itemCount == 0 ? 0 : (int)((unchecked((ulong)MclslActorAccessor.Id(actor) + (uint)year)) % (uint)itemCount);
        int listedStacks = 0;
        for (int offset = 0; offset < itemCount && listedStacks < 4
            && Listings.Count + staged.Count < MaxMarketListings
            && (listedStacks == 0 || !MclslAnnualFrameBudget.Expired); offset++)
        {
            int i = (start + offset) % itemCount;
            MclslOwnedItem owned = candidates[i];
            MclslItemDefinition item = MclslItemCatalog.Get(owned.ItemId);
            if (item == null) continue;
            if (item.Category == "Artifact") continue; // Individual artifacts use the equipment surplus path.
            if (owned.AcquiredYear >= 0 && year <= owned.AcquiredYear) continue;
            int reserve = MclslConsumableInventoryPolicy.ReserveCount(actor, item, ReservePlanScratch, CraftReserveScratch);
            int remaining = remainingCounts.TryGetValue(item.Id, out int total) ? total : 0;
            if (remaining <= reserve) continue;
            int count = Math.Min(MaxStackListingCount, Math.Min(owned.Count, remaining - reserve));
            if (count <= 0) continue;
            MclslOwnedItem offer = new() { ItemId = item.Id, Count = count, Durability = owned.Durability,
                InstanceId = owned.InstanceId, AcquiredYear = owned.AcquiredYear };
            int contributionPrice = ContributionPrice(item);
            MclslMarketListing listing = new() { ListingId = Guid.NewGuid().ToString("N"), SellerId = MclslActorAccessor.Id(actor),
                Item = offer, Price = contributionPrice, ContributionUnitPrice = contributionPrice,
                SpiritStoneUnitPrice = MarketStonePrice(item), Year = year };
            staged.Add((listing, item, count, contributionPrice));
            owned.Count -= count;
            if (owned.Count <= 0) bag.Items.Remove(owned);
            remainingCounts[item.Id] = remaining - count;
            listedStacks++;
        }
        if (staged.Count == 0) return listedAny;
        List<MclslMarketListing> added = new(staged.Count);
        int previousListedYear = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.MarketListedYear, -1);
        string previousListedIds = MclslActorAccessor.GetString(actor, MclslActorDataKeys.MarketListedItemIds);
        string listedIds = previousListedYear == year ? previousListedIds : string.Empty;
        foreach (var entry in staged)
            if (!MclslEconomicPolicy.ListedThisYear(year, listedIds, year, entry.Item.Id))
                listedIds = (listedIds.Length == 0 ? "|" : listedIds) + entry.Item.Id + "|";
        try
        {
            foreach (var entry in staged)
            {
                AddListing(entry.Listing);
                added.Add(entry.Listing);
            }
            MclslBagSystem.Write(actor, bag);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.MarketListedYear, year);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.MarketListedItemIds, listedIds);
        }
        catch
        {
            foreach (MclslMarketListing listing in added)
                try { RemoveListing(listing); }
                catch (Exception ex) { MclslDiagnostics.Error("market-list-rollback", ex.Message); }
            try { MclslBagSystem.Write(actor, before); }
            catch (Exception ex) { MclslDiagnostics.Error("market-bag-rollback", ex.Message); }
            try
            {
                MclslActorAccessor.Set(actor, MclslActorDataKeys.MarketListedYear, previousListedYear);
                MclslActorAccessor.Set(actor, MclslActorDataKeys.MarketListedItemIds, previousListedIds);
            }
            catch (Exception ex) { MclslDiagnostics.Error("market-plan-rollback", ex.Message); }
            throw;
        }
        foreach (var entry in staged)
            try { RecordActivity("Listed", entry.Item, actor, null, entry.Count, entry.ContributionPrice,
                entry.Count * entry.ContributionPrice, ContributionCurrency, year); }
            catch (Exception ex) { MclslDiagnostics.Error("market-list-activity", ex.Message); }
        MclslWorldArchiveStore.MarkDirty();
        listedAny = true;
        return listedAny;
    }

    internal static bool TryListArtifactSurplus(Actor actor, MclslOwnedItem owned, int year,
        bool deferWhenFull = true)
    {
        MclslItemDefinition item = MclslItemCatalog.Get(owned?.ItemId);
        if (!MclslActorAccessor.Alive(actor) || item?.Category != "Artifact"
            || item.Id is "B080" or "B081" || string.IsNullOrWhiteSpace(owned.InstanceId)) return false;
        EnsureIndex();
        if (ArtifactInstanceIds.Contains(owned.InstanceId)) return false;
        if (Listings.Count >= MaxMarketListings
            && (!deferWhenFull || PendingListingCount >= MaxPendingArtifactListings)) return false;
        MclslMarketListing listing = new()
        {
            ListingId = Guid.NewGuid().ToString("N"),
            SellerId = MclslActorAccessor.Id(actor),
            Item = new MclslOwnedItem
            {
                ItemId = item.Id, InstanceId = owned.InstanceId, Count = 1,
                Durability = Math.Clamp(owned.Durability, 1, 100), AcquiredYear = owned.AcquiredYear
            },
            Price = ContributionPrice(item),
            ContributionUnitPrice = ContributionPrice(item),
            SpiritStoneUnitPrice = MarketStonePrice(item),
            Year = Math.Max(0, year)
        };
        if (Listings.Count < MaxMarketListings) AddListing(listing);
        else
        {
            MclslWorldRunState run = MclslWorldRunRepository.Current;
            run.PendingArtifactListings ??= new Queue<MclslMarketListing>();
            run.PendingArtifactListings.Enqueue(listing);
            PendingByItem.TryGetValue(listing.Item.ItemId, out int pendingStock);
            PendingByItem[listing.Item.ItemId] = pendingStock + listing.Item.Count;
            ArtifactInstanceIds.Add(listing.Item.InstanceId);
            _indexedPendingCount = run.PendingArtifactListings.Count;
        }
        try { RecordActivity("Listed", item, actor, null, 1, ContributionPrice(item),
            ContributionPrice(item), ContributionCurrency, year); }
        catch (Exception ex) { MclslDiagnostics.Error("market-artifact-activity", ex.Message); }
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
        EnsureIndex();
        quota = Math.Min(quota, pending.Count);
        while (inspected < quota && Listings.Count < MaxMarketListings && pending.Count > 0)
        {
            MclslMarketListing listing = pending.Dequeue();
            if (listing?.Item != null && PendingByItem.TryGetValue(listing.Item.ItemId, out int pendingStock))
            {
                pendingStock -= listing.Item.Count;
                if (pendingStock > 0) PendingByItem[listing.Item.ItemId] = pendingStock;
                else PendingByItem.Remove(listing.Item.ItemId);
            }
            _indexedPendingCount = pending.Count;
            inspected++;
            MclslItemDefinition pendingItem = MclslItemCatalog.Get(listing?.Item?.ItemId);
            if (listing?.Item == null || pendingItem == null
                || listing.SellerId != 0 && pendingItem.Category != "Artifact"
                || listing.SellerId == 0 && pendingItem.Category == "Artifact" && string.IsNullOrWhiteSpace(listing.Item.InstanceId))
            {
                if (!string.IsNullOrEmpty(listing?.Item?.InstanceId))
                    ArtifactInstanceIds.Remove(listing.Item.InstanceId);
                continue;
            }
            if (listing.SellerId != 0 && (!MclslActorRegistry.ResolveKnownOrWorld(listing.SellerId, out Actor seller)
                || !MclslActorAccessor.Alive(seller)))
            {
                // The owner may be temporarily unavailable. Keep the item in escrow
                // until identity/death settlement can be checked; never drop it here.
                pending.Enqueue(listing);
                PendingByItem.TryGetValue(listing.Item.ItemId, out int deferredStock);
                PendingByItem[listing.Item.ItemId] = deferredStock + listing.Item.Count;
                _indexedPendingCount = pending.Count;
                continue;
            }
            AddListing(listing);
            published++;
        }
        if (inspected > 0) MclslWorldArchiveStore.MarkDirty();
        return published;
    }

    internal static bool QueueEstateItems(IReadOnlyList<MclslOwnedItem> items, int year, Func<bool> beforeEnqueue = null)
    {
        if (items == null) return false;
        EnsureIndex();
        List<MclslMarketListing> staged = new();
        HashSet<string> newInstances = new(StringComparer.Ordinal);
        foreach (MclslOwnedItem owned in items)
        {
            MclslItemDefinition definition = MclslItemCatalog.Get(owned?.ItemId);
            if (definition == null || owned.Count <= 0) return false;
            bool artifact = definition.Category == "Artifact";
            if (artifact && (owned.Count != 1 || string.IsNullOrWhiteSpace(owned.InstanceId)
                || ArtifactInstanceIds.Contains(owned.InstanceId) || !newInstances.Add(owned.InstanceId))) return false;
            int remaining = owned.Count;
            while (remaining > 0)
            {
                int count = artifact ? 1 : Math.Min(MaxStackListingCount, remaining);
                staged.Add(new MclslMarketListing
                {
                    ListingId = Guid.NewGuid().ToString("N"), SellerId = 0,
                    Item = new MclslOwnedItem { ItemId = definition.Id, InstanceId = owned.InstanceId,
                        Count = count, Durability = owned.Durability, AcquiredYear = owned.AcquiredYear },
                    Price = ContributionPrice(definition), ContributionUnitPrice = ContributionPrice(definition),
                    SpiritStoneUnitPrice = MarketStonePrice(definition), Year = Math.Max(0, year)
                });
                remaining -= count;
            }
        }
        // Validate every item before the family money is settled. A failed callback
        // leaves both the family inventory and the pending market queue untouched.
        if (beforeEnqueue != null && !beforeEnqueue()) return false;
        Queue<MclslMarketListing> pending = MclslWorldRunRepository.Current.PendingArtifactListings ??= new();
        foreach (MclslMarketListing listing in staged) pending.Enqueue(listing);
        _indexedPendingCount = -1;
        MclslWorldArchiveStore.MarkDirty();
        return true;
    }

    internal static bool TryBuy(Actor buyer, string listingId, int year = -1, int requestedCount = 1, string currency = null)
    {
        if (!MclslActorAccessor.Alive(buyer) || MclslBagSystem.IsLocked(buyer)
            || string.IsNullOrEmpty(listingId) || requestedCount < 1 || requestedCount > MaxStackListingCount) return false;
        int acquiredYear = year < 0 ? Math.Max(1, MclslRuntime.CurrentYear()) : year;
        if (acquiredYear <= 0) return false;
        EnsureIndex();
        if (!ById.TryGetValue(listingId, out MclslMarketListing listing)) return false;
        MclslItemDefinition item = MclslItemCatalog.Get(listing?.Item?.ItemId);
        if (item == null) return false;
        long buyerId = MclslActorAccessor.Id(buyer);
        if (buyerId == listing.SellerId || listing.Item.Count < requestedCount
            || item.Category == "Artifact" && requestedCount != 1) return false;
        int contributionUnit = listing.ContributionUnitPrice > 0 ? listing.ContributionUnitPrice : Math.Max(1, listing.Price);
        int stoneUnit = listing.SpiritStoneUnitPrice > 0 ? listing.SpiritStoneUnitPrice : SpiritStonePrice(item, contributionUnit);
        long contributionFunds = MclslActorAccessor.GetMoney(buyer, MclslActorDataKeys.Contribution);
        long stoneFunds = MclslActorAccessor.GetMoney(buyer, MclslActorDataKeys.SpiritStones);
        Actor seller = null;
        if (listing.SellerId != 0 && !MclslActorRegistry.ResolveKnownOrWorld(listing.SellerId, out seller))
            return false;
        if (seller != null && !MclslActorAccessor.Alive(seller))
        {
            MclslFamilySystem.OnDeath(seller);
            return false;
        }
        long sellerContribution = seller == null ? 0 : MclslActorAccessor.GetMoney(seller, MclslActorDataKeys.Contribution);
        long sellerStones = seller == null ? 0 : MclslActorAccessor.GetMoney(seller, MclslActorDataKeys.SpiritStones);
        if (currency == null)
        {
            MclslMarketPayment selected = MclslMarketPaymentPolicy.Select(contributionUnit, stoneUnit, requestedCount,
                contributionFunds, stoneFunds, sellerContribution, sellerStones);
            currency = selected switch { MclslMarketPayment.Contribution => ContributionCurrency,
                MclslMarketPayment.SpiritStone => SpiritStoneCurrency, _ => null };
        }
        MclslMarketPayment payment = currency == ContributionCurrency ? MclslMarketPayment.Contribution
            : currency == SpiritStoneCurrency ? MclslMarketPayment.SpiritStone : MclslMarketPayment.None;
        int unitPrice = currency == ContributionCurrency ? contributionUnit : stoneUnit;
        long buyerFunds = payment == MclslMarketPayment.Contribution ? contributionFunds : stoneFunds;
        long sellerFunds = payment == MclslMarketPayment.Contribution ? sellerContribution : sellerStones;
        if (!MclslMarketPaymentPolicy.CanPay(payment, unitPrice, requestedCount, buyerFunds, sellerFunds)) return false;
        int totalPrice = checked(unitPrice * requestedCount);
        MclslBagState originalBag = MclslBagSystem.Copy(MclslBagSystem.Read(buyer));
        if (item.Category == "Artifact" && MclslBagSystem.Count(originalBag, item.Id) > 0) return false;
        MclslBagState bag = MclslBagSystem.Copy(originalBag);
        string fundsKey = currency == ContributionCurrency ? MclslActorDataKeys.Contribution : MclslActorDataKeys.SpiritStones;
        string feeCarryKey = currency == ContributionCurrency ? MclslActorDataKeys.MarketFeeCarryContribution
            : MclslActorDataKeys.MarketFeeCarryStones;
        int originalFeeCarry = seller == null ? 0 : MclslActorAccessor.GetInt(seller, feeCarryKey);
        int fee = MclslEconomicPolicy.SaleServiceFee(totalPrice, originalFeeCarry, out int nextFeeCarry);
        bool completedListing = requestedCount == listing.Item.Count;
        int originalListingCount = listing.Item.Count;
        int originalPurchaseYear = MclslActorAccessor.GetInt(buyer, MclslActorDataKeys.MarketPurchaseYear, -1);
        int originalPurchaseCount = MclslActorAccessor.GetInt(buyer, MclslActorDataKeys.MarketPurchaseCount, 0);
        int purchasedBeforeSettlement = PurchasesInYear(buyer, acquiredYear);
        if (purchasedBeforeSettlement == int.MaxValue) return false;
        bool listingChanged = false;
        try
        {
            if (item.Category == "Artifact")
                bag.Items.Add(new MclslOwnedItem { ItemId = item.Id, InstanceId = listing.Item.InstanceId,
                    Durability = listing.Item.Durability, Count = 1, AcquiredYear = acquiredYear });
            else MclslBagSystem.Add(bag, item.Id, requestedCount, acquiredYear: acquiredYear);
            MclslBagSystem.Write(buyer, bag);
            if (seller != null)
            {
                MclslActorAccessor.Set(seller, feeCarryKey, nextFeeCarry);
            }
            listingChanged = true;
            if (completedListing)
            {
                if (!RemoveListing(listing)) throw new InvalidOperationException("成交挂单已失效");
            }
            else
            {
                listing.Item.Count -= requestedCount;
                _snapshot = null;
                _revision++;
            }
            MclslActorAccessor.Set(buyer, MclslActorDataKeys.MarketPurchaseYear, acquiredYear);
            MclslActorAccessor.Set(buyer, MclslActorDataKeys.MarketPurchaseCount,
                MclslMarketPurchaseSequence.AfterPurchase(purchasedBeforeSettlement));
            string buyerAccount = MclslEconomyCommands.Account(buyer);
            MclslCurrency coin = MclslEconomyCommands.Currency(fundsKey);
            MclslEconomicOperation[] operations = seller == null
                ? new[] { new MclslEconomicOperation(MclslEconomicKind.Consume, coin, totalPrice, fromAccount: buyerAccount) }
                : new[]
                {
                    new MclslEconomicOperation(MclslEconomicKind.Transfer, coin, totalPrice - fee,
                        buyerAccount, MclslEconomyCommands.Account(seller)),
                    new MclslEconomicOperation(MclslEconomicKind.Consume, coin, fee, fromAccount: buyerAccount)
                };
            long receipt = ((long)acquiredYear << 32) | (uint)(purchasedBeforeSettlement + 1);
            MclslEconomicResult settlement = MclslEconomyCommands.Commit(acquiredYear, operations,
                "market/" + buyerAccount, receipt);
            if (settlement != MclslEconomicResult.Applied)
                throw new InvalidOperationException("交易资金提交失败：" + settlement);
        }
        catch (Exception ex)
        {
            try
            {
                if (listingChanged)
                {
                    listing.Item.Count = originalListingCount;
                    if (completedListing && !ById.ContainsKey(listing.ListingId)) AddListing(listing);
                    else { _snapshot = null; _revision++; }
                }
                if (seller != null) MclslActorAccessor.Set(seller, feeCarryKey, originalFeeCarry);
                MclslActorAccessor.Set(buyer, MclslActorDataKeys.MarketPurchaseYear, originalPurchaseYear);
                MclslActorAccessor.Set(buyer, MclslActorDataKeys.MarketPurchaseCount, originalPurchaseCount);
                MclslBagSystem.Write(buyer, originalBag);
                MclslWorldArchiveStore.MarkDirty();
            }
            catch (Exception rollback) { MclslDiagnostics.Error("market-rollback", rollback.Message); }
            MclslDiagnostics.Error("market-settlement", ex.Message);
            return false;
        }
        MclslEconomyCommands.NotifyWallet(buyer);
        if (seller != null) MclslEconomyCommands.NotifyWallet(seller);
        if (item.Category == "SpellScroll")
            try { MclslSpellSystem.TryStudyScroll(buyer, item.Id); }
            catch (Exception ex) { MclslDiagnostics.Error("market-scroll-study", ex.Message); }
        try
        {
            if (item.Category == "Artifact") MclslArtifactSystem.OnArtifactAcquired(buyer);
            MclslRecipeKnowledge.LearnFromOwnedItem(buyer, item.Id);
        }
        catch (Exception ex)
        {
            MclslDiagnostics.Error("market-post-settlement", "天玄镜成交后回调失败：" + ex.Message);
        }
        try { RecordActivity("Purchased", item, buyer, seller, requestedCount, unitPrice, totalPrice, currency, acquiredYear); }
        catch (Exception ex) { MclslDiagnostics.Error("market-record-activity", ex.Message); }
        try { MclslWorldRunRepository.AddItemAcquisitionEvent(
            acquiredYear, buyer, item.Id, requestedCount, "天玄镜交易"); }
        catch (Exception ex) { MclslDiagnostics.Error("market-acquisition-event", ex.Message); }
        return true;
    }

    internal static int PurchasesInYear(Actor actor, int year)
    {
        if (actor?.data == null) return 0;
        return MclslMarketPurchaseSequence.CountInYear(
            MclslActorAccessor.GetInt(actor, MclslActorDataKeys.MarketPurchaseYear),
            MclslActorAccessor.GetInt(actor, MclslActorDataKeys.MarketPurchaseCount), year);
    }

    internal static void ForgetPurchaseBatch(Actor actor) => ForgetPurchaseBatch(MclslActorAccessor.Id(actor));

    internal static void ForgetPurchaseBatch(long id)
    {
        if (id > 0 && (MclslWorldRunRepository.Current?.PendingMarketPurchases?.Remove(id) ?? false))
            MclslWorldArchiveStore.MarkDirty();
    }

    internal static MclslMarketPurchaseStep ProcessPurchaseBatch(Actor actor, int year)
    {
        if (!MclslActorAccessor.Alive(actor) || MclslBagSystem.IsLocked(actor) || year <= 0)
        {
            ForgetPurchaseBatch(actor);
            return MclslMarketPurchaseStep.Unavailable;
        }
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        run.PendingMarketPurchases ??= new();
        EnsureIndex();
        if (Listings.Count == 0) { ForgetPurchaseBatch(actor); return MclslMarketPurchaseStep.Complete; }
        long actorId = MclslActorAccessor.Id(actor);
        if (!run.PendingMarketPurchases.TryGetValue(actorId, out MclslMarketPurchaseBatch batch)
            || batch?.Year != year || !MclslMarketPurchasePolicy.ValidShape(batch, MaxPurchaseNeeds))
        {
            long needsSample = MclslPerformanceProbe.Begin();
            try { batch = BuildPurchaseBatch(actor, year); }
            finally { MclslPerformanceProbe.End("天玄镜.需求清单生成", needsSample); }
            if (batch.Needs.Count == 0)
            {
                ForgetPurchaseBatch(actor);
                return MclslMarketPurchaseStep.Complete;
            }
            run.PendingMarketPurchases[actorId] = batch;
            MclslWorldArchiveStore.MarkDirty();
        }
        if (MclslMarketPurchasePolicy.IsComplete(batch))
        {
            ForgetPurchaseBatch(actor);
            return MclslMarketPurchaseStep.Complete;
        }
        MclslMarketPurchaseNeed need = batch.Needs[batch.Cursor];
        if (need == null || string.IsNullOrEmpty(need.ItemId))
        {
            MclslMarketPurchasePolicy.Advance(batch, false);
            MclslWorldArchiveStore.MarkDirty();
            return MclslMarketPurchaseStep.Pending;
        }
        MclslBagState bag = MclslBagSystem.Peek(actor);
        bool craftApplicable = (need.Kind is "Craft" or "Shared")
            && batch.Profession == CurrentProfession(actor) && batch.Grade == MclslProfessionSystem.GetGrade(actor);
        bool finishedApplicable = (need.Kind is "Finished" or "Shared")
            && MclslBagSystem.Count(bag, need.ItemId) == 0
                && (!need.ItemId.EndsWith("_SCROLL", StringComparison.Ordinal)
                    || !MclslSpellSystem.Knows(actor, need.ItemId.Substring(0, need.ItemId.Length - "_SCROLL".Length)));
        if (!(craftApplicable || finishedApplicable) || need.Remaining <= 0 || need.Remaining > 2
            || need.Kind is not ("Craft" or "Finished" or "Shared") || MclslItemCatalog.Get(need.ItemId) == null)
        {
            MclslMarketPurchasePolicy.Advance(batch, false);
            MclslWorldArchiveStore.MarkDirty();
            return MclslMarketPurchaseStep.Pending;
        }
        long sample = MclslPerformanceProbe.Begin();
        bool purchased;
        try { purchased = TryBuyDemand(actor, need.ItemId, year, bag); }
        finally { MclslPerformanceProbe.End("天玄镜.挂单查找交割", sample); }
        if (purchased)
        {
            // TryBuy already invokes optional scroll study after settlement; do not repeat it here.
            MclslMarketPurchasePolicy.Advance(batch, true);
            MclslWorldArchiveStore.MarkDirty();
        }
        else
        {
            MclslMarketPurchasePolicy.Advance(batch, false);
            MclslWorldArchiveStore.MarkDirty();
        }
        return MclslMarketPurchaseStep.Pending;
    }

    private static MclslMarketPurchaseBatch BuildPurchaseBatch(Actor actor, int year)
    {
        MclslMarketPurchaseBatch batch = new() { Year = year,
            Profession = CurrentProfession(actor), Grade = MclslProfessionSystem.GetGrade(actor) };
        MclslBagState bag = MclslBagSystem.Peek(actor);
        MclslProfessionSystem.BuildCraftMaterialReserves(actor, bag, CraftReserveScratch, year);
        List<MclslMarketPurchaseNeed> craft = CraftNeedsScratch;
        craft.Clear();
        CraftKeysScratch.Clear();
        foreach (string key in CraftReserveScratch.Keys) CraftKeysScratch.Add(key);
        CraftKeysScratch.Sort(StringComparer.Ordinal);
        foreach (string key in CraftKeysScratch)
        {
            int missing = Math.Max(0, CraftReserveScratch[key] - MclslBagSystem.Count(bag, key));
            if (missing > 0) craft.Add(new MclslMarketPurchaseNeed { ItemId = key, Kind = "Craft", Remaining = missing });
        }
        MclslSpellScrollCrafting.AddMissingPurchaseNeeds(actor, bag, craft);
        DesiredItemIdsScratch.Clear();
        MclslConsumableInventoryPolicy.BuildReservePlan(actor, ReservePlanScratch);
        BuildFinishedGoodsNeeds(actor, bag, DesiredItemIdsScratch);
        AddSpellScrollNeed(actor, bag, DesiredItemIdsScratch);
        List<MclslMarketPurchaseNeed> finished = FinishedNeedsScratch;
        finished.Clear(); FinishedIdsScratch.Clear();
        foreach (string id in DesiredItemIdsScratch)
            if (FinishedIdsScratch.Add(id))
                finished.Add(new MclslMarketPurchaseNeed { ItemId = id, Kind = "Finished", Remaining = 1 });
        Dictionary<string, MclslMarketPurchaseNeed> craftByItem = CraftNeedsByItemScratch;
        craftByItem.Clear();
        foreach (MclslMarketPurchaseNeed need in craft) craftByItem.Add(need.ItemId, need);
        for (int i = finished.Count - 1; i >= 0; i--)
            if (craftByItem.TryGetValue(finished[i].ItemId, out MclslMarketPurchaseNeed shared))
            {
                shared.Kind = "Shared";
                finished.RemoveAt(i);
            }
        DesiredItemIdsScratch.Clear();
        CraftReserveScratch.Clear();
        int first = (int)(unchecked((ulong)MclslActorAccessor.Id(actor) + (uint)year) % 2UL);
        for (int i = 0; i < Math.Max(craft.Count, finished.Count); i++)
        {
            if (first == 0 && i < craft.Count) batch.Needs.Add(craft[i]);
            if (i < finished.Count) batch.Needs.Add(finished[i]);
            if (first == 1 && i < craft.Count) batch.Needs.Add(craft[i]);
        }
        return batch;
    }

    private static string CurrentProfession(Actor actor)
    {
        string profession = MclslProfessionSystem.FromTrait(actor);
        return string.IsNullOrWhiteSpace(profession)
            ? MclslActorAccessor.GetString(actor, MclslActorDataKeys.Profession, string.Empty)
            : profession;
    }

    private static bool TryBuyDemand(Actor actor, string itemId, int year, MclslBagState bag)
    {
        if (MclslEconomicPolicy.ListedThisYear(
            MclslActorAccessor.GetInt(actor, MclslActorDataKeys.MarketListedYear, -1),
            MclslActorAccessor.GetString(actor, MclslActorDataKeys.MarketListedItemIds), year, itemId)
            && !MclslEconomicPolicy.IsPriorityPurchase(itemId)) return false;
        if (!ByItem.TryGetValue(itemId, out List<MclslMarketListing> offers)) return false;
        MclslItemDefinition item = MclslItemCatalog.Get(itemId);
        if (item == null || item.Category == "Artifact" && MclslBagSystem.Count(bag, itemId) > 0) return false;
        long actorId = MclslActorAccessor.Id(actor);
        int realmIndex = MclslRealmIds.Index(MclslActorAccessor.Realm(actor));
        bool priority = MclslEconomicPolicy.IsPriorityPurchase(itemId);
        long contributionFunds = MclslEconomicPolicy.SpendableForAutomaticPurchase(
            MclslActorAccessor.GetMoney(actor, MclslActorDataKeys.Contribution), realmIndex, true, priority);
        long stoneFunds = MclslEconomicPolicy.SpendableForAutomaticPurchase(
            MclslActorAccessor.GetMoney(actor, MclslActorDataKeys.SpiritStones), realmIndex, false, priority);
        HashSet<string> rejected = null;
        while (true)
        {
            MclslMarketListing best = null;
            string bestCurrency = null;
            long bestStoneCost = long.MaxValue;
            MclslPerformanceProbe.RecordMarketOffersInspected(offers.Count);
            for (int i = 0; i < offers.Count; i++)
            {
                MclslMarketListing offer = offers[i];
                if (offer?.Item == null || offer.Item.Count < 1 || offer.SellerId == actorId
                    || rejected?.Contains(offer.ListingId) == true) continue;
                int contribution = offer.ContributionUnitPrice > 0 ? offer.ContributionUnitPrice : Math.Max(1, offer.Price);
                int stones = offer.SpiritStoneUnitPrice > 0 ? offer.SpiritStoneUnitPrice : SpiritStonePrice(item, contribution);
                if (contribution > contributionFunds && stones > stoneFunds) continue;
                Actor seller = null;
                if (offer.SellerId != 0 && (!MclslActorRegistry.ResolveKnownOrWorld(offer.SellerId, out seller)
                    || !MclslActorAccessor.Alive(seller))) continue;
                MclslMarketPayment payment = MclslMarketPaymentPolicy.Select(contribution, stones, 1,
                    contributionFunds, stoneFunds,
                    seller == null ? 0 : MclslActorAccessor.GetMoney(seller, MclslActorDataKeys.Contribution),
                    seller == null ? 0 : MclslActorAccessor.GetMoney(seller, MclslActorDataKeys.SpiritStones));
                if (payment == MclslMarketPayment.None) continue;
                long stoneCost = payment == MclslMarketPayment.Contribution ? (long)contribution * 2 : stones;
                if (best == null || stoneCost < bestStoneCost
                    || stoneCost == bestStoneCost && offer.Year < best.Year)
                {
                    best = offer;
                    bestStoneCost = stoneCost;
                    bestCurrency = payment == MclslMarketPayment.Contribution ? ContributionCurrency : SpiritStoneCurrency;
                }
            }
            if (best == null) return false;
            if (TryBuy(actor, best.ListingId, year, currency: bestCurrency)) return true;
            (rejected ??= new HashSet<string>(StringComparer.Ordinal)).Add(best.ListingId);
        }
    }


    private static void AddSpellScrollNeed(Actor actor, MclslBagState bag, List<string> desired)
    {
        int realm = MclslRealmIds.Index(MclslActorAccessor.Realm(actor));
        if (realm < 0) return;
        long actorId = MclslActorAccessor.Id(actor);
        string[] roots = MclslSpiritualRootSystem.RootAttributes(actor);
        for (int pass = 0; pass < 2; pass++)
        {
            string choice = null;
            int lowestPrice = int.MaxValue;
            foreach (MclslSpellDefinition spell in MclslSpellSystem.All)
            {
                if ((Array.IndexOf(roots, spell.Law) >= 0) != (pass == 0)) continue;
                string scrollId = spell.Id + "_SCROLL";
                if (spell.MinRealm > realm || MclslSpellSystem.Knows(actor, spell.Id)
                    || MclslBagSystem.Count(bag, scrollId) > 0
                    || !ByItem.TryGetValue(scrollId, out List<MclslMarketListing> offers)) continue;
                foreach (MclslMarketListing offer in offers)
                {
                    if (offer?.Item == null || offer.Item.Count < 1 || offer.SellerId == actorId) continue;
                    int contribution = offer.ContributionUnitPrice > 0 ? offer.ContributionUnitPrice
                        : Math.Max(1, offer.Price);
                    int stones = offer.SpiritStoneUnitPrice > 0 ? offer.SpiritStoneUnitPrice
                        : SpiritStonePrice(MclslItemCatalog.Get(scrollId), contribution);
                    int price = Math.Min(stones, contribution * MclslEconomicPolicy.StonePerContribution);
                    if (price >= lowestPrice) continue;
                    if (offer.SellerId != 0 && (!MclslActorRegistry.ResolveKnownOrWorld(offer.SellerId, out Actor seller)
                        || !MclslActorAccessor.Alive(seller))) continue;
                    lowestPrice = price;
                    choice = scrollId;
                }
            }
            if (choice == null) continue;
            desired.Add(choice);
            return;
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
            if (itemId == "D004" && MclslActorAccessor.GetInt(actor, "mclsl.architecture.v2.actor.taishang_taken") != 0) continue;
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
                if (MclslBagSystem.Count(bag, talismanId) > 0
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

    private static bool TryReturnExpiredListing(MclslMarketListing listing, Actor seller, int year)
    {
        if (listing?.Item == null || MclslBagSystem.IsLocked(seller)) return false;
        MclslBagState before = MclslBagSystem.Copy(MclslBagSystem.Read(seller));
        MclslBagState returned = MclslBagSystem.Copy(before);
        MclslOwnedItem item = MclslFamilyTransferPolicy.CopyItem(listing.Item);
        item.AcquiredYear = year; // Prevent an immediate listing of the returned stock.
        returned.Items.Add(item);
        if (!MclslInventoryDataRules.IsValid(returned)) return false;
        MclslBagSystem.Write(seller, returned);
        try
        {
            if (!RemoveListing(listing)) throw new InvalidOperationException("到期挂单在退回时失效");
        }
        catch
        {
            MclslBagSystem.Write(seller, before);
            throw;
        }
        MclslItemDefinition definition = MclslItemCatalog.Get(item.ItemId);
        try { RecordActivity("Cancelled", definition, seller, null, item.Count,
            listing.ContributionUnitPrice, item.Count * listing.ContributionUnitPrice,
            ContributionCurrency, year); }
        catch (Exception ex) { MclslDiagnostics.Error("market-cancel-activity", ex.Message); }
        MclslWorldArchiveStore.MarkDirty();
        return true;
    }


    internal static void BeginAnnualPrune(int year)
    {
        MclslAnnualBatchState batch = MclslWorldRunRepository.Current.AnnualBatch;
        if (batch.MarketPruneYear == year) return;
        UpdateMarketPrices(year);
        batch.MarketPruneYear = year;
        batch.MarketPruneCursor = 0;
        batch.MarketRecycleSpentStones = 0;
        batch.MarketRecycleBudgetStones = MclslWorldRunRepository.Current.Economy.Years.TryGetValue(year - 1,
            out MclslEconomicYearTotals previousYear) ? previousYear.SpiritStones.Issued / 10 : 0;
        batch.MarketPruneListingIds = new List<string>(Listings.Count);
        foreach (MclslMarketListing listing in Listings)
            if (!string.IsNullOrWhiteSpace(listing?.ListingId)) batch.MarketPruneListingIds.Add(listing.ListingId);
        MclslWorldArchiveStore.MarkDirty();
    }

    private static bool TryRecycleExpiredListing(MclslMarketListing listing, MclslItemDefinition definition,
        Actor seller, MclslAnnualBatchState batch)
    {
        if (listing?.Item == null || definition == null || seller == null
            || !MclslEconomicPolicy.CanDiscardPublicStock(definition)) return false;
        int unitPrice = MclslEconomicPolicy.RecycleStonePrice(ReferenceStonePrice(definition));
        long total = checked((long)unitPrice * listing.Item.Count);
        long available = Math.Max(0, batch.MarketRecycleBudgetStones - batch.MarketRecycleSpentStones);
        if (total <= 0 || total > available) return false;
        MclslEconomicResult result = MclslEconomyCommands.Commit(batch.MarketPruneYear,
            new[] { new MclslEconomicOperation(MclslEconomicKind.Recycle, MclslCurrency.SpiritStone,
                total, toAccount: MclslEconomyCommands.Account(seller)) },
            "market-recycle/" + listing.ListingId, 1);
        if (result is not (MclslEconomicResult.Applied or MclslEconomicResult.AlreadyApplied)) return false;
        listing.SellerId = 0;
        listing.Year = batch.MarketPruneYear;
        listing.SpiritStoneUnitPrice = MarketStonePrice(definition);
        listing.ContributionUnitPrice = ContributionPrice(definition);
        listing.Price = listing.ContributionUnitPrice;
        batch.MarketRecycleSpentStones = checked(batch.MarketRecycleSpentStones + total);
        MclslEconomyCommands.NotifyWallet(seller);
        RecordActivity("Recycled", definition, seller, null, listing.Item.Count, unitPrice,
            checked(unitPrice * listing.Item.Count), SpiritStoneCurrency, batch.MarketPruneYear);
        _snapshot = null;
        _revision++;
        return true;
    }

    private static void RecordMarketFlow(string action, string itemId, int count, int year)
    {
        if (year <= 0 || count <= 0 || string.IsNullOrWhiteSpace(itemId)
            || action is not ("Listed" or "Purchased")) return;
        Dictionary<string, MclslMarketPriceState> prices = MclslWorldRunRepository.Current.MarketPrices ??= new();
        if (!prices.TryGetValue(itemId, out MclslMarketPriceState state) || state == null)
            prices[itemId] = state = new MclslMarketPriceState { StonePrice = ReferenceStonePrice(MclslItemCatalog.Get(itemId)) };
        Dictionary<int, int> samples = action == "Listed" ? state.SupplyByYear : state.FundedDemandByYear;
        samples.TryGetValue(year, out int current);
        samples[year] = current >= int.MaxValue - count ? int.MaxValue : current + count;
    }

    private static void UpdateMarketPrices(int year)
    {
        Dictionary<string, MclslMarketPriceState> prices = MclslWorldRunRepository.Current.MarketPrices ??= new();
        int firstYear = Math.Max(0, year - 5);
        foreach (MclslItemDefinition item in MclslItemCatalog.All)
        {
            if (item == null) continue;
            if (!prices.TryGetValue(item.Id, out MclslMarketPriceState state) || state == null)
                prices[item.Id] = state = new MclslMarketPriceState { StonePrice = ReferenceStonePrice(item) };
            state.SupplyByYear ??= new();
            state.FundedDemandByYear ??= new();
            foreach (int oldYear in state.SupplyByYear.Keys.Where(x => x < firstYear).ToArray()) state.SupplyByYear.Remove(oldYear);
            foreach (int oldYear in state.FundedDemandByYear.Keys.Where(x => x < firstYear).ToArray()) state.FundedDemandByYear.Remove(oldYear);
            long supply = state.SupplyByYear.Where(x => x.Key < year).Sum(x => (long)x.Value);
            long demand = state.FundedDemandByYear.Where(x => x.Key < year).Sum(x => (long)x.Value);
            state.FiveYearSupply = (int)Math.Min(int.MaxValue, supply);
            state.FiveYearFundedDemand = (int)Math.Min(int.MaxValue, demand);
            state.StonePrice = MclslEconomicPolicy.AdjustMarketStonePrice(ReferenceStonePrice(item),
                state.StonePrice, state.FiveYearSupply, state.FiveYearFundedDemand);
            state.LastUpdatedYear = year;
        }
    }

    internal static bool TickAnnualPrune()
    {
        MclslAnnualBatchState batch = MclslWorldRunRepository.Current.AnnualBatch;
        batch.MarketPruneListingIds ??= new();
        try { EnsureIndex(); }
        catch (Exception ex)
        {
            MclslWorldRunRepository.RecordAnnualFailure(batch.MarketPruneYear, "天玄镜", string.Empty,
                "MarketPruneIndex", 1, "未结算", ex.Message);
            batch.WorldBlocked = true;
            MclslWorldArchiveStore.MarkDirty();
            return false;
        }
        batch.MarketPruneCursor = Math.Max(0, batch.MarketPruneCursor);
        if (batch.MarketPruneCursor >= batch.MarketPruneListingIds.Count) return true;
        while (batch.MarketPruneCursor < batch.MarketPruneListingIds.Count
            && MclslAnnualFrameBudget.TryConsumeOperation())
        {
            string id = batch.MarketPruneListingIds[batch.MarketPruneCursor];
            // Removing a listing and advancing its cursor happen on the game thread.
            // Retain a failed target's cursor; it may already have changed escrow.
            try
            {
                if (ById.TryGetValue(id, out MclslMarketListing listing))
                {
                    MclslItemDefinition definition = MclslItemCatalog.Get(listing.Item?.ItemId);
                    if (listing.SellerId != 0)
                    {
                        bool sellerResolved = MclslActorRegistry.ResolveKnownOrWorld(listing.SellerId, out Actor seller);
                        if (sellerResolved && !MclslActorAccessor.Alive(seller))
                            MclslFamilySystem.OnDeath(seller);
                        // Family settlement may already have removed the escrow.
                        if (sellerResolved && MclslActorAccessor.Alive(seller) && ById.ContainsKey(id)
                            && MclslEconomicPolicy.OrderExpired(batch.MarketPruneYear, listing.Year))
                        {
                            if (!TryRecycleExpiredListing(listing, definition, seller, batch))
                                TryReturnExpiredListing(listing, seller, batch.MarketPruneYear);
                        }
                    }
                    else if (MclslEconomicPolicy.CanDiscardPublicStock(definition)
                        && MclslEconomicPolicy.PublicOrdinaryStockExpired(batch.MarketPruneYear, listing.Year)
                        && RemoveListing(listing))
                    {
                        try { RecordActivity("Destroyed", definition, null, null, listing.Item.Count,
                            listing.ContributionUnitPrice,
                            listing.Item.Count * listing.ContributionUnitPrice,
                            ContributionCurrency, batch.MarketPruneYear); }
                        catch (Exception ex) { MclslDiagnostics.Error("market-destroy-activity", ex.Message); }
                        MclslWorldArchiveStore.MarkDirty();
                    }
                }
            }
            catch (Exception ex)
            {
                MclslWorldRunRepository.RecordAnnualFailure(batch.MarketPruneYear, "天玄镜", id,
                    "MarketPrune", 1, "未结算", ex.Message);
                batch.WorldBlocked = true;
                MclslWorldArchiveStore.MarkDirty();
                return false;
            }
            batch.MarketPruneCursor++;
            MclslWorldArchiveStore.MarkDirty();
        }
        if (batch.MarketPruneCursor < batch.MarketPruneListingIds.Count) return false;
        batch.MarketPruneListingIds.Clear();
        batch.MarketPruneCursor = 0;
        return true;
    }

}
