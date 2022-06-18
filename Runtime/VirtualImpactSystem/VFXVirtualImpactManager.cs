using UnityEngine;

public class VFXVirtualImpactManager : MonoBehaviour
{
    const string managerName = "VFXVirtualImpactManager";
    static void CreateManagerInstance()
    {
        var go = Resources.Load<GameObject>(managerName);
        GameObject instance;
        if (go != null)
        {
            instance = Instantiate<GameObject>(go);
            if (!instance.TryGetComponent(out VFXVirtualImpactManager manager))
            {
                Debug.LogWarning("No VFXVirtualImpactManager component found in VFXVirtualImpactManager prefab, creating one...");
                instance.AddComponent<VFXVirtualImpactManager>();
            }
        }
        else // no default prefab
        {
            Debug.LogWarning("No VFXVirtualImpactManager component found in VFXVirtualImpactManager prefab, creating one...");
            instance = new GameObject();
            instance.AddComponent<VFXVirtualImpactManager>();
        }

        instance.name = managerName;
        DontDestroyOnLoad(instance);

    }
}
