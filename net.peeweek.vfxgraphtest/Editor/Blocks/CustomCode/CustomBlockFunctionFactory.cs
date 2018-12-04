using UnityEngine;
using UnityEditor.Callbacks;
using UnityEditor.ProjectWindowCallback;
using System.IO;

namespace UnityEditor.VFX
{
    public class CustomBlockFunctionFactory
    {
        [MenuItem("Assets/Create/Visual Effects/Custom Nodeblock Function", priority = 301)]
        private static void MenuCreateCustomBlockFunctionFactory()
        {
            var icon = EditorGUIUtility.FindTexture("ScriptableObject Icon");
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<DoCreateCustomBlockFunction>(), "New CustomBlockFunction.asset", icon, null);
        }

        internal static CustomBlockFunction CreateCustomBlockFunctionAtPath(string path)
        {
            CustomBlockFunction asset = ScriptableObject.CreateInstance<CustomBlockFunction>();
            asset.name = Path.GetFileName(path);
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }

    internal class DoCreateCustomBlockFunction : EndNameEditAction
    {
        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            CustomBlockFunction asset = CustomBlockFunctionFactory.CreateCustomBlockFunctionAtPath(pathName);
            ProjectWindowUtil.ShowCreatedAsset(asset);
        }
    }
}
