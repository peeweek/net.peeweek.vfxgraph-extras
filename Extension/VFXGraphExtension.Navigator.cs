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
using System.Globalization;

public partial class VFXGraphExtension //.Navigator
{
    const string kNavigatorVisiblePreferenceName = "VFXGraphExtension.navigatorVisible:";
    const string kNavigatorRectPreferenceName = "VFXGraphExtension.navigatorRect";

    static readonly Rect defaultNavigatorPosition = new Rect(16, 48, 180, 480);



    public static void NavigatorSavePosition(Rect r)
    {
        EditorPrefs.SetString(kNavigatorRectPreferenceName, string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3}", r.x, r.y, r.width, r.height));
    }

    public static Rect NavigatorLoadPosition()
    {
        string str = EditorPrefs.GetString(kNavigatorRectPreferenceName);

        if (!string.IsNullOrEmpty(str))
        {
            var rectValues = str.Split(',');

            if (rectValues.Length == 4)
            {
                float x, y, width, height;
                if (float.TryParse(rectValues[0], NumberStyles.Float, CultureInfo.InvariantCulture, out x) &&
                    float.TryParse(rectValues[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y) &&
                    float.TryParse(rectValues[2], NumberStyles.Float, CultureInfo.InvariantCulture, out width) &&
                    float.TryParse(rectValues[3], NumberStyles.Float, CultureInfo.InvariantCulture, out height))
                {
                    return new Rect(x, y, width, height);
                }
            }
        }
        else
        {
            NavigatorSavePosition(defaultNavigatorPosition);
            return defaultNavigatorPosition;
        }

        return defaultNavigatorPosition;
    }

    static Dictionary<VFXViewWindow, VFXNavigator> navigators = new Dictionary<VFXViewWindow, VFXNavigator>();
    static Dictionary<VFXViewWindow, bool> navigatorVisibility = new Dictionary<VFXViewWindow, bool>();

    static void InitializeNavigator(VFXViewWindow window)
    {
        CreateNavigatorIfRequired(window);
        SetNavigatorVisible(window, navigatorVisibility[window]);
    }

    static void CreateNavigatorIfRequired(VFXViewWindow window)
    {
        if (!navigators.ContainsKey(window))
        {
            navigators.Add(window, new VFXNavigator(window, NavigatorLoadPosition()));
            navigatorVisibility.Add(window, GetNavigatorVisible(window));
        }
    }

    static void ToggleNavigatorMenu(object wnd)
    {
        ToggleNavigator(wnd as VFXViewWindow);
    }

    static void ToggleNavigator(VFXViewWindow window)
    {
        CreateNavigatorIfRequired(window);
        navigatorVisibility[window] = !navigatorVisibility[window];
        SetNavigatorVisible(window, navigatorVisibility[window]);
    }

    static bool GetNavigatorVisible(VFXViewWindow window)
    {
        var asset = window.graphView?.controller?.model?.visualEffectObject?.GetResource()?.asset;
        if (asset != null)
        {
            var guid = AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(asset));
            return EditorPrefs.GetBool(kNavigatorVisiblePreferenceName + guid.ToString(), false);
        }
        else 
            return false;
    }

    static void SetNavigatorVisible(VFXViewWindow window, bool visible)
    {
        CreateNavigatorIfRequired(window);

        var gv = window.graphView;
        var navigator = navigators[window];
        if (visible)
        {
            gv.Insert(gv.childCount - 1, navigator);
            navigator.SetPosition(NavigatorLoadPosition());
        }
        else
        {
            NavigatorSavePosition(navigator.GetPosition());
            navigator.RemoveFromHierarchy();
        }

        var asset = window.graphView?.controller?.model?.visualEffectObject?.GetResource()?.asset;

        if (asset != null)
        {
            var guid = AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(asset));
            EditorPrefs.SetBool(kNavigatorVisiblePreferenceName + guid.ToString(), visible);
        }

        navigator.UpdatePresenterPosition();
    }


}
