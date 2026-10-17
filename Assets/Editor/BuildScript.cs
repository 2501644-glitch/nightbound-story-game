using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildScript
{
    [MenuItem("Nightbound/Build Windows Standalone")]
    public static void BuildWindows()
    {
        string buildPath = "Builds/Nightbound.exe";
        string[] scenes = { "Assets/Scenes/SampleScene.unity" };

        Debug.Log("Starting NIGHTBOUND CLI Build...");

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"NIGHTBOUND build succeeded! Total size: {summary.totalSize} bytes. Location: {buildPath}");
        }
        else
        {
            Debug.LogError($"NIGHTBOUND build failed with {summary.totalErrors} errors!");
            EditorApplication.Exit(1);
        }
    }
}
