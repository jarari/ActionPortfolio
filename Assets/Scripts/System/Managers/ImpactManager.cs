using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.VFX;

public class  VFXAliveCheck : MonoBehaviour {
    public IObjectPool<VisualEffect> pool;

    private VisualEffect _vfx;
    private void Start() {
        _vfx = GetComponent<VisualEffect>();
    }

    private void Update() {
        if (!_vfx.HasAnySystemAwake()) {
            pool.Release(_vfx);
        }
    }
}

public class ImpactManager : MonoBehaviour {
    public static ImpactManager instance;
    [System.Serializable]
    public class Impact {
        public PhysicMaterial material;
        public VisualEffectAsset effectAsset;
    }
    public int maxPoolSize = 1000;
    public List<Impact> impacts = new List<Impact>();

    private Dictionary<PhysicMaterial, VisualEffectAsset> _impactDict = new Dictionary<PhysicMaterial, VisualEffectAsset>();
    IObjectPool<VisualEffect> _pool; 
    public IObjectPool<VisualEffect> Pool {
        get {
            if (_pool == null) {
                _pool = new ObjectPool<VisualEffect>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, true, 10, maxPoolSize);
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

        foreach(var impact in impacts) {
            if (!_impactDict.ContainsKey(impact.material)) {
                _impactDict.Add(impact.material, impact.effectAsset);
            }
        }
    }

    private VisualEffect CreatePooledItem() {
        var go = new GameObject("Pooled VFX");
        var vfx = go.AddComponent<VisualEffect>();
        vfx.Stop();

        var aliveCheck = go.AddComponent<VFXAliveCheck>();
        aliveCheck.pool = Pool;
        return vfx;
    }

    private void OnReturnedToPool(VisualEffect vfx) {
        vfx.gameObject.SetActive(false);
    }

    private void OnTakeFromPool(VisualEffect vfx) {
        vfx.gameObject.SetActive(true);
    }

    private void OnDestroyPoolObject(VisualEffect vfx) {
        Destroy(vfx.gameObject);
    }

    public void SpawnImpactEffect(Vector3 position, Vector3 normal, PhysicMaterial material = null) {
        VisualEffect vfx = Pool.Get();
        vfx.transform.position = position;
        vfx.transform.up = normal;
        if (material != null && _impactDict.TryGetValue(material, out var asset)) {
            vfx.visualEffectAsset = asset;
        }
        else {
            vfx.visualEffectAsset = _impactDict.ElementAt(0).Value;
        }
        vfx.Play();
    }
}
