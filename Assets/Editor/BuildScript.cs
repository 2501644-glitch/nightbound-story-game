using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

public static class BuildScript
{
    [MenuItem("Nightbound/Build Windows Standalone")]
    public static void BuildWindows() => BuildPlayer(BuildTarget.StandaloneWindows64, "Nightbound.exe");

    [MenuItem("Nightbound/Build macOS Standalone")]
    public static void BuildMacOS() => BuildPlayer(BuildTarget.StandaloneOSX, "Nightbound.app");

    private static void BuildPlayer(BuildTarget target, string outputName)
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string buildDirectory = Path.Combine(projectRoot, "Builds");
        string buildPath = Path.Combine(buildDirectory, outputName);
        string[] scenes = { "Assets/Scenes/SampleScene.unity" };

        Directory.CreateDirectory(buildDirectory);
        Debug.Log($"Starting NIGHTBOUND {target} build...");

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = target,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"NIGHTBOUND {target} build succeeded! Total size: {summary.totalSize} bytes. Location: {buildPath}");
        }
        else
        {
            Debug.LogError($"NIGHTBOUND {target} build failed with {summary.totalErrors} errors!");
            EditorApplication.Exit(1);
        }
    }
}
