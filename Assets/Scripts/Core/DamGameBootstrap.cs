using UnityEngine;

namespace Riverforge
{
    public sealed class DamGameBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            Application.targetFrameRate=60;
            Screen.sleepTimeout=SleepTimeout.NeverSleep;
            Screen.orientation=ScreenOrientation.LandscapeLeft;

            var systems=new GameObject("Game Systems");
            var sim=systems.AddComponent<DamSimulation>();

            var world=new GameObject("Procedural World").AddComponent<ProceduralDamWorld>();

            var camGo=new GameObject("Main Camera");
            camGo.tag="MainCamera";
            var cam=camGo.AddComponent<Camera>();
            cam.fieldOfView=48f;cam.nearClipPlane=.15f;cam.farClipPlane=220f;
            cam.clearFlags=CameraClearFlags.Skybox;
            var orbit=camGo.AddComponent<MobileOrbitCamera>();

            var hud=new GameObject("HUD Controller").AddComponent<DamHUD>();

            sim.Initialize(true);
            world.Initialize(sim);
            orbit.Initialize();
            hud.Initialize(sim);
        }
    }
}
