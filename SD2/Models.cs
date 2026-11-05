using System.Text.Json.Serialization;

namespace RpgBattle;

public enum Element { None, Fire, Earth, Air, Water }
public enum StatusType { Burning, Poisoned, Wet, Stunned, Haste, Slow, Guarding }
public enum SkillKind { Damage, Heal, Buff }

public record Skill(string Name, SkillKind Kind, Element Element, double Power, int MpCost,
                    bool Magic, StatusType? Status = null, int Duration = 0);

public class StatusEffect
{
    public StatusType Type { get; }
    public int Duration { get; set; }
    public StatusEffect(StatusType type, int duration) { Type = type; Duration = duration; }
}

public static class SkillCatalog
{
    public static readonly Skill Basic = new("Attack", SkillKind.Damage, Element.None, 1.0, 0, false);
    public static readonly Dictionary<string, Skill> All = new Skill[]
    { 
        new("HeroicStrike",   SkillKind.Damage, Element.None,  1.6, 5,  false),
        new("ThrowingKnife",  SkillKind.Damage, Element.None,  1.6, 5,  false),
        new("ShieldBash",     SkillKind.Damage, Element.None,  1.3, 5,  false, StatusType.Stunned, 1),
        new("FlameStrike",    SkillKind.Damage, Element.Fire,  2.0, 8,  false, StatusType.Burning, 3),
        new("PiercingArrow",  SkillKind.Damage, Element.None,  1.8, 4,  false),
        new("PoisonTip",      SkillKind.Damage, Element.None,  1.5, 6,  false, StatusType.Poisoned, 3),
        new("PoisonDart",     SkillKind.Damage, Element.None,  1.0, 6,  false, StatusType.Poisoned, 3),
        new("Chloroform",     SkillKind.Damage, Element.None,  0.0, 12,  false, StatusType.Stunned, 2),
        new("WindGust",       SkillKind.Buff,   Element.Air,   0.0, 6,  false, StatusType.Haste, 3),
        new("StoneSpike",     SkillKind.Damage, Element.Earth, 2.0, 8,  true),
        new("PoisonCloud",    SkillKind.Damage, Element.Earth, 0.9, 10, true,  StatusType.Poisoned, 3),
        new("MendingEarth",   SkillKind.Heal,   Element.Earth, 1.0, 10, true),
        new("WaterBolt",      SkillKind.Damage, Element.Water, 1.8, 8,  true,  StatusType.Wet, 2),
        new("MendingWaters",  SkillKind.Heal,   Element.Water, 1.0, 10, true),
        new("CrushingBlow",   SkillKind.Damage, Element.None,  2.2, 6,  false, StatusType.Stunned, 1),
        new("LightningStrike",SkillKind.Damage, Element.Air,   2.2, 10, true),
        new("TimeWarp",       SkillKind.Damage, Element.Air,   0.8, 8,  true,  StatusType.Slow, 3),
    }.ToDictionary(s => s.Name);
}

public abstract class Character
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Class { get; set; } = "";

    [JsonPropertyName("hp")] public int MaxHp { get; set; }
    [JsonPropertyName("mp")] public int MaxMp { get; set; }
    public int Strength { get; set; }
    public int Defense { get; set; }
    public int Intelligence { get; set; }
    public int Agility { get; set; }

    public Dictionary<string, double> Resistances { get; set; } = new();
    [JsonPropertyName("skills")] public List<string> SkillNames { get; set; } = new();

    [JsonIgnore] public int Hp { get; set; }
    [JsonIgnore] public int Mp { get; set; }
    [JsonIgnore] public List<Skill> Skills { get; private set; } = new();
    [JsonIgnore] public SLL<StatusEffect> Effects { get; private set; } = new();
    [JsonIgnore] public bool IsAlive => Hp > 0;

    public void Init()
    {
        Hp = MaxHp;
        Mp = MaxMp;
        Skills = SkillNames.Select(n => SkillCatalog.All[n]).ToList();
        Effects = new SLL<StatusEffect>();
    }

    public double GetResistance(Element e) =>
        Resistances.TryGetValue(e.ToString(), out var r) ? r : 1.0;
}

public class PlayerCharacter : Character { }

public class EnemyCharacter : Character
{
    public EnemyCharacter Clone()
    {
        var copy = (EnemyCharacter)MemberwiseClone();
        copy.Init();
        return copy;
    }
}
