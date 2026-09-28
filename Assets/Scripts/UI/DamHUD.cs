using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Riverforge
{
    public sealed class DamHUD : MonoBehaviour
    {
        private DamSimulation _sim;
        private Font _font;
        private Text _money,_water,_power,_safety,_eco,_time,_weather,_message,_mission,_buildLabel,_speedLabel,_resultText;
        private Slider _turbine,_spill;
        private GameObject _resultPanel;
        private Button _build,_wall,_turbineUp,_spillUp;

        public void Initialize(DamSimulation sim)
        {
            _sim=sim; _font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            EnsureEventSystem(); BuildUI();
            _sim.Changed+=Refresh; _sim.Message+=OnMessage; _sim.WeatherChanged+=w=>_weather.text=w;
            Refresh(_sim.State);
        }

        private void EnsureEventSystem()
        {
            if(FindFirstObjectByType<EventSystem>()!=null)return;
            var e=new GameObject("EventSystem"); e.AddComponent<EventSystem>(); e.AddComponent<StandaloneInputModule>();
        }

        private void BuildUI()
        {
            var canvas=new GameObject("Dam HUD").AddComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.gameObject.AddComponent<CanvasScaler>().uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; canvas.GetComponent<CanvasScaler>().referenceResolution=new Vector2(1920,1080); canvas.gameObject.AddComponent<GraphicRaycaster>();
            var top=Panel(canvas.transform,"Top",new Vector2(0,1),new Vector2(1,1),new Vector2(0,-135),new Vector2(0,0),new Color(.055f,.075f,.085f,.92f));
            var row=top.AddComponent<HorizontalLayoutGroup>(); row.padding=new RectOffset(28,28,18,18); row.spacing=18; row.childForceExpandWidth=true;
            _money=Stat(top.transform,"FUNDS");_water=Stat(top.transform,"RESERVOIR");_power=Stat(top.transform,"POWER");_safety=Stat(top.transform,"SAFETY");_eco=Stat(top.transform,"ECOLOGY");_time=Stat(top.transform,"TIME");
            _weather=TextNode(canvas.transform,"Weather",26,TextAnchor.MiddleCenter,new Color(.96f,.96f,.92f)); SetRect(_weather.rectTransform,new Vector2(.35f,1),new Vector2(.65f,1),new Vector2(0,-195),new Vector2(0,-145));

            var left=Panel(canvas.transform,"Controls",new Vector2(0,0),new Vector2(0,0),new Vector2(28,28),new Vector2(650,520),new Color(.055f,.075f,.085f,.94f));
            var v=left.AddComponent<VerticalLayoutGroup>();v.padding=new RectOffset(22,22,20,20);v.spacing=12;v.childControlHeight=false;v.childForceExpandWidth=true;
            AddHeading(left.transform,"DAM CONTROL");
            _mission=TextNode(left.transform,"Mission",21,TextAnchor.UpperLeft,Color.white);_mission.text=_sim.MissionText; SetPreferred(_mission.gameObject,80);
            _build=ButtonNode(left.transform,"Build dam",()=>_sim.StartDamConstruction(),out _buildLabel); SetPreferred(_build.gameObject,58);
            AddSlider(left.transform,"Turbine load",out _turbine,v=>_sim.SetTurbine(v));
            AddSlider(left.transform,"Spillway gates",out _spill,v=>_sim.SetSpillway(v));
            var emergency=ButtonNode(left.transform,"EMERGENCY RELEASE",()=>_sim.EmergencyRelease(),out _); SetPreferred(emergency.gameObject,52);
            _wall=ButtonNode(left.transform,"Upgrade wall",()=>_sim.BuyWallUpgrade(),out _); SetPreferred(_wall.gameObject,48);
            _turbineUp=ButtonNode(left.transform,"Upgrade turbine",()=>_sim.BuyTurbineUpgrade(),out _); SetPreferred(_turbineUp.gameObject,48);
            _spillUp=ButtonNode(left.transform,"Upgrade spillway",()=>_sim.BuySpillwayUpgrade(),out _); SetPreferred(_spillUp.gameObject,48);

            var bottom=Panel(canvas.transform,"Bottom",new Vector2(1,0),new Vector2(1,0),new Vector2(-650,28),new Vector2(-28,230),new Color(.055f,.075f,.085f,.94f));
            var bv=bottom.AddComponent<VerticalLayoutGroup>();bv.padding=new RectOffset(20,20,16,16);bv.spacing=10;bv.childForceExpandWidth=true;
            _message=TextNode(bottom.transform,"Message",22,TextAnchor.UpperLeft,Color.white);_message.text="Welcome to Riverforge.";SetPreferred(_message.gameObject,80);
            var buttons=new GameObject("Actions",typeof(RectTransform),typeof(HorizontalLayoutGroup));buttons.transform.SetParent(bottom.transform,false);SetPreferred(buttons,55);var hl=buttons.GetComponent<HorizontalLayoutGroup>();hl.spacing=10;hl.childForceExpandWidth=true;
            ButtonNode(buttons.transform,"Pause",()=>_sim.TogglePause(),out _);
            ButtonNode(buttons.transform,"Speed",()=>_sim.CycleSpeed(),out _speedLabel);
            ButtonNode(buttons.transform,"Save",()=>_sim.SaveNow(),out _);

            _resultPanel=Panel(canvas.transform,"Result",new Vector2(.25f,.25f),new Vector2(.75f,.75f),Vector2.zero,Vector2.zero,new Color(.04f,.05f,.06f,.96f));
            var rv=_resultPanel.AddComponent<VerticalLayoutGroup>();rv.padding=new RectOffset(32,32,32,32);rv.spacing=18;rv.childAlignment=TextAnchor.MiddleCenter;
            _resultText=TextNode(_resultPanel.transform,"ResultText",38,TextAnchor.MiddleCenter,Color.white);SetPreferred(_resultText.gameObject,180);
            var restart=ButtonNode(_resultPanel.transform,"Start new project",()=>{_resultPanel.SetActive(false);_sim.NewGame();},out _);SetPreferred(restart.gameObject,62);
            _resultPanel.SetActive(false);
        }

        private void Refresh(DamGameState s)
        {
            _money.text="$"+(s.money/1000f).ToString("0.0")+"K";_water.text=(s.waterLevel*100f).ToString("0")+"%";_power.text=_sim.CurrentPowerMW.ToString("0.0")+" MW";_safety.text=s.safety.ToString("0")+"%";_eco.text=s.ecology.ToString("0")+"%";_time.text=$"Day {s.day}  {Mathf.FloorToInt(s.hour):00}:00";_weather.text=s.weather;
            _turbine.SetValueWithoutNotify(s.turbineLoad);_spill.SetValueWithoutNotify(s.spillway);
            _build.interactable=!s.damBuilt&&s.buildProgress<=0f&&s.money>=32000f;_buildLabel.text=s.damBuilt?"DAM COMMISSIONED":s.buildProgress>0f?$"BUILDING  {s.buildProgress*100f:0}%":"BUILD DAM  ·  $32K";
            _wall.interactable=s.damBuilt&&s.wallLevel<3;_turbineUp.interactable=s.damBuilt&&s.turbineLevel<3;_spillUp.interactable=s.damBuilt&&s.spillwayLevel<3;
            _speedLabel.text="Speed  "+_sim.TimeScale.ToString("0")+"×";
            if((s.won||s.failed)&&!_resultPanel.activeSelf){_resultPanel.SetActive(true);_resultText.text=s.won?$"PROJECT COMPLETE\n{s.totalEnergyMWh:0} MWh generated\nFinal safety {s.safety:0}%":"PROJECT FAILED\nReview reservoir releases and upgrade timing.";}
        }
        private void OnMessage(string m){_message.text=m;}

        private GameObject Panel(Transform p,string n,Vector2 amin,Vector2 amax,Vector2 omin,Vector2 omax,Color c){var go=new GameObject(n,typeof(RectTransform),typeof(Image));go.transform.SetParent(p,false);go.GetComponent<Image>().color=c;SetRect(go.GetComponent<RectTransform>(),amin,amax,omin,omax);return go;}
        private Text Stat(Transform p,string label){var holder=new GameObject(label,typeof(RectTransform),typeof(VerticalLayoutGroup));holder.transform.SetParent(p,false);var l=TextNode(holder.transform,"Label",17,TextAnchor.MiddleCenter,new Color(.7f,.75f,.77f));l.text=label;var v=TextNode(holder.transform,"Value",28,TextAnchor.MiddleCenter,Color.white);v.text="-";return v;}
        private void AddHeading(Transform p,string text){var t=TextNode(p,"Heading",30,TextAnchor.MiddleLeft,Color.white);t.text=text;SetPreferred(t.gameObject,42);}
        private Text TextNode(Transform p,string n,int size,TextAnchor anchor,Color color){var go=new GameObject(n,typeof(RectTransform),typeof(Text));go.transform.SetParent(p,false);var t=go.GetComponent<Text>();t.font=_font;t.fontSize=size;t.alignment=anchor;t.color=color;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;return t;}
        private Button ButtonNode(Transform p,string title,UnityEngine.Events.UnityAction action,out Text label){var go=new GameObject(title,typeof(RectTransform),typeof(Image),typeof(Button));go.transform.SetParent(p,false);go.GetComponent<Image>().color=new Color(.13f,.23f,.25f,.98f);var b=go.GetComponent<Button>();b.onClick.AddListener(action);label=TextNode(go.transform,"Label",21,TextAnchor.MiddleCenter,Color.white);label.text=title;SetRect(label.rectTransform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);return b;}
        private void AddSlider(Transform p,string title,out Slider slider,UnityEngine.Events.UnityAction<float> action){var holder=new GameObject(title,typeof(RectTransform),typeof(VerticalLayoutGroup));holder.transform.SetParent(p,false);SetPreferred(holder,72);var l=TextNode(holder.transform,"Label",18,TextAnchor.MiddleLeft,new Color(.84f,.87f,.87f));l.text=title;var go=new GameObject("Slider",typeof(RectTransform),typeof(Slider));go.transform.SetParent(holder.transform,false);SetPreferred(go,32);slider=go.GetComponent<Slider>();slider.minValue=0;slider.maxValue=1;slider.onValueChanged.AddListener(action);var bg=new GameObject("Background",typeof(RectTransform),typeof(Image));bg.transform.SetParent(go.transform,false);bg.GetComponent<Image>().color=new Color(.22f,.25f,.26f,1);SetRect(bg.GetComponent<RectTransform>(),new Vector2(0,.3f),new Vector2(1,.7f),Vector2.zero,Vector2.zero);var fill=new GameObject("Fill",typeof(RectTransform),typeof(Image));fill.transform.SetParent(go.transform,false);fill.GetComponent<Image>().color=new Color(.25f,.64f,.68f,1);SetRect(fill.GetComponent<RectTransform>(),new Vector2(0,.25f),new Vector2(1,.75f),Vector2.zero,Vector2.zero);slider.fillRect=fill.GetComponent<RectTransform>();var handle=new GameObject("Handle",typeof(RectTransform),typeof(Image));handle.transform.SetParent(go.transform,false);handle.GetComponent<Image>().color=Color.white;var hr=handle.GetComponent<RectTransform>();hr.sizeDelta=new Vector2(24,24);slider.handleRect=hr;slider.targetGraphic=handle.GetComponent<Image>();}
        private void SetPreferred(GameObject go,float h){var le=go.GetComponent<LayoutElement>()??go.AddComponent<LayoutElement>();le.preferredHeight=h;le.minHeight=h;}
        private void SetRect(RectTransform r,Vector2 amin,Vector2 amax,Vector2 omin,Vector2 omax){r.anchorMin=amin;r.anchorMax=amax;r.offsetMin=omin;r.offsetMax=omax;}
    }
}
