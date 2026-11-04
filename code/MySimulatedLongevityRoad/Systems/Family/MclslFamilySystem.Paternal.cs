using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static partial class MclslFamilySystem
{
    /// <summary>
    /// 父系模式观察入口：按实际父亲（parent_id_1/2 性别识别）的父系家族/已知根祖归属。
    /// 不读取 native family，也不以母系兜底；夫妻各属自己父系。
    /// </summary>
    private static MclslFamilyRecord ObservePaternal(Actor actor, long id, MclslWorldRunState run)
    {
        if (ByActor.TryGetValue(id, out MclslFamilyRecord known))
        {
            RefreshMember(known, actor);
            return known;
        }
        bool cultivator = MclslActorAccessor.IsCultivator(actor);
        // 凡人不再逐人记录/判定：不建立成员记录、不刷新、不参与家主/结算/遗产。
        // 仅返回其已知家族（若有）；数量由成员视图派生的 MortalCount 统计。
        if (!cultivator)
            return ByActor.TryGetValue(id, out MclslFamilyRecord mortalKnown) ? mortalKnown : null;
        long first = actor.data.parent_id_1, second = actor.data.parent_id_2;
        long father = PaternalFatherId(actor, first, second);
        long root = PaternalRootId(id, father);

        MclslFamilyRecord family = null;
        if (root > 0) ByPaternalRoot.TryGetValue(root, out family);
        if (family == null && father > 0) ByActor.TryGetValue(father, out family);

        if (family == null)
        {
            // 只有修士触发立族；凡人相关亲属通过 QueueRelativesPaternal 补录，不全世界预建凡人家族。
            if (!cultivator) return null;
            string familyId = run.RunId + ":family:" + Guid.NewGuid().ToString("N");
            long paternalRoot = root > 0 ? root : id;
            family = new MclslFamilyRecord
            {
                Id = familyId,
                Surname = MclslHonorificNameCatalog.StableFamilySurname(SafeName(actor), id),
                FounderActorId = id,
                PaternalRootActorId = paternalRoot,
                NativeFamilyId = 0,
                FoundedYear = Math.Max(0, MclslRuntime.CurrentYear()),
                LastAidYear = -1,
                LastAidAttemptYear = -1
            };
            family.Name = family.Surname + "氏";
            run.Families.Add(family);
            VisibleFamilies.Add(family);
            ById[familyId] = family;
            if (!ByPaternalRoot.ContainsKey(paternalRoot)) ByPaternalRoot[paternalRoot] = family;
            QueueRelativesPaternal(id);
            AddHistory(family, MclslRuntime.CurrentYear() + "年 · " + family.Name + "立族");
        }

        int generation = PaternalGeneration(id, father, family.PaternalRootActorId);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.FamilyId, family.Id);
        string memberName = EnsureActorSurname(family, actor);
        var addedMember = new MclslFamilyMemberRecord
        {
            ActorId = id, Name = memberName, ParentId1 = first, ParentId2 = second,
            Sex = (int)actor.data.sex,
            Generation = generation, RealmIndex = MclslRealmIds.Index(MclslActorAccessor.Realm(actor)),
            Aptitude = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 50),
            CityId = actor.city?.data?.id ?? 0,
            IsCultivator = cultivator
        };
        family.Members.Add(addedMember);
        InvalidateMemberView(family);
        MembersById[(family.Id, id)] = addedMember;
        foreach (long child in PaternalChildren(id)) QueueMember(child);
        ByActor[id] = family;
        MclslActorAccessor.Set(actor, MclslActorDataKeys.FamilyId, family.Id);
        RecordTechnique(family, actor);
        family.Extinct = false;
        family.RemoveAfterLiquidation = false;
        // 普通补录/新增成员不立即重算家主，统一交给 ChangedFamilies 每族一次。
        QueueFamily(family);
        NotifyChanged();
        return family;
    }

    private static Actor ResolvePaternalActor(long id)
    {
        if (id <= 0) return null;
        if (MclslActorRegistry.Resolve(id, out Actor known) && known?.data != null) return known;
        return World.world?.units?.get(id);
    }

    private static long PaternalFatherId(Actor actor, long first, long second)
    {
        if (actor?.data == null) return 0;
        Actor firstActor = first > 0 ? ResolvePaternalActor(first) : null;
        Actor secondActor = second > 0 ? ResolvePaternalActor(second) : null;
        if (ParentOfSex(firstActor, first, ActorSex.Male)) return first;
        if (ParentOfSex(secondActor, second, ActorSex.Male)) return second;
        return 0;
    }

    /// <summary>沿实际父亲链上溯得到已知根祖角色 ID。无已知父亲时以自身为根。</summary>
    private static long PaternalRootId(long actorId, long fatherId)
    {
        long cursor = fatherId;
        while (cursor > 0)
        {
            if (ByActor.TryGetValue(cursor, out MclslFamilyRecord known))
                return known.PaternalRootActorId > 0 ? known.PaternalRootActorId : cursor;
            Actor current = ResolvePaternalActor(cursor);
            if (current?.data == null) return cursor;
            long next = PaternalFatherId(current, current.data.parent_id_1, current.data.parent_id_2);
            if (next <= 0) return cursor;
            cursor = next;
        }
        return cursor > 0 ? cursor : actorId;
    }

    private static int PaternalGeneration(long actorId, long fatherId, long rootId)
    {
        int generation = 1;
        long cursor = actorId == rootId ? 0 : fatherId;
        while (cursor > 0)
        {
            generation++;
            if (cursor == rootId) break;
            Actor ancestor = ResolvePaternalActor(cursor);
            if (ancestor?.data != null)
                cursor = PaternalFatherId(ancestor, ancestor.data.parent_id_1, ancestor.data.parent_id_2);
            else if (ByActor.TryGetValue(cursor, out MclslFamilyRecord family))
                return generation - 1 + (Member(family, cursor)?.Generation ?? 1);
            else break;
        }
        return generation;
    }

    /// <summary>仅返回以 fatherId 为实际父亲的孩子，避免把女儿所生子女并入母系。</summary>
    private static IEnumerable<long> PaternalChildren(long fatherId)
    {
        foreach (long childId in MclslActorRelations.Descendants(fatherId))
        {
            Actor child = ResolvePaternalActor(childId);
            if (child?.data == null) continue;
            if (PaternalFatherId(child, child.data.parent_id_1, child.data.parent_id_2) == fatherId)
                yield return childId;
        }
    }

    /// <summary>
    /// 父系模式补录：上溯父亲链、加入本人父系子嗣；不以 native family 补入配偶，
    /// 配偶按其自身父系在各自观察时归属。
    /// </summary>
    private static void QueueRelativesPaternal(long actorId)
    {
        Actor actor = ResolvePaternalActor(actorId);
        long cursor = actor?.data != null
            ? PaternalFatherId(actor, actor.data.parent_id_1, actor.data.parent_id_2)
            : 0;
        while (cursor > 0)
        {
            QueueMember(cursor);
            foreach (long child in PaternalChildren(cursor)) QueueMember(child);
            Actor ancestor = ResolvePaternalActor(cursor);
            if (ancestor?.data == null) break;
            cursor = PaternalFatherId(ancestor, ancestor.data.parent_id_1, ancestor.data.parent_id_2);
        }
        foreach (long child in PaternalChildren(actorId)) QueueMember(child);
    }
}
