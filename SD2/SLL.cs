namespace RpgBattle;

public class SLL<T>
{
    protected readonly LinkedList<T> list = new();

    public int Count => list.Count;
    public LinkedListNode<T>? First => list.First;

    public LinkedListNode<T> AddLast(T item) => list.AddLast(item);
    public LinkedListNode<T> AddFirst(T item) => list.AddFirst(item);
    public LinkedListNode<T> InsertAfter(LinkedListNode<T> prev, T value) => list.AddAfter(prev, value);
    public bool Remove(T item) => list.Remove(item);
    public void RemoveNode(LinkedListNode<T> node) => list.Remove(node);

    public IEnumerable<LinkedListNode<T>> Nodes()
    {
        var node = list.First;
        while (node != null)
        {
            yield return node;
            node = node.Next;
        }
    }

    public IEnumerable<T> Items()
    {
        foreach (var node in Nodes())
            yield return node.Value;
    }
}
