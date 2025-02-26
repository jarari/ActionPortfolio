using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour {
    public static BulletManager instance;
    private void Awake() {
        if (instance != null) {
            Destroy(this);
            return;
        }
        instance = this;
    }
}
