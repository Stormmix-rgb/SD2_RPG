using System.Reflection.Metadata.Ecma335;

namespace RpgBattle;

public record EnemyChoice(Skill Skill, Character Target);

class EnemyAI
{
    private const double LowHp = 0.3;

    public EnemyChoice Choose(Character actor, IEnumerable<Character> allies, IEnumerable<Character> foes)
    {
        Character? weakFoe = LowestHp(foes);
        if (weakFoe != null && IsLow(weakFoe))
        {
            Skill? attack = BestDamageSkill(actor);
            if (attack != null) return new EnemyChoice(attack, weakFoe);
        }

        Character? weakAlly = LowestHp(allies);
        if (weakAlly != null && IsLow(weakAlly))
        {
            Skill? heal = BestHealSkill(actor);
            if (heal != null) return new EnemyChoice(heal, weakAlly);
        }

        EnemyChoice? weaknessHit = FindWeaknessHit(actor, foes);
        if (weaknessHit != null) return weaknessHit;

        Character? target = LowestHp(foes);
        Skill fallback = BestDamageSkill(actor) ?? SkillCatalog.Basic;
        return new EnemyChoice(fallback, target!);
    }

    private static bool IsLow(Character c) => c.Hp < c.MaxHp * LowHp;

    private static Character? LowestHp(IEnumerable<Character> team)
    {
        Character? best = null;
        foreach (var c in team)
        {
            if (!c.IsAlive) continue;
            if (best == null || c.Hp < best.Hp) best = c;
        }
        return best;
    }

    private static Skill? BestDamageSkill(Character actor)
    {
        Skill? best = null;
        foreach (var s in actor.Skills)
        {
            if (s.Kind != SkillKind.Damage || s.MpCost > actor.Mp) continue;
            if (best == null || s.Power > best.Power) best = s;
        }
        return best;
    }

    private static Skill? BestHealSkill(Character actor)
    {
        foreach (var s in actor.Skills)
        {
            if (s.Kind == SkillKind.Heal && s.MpCost <= actor.Mp) return s;
        }
        return null;
    }

    private static EnemyChoice? FindWeaknessHit(Character actor, IEnumerable<Character> foes)
    {
        EnemyChoice? best = null;
        double bestScore = 0;

         foreach (var foe in foes)
        {
            if (!foe.IsAlive) continue;
            foreach (var s in actor.Skills)
            {
                if (s.Kind != SkillKind.Damage || s.MpCost > actor.Mp) continue;

                double multiplier = foe.GetResistance(s.Element);
                if (StatusEffectManager.Has(foe, StatusType.Wet) && s.Element == Element.Air)
                    multiplier *= 2;

                if (multiplier <= 1) continue;

                double score = multiplier * s.Power;
                if (score > bestScore)
                {
                    bestScore = score;
                    best = new EnemyChoice(s, foe);
                }
            }
        }
        return best;
    }
}
