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
        Dead
    }

    const float alertToIdleTime = 5f;

    [SerializeField]
    private float _searchDistanceIdle = 10f;
    private float _alertTimer = 0f;

    Character _character;
    Animator _animator;
    Rigidbody _rigidbody;
    Transform _headBone;
    ZombieState _currentState;
    Transform _chasingTarget;

    private void Awake() {
        _character = GetComponent<Character>();
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody>();
        _headBone = _animator.GetBoneTransform(HumanBodyBones.Head);
        _currentState = ZombieState.Idle;
        _character.OnRagdoll += OnRagdoll;
        _character.OnStandUp += OnStandUp;
        _character.OnDeath += OnDeath;
        _character.OnHit += OnHit;
    }

    private void Search() {

    }

    private void IdleBehavior() {
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head).position, _searchDistanceIdle);
    }

    private void AlertedBehavior() {
        _alertTimer += Time.deltaTime;
        if (_alertTimer >= alertToIdleTime) {
            _currentState = ZombieState.Idle;
        }
    }

    private void ChaseBehavior() {

    }

    private void OnRagdoll() {
        _currentState = ZombieState.Ragdoll;
    }

    private void OnStandUp() {
        if (_chasingTarget != null) {
            _currentState = ZombieState.Chase;
        }
        else {
            _currentState = ZombieState.Alerted;
        }
    }

    private void OnDeath() {
        _currentState = ZombieState.Dead;
    }

    private void OnHit(Character attacker, DamageType type, float damage) {
        _currentState = ZombieState.Chase;
        _chasingTarget = attacker.transform;
    }

    void Update() {
        switch (_currentState) {
            case ZombieState.Idle:
                IdleBehavior();
                break;
            case ZombieState.Alerted:
                AlertedBehavior();
                break;
            case ZombieState.Chase:
                ChaseBehavior();
                break;
        }
    }
}
