using System;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Systems.Combat;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.UI;

namespace MySimulatedLongevityRoad.UI;

internal sealed class MclslCareerSkillView : MonoBehaviour
{
    private Actor _actor;
    private long _actorId;
    private bool _skills;
    private string _raw, _expanded, _availability;
    private bool _cooling;
    private double _nextRefresh;
    private ScrollRect _original, _scroll;
    private RectTransform _content;
    private float _archivePosition=1, _skillPosition=1;
    internal static void Bind(Transform panel,Actor actor)
    {
        var view=panel.GetComponent<MclslCareerSkillView>()??panel.gameObject.AddComponent<MclslCareerSkillView>();
        view._actor=actor;
        long id=MclslActorAccessor.Id(actor);
        if(view._actorId!=id)
        {
            view._actorId=id;view._skills=false;view._expanded=null;view._raw=null;
            view._archivePosition=view._skillPosition=1;
        }
        view.Apply();
    }
    internal static void Toggle(Transform panel,Actor actor)
    {
        Bind(panel,actor);
        var view=panel.GetComponent<MclslCareerSkillView>();
        if(view._skills)view._skillPosition=view._scroll.verticalNormalizedPosition;
        else view._archivePosition=view._original.verticalNormalizedPosition;
        view._skills=!view._skills;view.Apply();
        if(view._skills){view.Build();view._scroll.verticalNormalizedPosition=view._skillPosition;}
        else view._original.verticalNormalizedPosition=view._archivePosition;
        MclslActorInfoPanel.RefreshCareerButton(panel);
    }
    internal static bool Showing(Transform panel)=>panel.GetComponent<MclslCareerSkillView>()?._skills==true;
    private void Apply()
    {
        if(_original==null)_original=transform.Find("MclslActorInfoBody").GetComponent<ScrollRect>();
        if(_scroll==null)Create();
        _original.gameObject.SetActive(!_skills);_scroll.gameObject.SetActive(_skills);
    }
    private void Create()
    {
        var root=new GameObject("CareerSkillsBody",typeof(RectTransform),typeof(Image),typeof(Mask),typeof(ScrollRect));
        root.transform.SetParent(transform,false);
        var rt=(RectTransform)root.transform;var original=(RectTransform)_original.transform;
        rt.anchorMin=original.anchorMin;rt.anchorMax=original.anchorMax;rt.pivot=original.pivot;
        rt.offsetMin=original.offsetMin;rt.offsetMax=original.offsetMax;
        root.GetComponent<Image>().color=new Color(0.03f,0.09f,0.08f,0.01f);root.GetComponent<Mask>().showMaskGraphic=false;
        _scroll=root.GetComponent<ScrollRect>();_scroll.horizontal=false;_scroll.scrollSensitivity=20;
        _scroll.viewport=rt;
        var content=new GameObject("Content",typeof(RectTransform),typeof(VerticalLayoutGroup),typeof(ContentSizeFitter));
        content.transform.SetParent(root.transform,false);_content=(RectTransform)content.transform;
        _content.anchorMin=new Vector2(0,1);_content.anchorMax=Vector2.one;_content.pivot=new Vector2(0.5f,1);
        _content.offsetMin=Vector2.zero;_content.offsetMax=Vector2.zero;
        var layout=content.GetComponent<VerticalLayoutGroup>();layout.spacing=4;layout.padding=new RectOffset(2,2,4,8);
        layout.childControlHeight=true;layout.childForceExpandHeight=false;layout.childForceExpandWidth=true;
        content.GetComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
        _scroll.content=_content;
        transform.Find("MclslActorActions")?.SetAsLastSibling();
    }
    private void Update()
    {
        if(!_skills||!MclslActorAccessor.Alive(_actor)||Time.unscaledTime<_nextRefresh)return;
        _nextRefresh=Time.unscaledTime+0.5;
        string raw=MclslActorAccessor.GetString(_actor,MclslCombatCareerCatalog.StateKey);
        if(raw!=_raw || _cooling || Availability()!=_availability)Build();
    }
    private string Availability()
    {
        var bag=MclslBagSystem.Read(_actor);
        return MclslSpellSystem.CurrentMana(_actor)+"/"+MclslActorAccessor.Realm(_actor)
            +"/"+MclslActorAccessor.IsCultivator(_actor)
            +"/"+MclslBagSystem.Count(bag,"CP_Talisman_3")
            +"/"+MclslBagSystem.Count(bag,"CP_Talisman_4")
            +"/"+MclslBagSystem.Count(bag,"CP_Talisman_5")
            +"/"+MclslBagSystem.Count(bag,"CP_Array_3")
            +"/"+MclslBagSystem.Count(bag,"CP_Array_4")
            +"/"+MclslBagSystem.Count(bag,"CP_Array_5");
    }
    private Text TextRow(string name,string text,int size=11)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(Text),typeof(ContentSizeFitter));go.transform.SetParent(_content,false);
        Text label=go.GetComponent<Text>();label.font=LocalizedTextManager.current_font;
        label.fontSize=MclslUiTheme.ReadableFontSize(size);label.supportRichText=true;label.text=text;
        label.color=new Color(0.87f,0.91f,0.82f);label.horizontalOverflow=HorizontalWrapMode.Wrap;
        go.GetComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;return label;
    }
    private void Build()
    {
        float position=_scroll.verticalNormalizedPosition;
        foreach(Transform child in _content){child.gameObject.SetActive(false);Destroy(child.gameObject);}
        if(!MclslCombatCareerSystem.Applicable(_actor))
        {TextRow("Unavailable","此人物不适用文明修士的战斗职业。");return;}
        var state=MclslCombatCareerSystem.Read(_actor);
        _cooling=Array.Exists(state.Cooldowns,t=>t>MclslSpellCasting.Now);
        _raw=MclslActorAccessor.GetString(_actor,MclslCombatCareerCatalog.StateKey);
        _availability=Availability();
        if(state.Career<0){TextRow("Admission",MclslCombatCareerSystem.Overview(_actor));return;}
        TextRow("Career","<b>"+MclslCombatCareerCatalog.Careers[state.Career].Name+" · "+MclslCombatCareerCatalog.Ranks[state.Rank]+"</b>",12);
        string[] realms={"炼气","筑基","金丹","元婴","化神"};
        for(int tier=1;tier<=5;tier++)
        {
            TextRow("Tier"+tier,"<color=#D6BD75>"+MclslCombatCareerCatalog.Ranks[tier]+" · "+realms[tier-1]+"</color>");
            AddSkill(MclslCombatCareerCatalog.Skill(state.Career,tier),state);
            AddSkill(MclslCombatCareerCatalog.Skill(state.Career,tier,true),state);
        }
        Canvas.ForceUpdateCanvases();_scroll.verticalNormalizedPosition=position;
    }
    private void AddSkill(MclslCareerSkill skill,MclslCombatCareerState state)
    {
        bool unlocked=state.Rank>=skill.Tier;
        bool realm=MclslActorAccessor.IsCultivator(_actor)&&MclslRealmIds.Index(MclslActorAccessor.Realm(_actor))>=skill.Tier-1;
        string status=!unlocked?"未解锁":!realm?"修为不足":skill.Passive?"生效":"可用";
        double remaining=state.Cooldowns[skill.Tier-1]-MclslSpellCasting.Now;
        if(unlocked&&realm&&!skill.Passive)
        {
            if(remaining>0)status="冷却 "+Math.Ceiling(remaining)+"秒";
            else if(skill.Item.Length>0&&MclslBagSystem.Count(MclslBagSystem.Read(_actor),skill.Item)<1)status="缺少道具";
            else if(MclslSpellSystem.CurrentMana(_actor)<MclslCareerCombat.Mana(_actor,skill))status="灵力不足";
        }
        var go=new GameObject(skill.Id,typeof(RectTransform),typeof(Image),typeof(Button),typeof(LayoutElement));
        go.transform.SetParent(_content,false);go.GetComponent<LayoutElement>().preferredHeight=34;
        go.GetComponent<Image>().color=new Color(0.06f,0.2f,0.17f,0.85f);
        var icon=new GameObject("Icon",typeof(RectTransform),typeof(Image));icon.transform.SetParent(go.transform,false);
        var ir=(RectTransform)icon.transform;ir.anchorMin=ir.anchorMax=new Vector2(0,0.5f);ir.pivot=new Vector2(0,0.5f);ir.anchoredPosition=new Vector2(2,0);ir.sizeDelta=new Vector2(28,28);
        Image image=icon.GetComponent<Image>();image.sprite=SpriteTextureLoader.getSprite(skill.Icon);image.preserveAspect=true;
        image.color=unlocked&&realm?Color.white:new Color(0.45f,0.45f,0.45f);
        var text=new GameObject("Label",typeof(RectTransform),typeof(Text));text.transform.SetParent(go.transform,false);
        var tr=(RectTransform)text.transform;tr.anchorMin=Vector2.zero;tr.anchorMax=Vector2.one;tr.offsetMin=new Vector2(33,0);tr.offsetMax=new Vector2(-2,0);
        Text label=text.GetComponent<Text>();label.font=LocalizedTextManager.current_font;label.fontSize=MclslUiTheme.ReadableFontSize(10);
        label.text=skill.Name+" · "+(skill.Passive?"被动":"主动")+"\n"+status;label.color=unlocked?Color.white:Color.gray;
        go.GetComponent<Button>().onClick.AddListener(()=>{_expanded=_expanded==skill.Id?null:skill.Id;Build();});
        if(_expanded!=skill.Id)return;
        string detail=skill.Description+"\n解锁："+MclslCombatCareerCatalog.Ranks[skill.Tier]+"，修为达到"+new[]{"炼气","筑基","金丹","元婴","化神"}[skill.Tier-1];
        if(!skill.Passive)
        {
            detail+="\n熟练度："+state.Mastery[skill.Tier-1]+" / 10";
            detail+="\n灵力："+MclslCareerCombat.Mana(_actor,skill)+" · 冷却："+skill.Cooldown+"秒";
            detail+="\n"+(skill.Preparation>0?"蓄力 "+skill.Preparation.ToString("0.##")+"秒":"即时起手")+" · 最多目标："+skill.Targets;
            if(skill.Item.Length>0)detail+="\n物品消耗："+MclslItemCatalog.Get(skill.Item).Name+" ×1";
            detail+="\n人物自动施放；点击本条仅查看说明。";
        }
        TextRow(skill.Id+"Detail",detail,10);
    }
}
