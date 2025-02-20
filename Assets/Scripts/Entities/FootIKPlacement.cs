using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootIKPlacement : MonoBehaviour {
    [SerializeField] public float rayYOffset = 0;
    [SerializeField] public float rayDistance = 0.15f;
    [SerializeField] public float plantedYOffset = 0.1f;
    [SerializeField] private LayerMask mask;

    private Vector3 rayOrigin;
    private Animator animator;
    public bool ikEnabled = true;
    private void Start() {
        animator = GetComponent<Animator>();
    }

    public void EnableFootIK() {
        ikEnabled = true;
    }

    public void DisableFootIK() {
        ikEnabled = false;
    }

    void SetFootIKTransform(AvatarIKGoal ikGoal) {
        Vector3 footPos = animator.GetIKPosition(ikGoal) + Vector3.down * rayYOffset;
        Debug.DrawRay(footPos, Vector3.down * rayDistance, Color.red);

        if (Physics.Raycast(footPos, Vector3.down, out var hit, rayDistance)) {
            var posFoot = hit.point;
            posFoot.y += plantedYOffset;
            if (footPos.y < posFoot.y) {
                animator.SetIKPosition(ikGoal, posFoot);
                var tarRot = Quaternion.FromToRotation(Vector3.up, hit.normal);
                animator.SetIKRotation(ikGoal, tarRot);
            }
        }
    }

    void OnAnimatorIK(int layerIndex) {
        if (ikEnabled) {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0.5f);

            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0.5f);

            SetFootIKTransform(AvatarIKGoal.LeftFoot);
            SetFootIKTransform(AvatarIKGoal.RightFoot);
        }
        else {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0f);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0f);

            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0f);
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0f);
        }
    }
}
