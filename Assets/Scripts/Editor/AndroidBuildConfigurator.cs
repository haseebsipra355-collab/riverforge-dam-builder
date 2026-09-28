#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Riverforge.Editor
{
    public static class AndroidBuildConfigurator
    {
        [MenuItem("Riverforge/Configure Android")]
        public static void ConfigureAndroid()
        {
            PlayerSettings.companyName = "Riverforge Studio";
            PlayerSettings.productName = "Riverforge Dam Builder";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.riverforge.dambuilder");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            EditorUserBuildSettings.buildAppBundle = true;
            Debug.Log("Riverforge Android settings configured: landscape, ARM64, IL2CPP, AAB.");
        }

        [MenuItem("Riverforge/Build Android AAB")]
        public static void BuildAab()
        {
            ConfigureAndroid();
            System.IO.Directory.CreateDirectory("Builds/Android");
            var options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/Main.unity" },
                locationPathName = "Builds/Android/Riverforge.aab",
                target = BuildTarget.Android,
                options = BuildOptions.None
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            Debug.Log("Android build result: " + report.summary.result);
        }
    }
}
#endif
