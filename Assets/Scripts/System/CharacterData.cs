using UnityEngine;

public enum CharacterType {
    Action,
    Shooter
}

public enum StatType {
    CurrentHP,
    MaxHP,
    Attack,
    Defense,
    CritChance,
    CritMult,
    DefFlatPenetration,
    DefPercentagePenetration
}

[System.Serializable]
public struct CharacterStats {
    public float CurrentHP { get; set; }
    public float MaxHP { get; set; }
    public float Attack { get; set; }
    public float Defense { get; set; }
    public float CritChance { get; set; }
    public float CritMult { get; set; }
    public float DefFlatPenetration { get; set; }
    public float DefPercentagePenetration { get; set; }
}

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CharacterData", order = 1)]
public class CharacterData : ScriptableObject {
    public CharacterStats stats;
    public CharacterType charType;
    public string standUpClip;
    public string standUpFaceDownClip;
}