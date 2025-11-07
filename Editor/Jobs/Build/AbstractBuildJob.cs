using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CiWizard.Editor.Jobs.Build.Processors;
using CiWizard.Editor.Jobs.Common;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace CiWizard.Editor.Jobs.Build
{
    public abstract class AbstractBuildJob : UnityJobWithBuildTarget
    {
        private static readonly string[] IgnoreBuildObjects = { "_DoNotShip", "_ButDontShipIt" };
        [Header("Build")]
        [SerializeField]
        private string _executableName;
        [SerializeField]
        private BuildOptions _buildOptions;
        [SerializeField]
        private string _tempBuildPath = "BuildCache";
        [SerializeField]
        private string _outputBuildPath = "Build";
        [SerializeField]
        private BuildScriptablePreprocessor[] _buildPreprocessors;
        [SerializeField]
        private BuildScriptablePostprocessor[] _buildPostprocessors;

        public abstract string FileExtension { get; }
        public string ExecutableName => _executableName;
        public string OutputBuildPath => _outputBuildPath;

        protected AbstractBuildJob() : base(new JobArtifacts(ArtifactCondition.OnSuccess, new[] { "assetSizeReport.txt", "Build/*" }))
        {
        }

        public override void Execute()
        {
            var buildPlayerOptions = ConstructBuildOptions();

            BuildLogHandler.WriteSectionBegin("run_Build_Preprocessors", "[Ci Job] Run Build Preprocessors");
            RunBuildPreprocessors(buildPlayerOptions);
            BuildLogHandler.WriteSectionEnd("run_Build_Preprocessors");

            BuildLogHandler.WriteSectionBegin("reimport_Zenject_Assets", "[Ci Job] Reimport Zenject Assets");
            ReimportZenjectAssets();
            BuildLogHandler.WriteSectionEnd("reimport_Zenject_Assets");

            BuildLogHandler.WriteSectionBegin("start_Build_Player", "[Ci Job] Start Build Player");
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            WriteAssetReport(report);
            BuildLogHandler.WriteSectionEnd("start_Build_Player");

            BuildLogHandler.WriteSectionBegin("move_Build_To_Output_Folder", "[Ci Job] Move Build To Output Folder");
            MoveBuildToOutputFolder();
            BuildLogHandler.WriteSectionEnd("move_Build_To_Output_Folder");

            BuildLogHandler.WriteSectionBegin("run_Build_Postprocessors", "[Ci Job] Run Build Postprocessors");
            RunBuildPostprocessors(buildPlayerOptions, report);
            BuildLogHandler.WriteSectionEnd("run_Build_Postprocessors");

            PlayerSettings.Android.splitApplicationBinary = false;
        }

        public static void ReimportZenjectAssets()
        {
            try
            {
                AssetDatabase.StartAssetEditing();

                string[] assetGUIDs = AssetDatabase.FindAssets("SceneContext");
                foreach (string guid in assetGUIDs)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Ci Job] Error in packing importer: {e.Message}");
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }
        }

        protected virtual BuildPlayerOptions ConstructBuildOptions()
        {
            PlayerSettings.SplashScreen.showUnityLogo = false;
            var buildPlayerOptions = new BuildPlayerOptions
            {
                target = JobBuildTarget,
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled)
                    .Where(s => !string.IsNullOrEmpty(s.path))
                    .Select(s => s.path).ToArray(),
                locationPathName = Path.Combine(_tempBuildPath,
                    string.IsNullOrEmpty(FileExtension) ? _executableName : $"{_executableName}.{FileExtension}"),
                options = _buildOptions
            };
            return buildPlayerOptions;
        }

        private void RunBuildPreprocessors(BuildPlayerOptions buildPlayerOptions)
        {
            if (_buildPreprocessors == null) return;
            foreach (var buildPreprocessor in _buildPreprocessors)
            {
                buildPreprocessor.PreprocessBuild(buildPlayerOptions);
            }
        }

        private void WriteAssetReport(BuildReport report)
        {
            if (report.packedAssets.Length <= 0) return;
            var packedAssets = new List<PackedAssetInfo>();
            var totalSize = 0ul;
            var longestTypeLength = 0;
            var longestPathLength = 0;
            foreach (var asset in report.packedAssets)
            {
                foreach (var packedAsset in asset.contents)
                {
                    packedAssets.Add(packedAsset);
                    totalSize += packedAsset.packedSize;
                    longestPathLength = Math.Max(longestPathLength, packedAsset.sourceAssetPath.Length);
                    longestTypeLength = Math.Max(longestTypeLength, packedAsset.type.ToString().Split('.').Last().Length);
                }
            }
            var file = new StreamWriter("assetSizeReport.txt", false);
            foreach (var asset in packedAssets.OrderByDescending(pa => pa.packedSize))
            {
                file.WriteLine(
                    $"{asset.type.ToString().Split('.').Last().PadRight(longestTypeLength)} | {asset.sourceAssetPath.PadRight(longestPathLength)} | {asset.packedSize.ToString(),20} | {(100.0f * asset.packedSize / totalSize):0.000}%");
            }
            file.Close();
        }

        private void MoveBuildToOutputFolder()
        {
            var buildFolder = _tempBuildPath;
            var outputFolder = _outputBuildPath;
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }
            var objectsInBuildFolder = Directory.GetDirectories(buildFolder).Concat(Directory.GetFiles(buildFolder));
            foreach (var objectPath in objectsInBuildFolder)
            {
                var ignore = IgnoreBuildObjects.Any(ignoreBuildObject => objectPath.Contains(ignoreBuildObject));
                if (ignore) continue;
                var attributes = File.GetAttributes(objectPath);
                if (attributes.HasFlag(FileAttributes.Directory))
                {
                    var directoryInfo = new DirectoryInfo(objectPath);
                    directoryInfo.MoveTo(Path.Combine(outputFolder, directoryInfo.Name));
                    continue;
                }
                var fileInfo = new FileInfo(objectPath);
                fileInfo.MoveTo(Path.Combine(outputFolder, fileInfo.Name));
            }
        }

        private void RunBuildPostprocessors(BuildPlayerOptions buildPlayerOptions, BuildReport buildReport)
        {
            if (_buildPostprocessors == null) return;
            foreach (var buildPostprocessor in _buildPostprocessors)
            {
                buildPostprocessor.PostprocessBuild(buildPlayerOptions, _outputBuildPath, buildReport);
            }
        }
    }
}