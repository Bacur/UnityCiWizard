using UnityEditor;
using UnityEngine;

#if UNITY_ANDROID
using UnityEditor.Android;
using Unity.Android.Types;
#endif

namespace CiWizard.Editor.Jobs.Build.Targets
{
    [CreateAssetMenu(fileName = "AndroidAAB", menuName = "CI/Jobs/Build/Android AAB")]
    public class AndroidAabBuildJob : AndroidApkBuildJob
    {
        public override string FileExtension => "aab";

        [SerializeField]
        private bool _splitApplicationBinary = true;

        protected override BuildPlayerOptions ConstructBuildOptions()
        {
#if UNITY_ANDROID
#if UNITY_6000_0_OR_NEWER
            UserBuildSettings.DebugSymbols.format = DebugSymbolFormat.Zip;
            UserBuildSettings.DebugSymbols.level = DebugSymbolLevel.SymbolTable;
#else
            // Old version Unity (2021 - 2023)
            EditorUserBuildSettings.androidCreateSymbols = UnityEditor.AndroidCreateSymbols.Public;
#endif
#endif
            var buildOptions = base.ConstructBuildOptions();

            EditorUserBuildSettings.buildAppBundle = true;
            SplitApplicationBinary(_splitApplicationBinary);

            return buildOptions;
        }
    }
}