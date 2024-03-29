using System;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace UnityEngine.VFX.Extras
{
    [Serializable]
    public abstract class VFXEventAttributeSetup
    {
        public bool enabled = true;

        public abstract void ApplyEventAttribute(VFXEventAttribute attrib);
    }


    [Serializable, VFXEventAttributeSetup("Float", "Float (Constant)")]
    public class ConstantFloat : VFXEventAttributeSetup
    {
        public string attributeName = "size";
        public float value = 1.0f;

        public override void ApplyEventAttribute(VFXEventAttribute attrib)
        {
            if (attrib.HasFloat(attributeName))
                attrib.SetFloat(attributeName, value);
        }
    }

    [Serializable, VFXEventAttributeSetup("Float", "Float (Random)")]
    public class RandomFloat : VFXEventAttributeSetup
    {
        public string attributeName = "size";
        public float min = 0.0f;
        public float max = 1.0f;

        public override void ApplyEventAttribute(VFXEventAttribute attrib)
        {
            if (attrib.HasFloat(attributeName))
                attrib.SetFloat(attributeName, UnityEngine.Random.Range(min, max));
        }
    }

    [Serializable, VFXEventAttributeSetup("Float", "Float (Random from Curve)")]
    public class RandomFloatFromCurve : VFXEventAttributeSetup
    {
        public string attributeName = "size";
        public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);

        public override void ApplyEventAttribute(VFXEventAttribute attrib)
        {
            if (attrib.HasFloat(attributeName))
                attrib.SetFloat(attributeName, curve.Evaluate(UnityEngine.Random.Range(0f, 1f)));
        }
    }

    public enum RandomVectorMode
    {
        Uniform,
        PerComponent
    }

    [Serializable, VFXEventAttributeSetup("Vector2", "Vector2 (Constant)")]
    public class ConstantVector2 : VFXEventAttributeSetup
    {
        public string attributeName = "position";
        public Vector2 value = Vector2.one;

        public override void ApplyEventAttribute(VFXEventAttribute attrib)
        {
            if (attrib.HasVector2(attributeName))
                attrib.SetVector2(attributeName, value);
        }
    }

    [Serializable, VFXEventAttributeSetup("Vector2", "Vector2 (Random)")]
    public class RandomVector2 : VFXEventAttributeSetup
    {
        public string attributeName = "position";
        public Vector2 min = Vector2.zero;
        public Vector2 max = Vector2.one;
        public RandomVectorMode randomMode = RandomVectorMode.PerComponent;
        public bool normalize;

        public override void ApplyEventAttribute(VFXEventAttribute attrib)
        {
            if (!attrib.HasVector2(attributeName))
                return;

            Vector2 value;
            if (randomMode == RandomVectorMode.PerComponent)
                value = new Vector2(
                    Mathf.Lerp(min.x, max.x, UnityEngine.Random.Range(0f, 1f)),
                    Mathf.Lerp(min.y, max.y, UnityEngine.Random.Range(0f, 1f))
                    );
            else
            {
                var r = UnityEngine.Random.Range(0f, 1f);
                value = Vector2.Lerp(min, max, r);
            }

            if (normalize)
                value.Normalize();

            attrib.SetVector2(attributeName, value);
        }
    }

    [Serializable, VFXEventAttributeSetup("Vector3", "Vector3 (Constant)")]
    public class ConstantVector3 : VFXEventAttributeSetup
    {
        public string attributeName = "position";
        public Vector3 value = Vector3.one;

        public override void ApplyEventAttribute(VFXEventAttribute attrib)
        {
            if (attrib.HasVector3(attributeName))
                attrib.SetVector3(attributeName, value);
        }
    }

    [Serializable, VFXEventAttributeSetup("Vector3", "Vector3 (GameObject Position)")]
    public class Vector3FromPosition : VFXEventAttributeSetup
    {
        public string attributeName = "position";
        public Transform transform;
        public bool localPosition = false;

        public override void ApplyEventAttribute(VFXEventAttribute attrib)
        {
            if (attrib.HasVector3(attributeName) && transform != null)
                attrib.SetVector3(attributeName, localPosition? transform.localPosition : transform.position);
        }
    }

    [Serializable, VFXEventAttributeSetup("Vector3", "Vector3 (GameObject Scale)")]
    public class Vector3FromScale : VFXEventAttributeSetup
    {
        public string attributeName = "scale";
        public Transform transform;
        public bool localScale = false;

        public override void ApplyEventAttribute(VFXEventAttribute attrib)
        {
            if (attrib.HasVector3(attributeName) && transform != null)
                attrib.SetVector3(attributeName, localScale ? transform.localScale : transform.lossyScale);
        }
    }

    [Serializable, VFXEventAttributeSetup("Vector3", "Vector3 (GameObject Axis)")]
    public class Vector3FromTransformAxis : VFXEventAttributeSetup
    {
        public enum Axis
        {
            Forward,
            Back,
            Right,
            Left,
            Up,
            Down
        }

        public string attributeName = "direction";
        public Transform transform;
        public Axis axis = Axis.Forward;

        public override void ApplyEventAttribute(VFXEventAttribute attrib)
        {
            if (attrib.HasVector3(attributeName) && transform != null)
            {
                Vector3 dir = Vector3.zero;
                switch (axis)
                {
                    default:
                    case Axis.Forward:
                        dir = transform.forward;
                        break;
                    case Axis.Back:
                        dir = -transform.forward;
                        break;
                    case Axis.Right:
                        dir = transform.right;
                        break;
                    case Axis.Left:
                        dir = -transform.right;
                        break;
                    case Axis.Up:
                        dir = transform.up;
                        break;
                    case Axis.Down:
                        dir = -transform.up;
                        break;
                }
                attrib.SetVector3(attributeName, dir);
            }
        }
    }

    [Serializable, VFXEventAttributeSetup("Vector3", "Vector3 (Random)")]
    public class RandomVector3 : VFXEventAttributeSetup
    {
        public string attributeName = "position";
        public Vector3 min = Vector3.zero;
        public Vector3 max = Vector3.one;
        public RandomVectorMode randomMode = RandomVectorMode.PerComponent;
        public bool normalize;

        public override void ApplyEventAttribute(VFXEventAttribute attrib)
        {
            if (!attrib.HasVector3(attributeName))
                return;

            Vector3 value;
            if (randomMode == RandomVectorMode.PerComponent)
                value = new Vector3(
                    Mathf.Lerp(min.x, max.x, UnityEngine.Random.Range(0f, 1f)),
                    Mathf.Lerp(min.y, max.y, UnityEngine.Random.Range(0f, 1f)),
                    Mathf.Lerp(min.z, max.z, UnityEngine.Random.Range(0f, 1f))
                    );
            else
            {
                var r = UnityEngine.Random.Range(0f, 1f);
                value = Vector3.Lerp(min, max, r);
            }

            if (normalize)
                value.Normalize();

            attrib.SetVector3(attributeName, value);
        }
    }

    [Serializable, VFXEventAttributeSetup("Color HDR", "Color HDR (Constant)")]
    public class ConstantHDRColor : VFXEventAttributeSetup
    {
        public string attributeName = "color";
        [ColorUsage(true, true)]
        public Color value = Color.white;
        public bool setAlpha = true;
        public string alphaAttributeName = "alpha";
        public override void ApplyEventAttribute(VFXEventAttribute attrib)
        {
            if (attrib.HasVector3(attributeName))
            {
                Vector3 rgb = new Vector3(value.r, value.g, value.b);
                attrib.SetVector3(attributeName, rgb);
            }

            if(setAlpha && attrib.HasFloat(alphaAttributeName))
            {
                attrib.SetFloat(alphaAttributeName, value.a);
            }
        }
    }

    [Serializable, VFXEventAttributeSetup("Color HDR", "Color HDR (Random)")]
    public class RandomHDRColor : VFXEventAttributeSetup
    {
        public string attributeName = "color";
        [ColorUsage(true, true)]
        public Color min = Color.black;
        [ColorUsage(true, true)]
        public Color max = Color.white;
        public RandomVectorMode randomMode = RandomVectorMode.PerComponent;
        public bool normalize;
        public bool setAlpha = true;
        public string alphaAttributeName = "alpha";

        public override void ApplyEventAttribute(VFXEventAttribute attrib)
        {
            if (attrib.HasVector3(attributeName))
            {
                Vector3 value;
                if (randomMode == RandomVectorMode.PerComponent)
                    value = new Vector3(
                        Mathf.Lerp(min.r, max.r, UnityEngine.Random.Range(0f, 1f)),
                        Mathf.Lerp(min.g, max.g, UnityEngine.Random.Range(0f, 1f)),
                        Mathf.Lerp(min.b, max.b, UnityEngine.Random.Range(0f, 1f))
                        );
                else
                {
                    var r = UnityEngine.Random.Range(0f, 1f);
                    Vector3 minRgb = new Vector3(min.r, min.g, min.b);
                    Vector3 maxRgb = new Vector3(max.r, max.g, max.b);
                    value = Vector3.Lerp(minRgb, maxRgb, r);
                }

                if (normalize)
                    value.Normalize();

                attrib.SetVector3(attributeName, value);
            }

            if (setAlpha && attrib.HasFloat(alphaAttributeName))
            {
                attrib.SetFloat(alphaAttributeName, UnityEngine.Random.Range(min.a, max.a));
            }
        }
    }

    [Serializable, VFXEventAttributeSetup("Color HDR", "Color HDR (Random from Gradient)")]
    public class RandomHDRColorFromGradient : VFXEventAttributeSetup
    {
        public string attributeName = "color";
        public bool setAlpha = true;
        public string alphaAttributeName = "alpha";
        [GradientUsage(hdr : true)]
        public Gradient gradient = new Gradient();

        public override void ApplyEventAttribute(VFXEventAttribute attrib)
        {
            if (attrib.HasVector3(attributeName))
            {
                Color c = gradient.Evaluate(UnityEngine.Random.Range(0f, 1f));

                attrib.SetVector3(attributeName, new Vector3(c.r, c.g, c.b));

                if (setAlpha && attrib.HasFloat(alphaAttributeName))
                    attrib.SetFloat(alphaAttributeName, c.a);
            }
        }
    }
}
