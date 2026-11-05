namespace RpgBattle;

public class ConsoleUI
{
    private readonly List<string> log = new();

    public void Log(string message)
    {
        log.Add(message);
        if (log.Count > 8) log.RemoveAt(0);
    }

    public void DrawBattle(List<Character> players, List<Character> enemies, 
                            Character active, List<Character> upcoming,
                            int round, string mode)

    {
        Console.Clear();
        Console.WriteLine($"=== Turn {round} · {mode} ===\n");
        Console.WriteLine($"Now: {active.Name}");
        Console.WriteLine("Next: " + (upcoming.Count == 0
            ? "(new round)"
            : string.Join(" -> ", upcoming.Select(c => c.Name))));
        Console.WriteLine();

        Console.WriteLine("Players:");
        foreach (var p in players) PrintCharacter(p, p == active);
        
        Console.WriteLine("\nEnemies:");
        foreach (var e in enemies) PrintCharacter(e, e == active);

        Console.WriteLine();
        Console.WriteLine($"==============================\n");
        Console.WriteLine("Log\n");
        foreach (var line in log) Console.WriteLine($" > {line}");
        Console.WriteLine();
    }

    public List<PlayerCharacter> PickTeam(List<PlayerCharacter> pool, int size = 3)
    {
        var chosen = new List<PlayerCharacter>();
        while (chosen.Count < size)
        {
            Console.Clear();
            Console.WriteLine($"Pick your team ({chosen.Count}/{size}):\n");
            
            var available = new List<PlayerCharacter>();
            foreach (var p in pool)
            {
                if (!chosen.Contains(p))
                    available.Add(p);
            }
            for (int i = 0; i < available.Count; i++)
            {
                var p = available[i];
                Console.WriteLine($"  {i + 1}. {p.Name} ({p.Class}) HP {p.MaxHp}  MP {p.MaxMp} " +
                                  $"STR {p.Strength}  DEF {p.Defense}  INT {p.Intelligence}  AGI {p.Agility}");
                Console.WriteLine($"     Skills: {string.Join(", ", p.Skills.Select(s => s.Name))}");
            }

            Console.WriteLine("  0. Create own character");

            int pick = ReadNumber(0, available.Count);
            if (pick == 0)
                chosen.Add(CreateCharacter(900 + chosen.Count));
            else
                chosen.Add(available[pick - 1]);
        }
        return chosen;
    }

    private const int StatPoints = 33;
    private const int MaxStat = 20;

    public PlayerCharacter CreateCharacter(int id)
    {
        Console.Clear();
        Console.WriteLine("=== Create your character ===\n");

        var c = new PlayerCharacter { Id = id };
        c.Name = ReadText("Name");
        c.Class = ReadText("Class");
        c.MaxHp = 100;
        c.MaxMp = 40;

        int left = StatPoints;
        c.Strength = AskStat("Strength", ref left);
        c.Defense = AskStat("Defense", ref left);
        c.Intelligence = AskStat("Intelligence", ref left);
        c.Agility = AskStat("Agility", ref left);

        ChooseResistances(c);
        ChooseSkills(c);

        c.Init();
        return c;
    }

    private static void ChooseResistances(PlayerCharacter c)
    {
        var elements = new List<Element>();
        foreach (var e in Enum.GetValues<Element>())
            if (e != Element.None) elements.Add(e);

        var used = new List<Element>();
        for (int i = 1; i <= 2; i++)
        {
            var e = PickElement($"Resistances {i}/2 (damage x0.5)", elements, used);
            c.Resistances[e.ToString()] = 0.5;
            used.Add(e);
        }

        var weak = PickElement("Weakness (damage x1.5)", elements, used);
        c.Resistances[weak.ToString()] = 1.5;
    }

    private static Element PickElement(string prompt, List<Element> elements, List<Element> used)
    {
        while (true)
        {
            Console.WriteLine($"\n{prompt}:");
            for (int i = 0; i < elements.Count; i++)
            {
                string taken = used.Contains(elements[i]) ? "  (taken)" : "";
                Console.WriteLine($"  {i + 1}. {elements[i]}{taken}");
            }

            var pick = elements[ReadChoice(elements.Count)];
            if (used.Contains(pick))
            {
                Console.WriteLine("Already chosen.");
                continue;
            }
            return pick;
        }
    }

    private static void ChooseSkills(PlayerCharacter c, int count = 3)
    {
        var all = new List<Skill>(SkillCatalog.All.Values);
        while (c.SkillNames.Count < count)
        {
            Console.WriteLine($"\nPick skill {c.SkillNames.Count + 1}/{count}:");
            for (int i = 0; i < all.Count; i++)
            {
                var s = all[i];
                string taken = c.SkillNames.Contains(s.Name) ? "  (taken)" : "";
                Console.WriteLine($"  {i + 1}. {s.Name} (MP {s.MpCost}, {s.Element}) {taken}");
            }

            var pick = all[ReadChoice(all.Count)];
            if (c.SkillNames.Contains(pick.Name))
            {
                Console.WriteLine("Already picked.");
                continue;
            }
            c.SkillNames.Add(pick.Name);
        }
    }

    private static int AskStat(string label, ref int left)
    {
        if (left <= 0)
        {
            Console.WriteLine($"{label}: no points left, set to 1.");
            return 1;
        }

        int max = Math.Min(MaxStat, left);
        Console.WriteLine($"\n{label} (1-{max}), points left: {left}");
        int value = ReadNumber(0, max);

        if (value == 0)
        {
            Console.WriteLine($"{label} skipped, set to 1.");
            return 1;
        }

        left -= value;
        return value;
    }

    private static string ReadText(string label)
    {
        while (true)
        {
            Console.Write($"{label}: ");
            string? s = Console.ReadLine();
            if (s == null) throw new InvalidOperationException("Input was null.");
            s = s.Trim();
            if (s.Length > 0) return s;
            Console.WriteLine("Cannot be empty.");
        }
    }

    private static string DescribeResistances(Character c)
    {
        var resist = new List<string>();
        var weak = new List<string>();

        foreach (var e in Enum.GetValues<Element>())
        {
            if (e == Element.None) continue;
            double r = c.GetResistance(e);
            if (r < 1) resist.Add(e.ToString());
            else if (r > 1) weak.Add(e.ToString());
        }

        var parts = new List<string>();
        if (resist.Count > 0) parts.Add("Resistant vs " + string.Join(", ", resist));
        if (weak.Count > 0) parts.Add("Weak to " + string.Join(", ", weak));
        return string.Join(" | ", parts);
    }

    private void PrintCharacter(Character c, bool isActive)
    {
        string marker = isActive ? ">" : " ";
        if (!c.IsAlive)
        {
            Console.WriteLine($"{marker} {c.Name} ({c.Class}) DEAD");
            return;
        }
        string effects = string.Join(", ", c.Effects.Items().Select(s => s.Type));
        Console.WriteLine($"{marker} {c.Name} ({c.Class})  HP {c.Hp}/{c.MaxHp}  MP {c.Mp}/{c.MaxMp}"
                      + (effects.Length > 0 ? $"  [{effects}]" : ""));

        string res = DescribeResistances(c);
        if (res.Length > 0) Console.WriteLine($"    {res}");
    }

    public int AskAction(Character actor, List<string> options)
    {
        Console.WriteLine($"{actor.Name}, choose an action:");
        for (int i = 0; i < options.Count; i++)
            Console.WriteLine($"  {i + 1}. {options[i]}");

        return ReadChoice(options.Count);
    }

    public Character? AskTarget(string prompt, List<Character> targets)
    {
        Console.WriteLine(prompt);
        for (int i = 0; i < targets.Count; i++)
        {
            var t = targets[i];
            Console.WriteLine($"  {i + 1}. {t.Name} ({t.Class}) HP {t.Hp}/{t.MaxHp}");
        }
        Console.WriteLine("  0. Back");

        int c = ReadNumber(0, targets.Count);
        return c == 0 ? null : targets[c - 1];
    }

    public int ShowStartmenu()
    {
        Console.Clear();
        Console.WriteLine("==============================");
        Console.WriteLine("    Divinity: Divine Blood");
        Console.WriteLine("==============================\n");
        Console.WriteLine("Choose turn order mode:");
        Console.WriteLine("  1. Fixed   (queue, rebuilt every round)");
        Console.WriteLine("  2. Dynamic (circular list)");
        Console.WriteLine("  0. Exit");
        return ReadNumber(0, 2);
    }

    public int AskMainAction(Character actor)
    {
        Console.WriteLine($"{actor.Name}, choose an action:");
        Console.WriteLine("  1. Attack");
        Console.WriteLine("  2. Skills");
        Console.WriteLine("  3. Guard");
        Console.WriteLine("  0. Leave");
        return ReadNumber(0, 3);
    }

    public Skill? AskSkill(Character actor)
    {
        Console.WriteLine("Skills:");
        for (int i = 0; i < actor.Skills.Count; i++)
        {
            var s = actor.Skills[i];
            string warn = s.MpCost > actor.Mp ? "  (not enough MP)" : "";
            Console.WriteLine($"  {i + 1}. {s.Name} (MP {s.MpCost}){warn} | Element : {s.Element}");
        }
        Console.WriteLine("  0. Back");

        int c = ReadNumber(0, actor.Skills.Count);
        return c == 0 ? null : actor.Skills[c - 1];
    }

    private static int ReadNumber(int min, int max)
    {
        while(true)
        {
            Console.Write("> ");
            string? input = Console.ReadLine();
            if (input == null) throw new InvalidOperationException("Input was null.");

            if (int.TryParse(input.Trim(), out int n) && n >= min && n <= max)
                return n;
            
            Console.WriteLine($"Input a number between {min} and {max}."); 
        }
    }

    private static int ReadChoice(int count) => ReadNumber(1, count) - 1;
}
