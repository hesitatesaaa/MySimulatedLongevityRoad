using System;
using System.Collections.Generic;
using System.Linq;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Traits;
using NeoModLoader.General;
using NeoModLoader.General.UI.Tab;
using UnityEngine;
using UnityEngine.Events;

namespace MySimulatedLongevityRoad.UI;

internal static class MclslLivingCatalog
{
    private static PowersTab _root,_beasts,_named,_sapient;
    internal static void Init()
    {
        _root=Create("mclsl_living_tab","生灵","妖兽、具名生物与智慧生物","ui/Icons/Xuanhuang",false);
        _beasts=Create("mclsl_beast_tab","妖兽","放置妖兽；境界与上限使用现有妖兽设置。","ui/Icons/TianDiZhiLi",true);
        _named=Create("mclsl_immortal_tab","具名生物","放置仙道人物、原著人物与具名妖兽。","trait/Bai",true);
        _sapient=Create("mclsl_sapient_tab","智慧生物","放置玄黄族智慧文明。","ui/Icons/Xuanhuang",true);
        Entry(_root,"mclsl_living_beasts","妖兽","ui/Icons/TianDiZhiLi",()=>_beasts.showTab(null));
        Entry(_root,"mclsl_living_named","具名生物","trait/Bai",()=>_named.showTab(null));
        Entry(_root,"mclsl_living_sapient","智慧生物","ui/Icons/Xuanhuang",()=>_sapient.showTab(null));
        foreach(var tab in new[]{_beasts,_named,_sapient})
            Entry(tab,tab.name+"_return","返回生灵","ui/icons/iconBook",()=>_root.showTab(null));
        MclslBeastActorRegistration.AddPowers(_beasts);
        MclslImmortalActorRegistration.AddPowers(_named);
        MclslNamedCharacterRegistration.AddPowers(_named);
        MclslXuanhuangRegistration.AddPowers(_sapient);
        var vanilla=UnityEngine.Object.FindObjectsOfType<PowersTab>(true).Single(t=>t.name=="units");
        PowerButton shortcut=Button("mclsl_vanilla_xuanhuang","玄黄族目录","ui/Icons/Xuanhuang",()=>_sapient.showTab(null));
        float x=vanilla.GetComponentsInChildren<PowerButton>(true).Select(b=>b.transform.localPosition.x).DefaultIfEmpty(0).Max()+40;
        shortcut.transform.SetParent(vanilla.transform,false);shortcut.transform.localPosition=new Vector3(x,0,0);
        vanilla._power_buttons.Add(shortcut);vanilla.findNeighbours();
    }
    private static PowersTab Create(string id,string name,string description,string icon,bool hidden)
    {
        MclslLocalizationBridge.RegisterKey(id,name);MclslLocalizationBridge.RegisterKey(id+" Description",description);
        var tab=TabManager.CreateTab(id,id,id+" Description",SpriteTextureLoader.getSprite(icon),"hotkey_tip_tab_other");
        PowersTabExtension.SetLayout(tab,new List<string>{"tab"});
        if(hidden)tab.powerButton.gameObject.SetActive(false);
        return tab;
    }
    private static PowerButton Button(string id,string label,string icon,UnityAction action)
    {
        MclslLocalizationBridge.RegisterKey(id,label);MclslLocalizationBridge.RegisterKey(id+" Description",label);
        var button=PowerButtonCreator.CreateSimpleButton(id,action,SpriteTextureLoader.getSprite(icon),null,default);
        TipButton tip=button.GetComponent<TipButton>();tip.textOnClick=id;tip.textOnClickDescription=id+" Description";
        return button;
    }
    private static void Entry(PowersTab tab,string id,string label,string icon,UnityAction action)
        =>PowersTabExtension.AddPowerButton(tab,"tab",Button(id,label,icon,action));
}
