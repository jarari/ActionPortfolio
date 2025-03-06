using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public static void DoDamage(Character victim, float damage) {
        victim.data.stats.CurrentHP -= damage;
        if (victim.data.stats.CurrentHP <= 0) {
            victim.Kill();
        }
    }
}
