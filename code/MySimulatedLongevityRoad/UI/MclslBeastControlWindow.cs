using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
    private static RectTransform _background,_body;
    private static RectTransform _ledger;
    private static GameObject _actionBar;
    private static Text _summary,_feedback;
    private static ScrollRect _scroll;
    private static Transform _content;
    private static int _page,_selectedLevel=1;
    private static float _nextRefresh;
    private static string _recordSignature;
    private static readonly List<Action> Refreshers=new();
    private static readonly List<Button> Tabs=new(),LevelButtons=new();
    internal static int CurrentPage=>_page;
    internal static void Show()
    {
        if(_window==null)Create();
        SelectPage(0);
        MclslWindowOpenGuard.Show(_window,Id,false);
        Fit();Refresh();
    }
    private static void Create()
    {
        MclslLocalizationBridge.RegisterKey(Id,"妖兽与兽潮");
        _window=WindowCreator.CreateEmptyWindow(Id,Id,"ui/Icons/TianDiZhiBian");
        _background=_window.transform.Find("Background").GetComponent<RectTransform>();
        var old=_background.Find("Scroll View");if(old!=null)old.gameObject.SetActive(false);
        _background.GetComponent<Image>().color=Color.white;
        var root=Box("BeastLedger",_background,MclslUiTheme.SurfaceWindow);
        _ledger=root.GetComponent<RectTransform>();
        Stretch(root.GetComponent<RectTransform>(),12,12,12,40);
        var title=_window.titleText.rectTransform;title.SetParent(_background,false);title.anchorMin=title.anchorMax=new Vector2(.5f,1);title.anchoredPosition=new Vector2(0,-17);title.sizeDelta=new Vector2(380,24);_window.titleText.color=MclslUiTheme.AccentGold;_window.titleText.resizeTextForBestFit=false;_window.titleText.fontSize=14;
        if(_window._close_button_tip!=null){var closeRect=_window._close_button_tip.GetComponent<RectTransform>();closeRect.SetParent(_background,false);closeRect.anchorMin=closeRect.anchorMax=new Vector2(1,1);closeRect.anchoredPosition=new Vector2(-17,-17);closeRect.sizeDelta=new Vector2(26,26);closeRect.SetAsLastSibling();}
        if(_window.close_background!=null){var closeRect=_window.close_background.rectTransform;closeRect.SetParent(_background,false);closeRect.anchorMin=closeRect.anchorMax=new Vector2(1,1);closeRect.anchoredPosition=new Vector2(-17,-17);closeRect.sizeDelta=new Vector2(26,26);_window._close_button_tip?.transform.SetAsLastSibling();}
        _summary=Text(root.transform,"兽潮概况",14,48,MclslUiTheme.AccentGold);
        Top(_summary.rectTransform,0,48);
        var nav=Row(root.transform,30);
        Top(nav.GetComponent<RectTransform>(),55,30);
        string[] labels={"兽潮调控","手动召潮","兽潮纪事"};
        for(int i=0;i<labels.Length;i++){int index=i;Tabs.Add(ActionButton(nav.transform,labels[i],()=>SelectPage(index),0,true));}
        var scrollObject=Box("LedgerScroll",root.transform,MclslUiTheme.SurfaceScroll);
        Stretch(scrollObject.GetComponent<RectTransform>(),0,37,10,92);
        _scroll=scrollObject.AddComponent<ScrollRect>();_scroll.horizontal=false;_scroll.movementType=ScrollRect.MovementType.Clamped;_scroll.scrollSensitivity=24;
        var viewport=Box("Viewport",scrollObject.transform,Color.white);viewport.AddComponent<RectMask2D>();viewport.GetComponent<Image>().enabled=false;
        Stretch(viewport.GetComponent<RectTransform>(),0,0,0,0);_body=viewport.GetComponent<RectTransform>();
        var content=new GameObject("Content",typeof(RectTransform),typeof(VerticalLayoutGroup),typeof(ContentSizeFitter));content.transform.SetParent(viewport.transform,false);
        var rect=content.GetComponent<RectTransform>();rect.anchorMin=new Vector2(0,1);rect.anchorMax=new Vector2(1,1);rect.pivot=new Vector2(.5f,1);rect.sizeDelta=Vector2.zero;
        var stack=content.GetComponent<VerticalLayoutGroup>();stack.spacing=6;stack.padding=new RectOffset(8,8,8,8);stack.childControlHeight=true;stack.childForceExpandHeight=false;
        content.GetComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
        _content=content.transform;_scroll.viewport=_body;_scroll.content=rect;
        // Without a visible scrollbar the ledger looks truncated; native UI has none here.
        var barObject=Box("Scrollbar",scrollObject.transform,MclslUiTheme.SurfaceDeep);
        var barRect=barObject.GetComponent<RectTransform>();
        barRect.anchorMin=new Vector2(1,0);barRect.anchorMax=new Vector2(1,1);barRect.pivot=new Vector2(1,.5f);
        barRect.offsetMin=new Vector2(-8,0);barRect.offsetMax=new Vector2(0,0);
        var handle=Box("Handle",barObject.transform,MclslUiTheme.AccentJade);
        var handleRect=handle.GetComponent<RectTransform>();handleRect.anchorMin=Vector2.zero;handleRect.anchorMax=Vector2.one;handleRect.offsetMin=Vector2.zero;handleRect.offsetMax=Vector2.zero;
        _scroll.verticalScrollbar=barObject.AddComponent<Scrollbar>();
        _scroll.verticalScrollbar.handleRect=handleRect;_scroll.verticalScrollbar.direction=Scrollbar.Direction.BottomToTop;
        _scroll.verticalScrollbar.targetGraphic=handle.GetComponent<Image>();
        _scroll.verticalScrollbarVisibility=ScrollRect.ScrollbarVisibility.Permanent;
        // Manual wave invocation stays reachable while the body scrolls.
        _actionBar=Box("LedgerActions",root.transform,MclslUiTheme.SurfacePanel);
        var actionRect=_actionBar.GetComponent<RectTransform>();
        actionRect.anchorMin=new Vector2(0,0);actionRect.anchorMax=new Vector2(1,0);actionRect.pivot=new Vector2(.5f,0);
        actionRect.anchoredPosition=new Vector2(0,31);actionRect.sizeDelta=new Vector2(0,34);
        var actionLayout=_actionBar.AddComponent<HorizontalLayoutGroup>();
        actionLayout.spacing=6;actionLayout.padding=new RectOffset(8,8,3,3);actionLayout.childControlWidth=true;actionLayout.childControlHeight=true;actionLayout.childForceExpandWidth=false;
        _actionBar.SetActive(false);
        _feedback=Text(root.transform,"调整立即保存 · 手动召潮可越过世界解锁等级",12,30,MclslUiTheme.TextMuted);
        var footer=_feedback.rectTransform;footer.anchorMin=Vector2.zero;footer.anchorMax=new Vector2(1,0);footer.pivot=new Vector2(.5f,0);footer.anchoredPosition=Vector2.zero;footer.sizeDelta=new Vector2(0,30);
        _window.gameObject.AddComponent<MclslBeastControlLifecycle>();
        _window.gameObject.SetActive(false);
    }
    private static void Fit()
    {
        var canvas=_background.GetComponentInParent<Canvas>().rootCanvas.GetComponent<RectTransform>();
        var size=new Vector2(Mathf.Min(500,canvas.rect.width-28),Mathf.Min(440,canvas.rect.height-28));
        if(_background.sizeDelta!=size){_background.sizeDelta=size;RebuildLayout();}
    }
    internal static void Tick()
    {
        if(_window==null||!_window.gameObject.activeInHierarchy||Time.unscaledTime<_nextRefresh)return;
        _nextRefresh=Time.unscaledTime+.75f;Fit();Refresh();
        if(_page==2&&Signature()!=_recordSignature){float position=_scroll.verticalNormalizedPosition;SelectPage(2);_scroll.verticalNormalizedPosition=position;}
    }
    private static void Refresh()
    {
        _summary.text="世界已解锁  "+MclslBeastWorld.State.Unlocked+" 级    自动最高  "+Math.Min(MclslBeastWorld.State.Unlocked,MclslBeastWorld.MaximumLevel)+" 级\n活动兽潮  "+MclslBeastWorld.Waves.Count(w=>w.Status!="已结束")+" 场    手动放置  "+Realm(MclslBeastWorld.ManualRealm);
        foreach(var refresh in Refreshers)refresh();
        if(_page==1&&!string.IsNullOrEmpty(MclslBeastWorld.ManualFeedback))_feedback.text=MclslBeastWorld.ManualFeedback;
    }
    internal static void SelectPage(int page)
    {
        _page=page;Refreshers.Clear();LevelButtons.Clear();
        foreach(Transform child in _actionBar.transform){child.gameObject.SetActive(false);UnityEngine.Object.Destroy(child.gameObject);}
        _actionBar.SetActive(page==1);
        Stretch(_scroll.GetComponent<RectTransform>(),0,page==1?71:37,10,92);
        foreach(Transform child in _content){child.gameObject.SetActive(false);UnityEngine.Object.Destroy(child.gameObject);}
        for(int i=0;i<Tabs.Count;i++)Tabs[i].GetComponent<Image>().color=i==page?new Color(.20f,.38f,.32f):MclslUiTheme.SurfacePanel;
        _feedback.text=page==0?"调整立即保存 · 输入数值后按回车或点击别处应用":"";
        if(page==0)Settings();else if(page==1)Manual();else Records();
        RebuildLayout();_scroll.verticalNormalizedPosition=1;Refresh();
    }
    private static void RebuildLayout()
    {
        if(_content!=null)LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_content);
        if(_ledger!=null)LayoutRebuilder.ForceRebuildLayoutImmediate(_ledger);
    }
    private static void Settings()
    {
        Section("出现规则");
        Switch("兽潮",()=>MclslBeastWorld.Enabled,v=>MclslBeastConfig.SetSwitch("MCLSL_config_beast_waves",v));
        Switch("自然出现",()=>MclslBeastConfig.NaturalSpawnEnabled,v=>MclslBeastConfig.SetSwitch("MCLSL_config_beast_natural_spawn",v));
        Switch("法术特效",()=>MclslSpellEffect.Enabled,v=>MclslBeastConfig.SetSwitch("MCLSL_config_spell_effects_enabled",v));
        Section("强度与数量");
        Number("兽潮间隔","年",()=>MclslBeastWorld.Interval,v=>Set("beast_wave_interval",v),1,1000);
        Number("数量倍率","倍",()=>Math.Round(MclslBeastWorld.CountMultiplier,1),v=>Set("beast_wave_count_percent",v*100),.1,10,.1);
        Number("自动最高等级","级",()=>MclslBeastWorld.MaximumLevel,v=>Set("beast_wave_max_level",v),1,7);
        Section("生成上限");
        Number("单次兽潮上限","只",()=>MclslBeastWorld.MaximumWaveCount,v=>Set("beast_wave_limit",v),1,256);
        Number("自动妖兽总上限","只",()=>MclslBeastWorld.AutomaticLimit,v=>Set("beast_auto_limit",v),1,4096);
        Number("自然妖兽上限","只",()=>MclslBeastWorld.NaturalLimit,v=>Set("beast_natural_limit",v),1,4096);
        Note("自动妖兽总上限由自然出现和兽潮共享。自动等级同时受世界解锁进度限制。");
    }
    private static void Set(string suffix,double value)=>MclslBeastConfig.SetInt("MCLSL_config_"+suffix,(int)Math.Round(value));
    private static void Manual()
    {
        Section("选择兽潮等级");Note("手动召潮不受世界解锁等级限制；仍遵循栖息地、数量和同时活动场次限制。");
        for(int row=0;row<3;row++)
        {
            var line=Row(_content,42);
            for(int col=0;col<3&&row*3+col<7;col++)
            {
                int level=row*3+col+1;
                var button=ActionButton(line.transform,level+"级 · "+Realm(level-1),()=>{_selectedLevel=level;RefreshLevels();},0,true);
                LevelButtons.Add(button);
            }
        }
        ActionButton(_actionBar.transform,"引动兽潮",()=>{MclslBeastWorld.RequestManual(_selectedLevel);Refresh();},0,true,28);
        Section("手动放置境界");
        Number("放置境界","",()=>MclslBeastWorld.ManualRealm,v=>MclslBeastWorld.ManualRealm=(int)v,0,6,1,v=>Realm((int)v));
        Note("这里仅改变生灵目录中妖兽的放置境界，不改变上方选中的兽潮等级。");RefreshLevels();
    }
    private static void RefreshLevels(){for(int i=0;i<LevelButtons.Count;i++)LevelButtons[i].GetComponent<Image>().color=i+1==_selectedLevel?new Color(.27f,.45f,.34f):MclslUiTheme.SurfacePanel;}
    private static string Realm(int index)=>MclslRealmIds.Display(MclslRealmIds.Ordered[Math.Clamp(index,0,6)]);
    private static string Signature()=>string.Join("|",MclslBeastWorld.Waves.Select(w=>w.Id+":"+w.Status+":"+MclslBeastWorld.Remaining(w)));
    private static void Records()
    {
        _recordSignature=Signature();Section("本世兽潮纪事");
        if(MclslBeastWorld.Waves.Count==0){Note("本世尚无兽潮。\n兽潮发生后，其等级、主题与去向将记于此册。");return;}
        foreach(var wave in MclslBeastWorld.Waves.Reverse())
        {
            var captured=wave;var card=Row(_content,62);
            Text(card.transform,wave.Year+"年 · "+wave.Level+"级"+wave.Theme+"兽潮\n"+wave.Status+"    剩余 "+MclslBeastWorld.Remaining(wave)+" 只",13,0,MclslUiTheme.TextPrimary,true);
            ActionButton(card.transform,"定位",()=>MoveCamera.instance?.focusOn(new Vector3(captured.X,captured.Y,0)),48);
        }
    }
    private static void Switch(string label,Func<bool> read,Action<bool> write)
    {
        var row=Row(_content,32);Text(row.transform,label,13,0,MclslUiTheme.TextPrimary,true);
        var button=ActionButton(row.transform,"",()=>{write(!read());Refresh();},82);
        Refreshers.Add(()=>{button.GetComponentInChildren<Text>().text=read()?"● 已开启":"○ 已关闭";button.GetComponent<Image>().color=read()?new Color(.17f,.38f,.30f):MclslUiTheme.SurfaceDeep;});
    }
    private static void Number(string label,string unit,Func<double> read,Action<double> write,double min,double max,double step=1,Func<double,string> display=null)
    {
        var row=Row(_content,34);row.name="Setting_"+label;
        Text(row.transform,label,13,0,MclslUiTheme.TextPrimary,true);
        Button minus=null,plus=null;InputField input=null;
        Action<double> commit=value=>{write(Math.Round(value,1));_feedback.text=label+"已调整为 "+(display==null?read().ToString("0.#",CultureInfo.InvariantCulture):display(read()))+unit;Refresh();};
        minus=ActionButton(row.transform,"−",()=>commit(Math.Max(min,read()-step)),26);
        var field=DefaultControls.CreateInputField(new DefaultControls.Resources());field.name="Value_"+label;field.transform.SetParent(row.transform,false);field.GetComponent<Image>().color=MclslUiTheme.SurfaceDeep;
        field.AddComponent<LayoutElement>().preferredWidth=display==null?60:72;input=field.GetComponent<InputField>();input.textComponent.font=_window.titleText.font;input.textComponent.fontSize=14;input.textComponent.color=MclslUiTheme.AccentJade;input.textComponent.alignment=TextAnchor.MiddleCenter;input.characterLimit=8;
        input.textComponent.rectTransform.offsetMin=new Vector2(4,0);input.textComponent.rectTransform.offsetMax=new Vector2(-4,0);input.textComponent.verticalOverflow=VerticalWrapMode.Overflow;
        if(input.placeholder!=null)input.placeholder.gameObject.SetActive(false);
        input.onEndEdit.AddListener(value=>
        {
            if(display!=null){input.SetTextWithoutNotify(display(read()));return;}
            if(!double.TryParse(value,NumberStyles.Float,CultureInfo.InvariantCulture,out double number)||double.IsNaN(number)||double.IsInfinity(number)||number<min||number>max||(step==1&&number!=Math.Truncate(number))||(step<1&&Math.Abs(number*10-Math.Round(number*10))>.00001))
            {_feedback.text=label+"需为 "+min.ToString(CultureInfo.InvariantCulture)+"–"+max.ToString(CultureInfo.InvariantCulture)+(step==1?" 的整数":"，最多一位小数");input.SetTextWithoutNotify(read().ToString("0.#",CultureInfo.InvariantCulture));return;}
            commit(number);
        });
        input.readOnly=display!=null;
        plus=ActionButton(row.transform,"+",()=>commit(Math.Min(max,read()+step)),26);
        if(unit.Length>0)Text(row.transform,unit,12,0,MclslUiTheme.TextMuted,false,18);
        Refreshers.Add(()=>{if(!input.isFocused)input.SetTextWithoutNotify(display==null?read().ToString("0.#",CultureInfo.InvariantCulture):display(read()));minus.interactable=read()>min;plus.interactable=read()<max;});
    }
    private static void Section(string label)=>Text(_content,label,14,26,MclslUiTheme.AccentGold);
    private static void Note(string label)=>Text(_content,label,12,44,MclslUiTheme.TextMuted);
    private static GameObject Box(string name,Transform parent,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);
        // A native sliced sprite has a large intrinsic border/minimum height.
        // Flat theme surfaces keep compact layout rows from expanding to it.
        var image=go.GetComponent<Image>();image.color=color;image.sprite=null;return go;
    }
    private static GameObject Row(Transform parent,float height)
    {
        var go=Box("Row",parent,MclslUiTheme.SurfacePanel);go.AddComponent<LayoutElement>().preferredHeight=height;
        var edge=go.AddComponent<Outline>();edge.effectColor=new Color(.49f,.65f,.55f,.25f);edge.effectDistance=new Vector2(1,-1);
        var layout=go.AddComponent<HorizontalLayoutGroup>();layout.spacing=5;layout.padding=new RectOffset(8,8,3,3);layout.childControlWidth=true;layout.childForceExpandWidth=false;layout.childControlHeight=true;return go;
    }
    private static Text Text(Transform parent,string value,int size,float height,Color color,bool flexible=false,float width=0)
    {
        var go=new GameObject("Text",typeof(RectTransform),typeof(Text),typeof(LayoutElement));go.transform.SetParent(parent,false);
        var label=go.GetComponent<Text>();label.text=value;label.font=_window.titleText.font;label.fontSize=size;label.color=color;label.alignment=TextAnchor.MiddleLeft;label.raycastTarget=false;
        var element=go.GetComponent<LayoutElement>();if(height>0)element.preferredHeight=height;if(width>0)element.preferredWidth=width;if(flexible)element.flexibleWidth=1;return label;
    }
    private static Button ActionButton(Transform parent,string label,Action action,float width=0,bool flexible=false,float height=0)
    {
        var go=Box("Button_"+label,parent,MclslUiTheme.SurfacePanel);var button=go.AddComponent<Button>();button.targetGraphic=go.GetComponent<Image>();
        var element=go.AddComponent<LayoutElement>();if(width>0)element.preferredWidth=width;if(flexible)element.flexibleWidth=1;if(height>0)element.preferredHeight=height;
        var text=Text(go.transform,label,13,0,MclslUiTheme.TextPrimary);text.alignment=TextAnchor.MiddleCenter;Stretch(text.rectTransform,2,2,2,2);
        button.onClick.AddListener(()=>action());return button;
    }
    private static void Stretch(RectTransform rect,float left,float bottom,float right,float top)
    {rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=new Vector2(left,bottom);rect.offsetMax=new Vector2(-right,-top);}
    private static void Top(RectTransform rect,float top,float height)
    {rect.anchorMin=new Vector2(0,1);rect.anchorMax=new Vector2(1,1);rect.pivot=new Vector2(.5f,1);rect.anchoredPosition=new Vector2(0,-top);rect.sizeDelta=new Vector2(0,height);}
}
internal sealed class MclslBeastControlLifecycle:MonoBehaviour
{
    private void Update()=>MclslBeastControlWindow.Tick();
}
