using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

//TODO: Add dynamic debris creation
public class DebrisManager : MonoBehaviour {
    public static DebrisManager instance;
    public int maxPoolSize = 1000;
    public GameObject tempDebrisPrefab;

    IObjectPool<TempDebris> _pool;
    public IObjectPool<TempDebris> Pool {
        get {
            if (_pool == null) {
                _pool = new ObjectPool<TempDebris>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, true, 10, maxPoolSize);
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
    }

    private TempDebris CreatePooledItem() {
        var go = Instantiate(tempDebrisPrefab);
        var debris = go.GetComponent<TempDebris>();
        debris.pool = Pool;
        return debris;
    }

    private void OnReturnedToPool(TempDebris debris) {
        debris.gameObject.SetActive(false);
    }

    private void OnTakeFromPool(TempDebris debris) {
        debris.gameObject.SetActive(true);
    }

    private void OnDestroyPoolObject(TempDebris debris) {
        Destroy(debris.gameObject);
    }

    public void SpawnTempDebris(Vector3 position, Quaternion rotation, Vector3 velocity, float duration) {
        TempDebris b = Pool.Get();
        b.transform.position = position;
        b.transform.rotation = rotation;
        b.GetComponent<Rigidbody>().velocity = velocity;
        b.duration = duration;
    }
}
