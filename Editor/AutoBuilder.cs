using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public static class AutoBuilder
{

    static string GetProjectName ()
    {
        string[] s = Application.dataPath.Split ('/');
        return s [s.Length - 2];
    }

    static string[] GetScenePaths ()
    {
        string[] scenes = new string[EditorBuildSettings.scenes.Length];

        for (int i = 0; i < scenes.Length; i++) {
            scenes [i] = EditorBuildSettings.scenes [i].path;
        }

        return scenes;
    }

    [MenuItem ("File/AutoBuilder/iOS")]
    static void PerformiOSBuild ()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget (BuildTarget.iPhone);
        BuildPipeline.BuildPlayer (GetScenePaths (), "Builds/iOS", BuildTarget.iPhone, BuildOptions.None);
    }

    [MenuItem ("File/AutoBuilder/Android")]
    static void PerformAndroidBuild ()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget (BuildTarget.Android);
        BuildPipeline.BuildPlayer (GetScenePaths (), "Builds/Android", BuildTarget.Android, BuildOptions.None);
    }
}
