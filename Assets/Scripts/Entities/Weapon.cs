using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType {
    Common,
    Gun
}

public class Weapon : MonoBehaviour {
    public Character owner;
    public GameObject projectileNode;
    public MuzzleFlash muzzleflash;
    public ParticleSystem casing;
    public WeaponType wepType;
    public LayerMask tempLayer;

    private void Awake() {
        if (owner != null)
            owner.EquipWeapon(this);
    }

    public void FireProjectile(Vector3 targetPos) {
        if (wepType == WeaponType.Gun) {
            if (muzzleflash != null) {
                muzzleflash.Play();
            }
            if (casing != null) {
                casing.Emit(1);
            }
            BulletManager.instance.SpawnBullet(projectileNode.transform.position, (targetPos - projectileNode.transform.position), 100f, true);
        }
    }
}
