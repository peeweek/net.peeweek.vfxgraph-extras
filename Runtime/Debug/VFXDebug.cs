using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.VFX.DebugTools
{
    public static class VFXDebug
    {
        public struct DebugEntry
        {
            public static DebugEntry Create(VisualEffect vfx)
            {
                return new DebugEntry()
                {
                    component = vfx,
                    renderer = vfx.gameObject.GetComponent<Renderer>()
                };
            }

            public VisualEffect component;
            public Renderer renderer;

            public GameObject gameObject => component.gameObject;
            public VisualEffectAsset asset => component.visualEffectAsset;


            // Validity Check, should be performed at least once each frame before accessing API
            public bool valid => component != null && gameObject != null && asset != null && renderer != null;

            // Game Object
            public bool active => component.enabled && gameObject.activeInHierarchy;
            public Vector3 position => gameObject.transform.position;
            public string name => gameObject.name;
            public string sceneName => gameObject.scene.name;

            // Global Values
            public bool culled => component.culled;
            public uint seed => component.startSeed;
            public bool resetSeedOnPlay => component.resetSeedOnPlay;
            public int aliveCount => component.aliveParticleCount;

            // Playback Control
            public void SetPaused(bool paused) { component.pause = paused; }
            public void TogglePause() { component.pause = !component.pause; }
            public bool paused => component.pause;

            public void Reset() { component.Reinit(); }
            public void Restart() { SetPaused(false); Reset();  Play(); }
            public void Play() { component.Play(); }
            public void Stop() { component.Stop(); }

            public void Step() { SetPaused(true); component.Simulate(VFXManager.fixedTimeStep); }

            public float playRate
            {
                get => component.playRate;
                set => component.playRate = value;
            }


            // Renderer-Specfic Debug Control
            public void SetRendered(bool rendered) { renderer.enabled = rendered; }
            public void ToggleRendered() { renderer.enabled = !renderer.enabled; }
            public bool rendered => renderer.enabled;

            Dictionary<string, uint> maxSystemCounts;

            public uint GetMaxAliveCount(string systemName, uint currentCount)
            {
                if (maxSystemCounts == null)
                    maxSystemCounts = new Dictionary<string, uint>();

                if(maxSystemCounts.ContainsKey(systemName))
                {
                    uint newCount = (uint)Mathf.Max(maxSystemCounts[systemName], currentCount);
                    maxSystemCounts[systemName] = newCount;
                    return newCount;
                }
                else
                {
                    maxSystemCounts.Add(systemName, currentCount);
                    return currentCount;
                }
            }
        }

        public static void UpdateAll(ref List<DebugEntry> list, bool deepSearch)
        {
            if (list == null) list = new List<DebugEntry>();


            VisualEffect[] all;
            if(deepSearch)
                all = Resources.FindObjectsOfTypeAll(typeof(VisualEffect)) as VisualEffect[];
            else
                all= Object.FindObjectsOfType(typeof(VisualEffect)) as VisualEffect[];

            foreach (var c in all)
            {
                if (!list.Any(e => e.component == c))
                {
                    list.Add(DebugEntry.Create(c));
                }
            }

            // Cleanup null (destroyed) entries
            list.RemoveAll(o => !o.valid);
        }

        public static void SortByScene(ref List<DebugEntry> list)
        {
            list = list.OrderBy(a => a.sceneName).ToList();
        }
        public static void SortByAsset(ref List<DebugEntry> list)
        {
            list = list.OrderBy(a => a.asset.name).ToList();
        }

        public static void SortByDistanceTo(Vector3 position, ref List<DebugEntry> list, bool ascending = true)
        {
            int d = ascending ? 1 : -1;
            list.Sort((a, b) => Vector3.SqrMagnitude(a.position - position) < Vector3.SqrMagnitude(b.position - position) ? -d : d);
        }

        public static void SortByParticleCount(ref List<DebugEntry> list, bool ascending = true)
        {
            int d = ascending ? 1 : -1;
            list.Sort((a, b) => a.aliveCount < b.aliveCount ? -d : d);
        }

    }

}

