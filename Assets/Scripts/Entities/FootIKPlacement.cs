using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootIKPlacement : MonoBehaviour {
    [SerializeField] public float rayYOffset = 0;
    [SerializeField] public float rayDistance = 0.15f;
    [SerializeField] public float plantedYOffset = 0.1f;
    [SerializeField] private LayerMask mask;

    private Animator _animator;
    public bool ikEnabled = true;
    private void Start() {
        _animator = GetComponent<Animator>();
    }

    public void EnableFootIK() {
        ikEnabled = true;
    }

    public void DisableFootIK() {
        ikEnabled = false;
    }

    void SetFootIKTransform(AvatarIKGoal ikGoal) {
        Vector3 footPos = _animator.GetIKPosition(ikGoal) + Vector3.down * rayYOffset;
        Debug.DrawRay(footPos, Vector3.down * rayDistance, Color.red);

        if (Physics.Raycast(footPos, Vector3.down, out var hit, rayDistance)) {
            var posFoot = hit.point;
            posFoot.y += plantedYOffset;
            if (footPos.y < posFoot.y) {
                _animator.SetIKPosition(ikGoal, posFoot);
                var tarRot = Quaternion.FromToRotation(Vector3.up, hit.normal);
                var curRot = _animator.GetIKRotation(ikGoal);
                _animator.SetIKRotation(ikGoal, Quaternion.RotateTowards(curRot, tarRot, 15f * Time.deltaTime));
            }
        }
    }

    void OnAnimatorIK(int layerIndex) {
        if (ikEnabled) {
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
            _animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0.5f);

            _animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
            _animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0.5f);

            SetFootIKTransform(AvatarIKGoal.LeftFoot);
            SetFootIKTransform(AvatarIKGoal.RightFoot);
        }
        else {
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0f);
            _animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0f);

            _animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0f);
            _animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0f);
        }
    }
}
