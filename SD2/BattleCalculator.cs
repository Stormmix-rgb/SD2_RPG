namespace RpgBattle;

public record DamageResult(int Amount, bool IsCrit, double ElementMultiplier);

public class BattleCalculator
{
    private readonly Random _rng = new();

    public DamageResult CalculateDamage(Character attacker, Skill skill, Character target)
    {
        double damage = (skill.Magic ? attacker.Intelligence : attacker.Strength) * skill.Power;
        damage = Math.Max(1, damage - target.Defense);

        double multiplier = target.GetResistance(skill.Element);
        
        if (StatusEffectManager.Has(target, StatusType.Wet) && skill.Element == Element.Air)
            multiplier *= 2;
        
        damage *= multiplier;

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
        return Math.Max(1, (int)Math.Round(caster.Intelligence * skill.Power));
    }
}