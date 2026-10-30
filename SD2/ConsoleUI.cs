namespace RpgBattle;

public class CircularSLL<T> : SLL<T>
{
    public LinkedListNode<T> NextOf(LinkedListNode<T> node) => node.Next ?? list.First!;

    public LinkedListNode<T>? Find(T value) => list.Find(value);
}
