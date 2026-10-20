using System;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.UI;

namespace MySimulatedLongevityRoad.UI;

internal static class MclslBeastControlWindow
{
    private const string Id="MclslBeastControl";
    private static ScrollWindow _window;
    private static Transform _content;
    private static Text _summary;
    private static readonly System.Collections.Generic.List<GameObject> Records=new();
    private static readonly System.Collections.Generic.List<Action> RefreshLabels=new();
    internal static void Show()
    {
        if(_window==null)Create();
        foreach(var refresh in RefreshLabels)refresh();
        _summary.text="世界已解锁："+MclslBeastWorld.State.Unlocked+"级兽潮\n手动放置境界："+MclslRealmIds.Display(MclslRealmIds.Ordered[MclslBeastWorld.ManualRealm]);
        MclslWindowOpenGuard.Show(_window,Id,false);
    }
    private static void Create()
    {
        MclslLocalizationBridge.RegisterKey(Id,"妖兽与兽潮");
        _window=WindowCreator.CreateEmptyWindow(Id,Id,"ui/Icons/TianDiZhiBian");
        var background=_window.transform.Find("Background");background.GetComponent<RectTransform>().sizeDelta=new Vector2(370,540);
        var old=background.Find("Scroll View");if(old!=null)old.gameObject.SetActive(false);
        var scroll=DefaultControls.CreateScrollView(new DefaultControls.Resources());scroll.transform.SetParent(background,false);
        var rect=scroll.GetComponent<RectTransform>();rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=new Vector2(15,20);rect.offsetMax=new Vector2(-15,-45);
        scroll.GetComponent<Image>().color=new Color(0.1f,0.12f,0.16f,0.95f);
        var scroller=scroll.GetComponent<ScrollRect>();scroller.horizontal=false;_content=scroller.content;
        var layout=_content.gameObject.AddComponent<VerticalLayoutGroup>();layout.spacing=5;layout.childControlHeight=true;layout.childForceExpandHeight=false;
        _content.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
        _summary=Label("妖兽与兽潮");
        Button(()=>"兽潮："+(MclslBeastWorld.Enabled?"开启":"关闭"),()=>MclslBeastConfig.SetSwitch("MCLSL_config_beast_waves",!MclslBeastWorld.Enabled));
        Button(()=>"自然出现："+(MclslBeastConfig.NaturalSpawnEnabled?"开启":"关闭"),()=>MclslBeastConfig.SetSwitch("MCLSL_config_beast_natural_spawn",!MclslBeastConfig.NaturalSpawnEnabled));
        Button(()=>"法术特效："+(MclslSpellEffect.Enabled?"开启":"关闭"),()=>MclslBeastConfig.SetSwitch("MCLSL_config_spell_effects_enabled",!MclslSpellEffect.Enabled));
        Button(()=>"间隔："+MclslBeastWorld.Interval+"年（点击递增）",()=>MclslBeastConfig.SetInt("MCLSL_config_beast_wave_interval",MclslBeastWorld.Interval>=30?1:MclslBeastWorld.Interval+1));
        Button(()=>"数量倍率："+MclslBeastWorld.CountMultiplier.ToString("0.0"),()=>MclslBeastConfig.SetInt("MCLSL_config_beast_wave_count_percent",MclslBeastWorld.CountMultiplier>=3?50:(int)(MclslBeastWorld.CountMultiplier*100)+50));
        Button(()=>"自动最高等级："+MclslBeastWorld.MaximumLevel,()=>MclslBeastConfig.SetInt("MCLSL_config_beast_wave_max_level",MclslBeastWorld.MaximumLevel%7+1));
        Button(()=>"单次兽潮上限："+MclslBeastWorld.MaximumWaveCount,()=>MclslBeastConfig.SetInt("MCLSL_config_beast_wave_limit",MclslBeastWorld.MaximumWaveCount>=256?16:Math.Min(256,MclslBeastWorld.MaximumWaveCount+16)));
        Button(()=>"自动妖兽上限："+MclslBeastWorld.AutomaticLimit,()=>MclslBeastConfig.SetInt("MCLSL_config_beast_auto_limit",MclslBeastWorld.AutomaticLimit>=2048?128:MclslBeastWorld.AutomaticLimit*2));
        Button(()=>"自然妖兽上限："+MclslBeastWorld.NaturalLimit,()=>MclslBeastConfig.SetInt("MCLSL_config_beast_natural_limit",MclslBeastWorld.NaturalLimit>=512?32:MclslBeastWorld.NaturalLimit*2));
        Button(()=>"手动放置："+MclslRealmIds.Display(MclslRealmIds.Ordered[MclslBeastWorld.ManualRealm]),()=>MclslBeastWorld.ManualRealm=(MclslBeastWorld.ManualRealm+1)%7);
        Label("手动触发（不受世界解锁限制）");
        for(int level=1;level<=7;level++){int captured=level;Button(()=>captured+"级 · "+MclslRealmIds.Display(MclslRealmIds.Ordered[captured-1]),()=>MclslBeastWorld.RequestManual(captured));}
        Button(()=>"查看兽潮记录",ShowRecords);_window.gameObject.SetActive(false);
    }
    private static Text Label(string text)
    {
        var go=DefaultControls.CreateText(new DefaultControls.Resources());go.transform.SetParent(_content,false);
        var label=go.GetComponent<Text>();label.text=text;label.font=_window.titleText.font;label.fontSize=13;label.color=Color.white;
        go.AddComponent<LayoutElement>().preferredHeight=45;return label;
    }
    private static GameObject Button(Func<string> title,Action action,bool persistent=true)
    {
        var go=DefaultControls.CreateButton(new DefaultControls.Resources());go.transform.SetParent(_content,false);
        go.GetComponent<Image>().color=new Color(0.22f,0.28f,0.34f,1);
        var text=go.GetComponentInChildren<Text>();text.font=_window.titleText.font;text.fontSize=13;text.color=Color.white;text.text=title();
        go.AddComponent<LayoutElement>().preferredHeight=32;
        if(persistent) RefreshLabels.Add(()=>text.text=title());
        go.GetComponent<Button>().onClick.AddListener(()=>{action();text.text=title();});return go;
    }
    private static void ShowRecords()
    {
        foreach(var record in Records)UnityEngine.Object.Destroy(record);Records.Clear();
        _summary.text=MclslBeastWorld.Waves.Count==0?"本世尚无兽潮。":"点击记录定位兽潮。";
        foreach(var wave in MclslBeastWorld.Waves)
        {
            var captured=wave;
            Records.Add(Button(()=>captured.Year+"年 "+captured.Level+"级"+captured.Theme+" · "+captured.Status+" · 剩余"+MclslBeastWorld.Remaining(captured),
                ()=>MoveCamera.instance?.focusOn(new Vector3(captured.X,captured.Y,0)),false));
        }
    }
}
