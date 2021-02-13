using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

namespace pdxpartyparrot.Core.Editor
{
    // https://forum.unity.com/threads/what-is-recovery-gameobject.1005352/
    public static class MissingPrefabComponentFinder
    {
        [MenuItem("PDX Party Parrot/Log Missing Prefabs And Components")]
        public static void Search()
        {
            var results = new List<string>();

            var gameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach(GameObject go in gameObjects) {
                Traverse(go.transform, results);
            }

            Debug.Log($"> Total Results: {results.Count}");
            foreach(string result in results) {
                Debug.Log($"> {result}");
            }
        }

        private static void AppendComponentResult(IList<string> results, string childPath, int index)
        {
            results.Add($"Missing Component {index} of {childPath}");
        }

        private static void AppendTransformResult(IList<string> results, string childPath, string name)
        {
            results.Add($"Missing Prefab for \"{name}\" of {childPath}");
        }

        private static void Traverse(Transform transform, IList<string> results, string path = "")
        {
            string thisPath = $"{path}/{transform.name}";

            var components = transform.GetComponents<Component>();
            for(int i = 0; i < components.Length; ++i) {
                Component component = components[i];
                if(null == component) {
                    AppendComponentResult(results, thisPath, i);
                }
            }

            for(int c = 0; c < transform.childCount; ++c) {
                Transform t = transform.GetChild(c);
                PrefabAssetType pt = PrefabUtility.GetPrefabAssetType(t.gameObject);
                if(pt == PrefabAssetType.MissingAsset) {
                    AppendTransformResult(results, thisPath, t.name);
                } else {
                    Traverse(t, results, thisPath);
                }
            }
        }
    }
}
