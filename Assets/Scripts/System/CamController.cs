using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamController : MonoBehaviour {
    [Header("Camera Following")]
    public GameObject camTarget;
    public Vector3 camTargetOffset = new Vector3(0, 1.6f, 0);
    public float camPitchMin = -45f;
    public float camPitchMax = 45f;

    [Header("Camera Boom")]
    public float armLength = 1.4f;
    public float armLengthMin = 0.4f;
    public float armLengthMax = 2.0f;
    public Vector2 armOffsetXY = new Vector2(0.4f, 0);

    [Header("Camera Raycast")]
    public float marginOnHit = 0.05f;
    public LayerMask collisionLayer;

    [Header("Camera Aim")]
    public float aimDistance = 1000f;
    public LayerMask aimLayer;

    private Camera _camera;

    void Awake() {
        _camera = GetComponentInChildren<Camera>();
    }

    void Update() {
        PositionBoomArm();
        PositionCamera();
    }

    private void PositionBoomArm() {
        if (camTarget == null)
            return;

        transform.position = camTarget.transform.position + camTargetOffset;
    }

    private void PositionCamera() {
        Vector3 camTargetPos = new Vector3(armOffsetXY.x, armOffsetXY.y, -armLength);
        Vector3 camFrom = transform.TransformPoint(armOffsetXY);
        RaycastHit hitInfo;
        if (Physics.Raycast(transform.position, (camFrom - transform.position).normalized, out hitInfo, armOffsetXY.magnitude, collisionLayer)) {
            Vector3 diff = hitInfo.point - transform.position;
            camTargetPos = transform.InverseTransformDirection(diff.normalized) * (hitInfo.distance - marginOnHit);
        }
        else if (Physics.Raycast(camFrom, -transform.forward, out hitInfo, armLength, collisionLayer)) {
            Vector3 diff = hitInfo.point - transform.position;
            camTargetPos = transform.InverseTransformDirection(diff.normalized) * (hitInfo.distance - marginOnHit);
        }
        _camera.transform.localPosition = camTargetPos;
    }

    public void RotateCamera(float pitch, float yaw) {
        Vector3 forwardProj = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        float currentPitch = Vector3.Angle(forwardProj, transform.forward) * Mathf.Sign(transform.forward.y);
        float targetPitch = Mathf.Clamp(currentPitch + pitch, camPitchMin, camPitchMax);
        transform.rotation = Quaternion.AngleAxis(yaw, Vector3.up) * transform.rotation * Quaternion.AngleAxis(currentPitch - targetPitch, Vector3.right);
    }

    public Vector3 RaycastForward() {
        Vector3 targetPos = _camera.transform.position + _camera.transform.forward * aimDistance;
        if (Physics.Raycast(_camera.transform.position + _camera.transform.forward * 1f, _camera.transform.forward, out var hit, aimDistance, aimLayer))
            targetPos = hit.point;
        return targetPos;
    }
}
