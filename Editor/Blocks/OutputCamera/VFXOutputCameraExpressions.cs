using UnityEngine.VFX;
using UnityEngine.VFX.Globals;

namespace UnityEditor.VFX
{
    class OutputCameraPositionExpression : VFXExpression
    {
        public OutputCameraPositionExpression() : base(VFXExpression.Flags.InvalidOnCPU)  { }

        public override VFXValueType valueType => VFXValueType.Float3;
 
        public override VFXExpressionOperation operation => VFXExpressionOperation.None;

        public override string GetCodeString(string[] parents) => "_WorldSpaceCameraPos.xyz";

    }

    class OutputCameraForwardExpression : VFXExpression
    {
        public OutputCameraForwardExpression() : base(VFXExpression.Flags.InvalidOnCPU) { }

        public override VFXValueType valueType => VFXValueType.Float3;

        public override VFXExpressionOperation operation => VFXExpressionOperation.None;

        public override string GetCodeString(string[] parents) => "-1 * mul((float3x3)UNITY_MATRIX_M, transpose(mul(UNITY_MATRIX_I_M, UNITY_MATRIX_I_V)) [2].xyz)";
    }

    class OutputCameraUpExpression : VFXExpression
    {
        public OutputCameraUpExpression() : base(VFXExpression.Flags.InvalidOnCPU) { }

        public override VFXValueType valueType => VFXValueType.Float3;

        public override VFXExpressionOperation operation => VFXExpressionOperation.None;

        public override string GetCodeString(string[] parents) => "mul((float3x3)UNITY_MATRIX_M, transpose(mul(UNITY_MATRIX_I_M, UNITY_MATRIX_I_V)) [1].xyz)";
    }

    class OutputCameraRightExpression : VFXExpression
    {
        public OutputCameraRightExpression() : base(VFXExpression.Flags.InvalidOnCPU) { }

        public override VFXValueType valueType => VFXValueType.Float3;

        public override VFXExpressionOperation operation => VFXExpressionOperation.None;

        public override string GetCodeString(string[] parents) => "mul((float3x3)UNITY_MATRIX_M, transpose(mul(UNITY_MATRIX_I_M, UNITY_MATRIX_I_V)) [0].xyz)";
    }

    class OutputCameraDimensionsExpression : VFXExpression
    {
        public OutputCameraDimensionsExpression() : base(VFXExpression.Flags.InvalidOnCPU) { }

        public override VFXValueType valueType => VFXValueType.Float2;

        public override VFXExpressionOperation operation => VFXExpressionOperation.None;

        public override string GetCodeString(string[] parents) => "_ScreenParams.xy";

    }

    class OutputCameraNearFarPlaneExpression : VFXExpression
    {
        public OutputCameraNearFarPlaneExpression() : base(VFXExpression.Flags.InvalidOnCPU) { }

        public override VFXValueType valueType => VFXValueType.Float2;

        public override VFXExpressionOperation operation => VFXExpressionOperation.None;

        public override string GetCodeString(string[] parents) => "_ProjectionParams.yz";
    }

    class OutputCameraFovExpression : VFXExpression
    {
        public OutputCameraFovExpression() : base(VFXExpression.Flags.InvalidOnCPU) { }

        public override VFXValueType valueType => VFXValueType.Float2;

        public override VFXExpressionOperation operation => VFXExpressionOperation.None;

        public override string GetCodeString(string[] parents) => "atan(1.0/UNITY_MATRIX_P._m11)";
    }

    class OutputCameraRatioExpression : VFXExpression
    {
        public OutputCameraRatioExpression() : base(VFXExpression.Flags.InvalidOnCPU) { }

        public override VFXValueType valueType => VFXValueType.Float2;

        public override VFXExpressionOperation operation => VFXExpressionOperation.None;

        public override string GetCodeString(string[] parents) => "_ScreenParams.x/_ScreenParams.y";
    }
}

