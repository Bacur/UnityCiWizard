using System;
using System.Collections.Generic;
using System.IO;
using CiWizard.Editor.Jobs;
using UnityEditor;
using UnityEngine;

namespace CiWizard.Editor
{
    [CreateAssetMenu(fileName = "CIConfig", menuName = "CI/CI Config", order = 0)]
    public class CiConfig : ScriptableObject
    {
        [SerializeField]
        private List<AbstractJob> _jobs;
        public IReadOnlyList<AbstractJob> Jobs => _jobs;

        public static void Execute()
        {
            BuildLogHandler.WriteSectionEnd("import_section");
            BuildLogHandler.WriteSectionBegin("build_section", "[CiWizard] Executing CI function");
            Debug.unityLogger.logHandler = new BuildLogHandler();

            BuildLogHandler.WriteSectionBegin("find_ci_config_section", "[CiWizard] Get Ci Config");
            var configGuid = Environment.GetEnvironmentVariable("UCI_CFG_JOB_UNITY_CONFIG_GUID");
            var jobIndex = int.Parse(Environment.GetEnvironmentVariable("UCI_CFG_JOB_UNITY_INDEX") ?? "-1");
            string configAssetPath = AssetDatabase.GUIDToAssetPath(configGuid);

            if (string.IsNullOrEmpty(configAssetPath))
            {
                Debug.LogWarning("[CiWizard] Lost ci config, try find ci config ...");

                var ciConfigs = AssetDatabase.FindAssets("t:CiConfig");
                if (ciConfigs.Length == 0)
                {
                    Debug.LogError("[CiWizard] Error : Not find ci config");
                    return;
                }
                else
                {
                    Debug.Log("[CiWizard] ci config finded");
                }

                configAssetPath = AssetDatabase.GUIDToAssetPath(ciConfigs[0]);
            }

            var configAsset = AssetDatabase.LoadAssetAtPath<CiConfig>(configAssetPath);
            BuildLogHandler.WriteSectionEnd("find_ci_config_section");

            BuildLogHandler.WriteSectionBegin("start_job_section", "[CiWizard] Start Job");
            var job = configAsset.Jobs[jobIndex] as UnityJob;
            job?.Execute();
            BuildLogHandler.WriteSectionEnd("start_job_section");

            BuildLogHandler.WriteSectionEnd("build_section");
            BuildLogHandler.WriteSectionBegin("exit_section", "[CiWizard] Closing Unity");
        }

        public string GetConfigGuid()
        {
            return AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(this)).ToString();
        }

        public string GetProjectRelativePath()
        {
            var directory = new DirectoryInfo(Application.dataPath);
            return Path.GetRelativePath(GetGitProjectRoot(), directory.Parent.FullName);
        }

        public static string GetGitProjectRoot()
        {
            var directory = new DirectoryInfo(Application.dataPath);
            while (directory.Parent != null && !Directory.Exists(Path.Combine(directory.FullName, ".git")))
            {
                directory = directory.Parent;
            }

            return directory.FullName;
        }

        internal void AddJob(AbstractJob job)
        {
            _jobs ??= new List<AbstractJob>();
            _jobs.Add(job);
        }
    }
}