using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace ThegameioRacing.EditorTools
{
    public static class GameBuildPipeline
    {
        private const string SCENE_PATH = "Assets/_Project/Scenes/VehiclePhysicsTest.unity";

        [MenuItem("Tools/THEGAMEIO/Configure Android Project Settings", false, 10)]
        public static void ConfigureAndroidSettings()
        {
            Debug.Log("[THEGAMEIO] Configuring Project Settings for Android Mobile...");

            PlayerSettings.companyName = "THEGAMEIO";
            PlayerSettings.productName = "THEGAMEIO RACING";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.thegameio.racing");

            // Mobile Orientation: Forced Landscape
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

            // Target Android Config
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24; // Android 7.0+
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

            // Rendering & Performance
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);

            Debug.Log("[THEGAMEIO] Android Player Settings successfully configured.");
        }

        [MenuItem("Tools/THEGAMEIO/Build Android APK", false, 11)]
        public static void BuildAndroid()
        {
            ConfigureAndroidSettings();

            string outputDir = "Builds/Android";
            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);
            string outputPath = $"{outputDir}/THEGAMEIO_Racing.apk";

            string[] scenes = new string[] { SCENE_PATH };

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            Debug.Log($"[THEGAMEIO] Initiating Android build to: {outputPath}...");

            try
            {
                BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
                BuildSummary summary = report.summary;

                if (summary.result == BuildResult.Succeeded)
                {
                    Debug.Log($"[THEGAMEIO] Android build SUCCEEDED: {summary.totalSize} bytes at {outputPath}");
                }
                else if (summary.result == BuildResult.Failed)
                {
                    Debug.LogError($"[THEGAMEIO] Android build FAILED with {summary.totalErrors} errors. Check Editor console.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[THEGAMEIO] Build execution exception: {ex.Message}");
            }
        }

        [MenuItem("Tools/THEGAMEIO/Build Windows Standalone (Fast Verification)", false, 12)]
        public static void BuildWindowsStandalone()
        {
            string outputDir = "Builds/Windows";
            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);
            string outputPath = $"{outputDir}/THEGAMEIO_Racing.exe";

            string[] scenes = new string[] { SCENE_PATH };

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log($"[THEGAMEIO] Initiating Windows Standalone verification build to: {outputPath}...");
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[THEGAMEIO] Windows build SUCCEEDED: {summary.totalSize} bytes at {outputPath}");
            }
            else
            {
                Debug.LogError($"[THEGAMEIO] Windows build FAILED with {summary.totalErrors} errors.");
            }
        }
    }
}
