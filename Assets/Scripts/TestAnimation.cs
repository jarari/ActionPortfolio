using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAnimation : MonoBehaviour {
    private class SimpleTransform {
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
    }
    public bool death;
    public bool idle;
    public bool fall;
    public bool punch;
    public bool hit;
    public bool stumble;
    public bool scream;
    public bool run;
    public bool ragdoll;
    public float timeToReset = 1.5f;
    public string standUpClip;
    public string standUpFaceDownClip;
    public GameObject cameraBoom;
    bool runPrev = false;
    bool ragdollPrev = false;
    bool isReset = false;
    float elapsedRagdollReset = 0f;
    Rigidbody[] _ragdollRBodies;
    Collider[] _ragdollColliders;
    Transform[] _boneTransforms;
    SimpleTransform[] _ragdollTransforms;
    SimpleTransform[] _standupTransforms;
    Animator _animator;
    Rigidbody _rigidbody;
    Transform _hipsBone;
    private void Awake() {
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody>();
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
        runPrev = run;
        ragdollPrev = false;
        if (!ragdoll)
            DisableRagdoll();
    }

    private void StoreBoneTransforms(SimpleTransform[] transforms) {
        for(int i = 0; i < _boneTransforms.Length; ++i) {
            transforms[i].Position = _boneTransforms[i].localPosition;
            transforms[i].Rotation = _boneTransforms[i].localRotation;
        }
    }

    private void SampleClipTransforms(string clipName, SimpleTransform[] transforms) {
        Vector3 prevPos = transform.position;
        Quaternion prevRot = transform.rotation;

        foreach(var clip in _animator.runtimeAnimatorController.animationClips) {
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
            StoreBoneTransforms(_ragdollTransforms);
            SampleClipTransforms(standUpClip, _standupTransforms);
            _animator.SetBool("FacingDown", false);
        }
        else {
            AlignGameObjectRotationToHips(false);
            StoreBoneTransforms(_ragdollTransforms);
            SampleClipTransforms(standUpFaceDownClip, _standupTransforms);
            _animator.SetBool("FacingDown", true);
        }
    }

    private void ResetFromRagdollToStandup() {
        elapsedRagdollReset += Time.deltaTime;
        float elapsed = elapsedRagdollReset / timeToReset;

        for (int i = 0; i < _boneTransforms.Length; ++i) {
            _boneTransforms[i].localPosition = Vector3.Lerp(_ragdollTransforms[i].Position, _standupTransforms[i].Position, elapsed);
            _boneTransforms[i].localRotation = Quaternion.Lerp(_ragdollTransforms[i].Rotation, _standupTransforms[i].Rotation, elapsed);
        }

        if (elapsed >= 1f) {
            isReset = false;
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
        Quaternion camRot = Quaternion.identity;
        if (cameraBoom != null)
            camRot = cameraBoom.transform.rotation;

        Vector3 toDir = _hipsBone.up;
        if (isHipReversed)
            toDir *= -1f;
        toDir.y = 0;
        toDir.Normalize();
        transform.rotation *= Quaternion.FromToRotation(transform.forward, toDir);

        _hipsBone.position = hipsPos;
        _hipsBone.rotation = hipsRot;
        if (cameraBoom != null)
            cameraBoom.transform.rotation = camRot;
    }

    public void SetState(int state) {
        _animator.SetInteger("State", state);
    }

    public void EnableRagdoll() {
        _animator.enabled = false;
        _rigidbody.isKinematic = true;
        _animator.Play("Ragdoll");
        SetState(4);
        foreach (var rbody in _ragdollRBodies) {
            rbody.isKinematic = false;
        }
        foreach (var col in _ragdollColliders) {
            col.enabled = true;
        }
        GetComponent<CapsuleCollider>().enabled = false;
        elapsedRagdollReset = 0f;
    }

    public void DisableRagdoll() {
        _animator.enabled = true;
        _rigidbody.isKinematic = false;
        foreach (var rbody in _ragdollRBodies) {
            rbody.isKinematic = true;
        }
        foreach (var col in _ragdollColliders) {
            col.enabled = false;
        }
        GetComponent<CapsuleCollider>().enabled = true;
    }

    private void Update() {
        if (death) {
            death = false;
            _animator.SetTrigger("Death");
        }
        if (idle) {
            idle = false;
            _animator.Play("Idle");
        }
        if (fall) {
            fall = false;
            _animator.Play("Falling");
        }
        if (punch) {
            punch = false;
            _animator.SetInteger("State", 2);
        }
        if (hit) {
            hit = false;
            //_animator.Play("Zombie Reaction Hit");
            _animator.SetTrigger("Hit");
        }
        if (stumble) {
            stumble = false;
            _animator.SetTrigger("Stumble");
        }
        if (scream) {
            scream = false;
            _animator.SetInteger("State", 3);
        }
        if (run) {
            _animator.SetInteger("State", 1);
        }
        else if (runPrev) {
            _animator.SetInteger("State", 0);
        }
        runPrev = run;
        if (ragdoll && !ragdollPrev) {
            EnableRagdoll();
        }
        else if (!ragdoll && ragdollPrev) {
            SelectStandupClip();
            isReset = true;
        }
        if (ragdoll) {
            AlignGameObjectPositionToHips();
        }
        if (isReset) {
            ResetFromRagdollToStandup();
        }
        ragdollPrev = ragdoll;
    }
}
