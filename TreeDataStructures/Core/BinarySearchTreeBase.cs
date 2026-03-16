using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using TreeDataStructures.Interfaces;

namespace TreeDataStructures.Core;

public abstract class BinarySearchTreeBase<TKey, TValue, TNode>(IComparer<TKey>? comparer = null) 
    : ITree<TKey, TValue>
    where TNode : Node<TKey, TValue, TNode>
{
    protected TNode? Root;
    public IComparer<TKey> Comparer { get; protected set; } = comparer ?? Comparer<TKey>.Default;
    public int Count { get; protected set; }
    public bool IsReadOnly => false;

    public ICollection<TKey> Keys => InOrder().Select(e => e.Key).ToList();
    public ICollection<TValue> Values => InOrder().Select(e => e.Value).ToList();

    public virtual void Add(TKey key, TValue value)
    {
        TNode newNode = CreateNode(key, value);

        TNode? parent = null;
        TNode? current = Root;

        while (current != null)
        {
            parent = current;

            int cmp = Comparer.Compare(key, current.Key);

            if (cmp == 0)
            {
                current.Value = value;
                return;
            }

            current = cmp < 0 ? current.Left : current.Right;
        }

        newNode.Parent = parent;

        if (parent == null)
        {
            Root = newNode;
        }
        else if (Comparer.Compare(key, parent.Key) < 0)
        {
            parent.Left = newNode;
        }
        else
        {
            parent.Right = newNode;
        }

        Count++;

        OnNodeAdded(newNode);
    }

    public virtual bool Remove(TKey key)
    {
        TNode? node = FindNode(key);
        if (node == null) { return false; }

        RemoveNode(node);
        Count--;

        return true;
    }
    
    
    protected virtual void RemoveNode(TNode node)
    {
        TNode? parent;
        TNode? child;

        if (node.Left == null)
        {
            Transplant(node, node.Right);
            parent = node.Parent;
            child = node.Right;
        }
        else if (node.Right == null)
        {
            Transplant(node, node.Left);
            parent = node.Parent;
            child = node.Left;
        }
        else
        {
            TNode successor = GetMinimum(node.Right!);
            parent = successor.Parent;
            child = successor.Right;

            if (successor.Parent != node)
            {
                Transplant(successor, successor.Right);
                successor.Right = node.Right;
                successor.Right!.Parent = successor;
            }
            Transplant(node, successor);
            successor.Left = node.Left;
            successor.Left!.Parent = successor;
            parent = successor;
            child = successor.Right;
        }

        OnNodeRemoved(parent, child);
    }

    public virtual bool ContainsKey(TKey key) => FindNode(key) != null;
    
    public virtual bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        TNode? node = FindNode(key);
        if (node != null)
        {
            value = node.Value;
            return true;
        }
        value = default;
        return false;
    }

    public TValue this[TKey key]
    {
        get => TryGetValue(key, out TValue? val) ? val : throw new KeyNotFoundException();
        set => Add(key, value);
    }

    
    #region Hooks
    protected virtual void OnNodeAdded(TNode newNode) { }
    protected virtual void OnNodeRemoved(TNode? parent, TNode? child) { }
    #endregion
    
    
    #region Helpers
    protected abstract TNode CreateNode(TKey key, TValue value);
    
    
    protected TNode? FindNode(TKey key)
    {
        TNode? current = Root;

        while (current != null)
        {
            int cmp = Comparer.Compare(key, current.Key);
            if (cmp == 0) { return current; }
            current = cmp < 0 ? current.Left : current.Right;
        }

        return null;
    }

    protected static TNode GetMinimum(TNode node)
    {
        while (node.Left != null)
            node = node.Left;
        return node;
    }

    protected static int GetHeight(TNode? node)
    {
        if (node == null) return 0;
        int leftH = GetHeight(node.Left);
        int rightH = GetHeight(node.Right);
        return 1 + Math.Max(leftH, rightH);
    }

    protected void RotateLeft(TNode x)
    {
        TNode y = x.Right!;

        x.Right = y.Left;
        if (y.Left != null)
            y.Left.Parent = x;

        y.Parent = x.Parent;
        if (x.Parent == null)
            Root = y;
        else if (x.IsLeftChild)
            x.Parent!.Left = y;
        else
            x.Parent!.Right = y;

        y.Left = x;
        x.Parent = y;
    }

    protected void RotateRight(TNode y)
    {
        TNode x = y.Left!;

        y.Left = x.Right;
        if (x.Right != null)
            x.Right.Parent = y;

        x.Parent = y.Parent;
        if (y.Parent == null)
            Root = x;
        else if (y.IsLeftChild)
            y.Parent!.Left = x;
        else
            y.Parent!.Right = x;

        x.Right = y;
        y.Parent = x;
    }
    
    protected void RotateDoubleLeft(TNode x)
    {
        RotateRight(x.Right!);
        RotateLeft(x);
    }
    
    protected void RotateDoubleRight(TNode y)
    {
        RotateLeft(y.Left!);
        RotateRight(y);
    }
    
    protected void RotateBigLeft(TNode x) => RotateDoubleLeft(x);
    protected void RotateBigRight(TNode y) => RotateDoubleRight(y);
    
    protected void Transplant(TNode u, TNode? v)
    {
        if (u.Parent == null)
            Root = v;
        else if (u.IsLeftChild)
            u.Parent.Left = v;
        else
            u.Parent.Right = v;
        
        v?.Parent = u.Parent;
    }
    #endregion
    
    public IEnumerable<TreeEntry<TKey, TValue>> InOrder() => new TreeIterator(this, TraversalStrategy.InOrder);
    public IEnumerable<TreeEntry<TKey, TValue>> PreOrder() => new TreeIterator(this, TraversalStrategy.PreOrder);
    public IEnumerable<TreeEntry<TKey, TValue>> PostOrder() => new TreeIterator(this, TraversalStrategy.PostOrder);
    public IEnumerable<TreeEntry<TKey, TValue>> InOrderReverse() => new TreeIterator(this, TraversalStrategy.InOrderReverse);
    public IEnumerable<TreeEntry<TKey, TValue>> PreOrderReverse() => new TreeIterator(this, TraversalStrategy.PreOrderReverse);
    public IEnumerable<TreeEntry<TKey, TValue>> PostOrderReverse() => new TreeIterator(this, TraversalStrategy.PostOrderReverse);
    
    private sealed class TreeIterator : IEnumerable<TreeEntry<TKey, TValue>>, IEnumerator<TreeEntry<TKey, TValue>>
    {
        private readonly BinarySearchTreeBase<TKey, TValue, TNode> _tree;
        private readonly TraversalStrategy _strategy;
        private readonly Stack<NodeFrame> _stack = new();
        private TreeEntry<TKey, TValue> _current;
        private bool _started;

        private struct NodeFrame
        {
            public TNode Node;
            public int Depth;
            public int State;
        }

        internal TreeIterator(BinarySearchTreeBase<TKey, TValue, TNode> tree, TraversalStrategy strategy)
        {
            _tree = tree;
            _strategy = strategy;
            _current = default;
            _started = false;

            if (_tree.Root != null)
            {
                bool reverse = _strategy is TraversalStrategy.InOrderReverse
                    or TraversalStrategy.PreOrderReverse
                    or TraversalStrategy.PostOrderReverse;

                if (_strategy is TraversalStrategy.InOrder or TraversalStrategy.InOrderReverse)
                    PushInOrderChain(_tree.Root, 0, reverse);
                else
                    _stack.Push(new NodeFrame { Node = _tree.Root, Depth = 0, State = 0 });
            }
        }

        private void PushInOrderChain(TNode node, int depth, bool reverse)
        {
            TNode? n = node;
            while (n != null)
            {
                _stack.Push(new NodeFrame { Node = n, Depth = depth, State = 0 });
                n = reverse ? n.Right : n.Left;
                depth++;
            }
        }

        public IEnumerator<TreeEntry<TKey, TValue>> GetEnumerator() => new TreeIterator(_tree, _strategy);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public TreeEntry<TKey, TValue> Current =>
            _started ? _current : throw new InvalidOperationException("Enumeration has not started.");
        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (_stack.Count == 0)
            {
                _started = true;
                return false;
            }

            bool reverse = _strategy is TraversalStrategy.InOrderReverse
                or TraversalStrategy.PreOrderReverse
                or TraversalStrategy.PostOrderReverse;

            if (_strategy is TraversalStrategy.InOrder or TraversalStrategy.InOrderReverse)
            {
                var f = _stack.Pop();
                _current = CreateEntry(f.Node, f.Depth);

                TNode? next = reverse ? f.Node.Left : f.Node.Right;
                if (next != null)
                    PushInOrderChain(next, f.Depth + 1, reverse);

                _started = true;
                return true;
            }

            if (_strategy is TraversalStrategy.PreOrder or TraversalStrategy.PostOrderReverse)
            {
                var f = _stack.Pop();
                _current = CreateEntry(f.Node, f.Depth);

                TNode? first = (_strategy == TraversalStrategy.PostOrderReverse)
                    ? f.Node.Right : f.Node.Left;
                TNode? second = (_strategy == TraversalStrategy.PostOrderReverse)
                    ? f.Node.Left : f.Node.Right;

                if (second != null)
                    _stack.Push(new NodeFrame { Node = second, Depth = f.Depth + 1, State = 0 });
                if (first != null)
                    _stack.Push(new NodeFrame { Node = first, Depth = f.Depth + 1, State = 0 });

                _started = true;
                return true;
            }

            if (_strategy is TraversalStrategy.PreOrderReverse)
            {
                while (_stack.Count > 0)
                {
                    var f = _stack.Pop();

                    if (f.State == 0)
                    {
                        TNode? child = f.Node.Right;
                        if (child != null)
                        {
                            _stack.Push(new NodeFrame { Node = f.Node, Depth = f.Depth, State = 1 });
                            _stack.Push(new NodeFrame { Node = child, Depth = f.Depth + 1, State = 0 });
                            continue;
                        }
                        f.State = 1;
                    }

                    if (f.State == 1)
                    {
                        TNode? child = f.Node.Left;
                        if (child != null)
                        {
                            _stack.Push(new NodeFrame { Node = f.Node, Depth = f.Depth, State = 2 });
                            _stack.Push(new NodeFrame { Node = child, Depth = f.Depth + 1, State = 0 });
                            continue;
                        }
                        f.State = 2;
                    }

                    if (f.State == 2)
                    {
                        _current = CreateEntry(f.Node, f.Depth);
                        _started = true;
                        return true;
                    }
                }
                _started = true;
                return false;
            }

            while (_stack.Count > 0)
            {
                var f = _stack.Pop();

                if (f.State == 0)
                {
                    TNode? child = reverse ? f.Node.Right : f.Node.Left;
                    if (child != null)
                    {
                        _stack.Push(new NodeFrame { Node = f.Node, Depth = f.Depth, State = 1 });
                        _stack.Push(new NodeFrame { Node = child, Depth = f.Depth + 1, State = 0 });
                        continue;
                    }
                    f.State = 1;
                }

                if (f.State == 1)
                {
                    TNode? child = reverse ? f.Node.Left : f.Node.Right;
                    if (child != null)
                    {
                        _stack.Push(new NodeFrame { Node = f.Node, Depth = f.Depth, State = 2 });
                        _stack.Push(new NodeFrame { Node = child, Depth = f.Depth + 1, State = 0 });
                        continue;
                    }
                    f.State = 2;
                }

                if (f.State == 2)
                {
                    _current = CreateEntry(f.Node, f.Depth);
                    _started = true;
                    return true;
                }
            }
            _started = true;
            return false;
        }

        private TreeEntry<TKey, TValue> CreateEntry(TNode node, int depth)
        {
            int height = BinarySearchTreeBase<TKey, TValue, TNode>.GetHeight(node);
            return new TreeEntry<TKey, TValue>(node.Key, node.Value, depth, height);
        }

        public void Reset() => throw new NotSupportedException("Reset is not supported.");
        public void Dispose() { }
    }

    private enum TraversalStrategy
    {
        InOrder,
        PreOrder,
        PostOrder,
        InOrderReverse,
        PreOrderReverse,
        PostOrderReverse
    }
    
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return new KeyValuePairIterator(this);
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private sealed class KeyValuePairIterator : IEnumerator<KeyValuePair<TKey, TValue>>
    {
        private readonly IEnumerator<TreeEntry<TKey, TValue>> _inner;

        internal KeyValuePairIterator(BinarySearchTreeBase<TKey, TValue, TNode> tree)
        {
            _inner = new TreeIterator(tree, TraversalStrategy.InOrder);
        }

        public KeyValuePair<TKey, TValue> Current => new(_inner.Current.Key, _inner.Current.Value);
        object IEnumerator.Current => Current;
        public bool MoveNext() => _inner.MoveNext();
        public void Reset() => throw new NotSupportedException();
        public void Dispose() => _inner.Dispose();
    }

    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
    public void Clear() { Root = null; Count = 0; }
    public bool Contains(KeyValuePair<TKey, TValue> item) => ContainsKey(item.Key);
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => throw new NotImplementedException();
    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);
}
