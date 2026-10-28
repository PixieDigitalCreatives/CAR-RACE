using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace ThegameioRacing.EditorTools
{
    public static class GameBuildPipeline
    {
        private const string SCENE_PATH = "Assets/_Project/Scenes/VehiclePhysicsTest.unity";

        [MenuItem("Tools/GAMEIUM/Configure Android Project Settings", false, 10)]
        public static void ConfigureAndroidSettings()
        {
            Debug.Log("[GAMEIUM] Configuring Project Settings for Android Mobile...");

            PlayerSettings.companyName = "GAMEIUM";
            PlayerSettings.productName = "GAMEIUM";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.gameium.racing");

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

            // Configure App Icon
            Texture2D appIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/Textures/UI/tex_gameium_app_icon.png");
            if (appIcon != null)
            {
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new Texture2D[] { appIcon });
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, new Texture2D[] { appIcon });
            }

            Debug.Log("[GAMEIUM] Android Player Settings successfully configured.");
        }

        [MenuItem("Tools/GAMEIUM/Build Android APK", false, 11)]
        public static void BuildAndroid()
        {
            ConfigureAndroidSettings();

            string outputDir = "Builds/Android";
            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);
            string outputPath = $"{outputDir}/GAMEIUM.apk";

            string[] scenes = new string[]
            {
                "Assets/_Project/Scenes/MainMenu.unity",
                "Assets/_Project/Scenes/DowntownCircuit.unity",
                "Assets/_Project/Scenes/SampleStreet_GateA.unity",
                "Assets/_Project/Scenes/VehiclePhysicsTest.unity"
            };

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            Debug.Log($"[GAMEIUM] Initiating Android build to: {outputPath}...");

            try
            {
                BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
                BuildSummary summary = report.summary;

                if (summary.result == BuildResult.Succeeded)
                {
                    Debug.Log($"[GAMEIUM] Android build SUCCEEDED: {summary.totalSize} bytes at {outputPath}");
                }
                else if (summary.result == BuildResult.Failed)
                {
                    Debug.LogError($"[GAMEIUM] Android build FAILED with {summary.totalErrors} errors. Check Editor console.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GAMEIUM] Build execution exception: {ex.Message}");
            }
        }

        [MenuItem("Tools/GAMEIUM/Build Windows Standalone (Fast Verification)", false, 12)]
        public static void BuildWindowsStandalone()
        {
            string outputDir = "Builds/Windows";
            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);
            string outputPath = $"{outputDir}/GAMEIUM.exe";

            string[] scenes = new string[] { SCENE_PATH };

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log($"[GAMEIUM] Initiating Windows Standalone verification build to: {outputPath}...");
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[GAMEIUM] Windows build SUCCEEDED: {summary.totalSize} bytes at {outputPath}");
            }
            else
            {
                Debug.LogError($"[GAMEIUM] Windows build FAILED with {summary.totalErrors} errors.");
            }
        }
    }

        [MenuItem("Tools/GAMEIUM/Configure WebGL Project Settings", false, 13)]
        public static void ConfigureWebGLSettings()
        {
            Debug.Log("[GAMEIUM] Configuring Project Settings for Web Browser (WebGL)...");

            PlayerSettings.companyName = "GAMEIUM";
            PlayerSettings.productName = "GAMEIUM";

            // WebGL Memory & Performance Tuning
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = false;
            PlayerSettings.WebGL.memorySize = 256;
            PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;

            // Color space: Linear for authentic URP nighttime street illumination
            PlayerSettings.colorSpace = ColorSpace.Linear;

            Debug.Log("[GAMEIUM] WebGL Project Settings successfully configured.");
        }

        [MenuItem("Tools/GAMEIUM/Build WebGL (Browser Game)", false, 14)]
        public static void BuildWebGL()
        {
            ConfigureWebGLSettings();

            string outputDir = "Builds/WebGL";
            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

            string[] scenes = new string[]
            {
                "Assets/_Project/Scenes/MainMenu.unity",
                "Assets/_Project/Scenes/DowntownCircuit.unity",
                "Assets/_Project/Scenes/SampleStreet_GateA.unity",
                "Assets/_Project/Scenes/VehiclePhysicsTest.unity"
            };

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputDir,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            Debug.Log($"[GAMEIUM] Initiating WebGL browser build to: {outputDir}...");

            try
            {
                BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
                BuildSummary summary = report.summary;

                if (summary.result == BuildResult.Succeeded)
                {
                    Debug.Log($"[GAMEIUM] WebGL build SUCCEEDED: {summary.totalSize} bytes at {outputDir}");
                }
                else if (summary.result == BuildResult.Failed)
                {
                    Debug.LogError($"[GAMEIUM] WebGL build FAILED with {summary.totalErrors} errors.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GAMEIUM] WebGL build execution exception: {ex.Message}");
            }
        }

}
