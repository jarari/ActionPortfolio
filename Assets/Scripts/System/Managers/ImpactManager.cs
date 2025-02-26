using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.VFX;

public class ImpactManager : MonoBehaviour {
    public static ImpactManager instance;
    [System.Serializable]
    public class Impact {
        public PhysicMaterial material;
        public VisualEffectAsset effectAsset;
    }
    public int maxPoolSize = 1000;
    public List<Impact> bulletImpacts = new List<Impact>();
    public List<Impact> meleeImpacts = new List<Impact>();

    private Dictionary<PhysicMaterial, VisualEffectAsset> _bulletImpactDict = new Dictionary<PhysicMaterial, VisualEffectAsset>();
    private Dictionary<PhysicMaterial, VisualEffectAsset> _meleeImpactDict = new Dictionary<PhysicMaterial, VisualEffectAsset>();
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

        foreach(var impact in bulletImpacts) {
            if (!_bulletImpactDict.ContainsKey(impact.material)) {
                _bulletImpactDict.Add(impact.material, impact.effectAsset);
            }
        }

        foreach (var impact in meleeImpacts) {
            if (!_meleeImpactDict.ContainsKey(impact.material)) {
                _meleeImpactDict.Add(impact.material, impact.effectAsset);
            }
        }
    }

    private VisualEffect CreatePooledItem() {
        var go = new GameObject("Pooled VFX");
        var vfx = go.AddComponent<VisualEffect>();
        vfx.Stop();
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

    }
}
