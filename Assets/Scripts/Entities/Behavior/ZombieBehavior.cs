using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Character))]
public class ZombieBehavior : MonoBehaviour {
    enum ZombieState {
        Idle,
        Chase,
        Attacking,
        Ragdoll,
        Dead
    }

    public bool IsStateLocked { get; private set; }

    [SerializeField]
    private float _searchDistance = 10f;
    private float _searchPeriod = 0.2f;
    private float _searchTimer = 0f;
    private float _searchAngle = 30f * Mathf.Deg2Rad;

    private float _circlingRadius = 3f;
    private int _circlingDir = 1;
    private float _attackRange = 1.5f;
    private float _attackDelay = 12f;
    private float _nextAttack = 0f;

    Character _character;
    Animator _animator;
    NavMeshAgent _agent;
    Rigidbody _rigidbody;
    Transform _headBone;
    ZombieState _currentState;
    GameObject _chasingTarget;

    private void Awake() {
        _character = GetComponent<Character>();
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody>();
        _headBone = _animator.GetBoneTransform(HumanBodyBones.Head);
        _currentState = ZombieState.Idle;
        _character.OnRagdoll += OnRagdoll;
        _character.OnStandUp += OnStandUp;
        _character.OnDeath += OnDeath;
        _character.OnHit += OnHit;
        _character.OnAttackEnd += OnAttackEnd;
        _nextAttack = Time.time;

        _animator.applyRootMotion = true;
        _agent.updatePosition = false;
        _agent.updateRotation = false;
        _agent.SetDestination(transform.position);
    }

    private void OnAnimatorMove() {
        Vector3 rootPosition = _animator.rootPosition;
        transform.position = _animator.rootPosition;
        transform.rotation = _animator.rootRotation;
        _agent.nextPosition = rootPosition;
    }

    private void Search() {
        var charList = CharacterManager.instance.GetCharactersInRangeByTeam(transform.position, _searchDistance, Character.CharacterTeam.Player);
        if (charList.Count > 0) {
            float minDist = float.MaxValue;
            foreach (var c in charList) {
                float ang = Mathf.Acos(Vector3.Dot(_headBone.forward, (c.GetHips().position - _headBone.position).normalized));
                float dist = c.GetDistanceFromBumper(transform.position);
                if (minDist > dist && ang <= _searchAngle) {
                    minDist = dist;
                    _chasingTarget = c.gameObject;
                }
            }
            if (_chasingTarget != null) {
                _animator.SetTrigger("Scream");
                _currentState = ZombieState.Chase;
            }
        }
    }

    private void IdleBehavior() {
        _searchTimer += Time.deltaTime;
        if (_searchTimer >= _searchPeriod) {
            _searchTimer = 0f;
            Search();
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        if (_agent != null) {
            Gizmos.DrawSphere(_agent.nextPosition, 0.1f);
        }
    }

    private void ChaseBehavior() {
        if (_chasingTarget.CompareTag("Character")) {
            Character targetCharacter = _chasingTarget.GetComponent<Character>();
            _agent.SetDestination(_chasingTarget.transform.position);
            Vector3 diff = (_agent.nextPosition - transform.position);
            diff.y = 0f;
            Vector3 diffNorm = diff.normalized;
            if (Time.time >= _nextAttack) {
                if (targetCharacter.GetDistanceFromBumper(transform.position) > _attackRange) {
                    _character.SetDesiredDirection(diffNorm.x, diffNorm.z);
                    _character.SetSprint(true);
                }
                else {
                    _character.SetDesiredDirection(0, 0);
                    _animator.SetTrigger("Punch");
                    _currentState = ZombieState.Attacking;
                    _nextAttack = Time.time + _attackDelay;
                    if (Random.value < 0.5f) {
                        _circlingDir = -1;
                    }
                    else {
                        _circlingDir = 1;
                    }
                }
            }
            else {
                Vector3 targetPos = _chasingTarget.transform.position - Quaternion.AngleAxis(5f * _circlingDir, Vector3.up) * diffNorm * _circlingRadius;
                _agent.SetDestination(targetPos);
                Vector3 moveDir = (_agent.nextPosition - transform.position).normalized;
                _character.SetDesiredDirection(moveDir.x, moveDir.z);
                _character.SetSprint(false);
            }
            _character.SetDesiredAimTo(targetCharacter.GetHead().position);
        }
    }

    private void AttackBehavior() {

    }

    private void OnRagdoll() {
        _currentState = ZombieState.Ragdoll;
    }

    private void OnStandUp() {
        if (_chasingTarget != null) {
            _currentState = ZombieState.Chase;
        }
        else {
            _currentState = ZombieState.Idle;
        }
    }

    private void OnDeath() {
        _currentState = ZombieState.Dead;
    }

    private void OnHit(Character attacker, DamageType type, float damage) {
        if (_currentState != ZombieState.Chase) {
            _currentState = ZombieState.Chase;
            _chasingTarget = attacker.gameObject;
        }
    }

    private void OnAttackEnd() {
        _currentState = ZombieState.Chase;
    }

    void Update() {
        switch (_currentState) {
            case ZombieState.Idle:
                IdleBehavior();
                break;
            case ZombieState.Chase:
                ChaseBehavior();
                break;
            case ZombieState.Attacking:
                AttackBehavior();
                break;
        }
    }

    public void SetChasingTarget(GameObject target) {
        _chasingTarget = target;
    }
}
