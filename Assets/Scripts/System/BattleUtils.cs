using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DamageType {
    Bullet,
    Melee,
    Explosion
}

public static class BattleUtils {
    public static float CalculateDamage(Character attacker, Character victim, float damageMult, bool canCrit) {
        float victimDef = (victim.GetFinalStat(StatType.Defense) - attacker.GetFinalStat(StatType.DefFlatPenetration)) * (1f - attacker.GetFinalStat(StatType.DefPercentagePenetration));
        float baseDamage = attacker.GetFinalStat(StatType.Attack) * damageMult;
        if (canCrit && Random.Range(float.Epsilon, 1f) <= attacker.GetFinalStat(StatType.CritChance)) {
            baseDamage *= attacker.GetFinalStat(StatType.CritMult);
        }
        float damageReduction = victimDef / (victimDef + 100f);
        return baseDamage * (1f - damageReduction);
    }

    public static void DoDamage(Character attacker, Character victim, DamageType type, float damage) {
        victim.NotifyOnHit(attacker, type, damage);
        victim.Data.stats.CurrentHP -= damage;
        if (victim.Data.stats.CurrentHP <= 0) {
            victim.Kill();
        }
    }
}
