using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX.Globals;

namespace UnityEditor.VFX
{
    [VFXInfo(category = "Output")]
    class OutputCameraOperator : VFXOperator
    {
        public override string name => $"Output Camera";

        public class OutputProperties
        {
            public Vector3 Position;
            public Vector3 Forward;
            public Vector3 Up;
            public Vector3 Right;
            public Vector2 Resolution;
            public Vector2 NearFarPlane;
            public float FieldOfView;
            public float ratio;
        }

        protected override VFXExpression[] BuildExpression(VFXExpression[] inputExpression)
        {
            return new VFXExpression[]
            {
                new OutputCameraPositionExpression(),
                new OutputCameraForwardExpression(),
                new OutputCameraUpExpression(),
                new OutputCameraRightExpression(),
                new OutputCameraDimensionsExpression(),
                new OutputCameraNearFarPlaneExpression(),
                new OutputCameraFovExpression(),
                new OutputCameraRatioExpression(),
            };
        }
    }
}