using System.Collections.Generic;
using UnityEngine;

namespace Riverforge
{
    public sealed class ProceduralDamWorld : MonoBehaviour
    {
        private DamSimulation _sim;
        private Transform _reservoir;
        private Transform _river;
        private Transform _damRoot;
        private Transform _constructionCrane;
        private readonly List<Transform> _clouds = new List<Transform>();
        private Material _waterMat;
        private float _lastBuild = -1f;
        private bool _lastBuilt;

        public void Initialize(DamSimulation sim)
        {
            _sim = sim;
            CreateLighting();
            CreateTerrain();
            CreateWater();
            CreateDam();
            CreateConstructionProps();
            CreateClouds();
            _sim.WeatherChanged += OnWeatherChanged;
            OnWeatherChanged(_sim.State.weather);
        }

        private Material Mat(Color color, float smoothness = .2f)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            var m = new Material(shader) { color = color };
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
            return m;
        }

        private GameObject Cube(string name, Vector3 pos, Vector3 scale, Color color, Transform parent = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name; go.transform.SetParent(parent); go.transform.position = pos; go.transform.localScale = scale;
            go.GetComponent<Renderer>().material = Mat(color);
            return go;
        }

        private void CreateLighting()
        {
            RenderSettings.ambientLight = new Color(.58f,.62f,.68f);
            var sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional; sun.intensity = 1.15f; sun.color = new Color(1f,.92f,.78f);
            sun.transform.rotation = Quaternion.Euler(45f,-35f,0f);
        }

        private void CreateTerrain()
        {
            var ground = Cube("Valley floor", new Vector3(0,-1.8f,0), new Vector3(90,3,48), new Color(.29f,.38f,.24f));
            ground.GetComponent<Collider>().enabled = false;
            for (int side=-1; side<=1; side+=2)
            {
                for (int i=0;i<8;i++)
                {
                    float z=-21+i*6f;
                    float x=side*(18f + Mathf.Sin(i*1.3f)*2.5f);
                    var bank=Cube("Rock bank",new Vector3(x,2.2f,z),new Vector3(18f,8f,7f),new Color(.31f,.33f,.26f));
                    bank.transform.rotation=Quaternion.Euler(0,side*(8+i*2),side*8);
                }
            }
            for(int i=0;i<34;i++)
            {
                float side=i%2==0?-1:1;
                float x=side*(8+((i*17)%9)); float z=-22+((i*13)%44);
                var trunk=Cube("Tree trunk",new Vector3(x,.7f,z),new Vector3(.25f,1.7f,.25f),new Color(.27f,.17f,.09f));
                var crown=GameObject.CreatePrimitive(PrimitiveType.Sphere); crown.name="Tree crown"; crown.transform.position=new Vector3(x,2f,z); crown.transform.localScale=new Vector3(1.4f,2f,1.4f); crown.GetComponent<Renderer>().material=Mat(new Color(.18f,.37f,.15f));
            }
            Cube("Powerhouse",new Vector3(5.7f,.25f,3f),new Vector3(5f,2.6f,6f),new Color(.72f,.69f,.61f));
            Cube("Substation",new Vector3(12f,-.15f,5f),new Vector3(4f,1.7f,5f),new Color(.38f,.42f,.43f));
        }

        private void CreateWater()
        {
            var r=Cube("Reservoir",new Vector3(-19f,-.1f,0),new Vector3(38f,.22f,28f),new Color(.11f,.45f,.62f));
            _reservoir=r.transform; _waterMat=r.GetComponent<Renderer>().material; _waterMat.SetFloat("_Metallic",.05f); if(_waterMat.HasProperty("_Smoothness"))_waterMat.SetFloat("_Smoothness",.82f);
            var river=Cube("Downstream river",new Vector3(18f,-.65f,0),new Vector3(34f,.14f,8f),new Color(.12f,.48f,.66f)); _river=river.transform;
        }

        private void CreateDam()
        {
            _damRoot=new GameObject("Dam").transform;
            _damRoot.position=Vector3.zero;
            Cube("Foundation",new Vector3(0,-.35f,0),new Vector3(4f,2f,18f),new Color(.48f,.49f,.47f),_damRoot);
            for(int i=0;i<5;i++) Cube("Dam lift "+i,new Vector3(0,1f+i*1.35f,0),new Vector3(3.5f-i*.33f,1.32f,18f-i*.8f),new Color(.58f,.59f,.56f),_damRoot);
            for(int z=-1;z<=1;z++) Cube("Spill gate",new Vector3(1.9f,4.5f,z*4f),new Vector3(.35f,2.5f,2.2f),new Color(.22f,.25f,.27f),_damRoot);
            _damRoot.gameObject.SetActive(false);
        }

        private void CreateConstructionProps()
        {
            _constructionCrane=new GameObject("Construction crane").transform;
            Cube("Crane mast",new Vector3(-2f,4f,-9f),new Vector3(.35f,8f,.35f),new Color(.88f,.62f,.12f),_constructionCrane);
            Cube("Crane boom",new Vector3(1.5f,7.7f,-9f),new Vector3(7f,.3f,.3f),new Color(.88f,.62f,.12f),_constructionCrane);
            for(int i=0;i<3;i++) Cube("Truck "+i,new Vector3(-8+i*3f,-.05f,11f+i),new Vector3(2.2f,.9f,1.2f),new Color(.86f,.48f,.1f));
            _constructionCrane.gameObject.SetActive(false);
        }

        private void CreateClouds()
        {
            for(int i=0;i<5;i++)
            {
                var c=GameObject.CreatePrimitive(PrimitiveType.Sphere); c.name="Cloud"; c.transform.position=new Vector3(-24+i*12,13f+(i%2),-12+i*5); c.transform.localScale=new Vector3(6f,2.4f,3f); c.GetComponent<Collider>().enabled=false; c.GetComponent<Renderer>().material=Mat(new Color(.86f,.88f,.9f)); _clouds.Add(c.transform);
            }
        }

        private void OnWeatherChanged(string weather)
        {
            Color c=weather=="Storm"?new Color(.31f,.34f,.39f):weather=="Rain"?new Color(.56f,.59f,.63f):new Color(.86f,.88f,.9f);
            foreach(var cloud in _clouds) cloud.GetComponent<Renderer>().material.color=c;
        }

        private void Update()
        {
            if(_sim?.State==null)return;
            var s=_sim.State;
            float safeCap=1f+s.wallLevel*.06f;
            _reservoir.localScale=new Vector3(38f+Mathf.Clamp01(s.waterLevel)*8f,.22f,28f);
            _reservoir.position=new Vector3(-19f,-.75f+s.waterLevel*3.8f,0);
            _river.localScale=new Vector3(34f,.12f,Mathf.Lerp(6f,11f,Mathf.Clamp01(_sim.CurrentOutflow/9f)));
            if(_waterMat!=null){float pulse=.04f*Mathf.Sin(Time.time*1.7f);_waterMat.color=new Color(.11f+pulse*.2f,.45f+pulse,.62f+pulse);}

            bool constructing=s.buildProgress>0f&&!s.damBuilt;
            _constructionCrane.gameObject.SetActive(constructing);
            _damRoot.gameObject.SetActive(constructing||s.damBuilt);
            float visual=s.damBuilt?1f:Mathf.Clamp01(s.buildProgress);
            if(Mathf.Abs(visual-_lastBuild)>.002f||_lastBuilt!=s.damBuilt)
            {
                _lastBuild=visual;_lastBuilt=s.damBuilt;
                for(int i=0;i<_damRoot.childCount;i++)_damRoot.GetChild(i).gameObject.SetActive(visual>i/8f||s.damBuilt);
            }
            if(constructing)_constructionCrane.rotation=Quaternion.Euler(0,Mathf.Sin(Time.time*.45f)*28f,0);
            foreach(var cloud in _clouds){cloud.position+=Vector3.right*Time.deltaTime*.7f;if(cloud.position.x>34f)cloud.position=new Vector3(-34f,cloud.position.y,cloud.position.z);}
        }
    }
}
