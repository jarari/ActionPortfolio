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
    [field: SerializeField]
    public float CurrentHP { get; set; }
    [field: SerializeField]
    public float MaxHP { get; set; }
    [field: SerializeField]
    public float Attack { get; set; }
    [field: SerializeField]
    public float Defense { get; set; }
    [field: SerializeField]
    public float CritChance { get; set; }
    [field: SerializeField]
    public float CritMult { get; set; }
    [field: SerializeField]
    public float DefFlatPenetration { get; set; }
    [field: SerializeField]
    public float DefPercentagePenetration { get; set; }
}

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CharacterData", order = 1)]
public class CharacterData : ScriptableObject {
    public CharacterStats stats;
    public CharacterType charType;
    public string standUpClip;
    public string standUpFaceDownClip;
}