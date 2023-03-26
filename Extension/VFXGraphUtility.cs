using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.VFX.UI;
using UnityEngine;
using UnityEngine.VFX;

namespace UnityEditor.VFX
{
    public static class VFXGraphUtility
    {
        public static void OpenAsset(VisualEffectAsset asset, bool openWindow)
        {
            var window = VFXViewWindow.GetWindow(asset, openWindow);
            if(window != null)
            {
                window.Focus();
            }
        }

        public static IEnumerable<VisualEffectAsset> GetLoadedAssets()
        {
            foreach(var window in VFXViewWindow.GetAllWindows())
            {
                var asset = GetLoadedAsset(window);
                if(asset != null)
                    yield return asset;
            }
        }

        public enum VFXAssetType
        { 
            None,
            VisualEffectAsset,
            VisualEffectSubgraphBlock,
            VisualEffectSubgraphOperator
        }

        internal static VisualEffectSubgraph GetLoadedSubgraph(VFXViewWindow window)
        {
            try
            {
                var vfxObject = window.graphView.controller.model.visualEffectObject;
                if (vfxObject is VisualEffectSubgraph)
                    return vfxObject as VisualEffectSubgraph;
                else
                    return null;
            }
            catch (NullReferenceException e)
            {
                return null;
            }
        }

        internal static VFXAssetType GetLoadedAssetType(VFXViewWindow window) 
        {
            try
            {
                var vfxObject = window.graphView.controller.model.visualEffectObject;
                if (vfxObject is VisualEffectSubgraphOperator)
                    return VFXAssetType.VisualEffectSubgraphOperator;
                else if (vfxObject is VisualEffectSubgraphBlock)
                    return VFXAssetType.VisualEffectSubgraphBlock;
                else
                    return VFXAssetType.VisualEffectAsset;
            }
            catch (NullReferenceException e)
            {
                return VFXAssetType.None;
            }
        }

        internal static VisualEffectAsset GetLoadedAsset(VFXViewWindow window)
        {
            try
            {
                return window.graphView.controller.model.asset;
            }
            catch (NullReferenceException e)
            {
                return null;
            }
        }

        #region UI to MODEL

        internal static VFXContext GetContext(VFXContextUI contextUI)
        {
            return contextUI.controller.model;
        }

        internal static VFXBlock GetBlock(VFXBlockUI blockUI)
        {
            return blockUI.controller.model;
        }

        internal static VFXOperator GetOperator(VFXOperatorUI operatorUI)
        {
            return operatorUI.controller.model;
        }
        #endregion


        #region MODEL UTILITIES

        internal static bool IsOrphanContext(VFXContext context)
        {
            return context.GetData().GetAttributes().Count() == 0;
        }

        internal static string GetSpawnSystemName(VFXBasicSpawner context)
        {
            return context.GetParent().systemNames.GetUniqueSystemName(context.GetData());
        }

        internal static string GetParticleSystemName(VFXContext context)
        {
            return context.GetParent().systemNames.GetUniqueSystemName(context.GetData());
        }

        internal static IEnumerable<VFXBlock> GetAllBlocks(VFXContext context)
        {
            return context.children;
        }

        #endregion


        [MenuItem("TEST/TEST")]
        static void Test()
        {
            foreach(var wnd in VFXViewWindow.GetAllWindows())
            {
                var type = GetLoadedAssetType(wnd);

                switch (type)
                {
                    case VFXAssetType.None:
                        Debug.Log($"NO ASSET");
                        break;
                    case VFXAssetType.VisualEffectAsset:
                        var asset = GetLoadedAsset(wnd);
                        Debug.Log($"Asset : {asset.name} ({type})");
                        break;
                    case VFXAssetType.VisualEffectSubgraphBlock:
                    case VFXAssetType.VisualEffectSubgraphOperator:
                        var assetSubgraph = GetLoadedSubgraph(wnd);
                        Debug.Log($"Asset : {assetSubgraph.name} ({type})");
                        break;
                    default:
                        break;
                }

            }
        }



    }
}

