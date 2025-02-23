using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class Character : MonoBehaviour
{
    public float acceleration = 20f;
    public float deceleration = 25f;
    public float maxSpeed = 3f;
    public float maxSprintSpeed = 4.5f;
    public float rotationSpeed = 12f;
    public bool movement360 = false;
    public PlayerActions playerActions;

    [SerializeField]
    private float currentMaxSpeed = 0f;
    [SerializeField]
    private float desiredSpeed = 0f;
    [SerializeField]
    private Vector3 desiredDir = Vector3.zero;
    [SerializeField]
    private Vector3 moveDir = Vector3.zero;


    Animator _animator;

    private void Awake() {
        _animator = GetComponent<Animator>();
        currentMaxSpeed = maxSpeed;
        playerActions = new PlayerActions();
    }

    void Update() {
        if (playerActions.Battle.Sprint.IsPressed()) {
            currentMaxSpeed = maxSprintSpeed;
        }
        else {
            currentMaxSpeed = maxSpeed;
        }
        Vector2 moveInput = playerActions.Battle.Move.ReadValue<Vector2>();
        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0f;
        camForward = camForward.normalized;
        Vector3 camRight = Camera.main.transform.right;
        camRight.y = 0f;
        camRight = camRight.normalized;
        Vector3 wantDir = (camForward * moveInput.y + camRight * moveInput.x).normalized;
        SetDesiredDirection(wantDir.x, wantDir.z);
        ProcessMove();
    }

    private void OnEnable() {
        playerActions.Battle.Enable();
    }

    private void OnDisable() {
        playerActions.Battle.Disable();
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
}
