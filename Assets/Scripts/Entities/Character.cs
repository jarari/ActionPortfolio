using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;


public class Character : MonoBehaviour {
    private class SimpleTransform {
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
    }

    enum CharacterState {
        Idle,
        Moving,
        Ragdoll,
        Resetting,
        WaitingStandUp,
        StandingUp,
        Dead
    }

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
    public float aimRotationSpeed = 36f;
    public GameObject aimTarget;
    public GameObject aimBase;

    [Header("Character")]
    public CharacterData baseData;
    public GameObject bumper;
    public Rig aimRig;

    public CharacterData Data { get; private set; }

    public Action OnRagdoll;
    public Action OnStandUp;
    public Action OnDeath;

    private float _currentMaxSpeed = 0f;
    private float _desiredSpeed = 0f;
    private Vector3 _desiredDir = Vector3.zero;
    private Vector3 _moveDir = Vector3.zero;

    private Vector3 _previousAimDir = Vector3.forward;
    private Vector3 _desiredAimPos = Vector3.zero;
    private float aimRigWeightTarget = 1f;
    private float aimRigWeightChangeTime = 0.5f;

    private bool _isReloading = false;
    private bool _isSprinting = false;

    private float _timeToReset = 0.5f;
    private float _elapsedRagdollReset = 0f;
    private string _currentStandUpClip = "";
    private Rigidbody[] _ragdollRBodies;
    private Collider[] _ragdollColliders;
    private Transform[] _boneTransforms;
    private SimpleTransform[] _ragdollTransforms;
    private SimpleTransform[] _standupTransforms;

    private CharacterState _currentState;

    private Dictionary<int, StatModifier> _statModifiers;

    Animator _animator;
    Weapon _weapon;
    Rigidbody _rigidbody;
    Transform _hipsBone;
    CapsuleCollider _collider;

    private void Awake() {
        _animator = GetComponent<Animator>();
        _currentMaxSpeed = maxSpeed;
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<CapsuleCollider>();
        if (bumper != null) {
            Physics.IgnoreCollision(_collider, bumper.GetComponent<Collider>());
        }
        _hipsBone = _animator.GetBoneTransform(HumanBodyBones.Hips);
        _ragdollRBodies = _hipsBone.GetComponentsInChildren<Rigidbody>();
        _ragdollColliders = _hipsBone.GetComponentsInChildren<Collider>();
        _boneTransforms = _hipsBone.GetComponentsInChildren<Transform>();
        _ragdollTransforms = new SimpleTransform[_boneTransforms.Length];
        _standupTransforms = new SimpleTransform[_boneTransforms.Length];
        for (int i = 0; i < _boneTransforms.Length; ++i) {
            _ragdollTransforms[i] = new SimpleTransform();
            _standupTransforms[i] = new SimpleTransform();
        }
        foreach (var col in _ragdollColliders) {
            Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), col);
        }
        _statModifiers = new Dictionary<int, StatModifier>();
        DisableRagdoll();
        Data = Instantiate(baseData);
    }

    private void StoreBoneTransforms(SimpleTransform[] transforms) {
        for (int i = 0; i < _boneTransforms.Length; ++i) {
            transforms[i].Position = _boneTransforms[i].localPosition;
            transforms[i].Rotation = _boneTransforms[i].localRotation;
        }
    }

    private void ApplyBoneTransforms(SimpleTransform[] transforms) {
        for (int i = 0; i < _boneTransforms.Length; ++i) {
            _boneTransforms[i].localPosition = transforms[i].Position;
            _boneTransforms[i].localRotation = transforms[i].Rotation;
        }
    }

    private void SampleClipTransforms(string clipName, SimpleTransform[] transforms) {
        Vector3 prevPos = transform.position;
        Quaternion prevRot = transform.rotation;

        foreach (var clip in _animator.runtimeAnimatorController.animationClips) {
            if (clip.name == clipName) {
                clip.SampleAnimation(gameObject, 0);
                StoreBoneTransforms(transforms);
                break;
            }
        }

        transform.position = prevPos;
        transform.rotation = prevRot;
    }

    private void SelectStandupClip() {
        float dot = Vector3.Dot(_hipsBone.forward, Vector3.down);
        if (dot < 0) {
            AlignGameObjectRotationToHips(true);
            _currentStandUpClip = Data.standUpClip;
            _animator.SetBool("FacingDown", false);
        }
        else {
            AlignGameObjectRotationToHips(false);
            _currentStandUpClip = Data.standUpFaceDownClip;
            _animator.SetBool("FacingDown", true);
        }
        StoreBoneTransforms(_ragdollTransforms);
        SampleClipTransforms(_currentStandUpClip, _standupTransforms);
        ApplyBoneTransforms(_ragdollTransforms);
    }

    private void ResetFromRagdollToStandup() {
        _elapsedRagdollReset += Time.deltaTime;
        float elapsed = _elapsedRagdollReset / _timeToReset;

        for (int i = 0; i < _boneTransforms.Length; ++i) {
            _boneTransforms[i].localPosition = Vector3.Lerp(_ragdollTransforms[i].Position, _standupTransforms[i].Position, elapsed);
            _boneTransforms[i].localRotation = Quaternion.Lerp(_ragdollTransforms[i].Rotation, _standupTransforms[i].Rotation, elapsed);
        }

        if (elapsed >= 1f) {
            ApplyBoneTransforms(_standupTransforms);
            DisableRagdoll();
        }
    }

    private void AlignGameObjectPositionToHips() {
        Vector3 hipsPos = _hipsBone.position;

        transform.position = hipsPos;
        if (Physics.Raycast(transform.position, Vector3.down, out var hit)) {
            transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
        }

        _hipsBone.position = hipsPos;
    }

    private void AlignGameObjectRotationToHips(bool isHipReversed) {
        Vector3 hipsPos = _hipsBone.position;
        Quaternion hipsRot = _hipsBone.rotation;

        Vector3 toDir = _hipsBone.up;
        if (isHipReversed)
            toDir *= -1f;
        toDir.y = 0;
        toDir.Normalize();
        transform.rotation *= Quaternion.FromToRotation(transform.forward, toDir);

        _hipsBone.position = hipsPos;
        _hipsBone.rotation = hipsRot;
    }

    private void UpdateStatModifiers() {
        List<int> markedForRemoval = new List<int>();
        foreach (var kvp in _statModifiers) {
            if (kvp.Value.Lifetime != -1) {
                kvp.Value.Elapsed += Time.deltaTime;
                if (kvp.Value.Elapsed >= kvp.Value.Lifetime) {
                    markedForRemoval.Add(kvp.Key);
                }
            }
        }
        foreach(int key in markedForRemoval) {
            _statModifiers.Remove(key);
        }
    }

    private void Update() {
        UpdateStatModifiers();
        if (_currentState == CharacterState.Ragdoll) {
            AlignGameObjectPositionToHips();
        }
        else if (_currentState == CharacterState.Resetting) {
            ResetFromRagdollToStandup();
        }
        else if (_currentState == CharacterState.WaitingStandUp && _animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == _currentStandUpClip) {
            _currentState = CharacterState.StandingUp;
        }
        else if (_currentState == CharacterState.StandingUp) {
            if (!_animator.IsInTransition(0) && _animator.GetCurrentAnimatorClipInfo(0)[0].clip.name != _currentStandUpClip) {
                _currentState = CharacterState.Idle;
                SetAimRigWeightTarget(1f);
                OnStandUp.Invoke();
            }
        }
        else if (_currentState != CharacterState.Dead) {
            ProcessAim();
        }
        if (aimRig.weight != aimRigWeightTarget) {
            float step = Mathf.Sign(aimRigWeightTarget - aimRig.weight) * Time.deltaTime / aimRigWeightChangeTime;
            if (step > 0) {
                aimRig.weight = Mathf.Min(aimRig.weight + step, aimRigWeightTarget);
            }
            else {
                aimRig.weight = Mathf.Max(aimRig.weight + step, aimRigWeightTarget);
            }
        }
    }

    private void FixedUpdate() {
        if (_currentState == CharacterState.Moving) {
            ProcessMove();
        }
        else if (_currentState == CharacterState.Idle) {
            if (_desiredDir.magnitude > 0f) {
                _currentState = CharacterState.Moving;
            }
        }
    }

    private void ProcessMove() {
        if (!IsAiming()) {
            if (_isSprinting)
                _currentMaxSpeed = maxSprintSpeed;
            else
                _currentMaxSpeed = maxSpeed;
        }
        else {
            _currentMaxSpeed = maxSpeed * 0.8f;
        }

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
                _currentState = CharacterState.Idle;
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

    private void ProcessAim() {
        Vector3 localPosition = transform.InverseTransformPoint(_desiredAimPos);
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
            Vector3 prevAimPos = Vector3.zero;
            if (aimTarget != null) {
                prevAimPos = aimTarget.transform.position;
            }
            Vector3 characterForward = Vector3.RotateTowards(transform.forward, Vector3.ProjectOnPlane(_desiredAimPos - transform.position, Vector3.up), aimRotationSpeed * Time.deltaTime, 0f);
            transform.forward = characterForward;
            if (aimTarget != null) {
                aimTarget.transform.position = prevAimPos;
            }
        }
    }

    public void SetDesiredDirection(float worldDirX, float worldDirZ) {
        _desiredDir.x = worldDirX;
        _desiredDir.z = worldDirZ;
        _desiredDir = _desiredDir.normalized;
    }

    public void SetDesiredAimTo(Vector3 worldPosition) {
        _desiredAimPos = worldPosition;
    }


    public void SetSprint(bool isSprinting) {
        _isSprinting = isSprinting;
    }

    public bool IsAiming() {
        return _animator.GetBool("Aiming");
    }

    public void SetAim(bool isAiming) {
        _animator.SetBool("Aiming", isAiming);
    }

    public bool IsFiring() {
        return _animator.GetBool("Firing");
    }

    public void SetFiring(bool isFiring) {
        if (!_isReloading)
            _animator.SetBool("Firing", isFiring);
    }

    public bool IsReloading() {
        return _isReloading;
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
                _weapon.FireProjectile(_desiredAimPos);
            }
        }
    }

    public void EquipWeapon(Weapon wep) {
        _weapon = wep;
        wep.owner = this;
    }

    public void EnableRagdoll() {
        _currentState = CharacterState.Ragdoll;
        _animator.enabled = false;
        _rigidbody.isKinematic = true;
        _animator.Play("Ragdoll");
        foreach (var rbody in _ragdollRBodies) {
            rbody.isKinematic = false;
        }
        _collider.enabled = false;
        if (bumper != null) {
            bumper.SetActive(false);
        }
        _elapsedRagdollReset = 0f;
        SetAimRigWeight(0f);
        OnRagdoll?.Invoke();
    }

    public void StandUpFromRagdoll() {
        SelectStandupClip();
        _currentState = CharacterState.Resetting;
    }

    public void DisableRagdoll() {
        _animator.enabled = true;
        _rigidbody.isKinematic = false;
        foreach (var rbody in _ragdollRBodies) {
            rbody.isKinematic = true;
        }
        _collider.enabled = true;
        if (bumper != null) {
            bumper.SetActive(true);
        }
        if (_currentState == CharacterState.Resetting) {
            _currentState = CharacterState.WaitingStandUp;
            _animator.Play(_currentStandUpClip);
        }
        else {
            _currentState = CharacterState.Idle;
            OnStandUp.Invoke();
        }
    }

    public void SetAimRigWeight(float weight) {
        if (aimRig != null) {
            aimRig.weight = weight;
            aimRigWeightTarget = weight;
        }
    }

    public void SetAimRigWeightTarget(float weight) {
        if (aimRig != null) {
            aimRigWeightTarget = weight;
        }
    }

    public void AddStatModifier(string id, StatType target, float value, StatModType type, int priority = 50, float lifetime = -1) {
        int _id = id.GetHashCode();
        if (!_statModifiers.ContainsKey(_id)) {
            StatModifier statMod = new StatModifier(_id, target, value, type, priority, lifetime);
            _statModifiers.Add(_id, statMod);
        }
    }

    public void RemoveStatModifier(string id) {
        int key = id.GetHashCode();
        _statModifiers.Remove(key);
    }

    public float GetBaseStat(StatType type) {
        switch (type) {
            case StatType.CurrentHP:
                return Data.stats.CurrentHP;
            case StatType.MaxHP:
                return Data.stats.MaxHP;
            case StatType.Attack:
                return Data.stats.Attack;
            case StatType.Defense:
                return Data.stats.Defense;
            case StatType.CritChance:
                return Data.stats.CritChance;
            case StatType.CritMult:
                return Data.stats.CritMult;
            case StatType.DefFlatPenetration:
                return Data.stats.DefFlatPenetration;
            case StatType.DefPercentagePenetration:
                return Data.stats.DefPercentagePenetration;
        }
        throw (new NotImplementedException());
    }

    public float GetFinalStat(StatType type) {
        float addModifier = 0f;
        float multModifier = 1f;
        float setModifier = 0f;
        bool hasSet = false;
        int maxPriority = int.MinValue;
        foreach (var kvp in _statModifiers) {
            if (kvp.Value.Target == type) {
                switch (kvp.Value.Type) {
                    case StatModType.Add:
                        addModifier += kvp.Value.Value;
                        break;
                    case StatModType.Multiply:
                        multModifier += kvp.Value.Value;
                        break;
                    case StatModType.Set:
                        if (kvp.Value.Priority >= maxPriority) {
                            hasSet = true;
                            maxPriority = kvp.Value.Priority;
                            setModifier = kvp.Value.Value;
                        }
                        break;
                }
            }
        }
        if (hasSet) {
            return setModifier;
        }
        return (GetBaseStat(type) + addModifier) * multModifier;
    }

    public void Kill() {
        if (_currentState == CharacterState.Dead)
            return;
        _currentState = CharacterState.Dead;
        if (UnityEngine.Random.Range(float.Epsilon, 1f) < 0.5f) {
            _animator.SetTrigger("Death");
        }
        else {
            EnableRagdoll();
        }
        for (int i = 1; i < _animator.layerCount; ++i) {
            _animator.SetLayerWeight(i, 0);
        }
        SetAimRigWeight(0f);
        OnDeath?.Invoke();
    }

    public bool IsDead() {
        return _currentState == CharacterState.Dead;
    }
}
