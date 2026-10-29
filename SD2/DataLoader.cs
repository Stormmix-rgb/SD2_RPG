using System.Text.Json;

namespace RpgBattle;

public class EnemyGroup
{
    public string Name { get; set; } = "";
    public List<int> Members { get; set; } = new();
}

public class EnemyData
{
    public List<EnemyCharacter> Enemies { get; set; } = new();
    public List<EnemyGroup> Groups { get; set; } = new();
}

public static class DataLoader
{
    static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public static List<PlayerCharacter> LoadCharacters(string path)
    {
        var list = JsonSerializer.Deserialize<List<PlayerCharacter>>(File.ReadAllText(path), Options) ?? new();
        list.ForEach(c => c.Init());
        return list;
    }

    public static EnemyData LoadEnemies(string path)
    {
        var data = JsonSerializer.Deserialize<EnemyData>(File.ReadAllText(path), Options) ?? new();
        data.Enemies.ForEach(e => e.Init());
        return data;
    }

    // Dvikryptis
    public static LinkedList<EnemyCharacter> PickRandomGroup(EnemyData data, Random rng)
    {
        var group = data.Groups[rng.Next(data.Groups.Count)];
        var team = new LinkedList<EnemyCharacter>();
        foreach (var id in group.Members)
            team.AddLast(data.Enemies.First(e => e.Id == id).Clone());
        return team;
    }
}
