using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Implementations.BST;

namespace TreeDataStructures.Implementations.Splay;

public class SplayTree<TKey, TValue> : BinarySearchTree<TKey, TValue>
    where TKey : IComparable<TKey>
{
    protected override BstNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value);
    
    protected override void OnNodeAdded(BstNode<TKey, TValue> newNode)
    {
        Splay(newNode); 
    }
    
    protected override void OnNodeRemoved(BstNode<TKey, TValue>? parent, BstNode<TKey, TValue>? child)
    {
        if (child != null && IsAttachedToCurrentTree(child))
            Splay(child);
        else if (parent != null && IsAttachedToCurrentTree(parent))
            Splay(parent);
    }
    
    public override bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        BstNode<TKey, TValue>? node = FindNode(key);
        if (node != null)
        {
            value = node.Value;
            Splay(node);
            return true;
        }

        value = default;
        return false;
    }

    public override bool ContainsKey(TKey key)
    {
        BstNode<TKey, TValue>? node = FindNode(key);
        if (node == null)
        {
            return false;
        }

        Splay(node);
        return true;
    }
    
    private void Splay(BstNode<TKey, TValue> node)
    {
        while (node.Parent != null)
        {
            BstNode<TKey, TValue> parent = node.Parent;
            BstNode<TKey, TValue>? grandParent = parent.Parent;

            if (grandParent == null)
            {
                if (parent.Left == node)
                    RotateRight(parent);
                else
                    RotateLeft(parent);
            }
            else if (parent.Left == node && grandParent.Left == parent)
            {
                RotateRight(grandParent);
                RotateRight(parent);
            }
            else if (parent.Right == node && grandParent.Right == parent)
            {
                RotateLeft(grandParent);
                RotateLeft(parent);
            }
            else if (parent.Left == node && grandParent.Right == parent)
            {
                RotateRight(parent);
                RotateLeft(grandParent);
            }
            else
            {
                RotateLeft(parent);
                RotateRight(grandParent);
            }
        }
    }

    private bool IsAttachedToCurrentTree(BstNode<TKey, TValue> node)
    {
        BstNode<TKey, TValue> current = node;
        while (current.Parent != null)
        {
            current = current.Parent;
        }

        return ReferenceEquals(current, Root);
    }
}
