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

public class Character : MonoBehaviour
{
    [Header("Movements")]
    public float acceleration = 20f;
    public float deceleration = 25f;
    public float maxSpeed = 3f;
    public float maxSprintSpeed = 4.5f;
    public float rotationSpeed = 12f;
    public bool movement360 = false;

    [Header("Tracking")]
    public float trackYawMin = -20f;
    public float trackYawMax = 50f;
    public float trackSpeed = 30f;
    public GameObject aimTarget;

    [Header("Character")]
    public CharacterType charType = CharacterType.Action;

    private float currentMaxSpeed = 0f;
    private float desiredSpeed = 0f;
    private Vector3 desiredDir = Vector3.zero;
    private Vector3 moveDir = Vector3.zero;

    private Vector3 aimDir = Vector3.forward;

    Animator _animator;

    private void Awake() {
        _animator = GetComponent<Animator>();
        currentMaxSpeed = maxSpeed;
    }

    private void FixedUpdate() {
        ProcessMove();
    }

    private void ProcessMove() {
        if (desiredDir.magnitude > 0f) {
            if (desiredSpeed < currentMaxSpeed) {
                desiredSpeed = Mathf.Min(desiredSpeed + acceleration * Time.deltaTime, currentMaxSpeed);
            }
            else if (desiredSpeed > currentMaxSpeed) {
                desiredSpeed = Mathf.Max(desiredSpeed - deceleration * Time.deltaTime, currentMaxSpeed);
            }
            moveDir = Vector3.ClampMagnitude(
                moveDir + desiredDir * acceleration * Time.deltaTime,
                desiredSpeed);
        }
        else {
            desiredSpeed = Mathf.Max(moveDir.magnitude - deceleration * Time.deltaTime, 0f);
            moveDir = Vector3.ClampMagnitude(moveDir, desiredSpeed);
        }

        float currentSpeed = moveDir.magnitude;
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
            Vector3 rotatedForward = Vector3.RotateTowards(transform.forward, moveDir, rotationSpeed * Time.deltaTime, 0f);
            transform.forward = rotatedForward;
        }
        Vector3 localMoveDir = transform.InverseTransformDirection(moveDir);
        _animator.SetFloat("Horizontal", localMoveDir.x);
        _animator.SetFloat("Vertical", localMoveDir.z);
    }

    public void SetDesiredDirection(float worldDirX, float worldDirZ) {
        desiredDir.x = worldDirX;
        desiredDir.z = worldDirZ;
        desiredDir = desiredDir.normalized;
    }

    public void SetAimTo(Vector3 worldPosition) {
        Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
        Vector3 projXZ = Vector3.ProjectOnPlane(localPosition.normalized, Vector3.up);
        float targetYaw = Mathf.Clamp(Vector3.Angle(projXZ, Vector3.forward) * Mathf.Sign(projXZ.x), trackYawMin, trackYawMax);
        float targetPitch = Vector3.Angle(localPosition.normalized, projXZ) * Mathf.Sign(localPosition.y);
        Vector3 rotatedForward = Quaternion.Euler(-targetPitch, targetYaw, 0) * Vector3.forward;
        if (aimTarget != null) {
            Vector3 currentAimTargetDir = aimTarget.transform.localPosition.normalized;
            currentAimTargetDir = Vector3.RotateTowards(currentAimTargetDir, rotatedForward, trackSpeed * Time.deltaTime, 0f);
            aimTarget.transform.localPosition = currentAimTargetDir * localPosition.magnitude;
        }

        if (movement360) {
            Vector3 characterForward = Vector3.RotateTowards(transform.forward, Vector3.ProjectOnPlane(worldPosition - transform.position, Vector3.up), rotationSpeed * Time.deltaTime, 0f);
            transform.forward = characterForward;
        }
    }


    public void SetSprint(bool isSprinting) {
        if (isSprinting)
            currentMaxSpeed = maxSprintSpeed;
        else
            currentMaxSpeed = maxSpeed;
    }

    public void SetAim(bool isAiming) {
        _animator.SetBool("Aiming", isAiming);
    }

    public void SetFiring(bool isFiring) {
        _animator.SetBool("Firing", isFiring);
    }
}
