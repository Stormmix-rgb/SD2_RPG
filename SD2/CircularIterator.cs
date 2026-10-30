namespace RpgBattle;

public class CircularIterator<T>
{
    private readonly CircularSLL<T> list;
    private LinkedListNode<T>? cur;

    public CircularIterator(CircularSLL<T> list)
    {
        this.list = list;
        cur = list.First;
    }

    public bool IsEmpty => cur == null;
    public T Current => cur!.Value;
    public LinkedListNode<T> CurrentNode => cur!;

    public void MoveNext()
    {
        if (cur != null) cur = list.NextOf(cur);
    }

    public void RemoveCurrent()
    {
        if (cur == null) return;
        var next = list.Count == 1 ? null : list.NextOf(cur!);
        list.RemoveNode(cur!);
        cur = next;
    }
}
