using UnityEditor.VFX.UI;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEditor.VFX;
using UnityEngine.VFX;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Profiling;
using System.Text;


public partial class VFXGraphExtension //.Navigator
{
    const string kNavigatorVisiblePreferenceName = "VFXGraphExtension.navigatorVisible";

    static bool navigatorVisible
    {
        get { return EditorPrefs.GetBool(kNavigatorVisiblePreferenceName, true); }
        set { EditorPrefs.SetBool(kNavigatorVisiblePreferenceName, value); }
    }

    static VFXNavigator navigator;

    static void ToggleNavigator()
    {
        navigatorVisible = !navigatorVisible;

        SetNavigatorVisible(navigatorVisible);
    }

    static void SetNavigatorVisible(bool visible)
    {
        if (navigator == null)
        {
            navigator = new VFXNavigator(VFXViewWindow.currentWindow);
        }

        var gv = VFXViewWindow.currentWindow.graphView;

        if (visible)
        {
            gv.Insert(gv.childCount - 1, navigator);
        }
        else
        {
            navigator.RemoveFromHierarchy();
        }
    }

    static void InitializeNavigator()
    {
        SetNavigatorVisible(navigatorVisible);
    }
}
