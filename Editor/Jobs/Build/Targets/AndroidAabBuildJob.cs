using UnityEditor;
using UnityEngine;

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
            var buildOptions = base.ConstructBuildOptions();

            EditorUserBuildSettings.buildAppBundle = true;
            PlayerSettings.Android.splitApplicationBinary = _splitApplicationBinary;

            return buildOptions;
        }
    }
}