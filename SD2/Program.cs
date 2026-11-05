using RpgBattle;

var ui = new ConsoleUI();
int mode = ui.ShowStartmenu();
if (mode == 0) return;

var heroes = DataLoader.LoadCharacters("characters.json");
var data = DataLoader.LoadEnemies("enemy_groups.json");
var enemies = DataLoader.PickRandomGroup(data, new Random());

var team = ui.PickTeam(heroes);
var players = new LinkedList<PlayerCharacter>(team);

var all = new List<Character>();
all.AddRange(players);
all.AddRange(enemies);

ITurnOrder order = mode == 1 ? new FixedTurnOrder(all) : new DynamicTurnOrder(all);
string modeName = mode == 1 ? "Fixed" : "Dynamic";

new Battle(players, enemies, order, ui, modeName).Run();
