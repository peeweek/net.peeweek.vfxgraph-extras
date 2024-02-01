using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.VFX.UI;
using UnityEngine;
using UnityEngine.UIElements;
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
                var asset = window.GetLoadedAsset();
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

        internal static bool HasLoadedAsset(this VFXViewWindow window)
        {
            return window.graphView.controller != null;
        }

        internal static VisualEffectAsset GetLoadedAsset(this VFXViewWindow window)
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

        #region UI UTILITIES

        internal static string GetContextUserName(this VFXContextUI contextUI)
        {
            return (contextUI.Q("user-label") as Label)?.text;
        }

        internal static string GetContextName(this VFXContextUI contextUI)
        {
            return contextUI.GetModel().name;
        }

        #endregion


        #region UI to MODEL

        internal static T GetModelAs<T>(this VFXContextUI contextUI) where T : VFXContext
        {
            return contextUI.GetModel() as T;
        }

        internal static VFXContext GetModel(this VFXContextUI contextUI)
        {
            return contextUI.controller.model;
        }

        internal static T GetModelAs<T>(this VFXBlockUI blockUI) where T : VFXBlock
        {
            return blockUI.GetModel() as T;
        }

        internal static VFXBlock GetModel(this VFXBlockUI blockUI)
        {
            return blockUI.controller.model;
        }

        internal static T GetModelAs<T>(this VFXOperatorUI operatorUI) where T : VFXOperator
        {
            return operatorUI.GetModel() as T;
        }

        internal static VFXOperator GetModel(this VFXOperatorUI operatorUI)
        {
            return operatorUI.controller.model;
        }

        internal static bool IsModel<T>(this VFXContextUI contextUI)
        {
            return contextUI.controller.model is T;
        }
        #endregion


        #region MODEL UTILITIES

        internal static bool IsOrphanContext(VFXContext context)
        {
            return context.GetData().GetAttributes().Count() == 0;
        }

        internal static bool HasData<T>(VFXContext context)
        {
            return !IsOrphanContext(context) && context.GetData() is T;
        }

        internal static string GetSpawnSystemName(this VFXBasicSpawner context)
        {
            return context.GetParent().systemNames.GetUniqueSystemName(context.GetData());
        }

        internal static string GetParticleSystemName(this VFXContext context)
        {
            return context.GetParent().systemNames.GetUniqueSystemName(context.GetData());
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

