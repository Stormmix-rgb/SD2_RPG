namespace RpgBattle;

public record TurnStartResult(int DotDamage, bool SkipTurn, bool SpeedChanged);

public class StatusEffectManager
{
    public void Apply(Character target, StatusType type, int duration)
    {
        if (type == StatusType.Wet) Remove(target, StatusType.Burning);
        if (type == StatusType.Burning) Remove(target, StatusType.Wet);

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

    public void Remove(Character target, StatusType type)
    {
        var node = target.Effects.First;
        while (node != null)
        {
            var next = node.Next;
            if (node.Value.Type == type)
                target.Effects.RemoveNode(node);
            node = next;
        }
    }

    public static bool Has(Character c, StatusType type)
    {
        foreach (var e in c.Effects.Items())
            if  (e.Type == type) return true;
        return false;
    }

    public TurnStartResult StartTurn(Character c)
    {
        int dotDamage = 0;
        bool skipTurn = false;
        bool speedChanged = false;

        if(Has(c, StatusType.Burning)) dotDamage += 5;
        if(Has(c, StatusType.Poisoned)) dotDamage += 5;
        if(Has(c, StatusType.Stunned)) skipTurn = true;

        c.Hp = Math.Max(0, c.Hp - dotDamage);

        var node = c.Effects.First;
        while (node != null)
        {
            var next = node.Next;
            node.Value.Duration--;
            if (node.Value.Duration <= 0)
            {
                if (node.Value.Type == StatusType.Haste || node.Value.Type == StatusType.Slow)
                    speedChanged = true;
                c.Effects.RemoveNode(node);
            }
            node = next;
        }

        return new TurnStartResult(dotDamage, skipTurn, speedChanged);
    }

    public static double GetEffectiveSpeed(Character c)
    {
        double speed = c.Agility;
        if(Has(c, StatusType.Haste)) speed *= 1.5;
        if(Has(c, StatusType.Slow)) speed *= 0.5;
        return speed;
    }
}
