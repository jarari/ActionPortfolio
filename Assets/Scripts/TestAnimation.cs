using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAnimation : MonoBehaviour {
    public bool death;
    public bool idle;
    public bool fall;
    public bool punch;
    public bool hit;
    public bool stumble;
    public bool scream;
    public bool run;
    public bool ragdoll;
    bool runPrev = false;
    bool ragdollPrev = false;
    Animator animator;
    Rigidbody rigidbody;
    Transform hipsBone;
    private void Awake() {
        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody>();
        hipsBone = animator.GetBoneTransform(HumanBodyBones.Hips);
        runPrev = run;
        ragdollPrev = false;
        DisableRagdoll();
    }

    public void SetState(int state) {
        animator.SetInteger("State", state);
    }

    public void EnableRagdoll() {
        animator.enabled = false;
        rigidbody.isKinematic = true;
        animator.Play("Ragdoll");
        SetState(4);
        foreach (var col in hipsBone.GetComponentsInChildren<Collider>()) {
            col.enabled = true;
        }
        foreach (var rbody in hipsBone.GetComponentsInChildren<Rigidbody>()) {
            rbody.velocity = Vector3.zero;
        }
        GetComponent<CapsuleCollider>().enabled = false;
    }

    public void DisableRagdoll() {
        animator.enabled = true;
        rigidbody.isKinematic = false;
        foreach (var col in hipsBone.GetComponentsInChildren<Collider>()) {
            col.enabled = false;
        }
        GetComponent<CapsuleCollider>().enabled = true;
        if (ragdollPrev != ragdoll) {
            Vector3 hipsPos = hipsBone.position;
            transform.position = hipsPos;
            if (Physics.Raycast(transform.position, Vector3.down, out var hit)) {
                transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
            }
            hipsBone.position = hipsPos;
        }
    }

    private void Update() {
        animator.SetBool("Death", death);
        if (idle) {
            idle = false;
            animator.Play("Idle");
        }
        if (fall) {
            fall = false;
            animator.Play("Falling");
        }
        if (punch) {
            punch = false;
            animator.SetInteger("State", 2);
        }
        if (!hit && animator.GetCurrentAnimatorStateInfo(0).IsName("Zombie Reaction Hit")) {
            animator.SetBool("Hit", false);
        }
        if (hit) {
            hit = false;
            //animator.Play("Zombie Reaction Hit");
            animator.SetBool("Hit", true);
        }
        animator.SetBool("Stumble", stumble);
        if (scream) {
            scream = false;
            animator.SetInteger("State", 3);
        }
        if (run) {
            animator.SetInteger("State", 1);
        }
        else if (runPrev) {
            animator.SetInteger("State", 0);
        }
        runPrev = run;
        if (ragdoll && !ragdollPrev) {
            EnableRagdoll();
        }
        else if (!ragdoll && ragdollPrev) {
            DisableRagdoll();
        }
        ragdollPrev = ragdoll;
    }
}
