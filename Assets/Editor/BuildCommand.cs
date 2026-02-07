using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

/// <summary>
/// Build script for GitHub Actions CI/CD pipeline.
/// This script is called by the Unity Builder action to create WebGL builds.
/// 
/// Location: Assets/Editor/BuildCommand.cs
/// </summary>
public class BuildCommand
{
    /// <summary>
    /// Performs a WebGL build of the Unity project.
    /// Called by: unity-builder action with -executeMethod BuildCommand.PerformBuild
    /// </summary>
    public static void PerformBuild()
    {
        // Get all enabled scenes from Build Settings
        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        // Configure build options
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "build/WebGL",
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        // Execute the build
        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        BuildSummary summary = report.summary;

        // Check build result
        if (summary.result == BuildResult.Succeeded)
        {
            UnityEngine.Debug.Log($"Build succeeded: {summary.totalSize} bytes");
            UnityEngine.Debug.Log($"Build time: {summary.totalTime}");
        }
        else if (summary.result == BuildResult.Failed)
        {
            UnityEngine.Debug.LogError("Build failed");
            EditorApplication.Exit(1);  // Exit with error code
        }
    }
}