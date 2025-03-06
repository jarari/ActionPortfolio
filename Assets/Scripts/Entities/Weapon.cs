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
    public GameObject casingNode;
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
            if (casingNode != null) {
                Vector3 spread = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.4f, 0.4f), Random.Range(-0.1f, 0.1f));
                Vector3 velocity = (casingNode.transform.forward + casingNode.transform.InverseTransformVector(spread)).normalized * Random.Range(2.5f, 4f) + owner.GetComponent<Rigidbody>().velocity;
                DebrisManager.instance.SpawnTempDebris(casingNode.transform.position, casingNode.transform.rotation, velocity, 5f);
            }
            BulletManager.instance.SpawnBullet(owner, projectileNode.transform.position, (targetPos - projectileNode.transform.position), 100f, true);
        }
    }
}
