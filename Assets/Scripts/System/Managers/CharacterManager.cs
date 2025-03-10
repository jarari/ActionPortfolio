using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour {
    public static CharacterManager instance;
    public Character defaultCharacter;
    public CamController characterCamera;

    private Character _currentCharacter;
    private HashSet<Character> _charactersList;
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
        _charactersList = new HashSet<Character>();
    }

    private void Start() {
        PlayerCharacter = defaultCharacter;
    }

    public void RegisterCharacter(Character c) {
        lock (_charactersList) {
            _charactersList.Add(c);
        }
    }

    public void UnregisterCharacter(Character c) {
        lock (_charactersList) {
            _charactersList.Remove(c);
        }
    }

    public List<Character> GetCharacters() {
        List<Character> tempList = new List<Character>();
        lock (_charactersList) {
            foreach (var c in _charactersList) {
                tempList.Add(c);
            }
        }
        return tempList;
    }

    public HashSet<Character> GetCharactersInRangeByTeam(Vector3 pos, float range, Character.CharacterTeam team) {
        var colliders = Physics.OverlapSphere(pos, range, GameSettings.characterMask);
        HashSet<Character> returnList = new HashSet<Character>();
        foreach(var col in colliders) {
            Character c = col.GetComponent<Character>();
            lock (_charactersList) {
                if (c != null 
                    && _charactersList.Contains(c) 
                    && c.team == team 
                    && c.GetDistanceFromBumper(pos) <= range) {
                    returnList.Add(c);
                }
            }
        }
        return returnList;
    }
}
