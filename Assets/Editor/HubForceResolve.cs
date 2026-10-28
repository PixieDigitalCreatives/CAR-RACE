using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

[InitializeOnLoad]
public static class HubForceResolve
{
    private const string k_StateKey = "THEGAMEIO.Setup.Initialized";

    static HubForceResolve()
    {
        EditorApplication.delayCall += OnEditorReady;
    }

    private static void OnEditorReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += OnEditorReady;
            return;
        }

        if (SessionState.GetBool(k_StateKey, false)) return;
        SessionState.SetBool(k_StateKey, true);

        Debug.Log("[THEGAMEIO] Auto-initializing project assets and test scene...");
        AssetDatabase.Refresh();

        try
        {
            ThegameioRacing.EditorTools.TestTrackSceneBuilder.BuildTestScene();
            ThegameioRacing.EditorTools.GameBuildPipeline.ConfigureAndroidSettings();
            Debug.Log("[THEGAMEIO] Test Scene & Android Configuration Completed Successfully!");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("[THEGAMEIO] Auto-setup delayed or waiting for compilation: " + ex.Message);
        }
    }
}