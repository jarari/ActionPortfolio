using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour {
    public static CharacterManager instance;
    public Character defaultCharacter;
    public CamController characterCamera;

    private Character _currentCharacter;
    public Character PlayerCharacter {
        get {
            return _currentCharacter;
        }
        set {
            _currentCharacter = value;
            InputManager.instance.SetCharacterInControl(value);
            characterCamera.SetCameraTarget(value.gameObject);
        }
    }

    private void Awake() {
        if (instance != null) {
            Destroy(this);
            return;
        }
        instance = this;
    }

    private void Start() {
        PlayerCharacter = defaultCharacter;
    }
}
