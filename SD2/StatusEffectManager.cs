using System.Runtime.CompilerServices;

namespace RpgBattle;

public record TurnStartResult(int DotDamage, bool SkipTurn);

public class StatusEffectManager
{
    public void Apply(Character target, StatusType type, int duration)
    {
        if(type == StatusType.Wet) 
            target.Effects.RemoveAll(e => e.Type == StatusType.Burning);

        var existing = target.Effects.Items().FirstOrDefault(e => e.Type == type);
        
        if(existing != null)
        {
            existing.Duration = duration;
        }
        else
        {
            target.Effects.AddLast(new StatusEffect(type, duration));
        }

    }

    public static bool Has(Character c, StatusType type)
    {
        return c.Effects.Items().Any(e => e.Type == type);
    }

    public TurnStartResult StartTurn(Character c)
    {
        int dotDamage = 0;
        bool skipTurn = false;

        if(Has(c, StatusType.Burning)) dotDamage += 5;
        if(Has(c, StatusType.Poisoned)) dotDamage += 5;
        if(Has(c, StatusType.Stunned)) skipTurn = true;

        c.Hp = Math.Max(0, c.Hp - dotDamage);

        foreach (var effect in c.Effects.Items())
            effect.Duration--;

        c.Effects.RemoveAll(e => e.Duration <= 0);

        return new TurnStartResult(dotDamage, skipTurn);
    }

    public double GetEffectiveSpeed(Character c)
    {
        double speed = c.Agility;
        if(Has(c, StatusType.Haste)) speed *= 1.5;
        if(Has(c, StatusType.Slow)) speed *= 0.5;
        return speed;
    }
}