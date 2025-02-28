using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public enum CharacterType {
    Action,
    Shooter
}

public struct CharacterStats {
    public float CurrentHP;
    public float MaxHP;
    public float Defense;
}

public class Character : MonoBehaviour
{
    [Serializable]
    public CharacterStats stats;

    [Header("Movements")]
    public float acceleration = 20f;
    public float deceleration = 25f;
    public float maxSpeed = 3f;
    public float maxSprintSpeed = 4.5f;
    public float rotationSpeed = 12f;
    public bool movement360 = false;
    public bool hasDirectionalMovement = true;

    [Header("Tracking")]
    public float trackYawMin = -20f;
    public float trackYawMax = 50f;
    public float trackTime = 0.1f;
    public GameObject aimTarget;
    public GameObject aimBase;

    [Header("Character")]
    public CharacterType charType = CharacterType.Action;

    private float _currentMaxSpeed = 0f;
    private float _desiredSpeed = 0f;
    private Vector3 _desiredDir = Vector3.zero;
    private Vector3 _moveDir = Vector3.zero;

    private Vector3 _previousAimDir = Vector3.forward;
    private Vector3 _actualAimPos = Vector3.zero;

    private bool _isReloading = false;

    Animator _animator;
    Weapon _weapon;

    private void Awake() {
        _animator = GetComponent<Animator>();
        _currentMaxSpeed = maxSpeed;
    }

    private void FixedUpdate() {
        ProcessMove();
    }

    private void ProcessMove() {
        if (_desiredDir.magnitude > 0f) {
            if (_desiredSpeed < _currentMaxSpeed) {
                _desiredSpeed = Mathf.Min(_desiredSpeed + acceleration * Time.deltaTime, _currentMaxSpeed);
            }
            else if (_desiredSpeed > _currentMaxSpeed) {
                _desiredSpeed = Mathf.Max(_desiredSpeed - deceleration * Time.deltaTime, _currentMaxSpeed);
            }
            _moveDir = Vector3.ClampMagnitude(
                _moveDir + _desiredDir * acceleration * Time.deltaTime,
                _desiredSpeed);
        }
        else {
            _desiredSpeed = Mathf.Max(_moveDir.magnitude - deceleration * Time.deltaTime, 0f);
            _moveDir = Vector3.ClampMagnitude(_moveDir, _desiredSpeed);
        }

        float currentSpeed = _moveDir.magnitude;
        if (currentSpeed > 0) {
            if (_animator.GetInteger("State") == 0) {
                _animator.SetInteger("State", 1);
            }
        }
        else {
            if (_animator.GetInteger("State") == 1) {
                _animator.SetInteger("State", 0);
            }
        }
        _animator.SetFloat("Speed", currentSpeed);

        if (!movement360) {
            Vector3 rotatedForward = Vector3.RotateTowards(transform.forward, _moveDir, rotationSpeed * Time.deltaTime, 0f);
            transform.forward = rotatedForward;
        }

        if (hasDirectionalMovement) {
            Vector3 local_moveDir = transform.InverseTransformDirection(_moveDir);
            _animator.SetFloat("Horizontal", local_moveDir.x);
            _animator.SetFloat("Vertical", local_moveDir.z);
        }
    }

    public void SetDesiredDirection(float worldDirX, float worldDirZ) {
        _desiredDir.x = worldDirX;
        _desiredDir.z = worldDirZ;
        _desiredDir = _desiredDir.normalized;
    }

    public void SetAimTo(Vector3 worldPosition) {
        Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
        Vector3 projXZ = Vector3.ProjectOnPlane(localPosition.normalized, Vector3.up);
        float targetYaw = Mathf.Clamp(Vector3.Angle(projXZ, Vector3.forward) * Mathf.Sign(projXZ.x), trackYawMin, trackYawMax);
        float targetPitch = Vector3.Angle(localPosition.normalized, projXZ) * Mathf.Sign(localPosition.y);
        Vector3 rotatedForward = Quaternion.Euler(-targetPitch, targetYaw, 0) * Vector3.forward;
        if (aimTarget != null && aimBase != null) {
            Vector3 localBasePos = transform.InverseTransformPoint(aimBase.transform.position);
            Vector3 targetPos = localBasePos + Vector3.ClampMagnitude(rotatedForward * localPosition.magnitude - localBasePos, 10f);
            aimTarget.transform.localPosition = Vector3.SmoothDamp(aimTarget.transform.localPosition, targetPos, ref _previousAimDir, trackTime);
        }

        if (movement360) {
            Vector3 prevAimPos = aimTarget.transform.position;
            if (aimTarget != null) {
                prevAimPos = aimTarget.transform.position;
            }
            Vector3 characterForward = Vector3.RotateTowards(transform.forward, Vector3.ProjectOnPlane(worldPosition - transform.position, Vector3.up), rotationSpeed * Time.deltaTime, 0f);
            transform.forward = characterForward;
            if (aimTarget != null) {
                aimTarget.transform.position = prevAimPos;
            }
        }
        _actualAimPos = worldPosition;
    }


    public void SetSprint(bool isSprinting) {
        if (isSprinting)
            _currentMaxSpeed = maxSprintSpeed;
        else
            _currentMaxSpeed = maxSpeed;
    }

    public void SetAim(bool isAiming) {
        _animator.SetBool("Aiming", isAiming);
    }

    public void SetFiring(bool isFiring) {
        if (!_isReloading)
            _animator.SetBool("Firing", isFiring);
    }

    public void DoReload() {
        if (!_isReloading)
            _animator.SetTrigger("Reload");
    }

    public void ProcessAnimationEvent(string animEvent) {
        string[] args = animEvent.Split(".");
        if (args[0] == "ReloadStart") {
            SetFiring(false);
            _isReloading = true;
        }
        else if (args[0] == "ReloadEnd") {
            _isReloading = false;
        }
        else if (args[0] == "WeaponFire") {
            if (_weapon != null) {
                _weapon.FireProjectile(_actualAimPos);
            }
        }
    }

    public void EquipWeapon(Weapon wep) {
        _weapon = wep;
    }
}
