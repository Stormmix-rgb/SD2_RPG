namespace RpgBattle;

public interface ITurnOrder
{
    Character? Next();
    void OnDeath(Character c);
    void OnSpeedChanged(Character c);
    List<Character> Preview();
}

public class FixedTurnOrder : ITurnOrder
{
    private readonly Queue<Character> fixedTurnOrder = new();
    private readonly List<Character> all;

    public FixedTurnOrder(List<Character> all) { this.all = all; }

    public Character? Next()
    {
        while (true)
        {
            if (fixedTurnOrder.Count == 0)
            {
                StartNewRound();
                if (fixedTurnOrder.Count == 0) return null;
            }

            var next = fixedTurnOrder.Dequeue();
            if (next.IsAlive) return next;
        }
    }

    private void StartNewRound()
    {
        var sorted = new List<Character>();
        foreach (var c in all)
        {
            if (!c.IsAlive) continue;

            double speed = StatusEffectManager.GetEffectiveSpeed(c);
            int i = 0;
            while (i < sorted.Count && StatusEffectManager.GetEffectiveSpeed(sorted[i]) >= speed)
                i++;
            sorted.Insert(i, c);
        }

        foreach (var c in sorted) fixedTurnOrder.Enqueue(c);
    }

    public void OnDeath(Character c) {}
    public void OnSpeedChanged(Character c) {}
    public List<Character> Preview()
    {
        var result = new List<Character>();
        foreach (var c in fixedTurnOrder)
            if (c.IsAlive) result.Add(c);
        return result;
    }
}

public class DynamicTurnOrder : ITurnOrder
{
    private readonly CircularSLL<Character> dynamicTurnOrder = new();
    private readonly CircularIterator<Character> it;

    public DynamicTurnOrder(List<Character> all)
    {
        foreach (var c in all)
            if (c.IsAlive) InsertBySpeed(c);
        it = new CircularIterator<Character>(dynamicTurnOrder);
    }

    private void InsertBySpeed(Character c)
    {
        double speed = StatusEffectManager.GetEffectiveSpeed(c);
        LinkedListNode<Character>? prev = null;
        foreach (var n in dynamicTurnOrder.Nodes())
        {
            if (StatusEffectManager.GetEffectiveSpeed(n.Value) < speed) break;
            prev = n;
        }
        if (prev == null) dynamicTurnOrder.AddFirst(c);
        else dynamicTurnOrder.InsertAfter(prev, c);
    }

    public Character? Next()
    {
        if (it.IsEmpty) return null;
        var active = it.Current;
        it.MoveNext();
        return active;
    }

    public void OnDeath(Character c)
    {
        var node = dynamicTurnOrder.Find(c);
        if (node == null) return;
        if (!it.IsEmpty && it.CurrentNode == node) it.RemoveCurrent();
        else dynamicTurnOrder.RemoveNode(node);
    }

    public void OnSpeedChanged(Character c)
    {
        var node = dynamicTurnOrder.Find(c);
        if (node == null || dynamicTurnOrder.Count == 1) return;

        if (node == it.CurrentNode) it.MoveNext();
        dynamicTurnOrder.RemoveNode(node);

        InsertBySpeed(c);
    }

    public List<Character> Preview()
    {
        var result = new List<Character>();
        if (it.IsEmpty) return result;

        var node = it.CurrentNode;
        for (int i = 0; i < dynamicTurnOrder.Count; i++)
        {
            result.Add(node.Value);
            node = dynamicTurnOrder.NextOf(node);
        }
        return result;
    }
}
