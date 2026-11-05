namespace RpgBattle;

public class Battle
{
    private readonly LinkedList<PlayerCharacter> players;
    private readonly LinkedList<EnemyCharacter> enemies;
    private readonly EnemyAI ai = new();
    private readonly ITurnOrder turnOrder;
    private readonly StatusEffectManager effect = new();
    private readonly BattleCalculator calc = new();
    private readonly ConsoleUI ui;
    private readonly string mode;
    private int turn = 1;
    private bool quit;

    public Battle(LinkedList<PlayerCharacter> players, LinkedList<EnemyCharacter> enemies, 
                            ITurnOrder turnOrder, ConsoleUI ui, string mode)
    {
        this.players = players;
        this.enemies = enemies;
        this.turnOrder = turnOrder;
        this.ui = ui;
        this.mode = mode;
    }
        
    public void Run()
    {
        while (!IsOver() && !quit)
        {
            var actor = turnOrder.Next();
            if (actor == null) break;
            if (!actor.IsAlive) continue;

            PlayTurn(actor);
            turn++;
        }

        if (quit) Console.WriteLine("\nYou left the battle.");
        else Console.WriteLine(AnyAlive(players) ? "\nYou win!" : "\nYou lose!");
    }

    private bool IsOver() => !AnyAlive(players) || !AnyAlive(enemies);

    private static bool AnyAlive(IEnumerable<Character> team)
    {
        foreach (var c in team)
            if (c.IsAlive) return true;
        return false;
    }

    private void PlayTurn(Character actor)
    {
        var start = effect.StartTurn(actor);
        if (start.DotDamage > 0)
            ui.Log($"{actor.Name} take {start.DotDamage} damage from status effects.");       

        if (!actor.IsAlive)
        {
            ui.Log($"{actor.Name} dies.");
            turnOrder.OnDeath(actor);
            return;
        }

        if (start.SpeedChanged) turnOrder.OnSpeedChanged(actor);

        if (start.SkipTurn)
        {
            ui.Log($"{actor.Name} is stunned and cannot act this turn.");
            return;
        }

        if (actor is PlayerCharacter) PlayerAction(actor);
        else EnemyAction(actor);
    }

    private void PlayerAction(Character actor)
    {
        while (true)
        {
            ui.DrawBattle(new List<Character>(players), new List<Character>(enemies), 
                            actor, turnOrder.Preview(), turn, mode);

            switch (ui.AskMainAction(actor))
            {
                case 0:
                    quit = true;
                    return;

                case 1:
                {
                    var target = ChooseTarget(SkillCatalog.Basic);
                    if (target == null) continue;
                    UseSkill(actor, SkillCatalog.Basic, target);
                    return;
                }
                case 2:
                {
                    var skill = ui.AskSkill(actor);
                    if (skill == null) continue;
                    if (skill.MpCost > actor.Mp)
                    {
                        ui.Log($"{actor.Name} does not have enough MP to use {skill.Name}.");
                        continue;
                    }
                    var target = ChooseTarget(skill);
                    if (target == null) continue;
                    UseSkill(actor, skill, target);
                    return;
                }
                case 3:
                    effect.Apply(actor, StatusType.Guarding, 1);
                    ui.Log($"{actor.Name} takes a defensive stance.");
                    return;
            }
        }
    }

    private Character? ChooseTarget(Skill skill)
    {
        var team = skill.Kind == SkillKind.Damage ? enemies.Cast<Character>() : players.Cast<Character>();

        var alive = new List<Character>();
        foreach (var c in team)
            if (c.IsAlive) alive.Add(c);
        
        return ui.AskTarget("Choose a target:", alive);
    }

    private void EnemyAction(Character actor)
    {
        var choice = ai.Choose(actor, enemies.Cast<Character>(), players.Cast<Character>());
        UseSkill(actor, choice.Skill, choice.Target);
    }

    private void UseSkill(Character actor, Skill skill, Character target)
    {
        actor.Mp -= skill.MpCost;

        switch (skill.Kind)
        {
            case SkillKind.Damage:
                bool wasGuarding = StatusEffectManager.Has(target, StatusType.Guarding);

                var result = calc.CalculateDamage(actor, skill, target);
                target.Hp = Math.Max(0, target.Hp - result.Amount);

                if (wasGuarding)
                {
                    effect.Remove(target, StatusType.Guarding);
                    ui.Log($"{target.Name} is no longer guarding.");
                }

                string extra = result.IsCrit ? " (CRITICAL!)" : "";
                if (result.ElementMultiplier > 1) extra += " (super effective!)";
                if (result.ElementMultiplier < 1) extra += " (not very effective...)";
                ui.Log($"{actor.Name} uses {skill.Name} on {target.Name}: {result.Amount} damage. {extra}");

                if (!target.IsAlive)
                {
                    ui.Log($"{target.Name} dies.");
                    turnOrder.OnDeath(target);
                }
                else if (skill.Status != null)
                {
                    ApplyStatus(target, skill.Status.Value, skill.Duration);
                }
                break;

            case SkillKind.Heal:
                int heal = calc.CalculateHeal(actor, skill);
                target.Hp = Math.Min(target.MaxHp, target.Hp + heal);
                ui.Log($"{actor.Name} uses {skill.Name} on {target.Name}: heals {heal} HP.");
                break;

            case SkillKind.Buff:
                ui.Log($"{actor.Name} uses {skill.Name} on {target.Name}: applies {skill.Status} for {skill.Duration} turns.");
                if (skill.Status != null)
                    ApplyStatus(target, skill.Status.Value, skill.Duration);
                break;
        }
    }

    private void ApplyStatus(Character target, StatusType status, int duration)
    {
        bool wasBurning = StatusEffectManager.Has(target, StatusType.Burning);
        bool wasWet = StatusEffectManager.Has(target, StatusType.Wet);

        effect.Apply(target, status, duration);

        if (status == StatusType.Wet && wasBurning)
            ui.Log($"{target.Name} stopped burning.");
        else if (status == StatusType.Burning && wasWet)
            ui.Log($"{target.Name} stopped being wet.");

        ui.Log($"{target.Name} is now {status} for {duration} turns.");

        if (status == StatusType.Haste || status == StatusType.Slow)
            turnOrder.OnSpeedChanged(target);
    }
}
