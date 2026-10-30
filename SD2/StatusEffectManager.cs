using System.Runtime.CompilerServices;

namespace RpgBattle;

public record TurnStartResult(int DotDamage, bool SkipTurn);

public class StatusEffectManager
{
    public void Apply(Character target, StatusType type, int duration)
    {
        if(type == StatusType.Wet)
        {
            foreach (var node in target.Effects.Nodes().ToList())
                if (node.Value.Type == StatusType.Burning)
                    target.Effects.RemoveNode(node);
        }

        StatusEffect? existing = null;
        foreach ( var e in target.Effects.Items())
        {
            if(e.Type == type) { existing = e; break;}
        }

        if(existing != null)
            existing.Duration = duration;
        else
            target.Effects.AddLast(new StatusEffect(type, duration));

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

        foreach (var node in c.Effects.Nodes().ToList())
            if (node.Value.Duration <= 0)
                c.Effects.RemoveNode(node);

        return new TurnStartResult(dotDamage, skipTurn);
    }

    public static double GetEffectiveSpeed(Character c)
    {
        double speed = c.Agility;
        if(Has(c, StatusType.Haste)) speed *= 1.5;
        if(Has(c, StatusType.Slow)) speed *= 0.5;
        return speed;
    }
}
