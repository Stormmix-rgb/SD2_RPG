using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RpgBattle;

public interface ITurnOrder
{
    Character? Next();
    void OnDeath(Character c);
    void OnSpeedChanged(Character c);
    List<Character> Preview();
}

public enum TurnOrderMode
{
    FixedQueue,
    DynamicCircular
}

class FixedTurnOrder : ITurnOrder
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
                var alive = all.Where(c => c.IsAlive)
                            .OrderByDescending(c => c.Agility)
                            .ToList();
                if (alive.Count == 0) return null;
                foreach (var c in alive) fixedTurnOrder.Enqueue(c);
            }

            var next = fixedTurnOrder.Dequeue();
            if (next.IsAlive) return next;
        }
    }

    public void OnDeath(Character c) {}
    public void OnSpeedChanged(Character c) {}
    public List<Character> Preview() => fixedTurnOrder.Where(c => c.IsAlive).ToList();
}

public class DynamicTurnOrder : ITurnOrder
{
    private readonly CircularSLL<Character> dynamicTurnOrder = new();
    private readonly CircularIterator<Character> it;

    public DynamicTurnOrder(List<Character> all)
    {
        foreach (var c in all.Where(c => c.IsAlive).OrderByDescending(c => c.Agility))
            dynamicTurnOrder.AddLast(c);
        it = new CircularIterator<Character>(dynamicTurnOrder);
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
        if (it.CurrentNode == node) it.MoveNext();
        dynamicTurnOrder.RemoveNode(node);
        dynamicTurnOrder.InsertSorted(c, (a, b) => a.Agility > b.Agility);
    }

    public List<Character> Preview()
    {
        var result = new List<Character>();
        if (it.IsEmpty) return result;
        var node = it.CurrentNode;
        for (int i = 0; i < dynamicTurnOrder.Count; i++)
        {
            if (node == null) break;
            result.Add(node.Value);
            node = dynamicTurnOrder.NextOf(node);
        }
        return result;
    }
}