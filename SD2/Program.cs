using RpgBattle;

var heroes = DataLoader.LoadCharacters("characters.json");
var data = DataLoader.LoadEnemies("enemy_groups.json");
var enemies = DataLoader.PickRandomGroup(data, new Random());

Console.WriteLine("--- Žaidėjai ---");
foreach (var h in heroes)
    Console.WriteLine($"{h.Name} ({h.Class}) HP {h.Hp}/{h.MaxHp} MP {h.Mp}/{h.MaxMp} | {string.Join(", ", h.SkillNames)}");

Console.WriteLine("--- Priešai ---");
foreach (var e in enemies)
    Console.WriteLine($"{e.Name} ({e.Class}) HP {e.Hp}/{e.MaxHp} | {string.Join(", ", e.SkillNames)}");