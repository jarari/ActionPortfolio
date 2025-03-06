using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BulletManager : MonoBehaviour {
    public static BulletManager instance;
    public int maxPoolSize = 1000;
    public GameObject bulletPrefab;

    IObjectPool<Bullet> _pool;
    public IObjectPool<Bullet> Pool {
        get {
            if (_pool == null) {
                _pool = new ObjectPool<Bullet>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, true, 10, maxPoolSize);
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

    private Bullet CreatePooledItem() {
        var go = Instantiate(bulletPrefab);
        return go.GetComponent<Bullet>();
    }

    private void OnReturnedToPool(Bullet bullet) {
        bullet.gameObject.SetActive(false);
    }

    private void OnTakeFromPool(Bullet bullet) {
        bullet.gameObject.SetActive(true);
    }

    private void OnDestroyPoolObject(Bullet bullet) {
        Destroy(bullet.gameObject);
    }

    public void SpawnBullet(Character attacker, Vector3 position, Vector3 dir, float speed = 50f, bool gravity = false) {
        Bullet b = Pool.Get();
        b.transform.position = position;
        b.transform.forward = dir;
        b.Initialize(attacker, speed, gravity);
    }
}
