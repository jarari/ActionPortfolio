using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class TempDebris : MonoBehaviour {
    public IObjectPool<TempDebris> pool;
    public float duration = 5f;

    private float elapsed = 0f;

    private void Update() {
        elapsed += Time.deltaTime;
        if (elapsed >= duration) {
            pool.Release(this);
            elapsed = 0f;
        }
    }
}
