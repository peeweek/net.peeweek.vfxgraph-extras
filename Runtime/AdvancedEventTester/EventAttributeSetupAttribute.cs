using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.VFX.Extras
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EventAttributeSetupAttribute : Attribute
    {
        public readonly string category;
        public readonly string name;

        public EventAttributeSetupAttribute(string category = "", string name = "")
        {
            this.category = category;
            this.name = name;
        }
    }
}

