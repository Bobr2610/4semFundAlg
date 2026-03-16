namespace TreeDataStructures.Interfaces;

public readonly record struct TreeEntry<TKey, TValue>(TKey Key, TValue Value, int Depth, int Height);

public interface ITree<TKey, TValue> : IDictionary<TKey, TValue>
{
    IEnumerable<TreeEntry<TKey, TValue>>  InOrder(); 
    IEnumerable<TreeEntry<TKey, TValue>>  PreOrder();
    IEnumerable<TreeEntry<TKey, TValue>>  PostOrder();
    
    IEnumerable<TreeEntry<TKey, TValue>>  InOrderReverse();
    IEnumerable<TreeEntry<TKey, TValue>>  PreOrderReverse();
    IEnumerable<TreeEntry<TKey, TValue>>  PostOrderReverse();
    
    IComparer<TKey> Comparer { get; }
}

    