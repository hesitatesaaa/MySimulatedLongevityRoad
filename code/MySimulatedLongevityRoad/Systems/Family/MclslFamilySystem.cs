using System;
using System.Collections.Generic;
using System.Linq;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslFamilySystem
{
    private static string TreasuryAccount(MclslFamilyRecord family) => "family/" + family.Id;

    internal static long TreasuryStones(MclslFamilyRecord family)
        => family == null ? 0 : MclslWorldRunRepository.Current.Economy.Balance(TreasuryAccount(family), MclslCurrency.SpiritStone);

    private static MclslEconomicResult MoveStones(MclslFamilyRecord family, Actor actor, long amount,
        bool deposit, int year, string source = null, long cursor = 0)
    {
        string actorAccount = MclslEconomyCommands.Account(actor);
        string familyAccount = TreasuryAccount(family);
        return MclslEconomyCommands.Commit(year, new[]
        {
            new MclslEconomicOperation(MclslEconomicKind.Transfer, MclslCurrency.SpiritStone, amount,
                deposit ? actorAccount : familyAccount, deposit ? familyAccount : actorAccount)
        }, source, cursor);
    }
    private static MclslWorldRunState _indexedRun;
    private static readonly Dictionary<string, MclslFamilyRecord> ById = new(StringComparer.Ordinal);
    private static readonly Dictionary<long, MclslFamilyRecord> ByActor = new();
    private static readonly Dictionary<long, MclslFamilyRecord> ByNative = new();
    private static readonly List<MclslFamilyRecord> VisibleFamilies = new();

    internal static void Clear()
    {
        _indexedRun = null;
        ById.Clear(); ByActor.Clear(); ByNative.Clear(); VisibleFamilies.Clear();
    }

    private static MclslWorldRunState EnsureIndex()
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run == null) return null;
        if (ReferenceEquals(_indexedRun, run)) return run;
        Clear();
        _indexedRun = run;
        run.Families ??= new List<MclslFamilyRecord>();
        bool repairedNames = false;
        foreach (MclslFamilyRecord family in run.Families)
        {
            if (family == null || string.IsNullOrWhiteSpace(family.Id)) continue;
            family.Members ??= new List<MclslFamilyMemberRecord>();
            family.Items ??= new List<MclslOwnedItem>();
            family.TechniqueIds ??= new List<string>();
            family.CityInfluence ??= new Dictionary<long, int>();
            family.History ??= new List<string>();
            if (!family.Extinct && family.Members.Count > 0 && family.Members.All(x => x == null || !x.Alive))
                family.Extinct = true;
            if (family.Extinct)
            {
                TryLiquidateEstate(run, family);
                continue;
            }
            VisibleFamilies.Add(family);
            ById[family.Id] = family;
            if (family.NativeFamilyId > 0) ByNative[family.NativeFamilyId] = family;
            foreach (MclslFamilyMemberRecord member in family.Members)
                if (member != null && member.ActorId > 0) ByActor[member.ActorId] = family;
            repairedNames |= RepairFamilyNames(family);
        }
        if (repairedNames) MclslWorldArchiveStore.MarkDirty();
        return run;
    }

    internal static IReadOnlyList<MclslFamilyRecord> Families
        => EnsureIndex() is MclslWorldRunState ? VisibleFamilies : Array.Empty<MclslFamilyRecord>();

    internal static MclslFamilyRecord Find(string familyId)
    {
        EnsureIndex();
        return familyId != null && ById.TryGetValue(familyId, out MclslFamilyRecord family) ? family : null;
    }

    internal static MclslFamilyRecord FamilyOf(Actor actor)
    {
        EnsureIndex();
        long id = MclslActorAccessor.Id(actor);
        return id > 0 && ByActor.TryGetValue(id, out MclslFamilyRecord family) ? family : null;
    }

    internal static void Observe(Actor actor)
    {
        Observe(actor, 0);
    }

    private static MclslFamilyRecord Observe(Actor actor, int depth)
    {
        if (EnsureIndex() is not MclslWorldRunState run || !MclslActorAccessor.Alive(actor)
            || !MclslActorAccessor.IsCultivator(actor) || depth > 8) return null;
        long id = MclslActorAccessor.Id(actor);
        if (id <= 0) return null;
        if (ByActor.TryGetValue(id, out MclslFamilyRecord known))
        {
            RefreshMember(known, actor);
            return known;
        }

        long first = actor.data.parent_id_1, second = actor.data.parent_id_2;
        Actor firstActor = first > 0 ? World.world?.units?.get(first) : null;
        Actor secondActor = second > 0 ? World.world?.units?.get(second) : null;
        long father = ParentOfSex(firstActor, first, ActorSex.Male) ? first
            : ParentOfSex(secondActor, second, ActorSex.Male) ? second : 0;
        long mother = ParentOfSex(firstActor, first, ActorSex.Female) ? first
            : ParentOfSex(secondActor, second, ActorSex.Female) ? second : 0;
        MclslFamilyRecord family = ParentFamily(father, depth) ?? ParentFamily(mother, depth);
        long nativeFamily = actor.data.family > 0 ? actor.data.family : actor.data.ancestor_family;
        if (family == null && nativeFamily > 0) ByNative.TryGetValue(nativeFamily, out family);
        if (family == null)
        {
            string familyId = run.RunId + ":" + (nativeFamily > 0 ? "native:" + nativeFamily : "founder:" + id);
            family = new MclslFamilyRecord
            {
                Id = familyId, Surname = MclslHonorificNameCatalog.StableFamilySurname(SafeName(actor), id),
                FounderActorId = id,
                NativeFamilyId = Math.Max(0, nativeFamily)
            };
            family.Name = family.Surname + "氏";
            run.Families.Add(family);
            VisibleFamilies.Add(family);
            ById[familyId] = family;
            if (nativeFamily > 0) ByNative[nativeFamily] = family;
            AddHistory(family, MclslRuntime.CurrentYear() + "年 · " + family.Name + "立族");
        }
        int generation = 1;
        if (father > 0 && ByActor.TryGetValue(father, out MclslFamilyRecord fatherFamily) && fatherFamily == family)
            generation = Math.Max(generation, Member(family, father)?.Generation + 1 ?? 1);
        else if (mother > 0 && ByActor.TryGetValue(mother, out MclslFamilyRecord motherFamily) && motherFamily == family)
            generation = Math.Max(generation, Member(family, mother)?.Generation + 1 ?? 1);
        string memberName = EnsureActorSurname(family, actor);
        family.Members.Add(new MclslFamilyMemberRecord
        {
            ActorId = id, Name = memberName, ParentId1 = first, ParentId2 = second,
            Sex = (int)actor.data.sex,
            Generation = generation, RealmIndex = MclslRealmIds.Index(MclslActorAccessor.Realm(actor)),
            Aptitude = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 50),
            CityId = actor.city?.data?.id ?? 0
        });
        ByActor[id] = family;
        MclslActorAccessor.Set(actor, MclslActorDataKeys.FamilyId, family.Id);
        RecordTechnique(family, actor);
        EnsureHead(family);
        MclslWorldArchiveStore.MarkDirty();
        return family;
    }

    private static bool ParentOfSex(Actor actor, long id, ActorSex sex)
        => id > 0 && (actor?.data?.sex == sex ||
            ByActor.TryGetValue(id, out MclslFamilyRecord family) && Member(family, id)?.Sex == (int)sex);

    private static MclslFamilyRecord ParentFamily(long parentId, int depth)
    {
        if (parentId <= 0) return null;
        if (ByActor.TryGetValue(parentId, out MclslFamilyRecord family)) return family;
        Actor parent = World.world?.units?.get(parentId);
        return parent != null && MclslActorAccessor.IsCultivator(parent) ? Observe(parent, depth + 1) : null;
    }

    private static bool RepairFamilyNames(MclslFamilyRecord family)
    {
        MclslFamilyMemberRecord founder = family.Members.Find(x => x?.ActorId == family.FounderActorId);
        string source = founder?.Name;
        if (string.IsNullOrWhiteSpace(source) && family.FounderActorId > 0
            && MclslActorRegistry.ResolveKnownOrWorld(family.FounderActorId, out Actor actor)) source = SafeName(actor);
        if (string.IsNullOrWhiteSpace(source)) source = family.Name?.TrimEnd('氏');
        string surname = string.IsNullOrWhiteSpace(family.Surname)
            ? MclslHonorificNameCatalog.StableFamilySurname(source, family.FounderActorId)
            : family.Surname;
        bool changed = !string.Equals(family.Surname, surname, StringComparison.Ordinal);
        family.Surname = surname;
        string previousFamilyName = family.Name;
        string familyName = surname + "氏";
        if (!string.Equals(family.Name, familyName, StringComparison.Ordinal))
        {
            family.Name = familyName;
            ReplaceFamilyHistoryText(family, previousFamilyName, familyName);
            changed = true;
        }
        foreach (MclslFamilyMemberRecord member in family.Members)
        {
            if (member == null || member.ActorId <= 0) continue;
            string previousName = member.Name;
            string corrected = MclslHonorificNameCatalog.ApplyFamilySurname(member.ActorId, member.Name, surname);
            if (!string.Equals(member.Name, corrected, StringComparison.Ordinal))
            {
                member.Name = corrected;
                ReplaceFamilyHistoryText(family, previousName, corrected);
                changed = true;
            }
            if (!member.Alive || !MclslActorRegistry.ResolveKnownOrWorld(member.ActorId, out Actor living)
                || !MclslActorAccessor.Alive(living)) continue;
            string liveName = EnsureActorSurname(family, living);
            if (!string.Equals(member.Name, liveName, StringComparison.Ordinal))
            {
                ReplaceFamilyHistoryText(family, member.Name, liveName);
                member.Name = liveName;
                changed = true;
            }
        }
        return changed;
    }

    private static void ReplaceFamilyHistoryText(MclslFamilyRecord family, string before, string after)
    {
        if (string.IsNullOrWhiteSpace(before) || string.IsNullOrWhiteSpace(after) || before == after) return;
        for (int i = 0; i < family.History.Count; i++)
            if (family.History[i]?.Contains(before, StringComparison.Ordinal) == true)
                family.History[i] = family.History[i].Replace(before, after, StringComparison.Ordinal);
    }

    private static string EnsureActorSurname(MclslFamilyRecord family, Actor actor)
    {
        if (actor == null) return string.Empty;
        string current = SafeName(actor);
        string corrected = MclslHonorificNameCatalog.ApplyFamilySurname(MclslActorAccessor.Id(actor), current, family.Surname);
        if (!string.Equals(current, corrected, StringComparison.Ordinal))
            MclslActorAccessor.ApplyDisplayName(actor, MclslActorAccessor.Realm(actor));
        return SafeName(actor);
    }

    private static string SafeName(Actor actor)
    {
        try { return actor?.getName() ?? "无名修士"; }
        catch { return "无名修士"; }
    }

    private static MclslFamilyMemberRecord Member(MclslFamilyRecord family, long actorId)
        => family?.Members?.Find(x => x != null && x.ActorId == actorId);

    private static void RefreshMember(MclslFamilyRecord family, Actor actor)
    {
        MclslFamilyMemberRecord member = Member(family, MclslActorAccessor.Id(actor));
        if (member == null || member.DeathSettled) return;
        member.Name = EnsureActorSurname(family, actor);
        member.Sex = (int)actor.data.sex;
        member.RealmIndex = MclslRealmIds.Index(MclslActorAccessor.Realm(actor));
        member.Aptitude = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 50);
        member.CityId = actor.city?.data?.id ?? 0;
        member.Alive = true;
        RecordTechnique(family, actor);
    }

    private static void RecordTechnique(MclslFamilyRecord family, Actor actor)
    {
        string technique = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueId);
        if (string.IsNullOrWhiteSpace(technique) || family.TechniqueIds.Contains(technique)) return;
        family.TechniqueIds.Add(technique);
        MclslWorldArchiveStore.MarkDirty();
    }

    private static void EnsureHead(MclslFamilyRecord family)
    {
        long next = 0;
        if (IsAdultLiveMember(family, family.PreferredHeadActorId)) next = family.PreferredHeadActorId;
        if (next == 0)
        {
            int bestRealm = -1;
            double bestPower = -1;
            foreach (MclslFamilyMemberRecord member in family.Members)
            {
                if (member == null || !IsAdultLiveMember(family, member.ActorId)) continue;
                MclslActorRegistry.ResolveKnownOrWorld(member.ActorId, out Actor candidate);
                double power = FamilyPower(candidate);
                if (member.RealmIndex > bestRealm || member.RealmIndex == bestRealm
                    && (power > bestPower || power == bestPower && (next == 0 || member.ActorId < next)))
                { bestRealm = member.RealmIndex; bestPower = power; next = member.ActorId; }
            }
        }
        if (family.HeadActorId == next) return;
        family.HeadActorId = next;
        if (next > 0) AddHistory(family, MclslRuntime.CurrentYear() + "年 · " + (Member(family, next)?.Name ?? "修士") + "继任家主");
        MclslWorldArchiveStore.MarkDirty();
    }

    private static double FamilyPower(Actor actor)
    {
        try { return actor?.stats == null ? 0d : Math.Max(0f, actor.stats["damage"]) + Math.Max(0f, actor.stats["health"]); }
        catch { return 0d; }
    }

    private static bool IsAdultLiveMember(MclslFamilyRecord family, long id)
    {
        if (id <= 0 || Member(family, id)?.Alive != true || !MclslActorRegistry.ResolveKnownOrWorld(id, out Actor actor)
            || !MclslActorAccessor.Alive(actor)) return false;
        return actor.getAge() >= 16f;
    }

    internal static bool SetHead(MclslFamilyRecord family, long actorId)
    {
        if (family == null || !IsAdultLiveMember(family, actorId)) return false;
        family.PreferredHeadActorId = actorId;
        EnsureHead(family);
        MclslWorldArchiveStore.MarkDirty();
        return true;
    }

    internal static bool SetFocus(MclslFamilyRecord family, long actorId)
    {
        if (family == null || Member(family, actorId)?.Alive != true) return false;
        family.FocusActorId = actorId;
        MclslWorldArchiveStore.MarkDirty();
        return true;
    }

    internal static bool TickAnnual(int year)
    {
        if (year <= 0 || year % 5 != 0) return true;
        MclslWorldRunState run = EnsureIndex();
        if (run == null) return true;
        MclslAnnualBatchState batch = run.AnnualBatch;
        if (batch.FamilySettlementYear != year)
        {
            batch.FamilySettlementYear = year;
            batch.FamilyCursor = batch.FamilyMemberCursor = 0;
        }
        while (batch.FamilyCursor < run.Families.Count && MclslAnnualFrameBudget.TryConsumeOperation())
        {
            MclslFamilyRecord family = run.Families[batch.FamilyCursor];
            if (family?.Extinct == true)
            {
                TryLiquidateEstate(run, family);
                batch.FamilyCursor++; batch.FamilyMemberCursor = 0; continue;
            }
            if (family == null || family.LastSettledYear == year)
            { batch.FamilyCursor++; batch.FamilyMemberCursor = 0; continue; }
            if (batch.FamilyMemberCursor == 0)
            {
                EnsureHead(family);
                family.CityInfluence.Clear();
                family.SettlementDuesStones = 0;
            }
            while (batch.FamilyMemberCursor < family.Members.Count && MclslAnnualFrameBudget.TryConsumeOperation())
            {
                MclslFamilyMemberRecord member = family.Members[batch.FamilyMemberCursor++];
                if (member == null || !member.Alive || !MclslActorRegistry.ResolveKnownOrWorld(member.ActorId, out Actor actor)
                    || !MclslActorAccessor.Alive(actor)) continue;
                RefreshMember(family, actor);
                if (member.CityId > 0)
                {
                    family.CityInfluence.TryGetValue(member.CityId, out int score);
                    family.CityInfluence[member.CityId] = score + 1 + 2 * Math.Max(0, member.RealmIndex)
                        + (member.ActorId == family.HeadActorId ? 3 : 0);
                }
                if (member.LastDuesYear != year)
                {
                    long stones = MclslActorAccessor.GetMoney(actor, MclslActorDataKeys.SpiritStones, 0);
                    if (stones >= 30)
                    {
                        long dues = Math.Min(Math.Min(20, stones / 10), long.MaxValue - TreasuryStones(family));
                        if (dues > 0)
                        {
                            MclslEconomicResult result = MoveStones(family, actor, dues, true, year,
                                "dues/" + family.Id + "/" + member.ActorId, year);
                            if (result == MclslEconomicResult.Applied)
                            {
                                family.SettlementDuesStones = checked(family.SettlementDuesStones + dues);
                                MclslEconomyCommands.NotifyWallet(actor);
                            }
                            else if (result != MclslEconomicResult.AlreadyApplied)
                                throw new InvalidOperationException("族费结算失败：" + result);
                        }
                    }
                    member.LastDuesYear = year;
                }
            }
            if (batch.FamilyMemberCursor < family.Members.Count) break;
            if (family.SettlementDuesStones > 0)
                AddHistory(family, year + "年 · 族费收入灵石" + family.SettlementDuesStones);
            family.SettlementDuesStones = 0;
            ApplyAid(family, year);
            family.LastSettledYear = year;
            batch.FamilyCursor++;
            batch.FamilyMemberCursor = 0;
        }
        MclslWorldArchiveStore.MarkDirty();
        if (batch.FamilyCursor < run.Families.Count) return false;
        batch.FamilyCursor = batch.FamilyMemberCursor = 0;
        return true;
    }

    private static void ApplyAid(MclslFamilyRecord family, int year)
    {
        if (family.LastAidYear == year || TreasuryStones(family) < 12) return;
        long focus = family.FocusActorId;
        if (focus <= 0 || Member(family, focus)?.Alive != true) focus = PickJunior(family);
        if (focus <= 0 || !MclslActorRegistry.ResolveKnownOrWorld(focus, out Actor actor)
            || !MclslActorAccessor.Alive(actor)) return;
        long before = MclslActorAccessor.GetMoney(actor, MclslActorDataKeys.SpiritStones, 0);
        long granted = Math.Min(12, long.MaxValue - before);
        if (granted == 0) return;
        MclslEconomicResult result = MoveStones(family, actor, granted, false, year, "family-aid/" + family.Id, year);
        if (result is not (MclslEconomicResult.Applied or MclslEconomicResult.AlreadyApplied))
            throw new InvalidOperationException("家族扶持结算失败：" + result);
        family.LastAidYear = year;
        MclslEconomyCommands.NotifyWallet(actor);
        AddHistory(family, year + "年 · 扶持" + SafeName(actor) + "灵石" + granted);
    }

    private static long PickJunior(MclslFamilyRecord family)
    {
        int headGeneration = Member(family, family.HeadActorId)?.Generation ?? 0;
        long best = 0;
        int bestAptitude = -1;
        foreach (MclslFamilyMemberRecord member in family.Members)
        {
            if (member == null || !member.Alive || member.Generation <= headGeneration
                || !MclslActorRegistry.ResolveKnownOrWorld(member.ActorId, out Actor actor)
                || !MclslActorAccessor.Alive(actor) || actor.getAge() > 60f) continue;
            if (member.Aptitude > bestAptitude || member.Aptitude == bestAptitude && (best == 0 || member.ActorId < best))
            { bestAptitude = member.Aptitude; best = member.ActorId; }
        }
        return best;
    }

    internal static bool TransferStones(MclslFamilyRecord family, Actor actor, int amount, bool deposit)
    {
        if (family == null || amount <= 0 || FamilyOf(actor) != family || !MclslActorAccessor.Alive(actor)) return false;
        if (MoveStones(family, actor, amount, deposit, Math.Max(0, MclslRuntime.CurrentYear())) != MclslEconomicResult.Applied)
            return false;
        MclslEconomyCommands.NotifyWallet(actor);
        AddHistory(family, MclslRuntime.CurrentYear() + "年 · " + SafeName(actor) + (deposit ? "存入" : "领取") + "灵石" + amount);
        MclslWorldArchiveStore.MarkDirty();
        return true;
    }

    internal static bool TransferItem(MclslFamilyRecord family, Actor actor, string itemId, bool deposit)
    {
        if (family == null || FamilyOf(actor) != family || !MclslActorAccessor.Alive(actor)
            || MclslItemCatalog.Get(itemId) == null || MclslBagSystem.IsLocked(actor)) return false;
        MclslBagState source = MclslBagSystem.Read(actor);
        MclslBagState bag = CopyBag(source);
        List<MclslOwnedItem> familyItems = family.Items.Select(MclslFamilyTransferPolicy.CopyItem).ToList();
        List<MclslOwnedItem> from = deposit ? bag.Items : familyItems;
        MclslOwnedItem held = from.Find(x => x?.ItemId == itemId && x.Count > 0
            && (!deposit || !MclslInventoryDataRules.IsEquipped(bag, x)));
        if (held == null) return false;
        MclslOwnedItem moved = MclslFamilyTransferPolicy.TakeOne(from, held);
        if (moved == null) return false;
        if (!deposit) bag.Items.Add(moved);
        if (!MclslInventoryDataRules.IsValid(bag)) return false;
        MclslBagSystem.Write(actor, bag);
        if (deposit) family.Items.Add(moved);
        else family.Items = familyItems;
        if (!deposit && MclslItemCatalog.Get(itemId)?.Category == "Artifact")
        {
            try { MclslArtifactSystem.OnArtifactAcquired(actor); }
            catch (Exception ex) { MclslDiagnostics.Error("family-artifact", "家库法宝领取后处理失败：" + ex.Message); }
        }
        AddHistory(family, MclslRuntime.CurrentYear() + "年 · " + SafeName(actor)
            + (deposit ? "存入" : "领取") + (MclslItemCatalog.Get(itemId)?.Name ?? itemId));
        MclslWorldArchiveStore.MarkDirty();
        return true;
    }

    private static MclslBagState CopyBag(MclslBagState source) => new()
    {
        Version = source.Version,
        Items = source.Items.Select(MclslFamilyTransferPolicy.CopyItem).ToList(),
        EquippedArtifactSlots = new Dictionary<string, string>(source.EquippedArtifactSlots),
        Books = new List<MclslMentorshipBook>(source.Books)
    };

    internal static void OnDeath(Actor actor)
    {
        MclslFamilyRecord family = FamilyOf(actor);
        long id = MclslActorAccessor.Id(actor);
        MclslFamilyMemberRecord member = Member(family, id);
        if (member == null)
        {
            MclslEconomyCommands.CloseDeadActorWallet(actor);
            MclslTianxuanMarket.ReleaseUnclaimedSellerEscrow(id, MclslRuntime.CurrentYear());
            return;
        }
        if (member.DeathSettled)
        {
            MclslEconomyCommands.CloseDeadActorWallet(actor);
            return;
        }
        long stones = MclslActorAccessor.GetMoney(actor, MclslActorDataKeys.SpiritStones, 0);
        long inherited = Math.Min(stones / 2, long.MaxValue - TreasuryStones(family));
        bool settled = false;
        MclslBagState originalBag = null;
        int originalItemCount = family.Items.Count;
        try
        {
            if (MclslBagSystem.IsLocked(actor)) throw new InvalidOperationException("乾坤袋已锁定");
            originalBag = CopyBag(MclslBagSystem.Read(actor));
            MclslBagState remainingBag = CopyBag(originalBag);
            List<MclslOwnedItem> inheritedItems = remainingBag.Items
                .Where(x => x != null && !MclslInventoryDataRules.IsEquipped(remainingBag, x))
                .Select(MclslFamilyTransferPolicy.CopyItem).ToList();
            remainingBag.Items.RemoveAll(x => x != null && !MclslInventoryDataRules.IsEquipped(remainingBag, x));
            if (!MclslInventoryDataRules.IsValid(remainingBag))
                throw new InvalidOperationException("遗物转移后乾坤袋无效");
            MclslBagSystem.Write(actor, remainingBag);
            family.Items.AddRange(inheritedItems);
            MclslTianxuanMarket.TransferSellerEscrowToFamily(id, family.Items, () =>
            {
                MclslEconomicResult result = MoveStones(family, actor, inherited, true,
                    Math.Max(0, MclslRuntime.CurrentYear()), "estate/" + MclslEconomyCommands.Account(actor), 1);
                if (result != MclslEconomicResult.Applied) throw new InvalidOperationException("遗产资金结算失败：" + result);
            });
            member.DeathSettled = true;
            settled = true;
        }
        catch (Exception ex)
        {
            member.DeathSettled = false;
            if (family.Items.Count > originalItemCount)
                family.Items.RemoveRange(originalItemCount, family.Items.Count - originalItemCount);
            try
            {
                if (originalBag != null) MclslBagSystem.Write(actor, originalBag);
            }
            catch (Exception rollback) { MclslDiagnostics.Error("family-inheritance-rollback", "家族遗物回滚失败：" + rollback.Message); }
            MclslDiagnostics.Error("family-inheritance", "家族遗物转移失败：" + ex.Message);
        }
        member.Alive = false;
        if (settled) MclslEconomyCommands.CloseDeadActorWallet(actor);
        if (family.PreferredHeadActorId == id) family.PreferredHeadActorId = 0;
        if (family.FocusActorId == id) family.FocusActorId = 0;
        AddHistory(family, MclslRuntime.CurrentYear() + "年 · " + member.Name + "陨落"
            + (settled ? "，遗灵石" + inherited + "入库" : "，遗物暂未入库"));
        EnsureHead(family);
        if (!family.Members.Any(x => x?.Alive == true
            && MclslActorRegistry.ResolveKnownOrWorld(x.ActorId, out Actor living)
            && MclslActorAccessor.Alive(living)))
        {
            family.Extinct = true;
            VisibleFamilies.Remove(family);
            ById.Remove(family.Id);
            if (family.NativeFamilyId > 0) ByNative.Remove(family.NativeFamilyId);
            TryLiquidateEstate(EnsureIndex(), family);
        }
        MclslWorldArchiveStore.MarkDirty();
    }

    private static void TryLiquidateEstate(MclslWorldRunState run, MclslFamilyRecord family)
    {
        if (run == null || family == null || !family.Extinct || family.EstateLiquidated) return;
        if (family.Members.Any(x => x != null && !x.Alive && !x.DeathSettled))
        {
            MclslDiagnostics.Error("family-estate-unsettled:" + family.Id, "绝嗣家族尚有未完成的死亡遗产转移，家库已保留。");
            return;
        }
        if (!MclslTianxuanMarket.QueueEstateItems(family.Items, MclslRuntime.CurrentYear(), () =>
        {
            MclslEconomicResult liquidation = MclslEconomyCommands.Commit(Math.Max(0, MclslRuntime.CurrentYear()),
                new[] { new MclslEconomicOperation(MclslEconomicKind.Destroy, MclslCurrency.SpiritStone,
                    TreasuryStones(family), fromAccount: TreasuryAccount(family)) }, "family-liquidation/" + family.Id, 1);
            return liquidation is MclslEconomicResult.Applied or MclslEconomicResult.AlreadyApplied;
        }))
        {
            MclslDiagnostics.Error("family-estate:" + family.Id, "绝嗣家族遗物未能上架，已保留家库供后续重试。");
            return;
        }
        // The queued items and the family inventory now cross the same commit boundary.
        // Mark this before later lineage/UI work so a retry cannot queue them twice.
        family.Items.Clear();
        family.EstateLiquidated = true;
        MclslWorldArchiveStore.MarkDirty();
        run.TechniqueLineages ??= new List<MclslTechniqueLineageRecord>();
        foreach (string techniqueId in family.TechniqueIds)
        {
            if (string.IsNullOrWhiteSpace(techniqueId)) continue;
            MclslTechniqueLineageRecord lineage = run.TechniqueLineages.Find(x => x?.Id == techniqueId);
            if (lineage == null)
            {
                MclslTechniqueDefinition definition = MclslCultivationCatalog.Techniques.FirstOrDefault(x => x.Id == techniqueId);
                lineage = new MclslTechniqueLineageRecord
                {
                    Id = techniqueId, Name = definition?.Name ?? techniqueId,
                    MaxRealm = definition?.MaxRealm ?? MclslRealmIds.HeDao,
                    FirstSeenYear = Math.Max(0, MclslRuntime.CurrentYear()),
                    SourceTechniqueId = techniqueId
                };
                run.TechniqueLineages.Add(lineage);
            }
            lineage.State = "散藏";
            lineage.Summary = family.Name + "绝嗣后留传于世。";
        }
    }

    private static void AddHistory(MclslFamilyRecord family, string text)
    {
        family.History.Add(text);
        if (family.History.Count > 60) family.History.RemoveRange(0, family.History.Count - 60);
    }
}
