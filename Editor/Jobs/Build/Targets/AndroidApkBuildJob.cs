using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CiWizard.Editor.Jobs.Build.Targets
{
    [CreateAssetMenu(fileName = "AndroidAPK", menuName = "CI/Jobs/Build/Android APK")]
    public class AndroidApkBuildJob : AbstractBuildJob
    {
        protected override BuildTarget JobBuildTarget => BuildTarget.Android;
        public override string FileExtension => "apk";

        [SerializeField]
        private string _keyStore;
        [SerializeField]
        private string _keyAlias;
        [SerializeField]
        private string _keyStorePass;
        [SerializeField]
        private string _keyAliasPass;

        protected override BuildPlayerOptions ConstructBuildOptions()
        {
            var buildPlayerOptions = base.ConstructBuildOptions();

            EditorUserBuildSettings.buildAppBundle = false;
            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.splitApplicationBinary = false;

            if (_keyStore == "")
            {
                var data = Convert.FromBase64String(Environment.GetEnvironmentVariable("UCI_ENV_ANDROID_KEYSTORE"));
                File.WriteAllBytes("ci.keystore", data);
                PlayerSettings.Android.keystoreName = "ci.keystore";
                PlayerSettings.Android.keyaliasName = Environment.GetEnvironmentVariable("UCI_ENV_ANDROID_KEYSTORE_ALIAS");
                PlayerSettings.Android.keystorePass = Environment.GetEnvironmentVariable("UCI_ENV_ANDROID_KEYSTORE_PASS");
                PlayerSettings.Android.keyaliasPass = Environment.GetEnvironmentVariable("UCI_ENV_ANDROID_KEYSTORE_ALIAS_PASS");
            }
            else
            {
                PlayerSettings.Android.keystoreName = _keyStore;
                PlayerSettings.Android.keyaliasName = _keyAlias;
                PlayerSettings.Android.keystorePass = _keyStorePass;
                PlayerSettings.Android.keyaliasPass = _keyAliasPass;
            }
            return buildPlayerOptions;
        }
    }
}