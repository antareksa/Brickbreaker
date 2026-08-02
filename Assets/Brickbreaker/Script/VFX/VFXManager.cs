using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    // One pool per prefab -- different VFX (normal hit, bomb explosion, etc.) are different
    // prefabs, so they can't share a single pool. Grows lazily: first request for a prefab
    // creates its pool, later requests reuse a free instance or add a new one.
    private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new Dictionary<GameObject, Queue<GameObject>>();

    public Transform GridField;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PlayVFX(GameObject vfxPrefab, Vector3 position)
    {
        PlayVFX(vfxPrefab, position, Quaternion.identity);
    }

    public void PlayVFX(GameObject vfxPrefab, Vector3 position, Quaternion rotation)
    {
        if (vfxPrefab == null) return;

        GameObject instance = GetPooledInstance(vfxPrefab);
        instance.transform.SetPositionAndRotation(position, rotation);
        instance.SetActive(true);

        ParticleSystem particleSystem = instance.GetComponent<ParticleSystem>();
        particleSystem.Play(true);

        instance.transform.SetParent(GridField.transform);

        StartCoroutine(ReturnWhenFinished(vfxPrefab, instance, particleSystem));
    }

    private GameObject GetPooledInstance(GameObject prefab)
    {
        if (!_pools.TryGetValue(prefab, out Queue<GameObject> pool))
        {
            pool = new Queue<GameObject>();
            _pools[prefab] = pool;
        }

        if (pool.Count > 0)
        {
            return pool.Dequeue();
        }

        return Instantiate(prefab, transform);
    }

    // IsAlive(withChildren: true) covers nested particle systems too, so this waits for the whole
    // effect (root + children) to finish before the instance goes back in its pool.
    private IEnumerator ReturnWhenFinished(GameObject prefab, GameObject instance, ParticleSystem particleSystem)
    {
        yield return null;

        while (particleSystem.IsAlive(true))
        {
            yield return null;
        }

        instance.SetActive(false);
        _pools[prefab].Enqueue(instance);
    }
}
