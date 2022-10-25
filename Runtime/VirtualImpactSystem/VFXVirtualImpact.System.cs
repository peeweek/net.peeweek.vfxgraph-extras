using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.VFX.VirtualImpacts
{
    public partial class VFXVirtualImpact : ScriptableObject
    {
        static GameObject systemsRoot = null;
        static VFXVirtualImpactUpdater updater = null;
        static List<VFXVirtualImpact> virtualImpacts;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            systemsRoot = new GameObject("VFXVirtualImpactSystem");
            GameObject.DontDestroyOnLoad(systemsRoot);
            updater = systemsRoot.AddComponent<VFXVirtualImpactUpdater>();
            virtualImpacts = new List<VFXVirtualImpact>();
        }

        public static void CreateVirtualImpact(VFXVirtualImpact virtualImpact)
        {
            Debug.Assert(virtualImpact.IsValid());

            if (virtualImpact.IsValid() && !virtualImpacts.Contains(virtualImpact))
            {
                var impactGameObject = virtualImpact.CreateVirtualImpactGameObject();
                impactGameObject.transform.parent = systemsRoot.transform;
                virtualImpacts.Add(virtualImpact);
            }
        }

        public static void DisposeVirtualImpact(VFXVirtualImpact virtualImpact)
        {
            if (virtualImpacts.Contains(virtualImpact))
            {
                virtualImpact.DisposeVirtualImpactGameObject();
                virtualImpacts.Remove(virtualImpact);
            }
        }

        public static bool TryGetImpact(VFXVirtualImpact virtualImpact, float lifeTime, out Impact instance)
        {
            CreateVirtualImpact(virtualImpact);
            return virtualImpact.TryGetImpact(lifeTime, out instance);
        }

        public static void UpdateAll(float deltaTime)
        {
            foreach(var impact in virtualImpacts)
            {
                impact.UpdateVirtualImpact(deltaTime);
            }
        }

        public static void DrawDebugGUI()
        {
            Rect r = new Rect(32, 32, 480, Screen.height - 64);
            GUI.Box(r, GUIContent.none);
            r = new RectOffset(16,16,16,16).Remove(r);
            using(new GUILayout.AreaScope(r))
            {
                GUILayout.Label("VirtualImpactSystem DEBUG");
                GUILayout.Space(16);
                foreach (var s in virtualImpacts)
                {
                    GUILayout.Label($"> {s.name} ({s.Asset.name}) ({s.available.Count} available) {s.bounds}");
                    for (int i = 0; i<s.instances.Length; i++)
                    {
                        var instance = s.instances[i];
                        if (instance.TTL < 0f)
                            continue;

                        GUILayout.Label($"     -> [{i.ToString("D3")}] : ({instance.TTL.ToString("F2")}s) ({instance.Bounds.center},{instance.Bounds.size})");

                    }
                    GUILayout.Space(16);
                }
            }
        }

        public static void DrawGizmosGUI()
        {
            foreach (var virtualImpact in virtualImpacts)
            {
                Gizmos.DrawWireCube(virtualImpact.bounds.center, virtualImpact.bounds.size);
            }
        }
    }
}
