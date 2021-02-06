using UnityEditor;
using UnityEditor.Compilation;

using UnityEngine;

namespace pdxpartyparrot.Core.Editor
{
    public static class EditorUtils
    {
        // this functionality is now available in Unity's menus
        /*[MenuItem("PDX Party Parrot/Reset PlayerPrefs")]
        public static void ResetPlayerPrefs()
        {
            if(EditorUtility.DisplayDialog("Reset Player Prefs", "Are you sure you wish to reset PlayerPrefs?", "Yes", "No")) {
                PlayerPrefs.DeleteAll();
                EditorUtility.DisplayDialog("Reset Player Prefs", "PlayerPrefs reset!", "Ok");
            }
        }*/

        [MenuItem("PDX Party Parrot/Force Rebuild")]
        public static void ForceRebuild()
        {
            CompilationPipeline.RequestScriptCompilation();
        }
    }
}
