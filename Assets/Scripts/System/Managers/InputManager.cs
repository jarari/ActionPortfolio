using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour {
    public static InputManager instance;
    public PlayerActions playerActions;
    public Character characterInControl;
    public CamController camController;

    private void Awake() {
        if (instance != null) {
            Destroy(this);
            return;
        }
        playerActions = new PlayerActions();
    }

    private void OnEnable() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        playerActions.Battle.Enable();
    }

    private void OnDisable() {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        playerActions.Battle.Disable();
    }

    private void Update() {
        if (characterInControl != null) {
            characterInControl.SetSprint(playerActions.Battle.Sprint.IsPressed());

            if (characterInControl.data.charType == CharacterType.Shooter) {
                if (playerActions.Battle.Aim.IsPressed() || playerActions.Battle.Fire.IsPressed()) {
                    characterInControl.movement360 = true;
                }
                else {
                    characterInControl.movement360 = false;
                }
                characterInControl.SetAim(playerActions.Battle.Aim.IsPressed());
                characterInControl.SetFiring(playerActions.Battle.Fire.IsPressed());
            }

            Vector2 moveInput = playerActions.Battle.Move.ReadValue<Vector2>();
            Vector3 camForward = Camera.main.transform.forward;
            camForward.y = 0f;
            camForward = camForward.normalized;
            Vector3 camRight = Camera.main.transform.right;
            camRight.y = 0f;
            camRight = camRight.normalized;
            Vector3 wantDir = (camForward * moveInput.y + camRight * moveInput.x).normalized;
            characterInControl.SetDesiredDirection(wantDir.x, wantDir.z);

            Vector2 lookInput = playerActions.Battle.Look.ReadValue<Vector2>() * GameSettings.mouseSensitivity;
            camController.RotateCamera(lookInput.y, lookInput.x);

            Vector3 aimPos = camController.RaycastForward();
            characterInControl.SetAimTo(aimPos);
        }
    }

    private void OnReload(InputValue val) {
        if (characterInControl != null) {
            characterInControl.DoReload();
        }
    }
}
