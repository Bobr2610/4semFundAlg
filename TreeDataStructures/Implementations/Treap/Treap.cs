using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.Treap;

public class Treap<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, TreapNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    /// <summary>
    /// Разрезает дерево с корнем <paramref name="root"/> на два поддерева:
    /// Left: все ключи <= <paramref name="key"/>
    /// Right: все ключи > <paramref name="key"/>
    /// </summary>
    protected virtual (TreapNode<TKey, TValue>? Left, TreapNode<TKey, TValue>? Right) Split(TreapNode<TKey, TValue>? root, TKey key)
    {
        if (root == null)
            return (null, null);
        else if (key.CompareTo(root.Key) < 0) {
            var (left, right) = Split(root.Left, key);
            root.Left = right;
            return (left, root);
        }
        else {
            var (left, right) = Split(root.Right, key);
            root.Right = left;
            return (root, right);
        }
    }

    /// <summary>
    /// Сливает два дерева в одно.
    /// Важное условие: все ключи в <paramref name="left"/> должны быть меньше ключей в <paramref name="right"/>.
    /// Слияние происходит на основе Priority (куча).
    /// </summary>
    protected virtual TreapNode<TKey, TValue>? Merge(TreapNode<TKey, TValue>? left, TreapNode<TKey, TValue>? right)
    {
        if (left == null) return right;
        if (right == null) return left;

        if (left.Priority > right.Priority) {
            left.Right = Merge(left.Right, right);
            return left;
        }
        else {
            right.Left = Merge(left, right.Left);
            return right;
        }
    }
    

    public override void Add(TKey key, TValue value)
    {
        if (ContainsKey(key)) throw new ArgumentException($"Key {key} is a dopelganger");

        var newNode = CreateNode(key, value);
        var (left, right) = Split(Root, key);
        Root = Merge(Merge(left, newNode), right);
        if (Root != null) Root.Parent = null;
        Count++;
        OnNodeAdded(newNode);
    }

    public override bool Remove(TKey key)
    {
        var node = FindNode(key);
        if (node == null) return false;
        
        var merged = Merge(node.Left, node.Right);
        if (merged != null) merged.Parent = node.Parent;

        if (node.Parent == null)
        {
            Root = merged;
        }
        else
        {
            if (node.Parent.Left == node) node.Parent.Left = merged;
            else node.Parent.Right = merged;
        }
        Count--;
        return true;
    }

    protected override TreapNode<TKey, TValue> CreateNode(TKey key, TValue value)
    {
        return new TreapNode<TKey, TValue>(key, value);
    }
    protected override void OnNodeAdded(TreapNode<TKey, TValue> newNode)
    {
    }

    protected override void OnNodeRemoved(TreapNode<TKey, TValue>? parent, TreapNode<TKey, TValue>? child)
    { 
        
    }
}