using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Character))]
public class ZombieBehavior : MonoBehaviour {
    enum ZombieState {
        Idle,
        Alerted,
        Chase,
        Ragdoll,
        StandingUp,
        Dead
    }

    Character _character;
    Animator _animator;
    Rigidbody _rigidbody;
    ZombieState _currentState;
    Transform _chasingTarget;

    private void Awake() {
        _character = GetComponent<Character>();
        _animator = GetComponent<Animator>();
        _currentState = ZombieState.Idle;
    }

    private void Start() {
    }

    private void StandUp() {
        _character.StandUpFromRagdoll();
    }

    private void IdleBehavior() {

    }

    void Update() {
        switch (_currentState) {
            case ZombieState.Idle:
                IdleBehavior();
                break;
        }
    }
}
