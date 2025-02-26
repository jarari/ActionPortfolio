using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType {
    Common,
    Gun
}

public class Weapon : MonoBehaviour {
    public Character owner;
    public MuzzleFlash muzzleflash;
    public WeaponType wepType;

    private void Awake() {
        if (owner != null)
            owner.EquipWeapon(this);
    }

    public void FireProjectile() {
        if (wepType == WeaponType.Gun)
            muzzleflash.Play();
    }
}
