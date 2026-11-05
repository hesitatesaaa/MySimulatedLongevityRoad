$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$source=Get-Content (Join-Path $root 'code/MySimulatedLongevityRoad/UI/MclslContractInfoBar.cs') -Raw -Encoding UTF8
$method=[regex]::Match($source,'(?s)    private static void BindClicks\(.*?\n    }').Value
$stubs=@'
using System;
using System.Collections.Generic;
namespace UnityEngine.Events {public delegate void UnityAction();}
class Selectable {public enum Transition{None}}
class Button {public class ButtonClickedEvent{readonly List<UnityEngine.Events.UnityAction> a=new List<UnityEngine.Events.UnityAction>();public void AddListener(UnityEngine.Events.UnityAction x){a.Add(x);}public void Invoke(){foreach(var x in a)x();}}public ButtonClickedEvent onClick=new ButtonClickedEvent();public object targetGraphic;public Selectable.Transition transition;}
class Node {public Button b;public T GetComponent<T>() where T:class=>b as T;public T AddComponent<T>() where T:new(){var t=new T();b=t as Button;return t;}}
class KeyValueField:Node {public Node value=new Node{b=new Button()};public object background=new object();public UnityEngine.Events.UnityAction on_click_value;public Node gameObject=>this;public void OnDisable(){on_click_value=null;}}
public class ContractRowClickTest {
 public static string Run(){
 var row=new KeyValueField();int owner=0,pet=0;UnityEngine.Events.UnityAction toPet=()=>pet++,toOwner=()=>owner++;
 BindClicks(row,toPet);row.value.b.onClick.Invoke();row.b.onClick.Invoke();if(pet!=2)throw new Exception("value/whole-row click");
 row.OnDisable();BindClicks(row,toPet);BindClicks(row,toPet);row.value.b.onClick.Invoke();if(pet!=3)throw new Exception("disable/rebind/duplicate");
 BindClicks(row,toOwner);row.b.onClick.Invoke();if(owner!=1||pet!=3)throw new Exception("switch target");
 return "PASS: value/whole-row clicks, disable/reactivate, no duplicate handlers, owner/pet switching";
 }
'@
Add-Type -TypeDefinition ($stubs+"`n"+$method+"`n}")
[ContractRowClickTest]::Run()
if($source.IndexOf('BindClicks(bar._row, bar.OnClick)') -lt $source.IndexOf('bar._row.gameObject.SetActive(true)')){throw 'Click rebinding must follow activation'}
