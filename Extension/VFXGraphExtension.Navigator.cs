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
    const string kNavigatorVisiblePreferenceName = "VFXGraphExtension.navigatorVisible";
    const string kNavigatorRectPreferenceName = "VFXGraphExtension.navigatorRect";

    public static void NavigatorSavePosition(Rect r)
    {
        EditorPrefs.SetString(kNavigatorRectPreferenceName, string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3}", r.x, r.y, r.width, r.height));
    }

    public static Rect NavigatorLoadPosition(Rect defaultPosition)
    {
        string str = EditorPrefs.GetString(kNavigatorRectPreferenceName, "32,32,200,200");

        Rect position = defaultPosition;
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
                    position = new Rect(x, y, width, height);
                }
            }
        }
        else
        {
            NavigatorSavePosition(defaultPosition);
        }

        return position;
    }

    static Dictionary<VFXViewWindow, VFXNavigator> navigators = new Dictionary<VFXViewWindow, VFXNavigator>();
    static Dictionary<VFXViewWindow, bool> navigatorVisibility = new Dictionary<VFXViewWindow, bool>();

    static void InitializeNavigator(VFXViewWindow window)
    {
        CreateNavigator(window);
        SetNavigatorVisible(window, navigatorVisibility[window]);
    }

    static void CreateNavigator(VFXViewWindow window)
    {
        if (!navigators.ContainsKey(window))
        {
            navigators.Add(window, new VFXNavigator(VFXViewWindow.currentWindow));
            navigatorVisibility.Add(window, false);
        }
    }

    static void ToggleNavigatorMenu(object wnd)
    {
        ToggleNavigator(wnd as VFXViewWindow);
    }

    static void ToggleNavigator(VFXViewWindow window)
    {
        CreateNavigator(window);
        navigatorVisibility[window] = !navigatorVisibility[window];
        SetNavigatorVisible(window, navigatorVisibility[window]);
    }

    static void SetNavigatorVisible(VFXViewWindow window, bool visible)
    {
        CreateNavigator(window);

        var gv = VFXViewWindow.currentWindow.graphView;
        var navigator = navigators[window];
        if (visible)
        {
            gv.Insert(gv.childCount - 1, navigator);
            navigator.SetPosition(NavigatorLoadPosition(new Rect(32, 32, 180, 480)));
        }
        else
        {
            NavigatorSavePosition(navigator.GetPosition());
            navigator.RemoveFromHierarchy();
        }

        navigator.UpdatePresenterPosition();
    }


}
