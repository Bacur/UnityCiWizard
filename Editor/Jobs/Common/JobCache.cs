using UnityEngine;

namespace CiWizard.Editor.Jobs.Common {
    [CreateAssetMenu(fileName = "Cache", menuName = "CI/Cache")]
    public class JobCache : ScriptableObject {
        [SerializeField] 
        private string[] _paths = { "Library", "BuildCache" };
        [SerializeField] 
        private string[] _exclude = { "Library/Bee/Android/Prj/IL2CPP/Gradle" };
        public string[] Paths => _paths;
        public string[] Exclude => _exclude;
    }
}