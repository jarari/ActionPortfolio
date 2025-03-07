using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering.Universal;

public class DecalExpireHelper : MonoBehaviour {
    public IObjectPool<DecalProjector> pool;

    private DecalProjector _decal;
    private float elapsed = 0f;
    private void Start() {
        _decal = GetComponent<DecalProjector>();
    }

    private void Update() {
        elapsed += Time.deltaTime;
        if (elapsed >= DecalManager.instance.decalLifetime) {
            pool.Release(_decal);
            elapsed = 0f;
        }
    }
}

public class DecalManager : MonoBehaviour {
    public static DecalManager instance;
    [System.Serializable]
    public class Impact {
        public PhysicMaterial material;
        public Material decalMaterial;
    }
    public int maxPoolSize = 1000;
    public float decalLifetime = 5f;
    public List<Impact> impacts = new List<Impact>();

    private Dictionary<PhysicMaterial, Material> _impactDict = new Dictionary<PhysicMaterial, Material>();
    IObjectPool<DecalProjector> _pool;
    public IObjectPool<DecalProjector> Pool {
        get {
            if (_pool == null) {
                _pool = new ObjectPool<DecalProjector>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, true, 10, maxPoolSize);
            }
            return _pool;
        }
    }

    private void Awake() {
        if (instance != null) {
            Destroy(this);
            return;
        }
        instance = this;

        foreach (var impact in impacts) {
            if (!_impactDict.ContainsKey(impact.material)) {
                _impactDict.Add(impact.material, impact.decalMaterial);
            }
        }
    }

    private DecalProjector CreatePooledItem() {
        var go = new GameObject("Pooled Decal");
        var decal = go.AddComponent<DecalProjector>();
        var helper = go.AddComponent<DecalExpireHelper>();
        helper.pool = Pool;
        return decal;
    }

    private void OnReturnedToPool(DecalProjector decal) {
        decal.gameObject.SetActive(false);
        decal.transform.parent = null;
    }

    private void OnTakeFromPool(DecalProjector decal) {
        decal.gameObject.SetActive(true);
    }

    private void OnDestroyPoolObject(DecalProjector decal) {
        Destroy(decal.gameObject);
    }

    public void SpawnImpactDecal(Vector3 position, Vector3 dir, PhysicMaterial material = null, Transform parent = null) {
        DecalProjector decal = Pool.Get();
        decal.transform.position = position;
        decal.transform.forward = dir;
        decal.size = new Vector3(0.2f, 0.2f, 0.2f);
        decal.pivot = new Vector3(0, 0, 0.1f);
        decal.uvScale = new Vector2(0.2f, 0.2f);
        decal.uvBias = new Vector2(0.4f, 0.4f);
        if (material != null && _impactDict.TryGetValue(material, out var mat)) {
            Material matInstance = new(mat);
            decal.material = matInstance;
        }
        else {
            Material matInstance = new(_impactDict.ElementAt(0).Value);
            decal.material = matInstance;
        }
        if (decal.material.HasVector("_Random_Seed")) {
            decal.material.SetVector("_Random_Seed", position);
        }
        if (parent != null) {
            decal.transform.parent = parent;
        }
        else {
            decal.transform.parent = null;
        }
    }
}

