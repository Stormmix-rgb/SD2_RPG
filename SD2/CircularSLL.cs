namespace RpgBattle;

public class CircularSLL<T> : SLL<T>
{
    public LinkedListNode<T>? NextOf(LinkedListNode<T> node) => node.Next ?? list.First;

    public LinkedListNode<T>? Find(T value)
    {
        var node = First;
        for (int i = 0; i < Count; i++)
        {
            if (EqualityComparer<T>.Default.Equals(node!.Value, value))
                return node;
            node = NextOf(node);
        }

        return null;
    }

    public void AddSorted(T item, Func<T, double> key) => 
        InsertSorted(item, (a, b) => key(a) > key(b));

    public void Reposition(LinkedListNode<T> node, Func<T, double> key)
    {
        var value = node.Value;
        list.Remove(node);
        AddSorted(value, key);
    }

    public void InsertSorted(T value, Func<T, T, bool> comesBefore)
    {
        LinkedListNode<T>? prev = null;
        var node = First;

        for (int i = 0; i < Count; i++)
        {
            if (comesBefore(value, node!.Value))
            {
                if (prev == null) list.AddFirst(value);
                else list.AddAfter(prev, value);
                return;
            }
            prev = node;
            node = NextOf(node);
        }
        list.AddLast(value);
    }
}