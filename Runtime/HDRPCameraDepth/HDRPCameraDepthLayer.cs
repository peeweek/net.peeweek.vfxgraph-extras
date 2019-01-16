using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.VFX.Utils
{
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class HDRPCameraDepthLayer : MonoBehaviour
    {
        public enum SizeMode
        {
            Default = 0,
            HalfResolution = 1,
            QuarterResolution = 2,
            Specific = 3,
        }

        [SerializeField]
        SizeMode m_SizeMode;

        [SerializeField, Range(1,4096)]
        int m_Width = 256;

        [SerializeField, Range(1, 4096)]
        int m_Height = 256;

        public Texture DepthTexture { get => m_Depth; }
        public Camera Camera { get; private set; }

        RenderTexture m_Depth;

        void UpdateRenderTarget()
        {
            Camera = GetComponent<Camera>();

            int width = Screen.currentResolution.width;
            int height = Screen.currentResolution.height;

            switch (m_SizeMode)
            {
                case SizeMode.Default:
                    break;
                case SizeMode.HalfResolution:
                    width /= 2;
                    height /= 2;
                    break;
                case SizeMode.QuarterResolution:
                    width /= 4;
                    height /= 4;
                    break;
                case SizeMode.Specific:
                    width = m_Width;
                    height = m_Height;
                    break;

            }

            if (Camera != null && Camera.enabled)
            {
                m_Depth = new RenderTexture(width, height, 16, RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);
                m_Depth.name = $"{Camera.name} Depth ({width}x{height})";
                Camera.targetTexture = m_Depth;
            }
        }

        private void OnValidate()
        {
            UpdateRenderTarget();
        }

        private void OnEnable()
        {
            UpdateRenderTarget();
        }
    }
}
