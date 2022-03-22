using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.VFX;
using UnityEditor.VFX.Block;
using UnityEngine;

namespace UnityEditor.VFX
{
    [VFXInfo(category = "Attribute", experimental = true)]
    class GetCollisionAttributeXT : VFXOperator
    {
        public enum CollisionAttribute
        {
            CollisionChannel,
            CollisionPosition,
            CollisionDirection
        }

        [VFXSetting(VFXSettingAttribute.VisibleFlags.All), Tooltip("Specifies the name of the custom attribute to use.")]
        public CollisionAttribute attribute = CollisionAttribute.CollisionChannel;


        [VFXSetting, Tooltip("Specifies which version of the parameter to use. It can return the current value, or the source value derived from a GPU event or a spawn attribute.")]
        public VFXAttributeLocation location = VFXAttributeLocation.Source;

        protected override IEnumerable<VFXPropertyWithValue> outputProperties
        {
            get
            {
                var attribute = currentAttribute;
                yield return new VFXPropertyWithValue(new VFXProperty(VFXExpression.TypeToType(attribute.type), attribute.name));
            }
        }

        private VFXAttribute currentAttribute
        {
            get
            {
                switch (attribute)
                {
                    case CollisionAttribute.CollisionChannel:
                        return TriggerOnCollisionXT.collisionChannel;
                    case CollisionAttribute.CollisionPosition:
                        return TriggerOnCollisionXT.collisionPosition;
                    case CollisionAttribute.CollisionDirection:
                        return TriggerOnCollisionXT.collisionDirection;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        override public string libraryName { get { return "Get Collision Attribute (XT)"; } }

        override public string name
        {
            get
            {
                return "Get " + attribute + "  (XT)";
            }
        }
        protected override VFXExpression[] BuildExpression(VFXExpression[] inputExpression)
        {
            var attribute = currentAttribute;

            var expression = new VFXAttributeExpression(attribute, location);
            return new VFXExpression[] { expression };
        }
    }
}
