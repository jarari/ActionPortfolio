using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterType {
    Action,
    Shooter
}

[System.Serializable]
public struct CharacterStats {
    public float CurrentHP;
    public float MaxHP;
    public float Attack;
    public float Defense;
}

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CharacterData", order = 1)]
public class CharacterData : ScriptableObject {
    public CharacterStats stats;
    public CharacterType charType;
}