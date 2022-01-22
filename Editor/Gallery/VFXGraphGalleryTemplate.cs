using System;
using UnityEngine;
using UnityEngine.VFX;

namespace UnityEditor.VFX
{
    [CreateAssetMenu(fileName ="New VFX Template.asset", menuName = "Visual Effects/Visual Effect Graph Template", order = 310)]
    public class VFXGraphGalleryTemplate : ScriptableObject
    {
        public string categoryName;
        public Template[] templates;

        [Serializable]
        public struct Template
        {
            public VisualEffectAsset templateAsset;
            [Tooltip("Preview Image (286x180)")]
            public Texture2D preview;
            public string name;
            [Multiline]
            public string description;
        }

        static string dir
        {
            get => EditorPrefs.GetString("VFXGraphGalleryTemplate.dir", "Assets/");
            set => EditorPrefs.SetString("VFXGraphGalleryTemplate.dir", value);
        }

            [ContextMenu("Capture From Scene View (286x180)")]
        void CaptureFromSceneView()
        {
            int width = 286;
            int height = 180;

            RenderTexture rt = RenderTexture.GetTemporary(width, height, 0,RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var c = SceneView.lastActiveSceneView.camera;
            c.targetTexture = rt;
            c.Render();
            c.targetTexture = null;
            RenderTexture.active = rt;
            Texture2D outTex = new Texture2D(width, height, TextureFormat.ARGB32, true);
            outTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);

            string outfile = EditorUtility.SaveFilePanel("Save Preview", dir, "PreviewTexture", "png");
            if(!string.IsNullOrEmpty(outfile))
            {

                byte[] bytes = outTex.EncodeToPNG();
                System.IO.File.WriteAllBytes(outfile, bytes);
                AssetDatabase.Refresh();

                outfile = outfile.Remove(0, Application.dataPath.Length-6); // Remove all but Assets from the datapath
                
                var exported = AssetDatabase.LoadAssetAtPath<Texture2D>(outfile);
                var importer = AssetImporter.GetAtPath(outfile) as TextureImporter;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.npotScale = TextureImporterNPOTScale.None;
                AssetDatabase.ImportAsset(outfile);
                ProjectWindowUtil.ShowCreatedAsset(exported);
            }
            RenderTexture.active = null;
        }

    }
}

