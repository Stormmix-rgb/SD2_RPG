using System.Reflection.Metadata;

namespace RpgBattle;

public class ConsoleUI
{
    private readonly List<string> log = new();

    public void Log(string message)
    {
        log.Add(message);
        if (log.Count > 6) log.RemoveAt(0);
    }

    public void DrawBattle(List<Character> players, List<Character> enemies, 
                            Character active, List<Character> upcoming,
                            int round, string mode)

    {
        Console.Clear();
        Console.WriteLine($"=== Round {round} · {mode} ===\n");
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
    
    private void PrintCharacter(Character c, bool isActive)
    {
        string marker = isActive ? "▶" : " ";
        if (!c.IsAlive)
        {
            Console.WriteLine($"{marker} {c.Name} ({c.Class}) DEAD");
            return;
        }
        string effects = string.Join(", ", c.Effects.Items().Select(s => s.Type));
        Console.WriteLine($"{marker} {c.Name} ({c.Class})  HP {c.Hp}/{c.MaxHp}  MP {c.Mp}/{c.MaxMp}"
                      + (effects.Length > 0 ? $"  [{effects}]" : ""));
    }

    public int AskAction(Character actor, List<string> options)
    {
        Console.WriteLine($"{actor.Name}, choose an action:");
        for (int i = 0; i < options.Count; i++)
            Console.WriteLine($"  {i + 1}. {options[i]}");

        return ReadChoice(options.Count);
    }

    public Character AskTarget(string prompt, List<Character> targets)
    {
        Console.WriteLine(prompt);
        for (int i = 0; i < targets.Count; i++)
        {
            var t = targets[i];
            Console.WriteLine($"  {i + 1}. {t.Name} ({t.Class}) HP {t.Hp}/{t.MaxHp}");
        }

        return targets[ReadChoice(targets.Count)];
    }

    private static int ReadChoice(int count)
    {
        while(true)
        {
            Console.Write("> ");
            string? input = Console.ReadLine();

            if (input == null)
                throw new InvalidOperationException("Invalid input.");

            if (int.TryParse(input.Trim(), out int choice) && choice >= 1 && choice <= count)
                return choice - 1;

            Console.WriteLine($"Input a number between 1 and {count}."); 
        }
    }
}
