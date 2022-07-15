using System;
using System.IO;
using System.Linq;
using CustomUnityBuilderAction.Input;
using CustomUnityBuilderAction.Reporting;
using CustomUnityBuilderAction.Versioning;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace CustomUnityBuilderAction
{
	public static class Builder
	{
		public static void BuildProject()
		{
			// Gather values from args
			var options = ArgumentsParser.GetValidatedOptions();

			// Gather values from project
			var scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(s => s.path).ToArray();

			// Get all buildOptions from options
			BuildOptions buildOptions = BuildOptions.None;
			foreach (string buildOptionString in Enum.GetNames(typeof(BuildOptions)))
			{
				if (options.ContainsKey(buildOptionString))
				{
					BuildOptions buildOptionEnum = (BuildOptions)Enum.Parse(typeof(BuildOptions), buildOptionString);
					buildOptions |= buildOptionEnum;
				}
			}

			{
				var commit = Environment.GetEnvironmentVariable("GITHUB_SHA");
				var stream = File.CreateText(Application.streamingAssetsPath + "/commit.txt");
				stream.WriteLine(commit.Substring(0, 7));
				stream.Close();
			}

			// Define BuildPlayer Options
			var buildPlayerOptions = new BuildPlayerOptions
			{
				scenes = scenes,
				locationPathName = options["customBuildPath"],
				target = (BuildTarget)Enum.Parse(typeof(BuildTarget), options["buildTarget"]),
				options = buildOptions
			};

			// Set version for this build
			VersionApplicator.SetVersion(options["buildVersion"]);

			// Apply Android settings
			if (buildPlayerOptions.target == BuildTarget.Android)
			{
				VersionApplicator.SetAndroidVersionCode(options["androidVersionCode"]);
				AndroidSettings.Apply(options);
			}

			// Addressables
			{
				AddressableAssetSettings.CleanPlayerContent(
					AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder
				);
				AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult buildResult);
				bool success = string.IsNullOrEmpty(buildResult.Error);

				if (!success)
				{
					UnityEngine.Debug.LogError("Addressables build error encountered: " + buildResult.Error);
				}
			}

			// Perform build
			BuildReport buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);

			// Summary
			BuildSummary summary = buildReport.summary;
			StdOutReporter.ReportSummary(summary);

			// Result
			BuildResult result = summary.result;
			StdOutReporter.ExitWithResult(result);
		}
	}
}
