using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Diagnostics;
using System;
using UnityEngine;

public class BuildScripts : MonoBehaviour{

#if UNITY_EDITOR
    static float timeToWait = 1.0f;
    static float currentTime = Time.realtimeSinceStartup;


    [MenuItem("TSDU/2 Players test (Build + Editor, Build is master)")]
    public static void BuildGame()
    {


        // Get filename.
        
        string path = Application.dataPath + "/../Builds/QuickBuild";
        UnityEngine.Debug.LogFormat("Starting build in foler: {0}", path);

        // Build player.
        BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, path + "/MultiTest.exe", BuildTarget.StandaloneWindows, BuildOptions.Development);

        EditorApplication.update += OnEditorUpdate;
        float currentTime = Time.realtimeSinceStartup;

        UnityEngine.Debug.Log("BuildDone");
        // Run the game (Process class from System.Diagnostics).
        Process proc = new Process();
        proc.StartInfo.FileName = path + "/MultiTest.exe";
        proc.Start();
    }

    private static void OnEditorUpdate()
    {
        if (Time.realtimeSinceStartup > currentTime + timeToWait)
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.isPlaying = true;
        }
    }
#endif
}
