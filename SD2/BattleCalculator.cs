namespace RpgBattle;

public record DamageResult(int Amount, bool IsCrit, double ElementMultiplier);

public class BattleCalculator
{
    private const double DamageScale = 1.5;
    private const double DefenseConstant = 30.0;
    private const double HealScale = 2.5;
    private readonly Random _rng = new();

    public DamageResult CalculateDamage(Character attacker, Skill skill, Character target)
    {
        double stat = skill == SkillCatalog.Basic
            ? attacker.Strength + 0.25 * (attacker.Intelligence + attacker.Agility)
            : (skill.Magic ? attacker.Intelligence : attacker.Strength);

        double damage = stat * skill.Power * DamageScale;
        damage *= DefenseConstant / (DefenseConstant + target.Defense);

        double multiplier = target.GetResistance(skill.Element);
        if (StatusEffectManager.Has(target, StatusType.Wet) && skill.Element == Element.Air)
            multiplier *= 2;  
        damage *= multiplier;

        if (StatusEffectManager.Has(target, StatusType.Guarding))
            damage *= 0.5;

        bool isCrit = false;
        if(skill.Element == Element.None && _rng.NextDouble() < 0.05) 
        { 
            damage *= 2; 
            isCrit = true; 
        }

        return new DamageResult(Math.Max(1, (int)Math.Round(damage)), isCrit, multiplier);
    }

    public int CalculateHeal(Character caster, Skill skill)
    {
        return Math.Max(1, (int)Math.Round(caster.Intelligence * skill.Power * HealScale));
    }
}
