using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace UnityEditor.VFX
{
    [Serializable]
    public abstract class EventAttributeSetup
    {
        public string name;
        public bool enabled = true;

        public abstract void ApplyEventAttribute(VFXEventAttribute attrib);
    }


    [Serializable]
    public class ConstantFloat : EventAttributeSetup
    {
        public string attributeName = "size";
        public float value = 1.0f;

        public override void ApplyEventAttribute(VFXEventAttribute attrib)
        {
            if (attrib.HasFloat(attributeName))
                attrib.SetFloat(attributeName, value);
        }
    }

    [Serializable]
    public class RandomFloat : EventAttributeSetup
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

    public enum RandomVectorMode
    {
        Uniform,
        PerComponent
    }

    [Serializable]
    public class ConstantVector2 : EventAttributeSetup
    {
        public string attributeName = "position";
        public Vector2 value = Vector2.one;

        public override void ApplyEventAttribute(VFXEventAttribute attrib)
        {
            if (attrib.HasVector2(attributeName))
                attrib.SetVector2(attributeName, value);
        }
    }

    [Serializable]
    public class RandomVector2 : EventAttributeSetup
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

    [Serializable]
    public class ConstantVector3 : EventAttributeSetup
    {
        public string attributeName = "position";
        public Vector3 value = Vector3.one;

        public override void ApplyEventAttribute(VFXEventAttribute attrib)
        {
            if (attrib.HasVector3(attributeName))
                attrib.SetVector3(attributeName, value);
        }
    }

    [Serializable]
    public class RandomVector3 : EventAttributeSetup
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

    [Serializable]
    public class ConstantHDRColor : EventAttributeSetup
    {
        public string attributeName = "color";
        [ColorUsage(false, true)]
        public Color value = Color.white;

        public override void ApplyEventAttribute(VFXEventAttribute attrib)
        {
            if (attrib.HasVector3(attributeName))
            {
                Vector3 rgb = new Vector3(value.r, value.g, value.b);
                attrib.SetVector3(attributeName, rgb);
            }
        }
    }

    [Serializable]
    public class RandomHDRColor : EventAttributeSetup
    {
        public string attributeName = "color";
        [ColorUsage(false, true)]
        public Color min = Color.black;
        [ColorUsage(false, true)]
        public Color max = Color.white;
        public RandomVectorMode randomMode = RandomVectorMode.PerComponent;
        public bool normalize;

        public override void ApplyEventAttribute(VFXEventAttribute attrib)
        {
            if (!attrib.HasVector3(attributeName))
                return;

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
    }



}
