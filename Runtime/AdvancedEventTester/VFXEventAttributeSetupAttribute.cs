using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.VFX.Extras
{
    [AttributeUsage(AttributeTargets.Class)]
    public class VFXEventAttributeSetupAttribute : Attribute
    {
        public readonly string category;
        public readonly string name;

        public VFXEventAttributeSetupAttribute(string category = "", string name = "")
        {
            this.category = category;
            this.name = name;
        }
    }
}

