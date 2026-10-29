namespace RpgBattle;

public class SLL<T>
{
    protected readonly LinkedList<T> list = new();

    public int Count => list.Count;
    public LinkedListNode<T>? First => list.First;

    public LinkedListNode<T> AddLast(T item) => list.AddLast(item);
    public LinkedListNode<T> AddFirst(T item) => list.AddFirst(item);
    public bool Remove(T item) => list.Remove(item);
    public void RemoveNode(LinkedListNode<T> node) => list.Remove(node);

    public int RemoveAll(Predicate<T> match)
    {
        int removed = 0;
        var cur = list.First;
        while (cur != null)
        {
            var next = cur.Next;
            if (match(cur.Value)) { list.Remove(cur); removed++;}
            cur = next;
        }
        return removed;
    } 
    public IEnumerable<T> Items() => list;
}