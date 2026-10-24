using System;
using System.Collections.Generic;
using System.Linq;
using MySimulatedLongevityRoad.Traits;
using MySimulatedLongevityRoad.Systems;
using NeoModLoader.General;
using NeoModLoader.General.UI.Tab;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MySimulatedLongevityRoad.UI;

internal static class MclslLivingCatalog
{
    private static PowersTab _root;
    private static readonly Dictionary<string,List<PowerButton>> Pages=new();
    internal static string CurrentPage {get;private set;}="home";
    internal static bool VanillaPreserved {get;private set;}
    internal static void Init()
    {
        if(_root!=null)return;
        var vanilla=UnityEngine.Object.FindObjectsOfType<PowersTab>(true).Where(t=>!t.name.StartsWith("mclsl_")).ToArray();
        var originals=vanilla.SelectMany(t=>t.GetComponentsInChildren<PowerButton>(true)).ToDictionary(b=>b,b=>b.gameObject.activeSelf);
        var entries=vanilla.Where(t=>t.powerButton!=null).Select(t=>t.powerButton).Distinct().ToDictionary(b=>b,b=>b.gameObject.activeSelf);
        const string id="mclsl_living_tab";
        MclslLocalizationBridge.RegisterKey(id,"生灵");
        MclslLocalizationBridge.RegisterKey(id+" Description","妖兽、仙道人物与玄黄族");
        _root=TabManager.CreateTab(id,id,id+" Description",SpriteTextureLoader.getSprite("ui/Icons/Xuanhuang"),"hotkey_tip_tab_other");
        // NML clones the units tab; its powerButton still points at the original
        // units entry. Bind our own entry before touching navigation state.
        _root.powerButton=UnityEngine.Object.FindObjectsOfType<Button>(true).Single(b=>b.name=="Button_"+id);
        PowersTabExtension.SetLayout(_root,new List<string>{"tab"});
        Pages["home"]=new List<PowerButton>();
        Entry("mclsl_living_beasts","妖兽","ui/Icons/TianDiZhiLi","beasts");
        Entry("mclsl_living_named","仙道人物","trait/Bai","named");
        Entry("mclsl_living_sapient","玄黄族","ui/Icons/Xuanhuang","xuanhuang");
        Populate("beasts",MclslBeastActorRegistration.AddPowers);
        Populate("named",t=>{MclslImmortalActorRegistration.AddPowers(t);MclslNamedCharacterRegistration.AddPowers(t);});
        Populate("xuanhuang",MclslXuanhuangRegistration.AddPowers);
        _root.gameObject.AddComponent<MclslLivingCatalogLifecycle>();
        ShowPage("home");
        VanillaPreserved=originals.All(p=>p.Key!=null&&p.Key.gameObject.activeSelf==p.Value)
            &&entries.All(p=>p.Key!=null&&p.Key.gameObject.activeSelf==p.Value);
        if(!VanillaPreserved)throw new InvalidOperationException("生灵目录改变了原版生物入口。");
    }
    private static void Populate(string page,Action<PowersTab> add)
    {
        var before=new HashSet<PowerButton>(_root.GetComponentsInChildren<PowerButton>(true));
        add(_root);
        Pages[page]=_root.GetComponentsInChildren<PowerButton>(true).Where(b=>!before.Contains(b)).ToList();
        Pages[page].Insert(0,Button("mclsl_living_"+page+"_return","返回生灵首页","ui/icons/iconBook",()=>ShowPage("home")));
    }
    private static PowerButton Button(string id,string label,string icon,UnityAction action)
    {
        MclslLocalizationBridge.RegisterKey(id,label);MclslLocalizationBridge.RegisterKey(id+" Description",label);
        var button=PowerButtonCreator.CreateSimpleButton(id,action,SpriteTextureLoader.getSprite(icon),null,default);
        button._button=button.GetComponent<Button>();
        button._button.onClick=new Button.ButtonClickedEvent();
        button._button.onClick.AddListener(action);
        TipButton tip=button.gameObject.AddComponent<TipButton>();tip.textOnClick=id;tip.textOnClickDescription=id+" Description";
        button.transform.SetParent(_root.transform,false);
        return button;
    }
    private static void Entry(string id,string label,string icon,string page)
        =>Pages["home"].Add(Button(id,label,icon,()=>ShowPage(page)));
    internal static void ShowPage(string page)
    {
        if(_root==null||!Pages.TryGetValue(page,out var visible))return;
        CurrentPage=page;
        foreach(var pair in Pages)foreach(var button in pair.Value)button.gameObject.SetActive(pair.Key==page);
        // Keep the native two-row power palette, including its horizontal scroll.
        for(int i=0;i<visible.Count;i++)
        {
            visible[i].transform.localPosition=new Vector3(119.4f+(i/2)*36,i%2==0?18:-18,0);
            visible[i].GetComponent<RectTransform>().sizeDelta=new Vector2(32,32);
        }
        _root._power_buttons.Clear();_root._power_buttons.AddRange(visible);
        _root.findNeighbours();
        if(_root.parentObj!=null)_root.setNewWidth();
    }
}
internal sealed class MclslLivingCatalogLifecycle:MonoBehaviour
{
    private void OnEnable()=>MclslLivingCatalog.ShowPage("home");
}
