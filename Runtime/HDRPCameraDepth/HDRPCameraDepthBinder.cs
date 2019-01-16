using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;
using UnityEngine.VFX.Utils;

namespace UnityEngine.VFX.Utils
{
    [VFXBinder("Camera/Camera Depth Binder")]
    public class HDRPCameraDepthBinder : VFXBinderBase
    {
        [VFXParameterBinding("UnityEngine.Texture2D")]
        public ExposedParameter DepthTextureParameter = "DepthTexture";
        [VFXParameterBinding("UnityEditor.VFX.CameraType")]
        public ExposedParameter CameraParameter = "Camera";
        public HDRPCameraDepthLayer CameraDepthLayer;

        ExposedParameter position;
        ExposedParameter angles;
        ExposedParameter scale;
        ExposedParameter fieldOfView;
        ExposedParameter nearPlane;
        ExposedParameter farPlane;
        ExposedParameter aspectRatio;
        ExposedParameter pixelDimensions;

        void UpdateSubParameters()
        {
            position = CameraParameter + "_transform_position";
            angles = CameraParameter + "_transform_angles";
            scale = CameraParameter + "_transform_scale";
            fieldOfView = CameraParameter + "_fieldOfView";
            nearPlane = CameraParameter + "_nearPlane";
            farPlane = CameraParameter + "_farPlane";
            aspectRatio = CameraParameter + "_aspectRatio";
            pixelDimensions = CameraParameter + "_pixelDimensions";
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateSubParameters();
        }

        private void OnValidate()
        {
            UpdateSubParameters();
        }

        public override bool IsValid(VisualEffect component)
        {
            return CameraDepthLayer != null && 
                CameraDepthLayer.Camera != null &&
                component.HasTexture(DepthTextureParameter) &&
                component.HasVector3(position) &&
                component.HasVector3(angles) &&
                component.HasVector3(scale) &&
                component.HasFloat(fieldOfView) &&
                component.HasFloat(nearPlane) &&
                component.HasFloat(farPlane) &&
                component.HasFloat(aspectRatio) &&
                component.HasVector2(pixelDimensions);
        }

        public override void UpdateBinding(VisualEffect component)
        {
            component.SetTexture(DepthTextureParameter, CameraDepthLayer.DepthTexture);

            component.SetVector3(position, CameraDepthLayer.Camera.transform.position);
            component.SetVector3(angles, CameraDepthLayer.Camera.transform.eulerAngles);
            component.SetVector3(scale, CameraDepthLayer.Camera.transform.lossyScale);
            component.SetFloat(fieldOfView, CameraDepthLayer.Camera.fieldOfView * Mathf.Deg2Rad);
            component.SetFloat(nearPlane, CameraDepthLayer.Camera.nearClipPlane);
            component.SetFloat(farPlane, CameraDepthLayer.Camera.farClipPlane);
            component.SetFloat(aspectRatio, CameraDepthLayer.Camera.aspect);
            component.SetVector2(pixelDimensions, new Vector2(CameraDepthLayer.Camera.pixelWidth, CameraDepthLayer.Camera.pixelHeight));
        }
    }
}
