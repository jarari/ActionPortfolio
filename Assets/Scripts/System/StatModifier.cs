using System.Collections;
using System.Collections.Generic;

public enum StatModType {
    Add,
    Multiply,
    Set
}

public class StatModifier {
    public int Id { get; private set; }
    public StatType Target { get; set; }
    public float Value { get; set; }
    public StatModType Type { get; set; }
    public int Priority { get; set; }
    public float Lifetime { get; set; }
    public float Elapsed { get; set; }
    public StatModifier(int _id, StatType _target, float _value, StatModType _type, int _priority, float _lifetime) {
        Id = _id;
        Target = _target;
        Value = _value;
        Type = _type;
        Priority = _priority;
        Lifetime = _lifetime;
        Elapsed = 0f;
    }
}
