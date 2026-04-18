using System;
using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.AVL;

public class AvlTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, AvlNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    protected override AvlNode<TKey, TValue> CreateNode(TKey key, TValue value) 
        => new(key, value);

    protected override void OnNodeAdded(AvlNode<TKey, TValue> newNode)
    {
        var current = newNode;
        while (current != null)
        {
            UpdateHeight(current);
            AvlNode<TKey, TValue> subtreeRoot = Rebalance(current);
            current = subtreeRoot.Parent;
        }
    }

    protected override void OnNodeRemoved(AvlNode<TKey, TValue>? parent, AvlNode<TKey, TValue>? child)
    {
        var current = parent ?? child;
        while (current != null)
        {
            UpdateHeight(current);
            AvlNode<TKey, TValue> subtreeRoot = Rebalance(current);
            current = subtreeRoot.Parent;
        }
    }

    private int GetHeight(AvlNode<TKey, TValue>? node) => node?.Height ?? 0;
    
    private void UpdateHeight(AvlNode<TKey, TValue> node)
    {
        node.Height = Math.Max(GetHeight(node.Left), GetHeight(node.Right)) + 1;
    }

    private int GetBalanceFactor(AvlNode<TKey, TValue> node)
    {
        return GetHeight(node.Left) - GetHeight(node.Right);
    }

    private AvlNode<TKey, TValue> Rebalance(AvlNode<TKey, TValue> node)
    {
        int balanceFactor = GetBalanceFactor(node);

        if (balanceFactor > 1)
        {
            if (node.Left != null && GetBalanceFactor(node.Left) < 0)
            {
                RotateLeft(node.Left);
                UpdateHeight(node.Left);
            }

            RotateRight(node);

            UpdateHeight(node);
            AvlNode<TKey, TValue> newRoot = node.Parent!;
            UpdateHeight(newRoot);
            return newRoot;
        }

        if (balanceFactor < -1)
        {
            if (node.Right != null && GetBalanceFactor(node.Right) > 0)
            {
                RotateRight(node.Right);
                UpdateHeight(node.Right);
            }

            RotateLeft(node);

            UpdateHeight(node);
            AvlNode<TKey, TValue> newRoot = node.Parent!;
            UpdateHeight(newRoot);
            return newRoot;
        }

        return node;
    }
}
