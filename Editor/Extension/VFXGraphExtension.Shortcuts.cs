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
using System.Reflection;

static partial class VFXGraphExtension
{ 
    static Dictionary<Event, ShortcutDelegate> ClearAndGetShortcuts(VFXViewWindow window)
    {
        FieldInfo sh_info = typeof(VFXViewWindow).GetField("m_ShortcutHandler", BindingFlags.NonPublic | BindingFlags.Instance);
        ShortcutHandler current = sh_info.GetValue(window) as ShortcutHandler;

        FieldInfo dict_info = typeof(ShortcutHandler).GetField("m_Dictionary", BindingFlags.NonPublic | BindingFlags.Instance);
        Dictionary<Event, ShortcutDelegate> dict = dict_info.GetValue(current) as Dictionary<Event, ShortcutDelegate>;
        dict.Clear();
        return dict;
    }

    static bool shortcuts_initialized = false;

    static void InitializeShortcuts(VFXViewWindow window)
    {
        if (shortcuts_initialized)
            return;

        UpdateShortcuts(window);
        shortcuts_initialized = true;
    }

    static void UpdateShortcuts(object wnd)
    {
        VFXViewWindow window = wnd as VFXViewWindow;
        Dictionary<Event, ShortcutDelegate> shortcuts = ClearAndGetShortcuts(window);
        shortcuts.Add(Event.KeyboardEvent("F7"), toto);
        window.graphView.isReframable = false;
        
    }

    static EventPropagation toto()
    {
        Debug.Log("AAA");
        return EventPropagation.Stop;
    }
}
