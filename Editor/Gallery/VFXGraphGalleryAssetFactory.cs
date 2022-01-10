using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using UnityEngine.VFX;

namespace UnityEditor.VFX
{
    internal class VFXGraphGalleryAssetFactory
    {
        [MenuItem("Assets/Create/Visual Effects/Visual Effect Graph (from Template)", priority = 1)]
        private static void MenuCreateVFXGraphGalleryAsset()
        {
            var icon = EditorGUIUtility.FindTexture(typeof(VisualEffectAsset));
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<DoCreateVFXGraphGalleryAsset>(), "New VFXGraph.vfx", icon, null);
        }

        public static void CreateAssetAtPath(string path)
        {
            VFXGraphGalleryWindow.OpenWindowCreateAsset(path);
        }
    }

    internal class DoCreateVFXGraphGalleryAsset : EndNameEditAction
    {
        public override void Action(int instanceId, string path, string resourceFile)
        {
            if(!string.IsNullOrEmpty(path))
                VFXGraphGalleryAssetFactory.CreateAssetAtPath(path);
        }
    }
}
