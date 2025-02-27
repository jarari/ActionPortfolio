using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public enum WeaponType {
    Common,
    Gun
}

public class Weapon : MonoBehaviour {
    public Character owner;
    public GameObject projectileNode;
    public MuzzleFlash muzzleflash;
    public WeaponType wepType;
    public LayerMask tempLayer;

    private void Awake() {
        if (owner != null)
            owner.EquipWeapon(this);
    }

    public void FireProjectile(Vector3 targetPos) {
        if (wepType == WeaponType.Gun)
            muzzleflash.Play();

        Vector3 rayDir = (targetPos - projectileNode.transform.position).normalized;
        if (Physics.Raycast(projectileNode.transform.position, rayDir, out var hit, 1000f, tempLayer)) {
            ImpactManager.instance.SpawnImpactEffect(hit.point, hit.normal, hit.collider.sharedMaterial);
            DecalManager.instance.SpawnImpactDecal(hit.point - rayDir * 0.5f, rayDir, hit.collider.sharedMaterial, hit.collider.transform);
        }
    }
}
