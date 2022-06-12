using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace UnityEditor.VFX
{
    [VFXInfo]
    class VFXSpawnMultiplexer : VFXContext
    {
        public enum MultiplexSpawnCountMode
        {
            OverwriteSpawnCount = 0,
            MultiplySpawnCount = 1,
            AddToSpawnCount = 2
        }

        [VFXSetting(VFXSettingAttribute.VisibleFlags.InGraph)]
        public MultiplexSpawnCountMode multiplexMode;

        public VFXSpawnMultiplexer() : base(VFXContextType.Spawner, VFXDataType.SpawnEvent, VFXDataType.SpawnEvent) { }

        public override string name { get { return "Multiplex Spawn (First-in, First-out)"; } }

        protected override int inputFlowCount => 2;

        public override bool AcceptChild(VFXModel model, int index = -1) => false;

        VFXBlock m_Block;

        protected override IEnumerable<VFXBlock> implicitPreBlock
        {
            get
            {
                yield return m_Block;
            }
        }

        void CreateImplicitBlock()
        {
            var blockCustomSpawner = ScriptableObject.CreateInstance<VFXSpawnerCustomWrapper>();
            blockCustomSpawner.SetSettingValue("m_customType", new SerializableType(typeof(VFXSpawnMultiplexerImplicitBlock)));
            m_Block = blockCustomSpawner;
        }

        static IEnumerable<VFXModelDescriptor<VFXBlock>> blocks; 

        protected override IEnumerable<VFXPropertyWithValue> inputProperties
        {
            get
            {
                if (m_Block == null)
                    CreateImplicitBlock();

                if (m_Block != null)
                {
                    
                    foreach (var prop in PropertiesFromSlots(m_Block.inputSlots))
                    {
                        if (!prop.property.name.StartsWith('_'))
                            yield return prop;
                    }

                    m_Block.GetInputSlot(2).value = (int)multiplexMode;
                }
            }
        }

        public override VFXExpressionMapper GetExpressionMapper(VFXDeviceTarget target)
        {
            var mapper = base.GetExpressionMapper(target);

            //CPU Only
            if (target == VFXDeviceTarget.GPU)
                return mapper;

            if (mapper == null)
                mapper = new VFXExpressionMapper();

            mapper.AddExpressionsFromSlot(m_Block.GetInputSlot(0), -1);
            mapper.AddExpressionsFromSlot(m_Block.GetInputSlot(1), -1);
            mapper.AddExpressionsFromSlot(m_Block.GetInputSlot(2), -1);

            return mapper;
        }
    }
}


