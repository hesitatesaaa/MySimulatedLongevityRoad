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
    private static readonly HashSet<string> ArtifactInstanceIds = new(StringComparer.Ordinal);
    private static readonly List<string> DesiredItemIdsScratch = new(8);
    private static readonly string[] ExtraCombatNeeds = { "F021", "F022", "F023", "F024", "F025", "F026", "F027" };
    private static readonly string[] ManaNeeds = { "D013", "D017", "D020", "F020" };
    private static readonly List<string> ReservePlanScratch = new();
    private static readonly Dictionary<string, int> CraftReserveScratch = new(StringComparer.Ordinal);
    private static MclslMarketListing[] _snapshot;
    private static MclslMarketActivity[] _listingActivitySnapshot;
    private static MclslMarketActivity[] _purchaseActivitySnapshot;
    private static int _revision;
    internal static int ActiveListingCount => MclslWorldRunRepository.Current?.TianxuanListings?.Count ?? 0;
    internal static int PendingPurchaseBatchCount => MclslWorldRunRepository.Current?.PendingMarketPurchases?.Count ?? 0;
    internal static int Revision { get { EnsureIndex(); return _revision; } }

    internal static void ClearRuntime()
    {
        _indexedRun = null;
        _indexedListings = null;
        _indexedPending = null;
        _indexedPendingCount = 0;
        ById.Clear();
        ByItem.Clear();
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
            if (listing.ContributionUnitPrice <= 0) listing.ContributionUnitPrice = Math.Max(1, listing.Price > 0 ? listing.Price : canonical.Price);
            if (listing.SpiritStoneUnitPrice <= 0) listing.SpiritStoneUnitPrice = SpiritStonePrice(canonical, listing.ContributionUnitPrice);
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
            if (item?.Category != "Artifact" || listing.Item.Count != 1
                || string.IsNullOrWhiteSpace(listing.ListingId)
                || string.IsNullOrWhiteSpace(listing.Item.InstanceId)
                || seenIds.Contains(listing.ListingId)
                || seenInstances.Contains(listing.Item.InstanceId)
                || keptPending >= MaxPendingArtifactListings)
            { repaired = true; continue; }
            seenIds.Add(listing.ListingId);
            seenInstances.Add(listing.Item.InstanceId);
            ArtifactInstanceIds.Add(listing.Item.InstanceId);
            pending.Enqueue(listing);
            keptPending++;
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
        EnsureIndex();
        if (Listings.Count >= MaxMarketListings) return false;
        bool listedArtifact = MclslArtifactSystem.TryListLowerGradeArtifact(actor, year);
        if (Listings.Count >= MaxMarketListings) return listedArtifact;
        if (MclslBagSystem.Peek(actor).Items.Count == 0) return listedArtifact;
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
            ArtifactInstanceIds.Add(listing.Item.InstanceId);
            _indexedPendingCount = run.PendingArtifactListings.Count;
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
        EnsureIndex();
        quota = Math.Min(quota, pending.Count);
        while (inspected < quota && Listings.Count < MaxMarketListings && pending.Count > 0)
        {
            MclslMarketListing listing = pending.Dequeue();
            _indexedPendingCount = pending.Count;
            inspected++;
            MclslItemDefinition pendingItem = MclslItemCatalog.Get(listing?.Item?.ItemId);
            if (listing?.Item == null || pendingItem?.Category != "Artifact"
                || !MclslActorRegistry.ResolveKnownOrWorld(listing.SellerId, out Actor seller)
                || !MclslActorAccessor.Alive(seller))
            {
                if (!string.IsNullOrEmpty(listing?.Item?.InstanceId))
                    ArtifactInstanceIds.Remove(listing.Item.InstanceId);
                continue;
            }
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
        int contributionFunds = MclslActorAccessor.GetInt(buyer, MclslActorDataKeys.Contribution);
        int stoneFunds = MclslActorAccessor.GetInt(buyer, MclslActorDataKeys.SpiritStones);
        if (!MclslActorRegistry.ResolveKnownOrWorld(listing.SellerId, out Actor seller) || !MclslActorAccessor.Alive(seller))
        {
            RemoveListing(listing);
            MclslWorldArchiveStore.MarkDirty();
            return false;
        }
        int sellerContribution = MclslActorAccessor.GetInt(seller, MclslActorDataKeys.Contribution);
        int sellerStones = MclslActorAccessor.GetInt(seller, MclslActorDataKeys.SpiritStones);
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
        int buyerFunds = payment == MclslMarketPayment.Contribution ? contributionFunds : stoneFunds;
        int sellerFunds = payment == MclslMarketPayment.Contribution ? sellerContribution : sellerStones;
        if (!MclslMarketPaymentPolicy.CanPay(payment, unitPrice, requestedCount, buyerFunds, sellerFunds)) return false;
        int totalPrice = checked(unitPrice * requestedCount);
        MclslBagState bag = MclslBagSystem.Read(buyer);
        if (item.Category == "Artifact" && MclslBagSystem.Count(bag, item.Id) > 0) return false;
        string fundsKey = currency == ContributionCurrency ? MclslActorDataKeys.Contribution : MclslActorDataKeys.SpiritStones;
        // The game thread settles one order at a time. A partial purchase leaves the order indexed.
        bool completedListing = requestedCount == listing.Item.Count;
        if (completedListing && !RemoveListing(listing)) return false;
        if (!completedListing) { listing.Item.Count -= requestedCount; _snapshot = null; _revision++; }
        if (item.Category == "Artifact")
            bag.Items.Add(new MclslOwnedItem { ItemId = item.Id, InstanceId = listing.Item.InstanceId,
                Durability = listing.Item.Durability, Count = 1, AcquiredYear = acquiredYear });
        else MclslBagSystem.Add(bag, item.Id, requestedCount, acquiredYear: acquiredYear);
        MclslBagSystem.Write(buyer, bag);
        MclslActorAccessor.Set(buyer, fundsKey, buyerFunds - totalPrice);
        MclslActorAccessor.Set(seller, fundsKey, sellerFunds + totalPrice);
        int purchasedBeforeSettlement = PurchasesInYear(buyer, acquiredYear);
        MclslActorAccessor.Set(buyer, MclslActorDataKeys.MarketPurchaseYear, acquiredYear);
        MclslActorAccessor.Set(buyer, MclslActorDataKeys.MarketPurchaseCount,
            MclslMarketPurchaseSequence.AfterPurchase(purchasedBeforeSettlement));
        MclslWorldArchiveStore.MarkDirty();
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
            // Progress follows the core settlement, before optional scroll callbacks.
            MclslMarketPurchasePolicy.Advance(batch, true);
            MclslWorldArchiveStore.MarkDirty();
            if (need.ItemId.EndsWith("_SCROLL", StringComparison.Ordinal))
                try { MclslSpellSystem.TryStudyScroll(actor, need.ItemId); }
                catch (Exception ex) { MclslDiagnostics.Error("market-scroll-study", ex.Message); }
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
        MclslProfessionSystem.BuildCraftMaterialReserves(actor, bag, CraftReserveScratch);
        List<MclslMarketPurchaseNeed> craft = new();
        foreach (KeyValuePair<string, int> reserve in CraftReserveScratch.OrderBy(x => x.Key, StringComparer.Ordinal))
        {
            int missing = Math.Max(0, reserve.Value - MclslBagSystem.Count(bag, reserve.Key));
            if (missing > 0) craft.Add(new MclslMarketPurchaseNeed { ItemId = reserve.Key, Kind = "Craft", Remaining = missing });
        }
        DesiredItemIdsScratch.Clear();
        MclslConsumableInventoryPolicy.BuildReservePlan(actor, ReservePlanScratch);
        BuildFinishedGoodsNeeds(actor, bag, DesiredItemIdsScratch);
        AddSpellScrollNeed(actor, bag, DesiredItemIdsScratch);
        List<MclslMarketPurchaseNeed> finished = DesiredItemIdsScratch
            .Distinct(StringComparer.Ordinal)
            .Select(id => new MclslMarketPurchaseNeed { ItemId = id, Kind = "Finished", Remaining = 1 }).ToList();
        Dictionary<string, MclslMarketPurchaseNeed> craftByItem = craft.ToDictionary(need => need.ItemId, StringComparer.Ordinal);
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
        if (!ByItem.TryGetValue(itemId, out List<MclslMarketListing> offers)) return false;
        MclslItemDefinition item = MclslItemCatalog.Get(itemId);
        if (item == null || item.Category == "Artifact" && MclslBagSystem.Count(bag, itemId) > 0) return false;
        long actorId = MclslActorAccessor.Id(actor);
        int contributionFunds = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Contribution);
        int stoneFunds = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.SpiritStones);
        HashSet<string> rejected = null;
        while (true)
        {
            MclslMarketListing best = null;
            string bestCurrency = null;
            int bestPrice = int.MaxValue;
            MclslPerformanceProbe.RecordMarketOffersInspected(offers.Count);
            for (int i = 0; i < offers.Count; i++)
            {
                MclslMarketListing offer = offers[i];
                if (offer?.Item == null || offer.Item.Count < 1 || offer.SellerId == actorId
                    || rejected?.Contains(offer.ListingId) == true) continue;
                int contribution = offer.ContributionUnitPrice > 0 ? offer.ContributionUnitPrice : Math.Max(1, offer.Price);
                int stones = offer.SpiritStoneUnitPrice > 0 ? offer.SpiritStoneUnitPrice : SpiritStonePrice(item, contribution);
                if (contribution > contributionFunds && stones > stoneFunds) continue;
                if (best != null && (contribution > bestPrice
                    || contribution == bestPrice && offer.Year >= best.Year)) continue;
                if (!MclslActorRegistry.ResolveKnownOrWorld(offer.SellerId, out Actor seller)
                    || !MclslActorAccessor.Alive(seller)) continue;
                MclslMarketPayment payment = MclslMarketPaymentPolicy.Select(contribution, stones, 1,
                    contributionFunds, stoneFunds,
                    MclslActorAccessor.GetInt(seller, MclslActorDataKeys.Contribution),
                    MclslActorAccessor.GetInt(seller, MclslActorDataKeys.SpiritStones));
                if (payment == MclslMarketPayment.None) continue;
                if (best == null || contribution < bestPrice || contribution == bestPrice && offer.Year < best.Year)
                {
                    best = offer;
                    bestPrice = contribution;
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
            if (itemId == "D004" && MclslActorAccessor.GetInt(actor, "mclsl.architecture.v1.actor.taishang_taken") != 0) continue;
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


    internal static void BeginAnnualPrune(int year)
    {
        MclslAnnualBatchState batch = MclslWorldRunRepository.Current.AnnualBatch;
        if (batch.MarketPruneYear == year) return;
        batch.MarketPruneYear = year;
        batch.MarketPruneCursor = 0;
        batch.MarketPruneListingIds = new List<string>(Listings.Count);
        foreach (MclslMarketListing listing in Listings)
            if (!string.IsNullOrWhiteSpace(listing?.ListingId)) batch.MarketPruneListingIds.Add(listing.ListingId);
        MclslWorldArchiveStore.MarkDirty();
    }

    internal static bool TickAnnualPrune()
    {
        MclslAnnualBatchState batch = MclslWorldRunRepository.Current.AnnualBatch;
        batch.MarketPruneListingIds ??= new();
        try { EnsureIndex(); }
        catch (Exception ex)
        {
            MclslWorldRunRepository.RecordAnnualFailure(batch.MarketPruneYear, "天玄镜", string.Empty,
                "MarketPruneIndex", 1, "跳过", ex.Message);
            batch.MarketPruneListingIds.Clear();
            batch.MarketPruneCursor = 0;
            MclslWorldArchiveStore.MarkDirty();
            return true;
        }
        batch.MarketPruneCursor = Math.Max(0, batch.MarketPruneCursor);
        if (batch.MarketPruneCursor >= batch.MarketPruneListingIds.Count) return true;
        long started = Stopwatch.GetTimestamp();
        double budgetMs = MclslRuntimeWorkBudget.ScaleMilliseconds(0.15d, 0.10d);
        int budget = MclslRuntimeWorkBudget.ScaleCount(16, 4);
        int examined = 0;
        while (batch.MarketPruneCursor < batch.MarketPruneListingIds.Count && examined < budget
            && !MclslAnnualFrameBudget.Expired)
        {
            if (examined > 0 && (Stopwatch.GetTimestamp() - started) * 1000d / Stopwatch.Frequency >= budgetMs) break;
            string id = batch.MarketPruneListingIds[batch.MarketPruneCursor];
            // Removing a listing and advancing its cursor happen on the game thread.
            // A failed target is isolated instead of replaying already removed orders.
            try
            {
                if (ById.TryGetValue(id, out MclslMarketListing listing)
                    && (!MclslActorRegistry.ResolveKnownOrWorld(listing.SellerId, out Actor seller)
                        || !MclslActorAccessor.Alive(seller)) && RemoveListing(listing))
                    MclslWorldArchiveStore.MarkDirty();
            }
            catch (Exception ex)
            {
                MclslWorldRunRepository.RecordAnnualFailure(batch.MarketPruneYear, "天玄镜", id,
                    "MarketPrune", 1, "跳过", ex.Message);
            }
            batch.MarketPruneCursor++;
            examined++;
            MclslWorldArchiveStore.MarkDirty();
        }
        if (batch.MarketPruneCursor < batch.MarketPruneListingIds.Count) return false;
        batch.MarketPruneListingIds.Clear();
        batch.MarketPruneCursor = 0;
        return true;
    }

}
