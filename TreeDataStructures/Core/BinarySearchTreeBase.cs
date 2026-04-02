using System.Collections;
using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Interfaces;

namespace TreeDataStructures.Core;

public abstract class BinarySearchTreeBase<TKey, TValue, TNode>(IComparer<TKey>? comparer = null) 
    : ITree<TKey, TValue>
    where TNode : Node<TKey, TValue, TNode>
{
    protected TNode? Root;
    public IComparer<TKey> Comparer { get; protected set; } = comparer ?? Comparer<TKey>.Default; // use it to compare Keys

    public int Count { get; protected set; }
    
    public bool IsReadOnly => false;

    public ICollection<TKey> Keys => throw new NotImplementedException();
    public ICollection<TValue> Values => throw new NotImplementedException();
    
    
    public virtual void Add(TKey key, TValue value)
    {
        if (Root == null)
        {
            Root = CreateNode(key, value);
            Count++;
            OnNodeAdded(Root);
            return;
        }

        TNode? current = Root;
        TNode? parent = null;

        while (current != null)
        {
            int cmp = Comparer.Compare(key, current.Key);
            parent = current;
            if (cmp < 0) current = current.Left;
            else if (cmp > 0) current = current.Right;
            else if (cmp == 0)
            {
                ArgumentException ex = new($"Key {key} is a dopelganger");
                throw ex;
            }
        }
        int cmp2 = Comparer.Compare(key, parent.Key);
        TNode newNode = CreateNode(key, value);
        newNode.Parent = parent;
        
        if (cmp2 < 0) parent.Left = newNode;
        else if (cmp2 > 0) parent.Right = newNode;
        else
        {
            ArgumentException ex = new($"Key {key} is a dopelganger");
            throw ex;
        }

        Count++;
        OnNodeAdded(newNode);
    }

    
    public virtual bool Remove(TKey key)
    {
        TNode? node = FindNode(key);
        if (node == null) { return false; }

        RemoveNode(node);
        this.Count--;
        return true;
    }
    
    
    protected virtual void RemoveNode(TNode node)
    {
        if (node.Left == null){
            Transplant(node, node.Right);
            OnNodeRemoved(node.Parent, node.Right);
        }
        else if (node.Right == null)
        {
            Transplant(node, node.Left);
            OnNodeRemoved(node.Parent, node.Left);
        }
        else {
            TNode successor = node.Right;
            while (successor.Left != null){
                successor = successor.Left;
            }
            TNode? orSuccessorParent = successor.Parent;
            if (successor.Parent != node) {
                Transplant(successor, successor.Right);
                successor.Right = node.Right;
                successor.Right.Parent = successor;
            }
            Transplant(node, successor);
            successor.Left = node.Left;
            successor.Left.Parent = successor;
            OnNodeRemoved(orSuccessorParent, successor);
        }
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
    
    /// <summary>
    /// Вызывается после успешной вставки
    /// </summary>
    /// <param name="newNode">Узел, который встал на место</param>
    protected virtual void OnNodeAdded(TNode newNode) { }
    
    /// <summary>
    /// Вызывается после удаления. 
    /// </summary>
    /// <param name="parent">Узел, чей ребенок изменился</param>
    /// <param name="child">Узел, который встал на место удаленного</param>
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

    protected void RotateLeft(TNode x)
    {
        throw new NotImplementedException();
    }

    protected void RotateRight(TNode y)
    {
        throw new NotImplementedException();
    }
    
    protected void RotateBigLeft(TNode x)
    {
        throw new NotImplementedException();
    }
    
    protected void RotateBigRight(TNode y)
    {
        throw new NotImplementedException();
    }
    
    protected void RotateDoubleLeft(TNode x)
    {
        throw new NotImplementedException();
    }
    
    protected void RotateDoubleRight(TNode y)
    {
        throw new NotImplementedException();
    }
    
    protected void Transplant(TNode u, TNode? v)
    {
        if (u.Parent == null)
        {
            Root = v;
        }
        else if (u.IsLeftChild)
        {
            u.Parent.Left = v;
        }
        else
        {
            u.Parent.Right = v;
        }
        v?.Parent = u.Parent;
    }
    #endregion
    
    public IEnumerable<TreeEntry<TKey, TValue>>  InOrder() => InOrderTraversal(Root);
    
    private IEnumerable<TreeEntry<TKey, TValue>>  InOrderTraversal(TNode? node)
    {
        return new TreeIterator(node, TraversalStrategy.InOrder);
    }
    
    public IEnumerable<TreeEntry<TKey, TValue>>  PreOrder() => PreOrderTraversal(Root);

    private IEnumerable<TreeEntry<TKey, TValue>>  PreOrderTraversal(TNode? node)
    {
        return new TreeIterator(node, TraversalStrategy.PreOrder);
    }

    public IEnumerable<TreeEntry<TKey, TValue>>  PostOrder() => PostOrderTraversal(Root);

    private IEnumerable<TreeEntry<TKey, TValue>>  PostOrderTraversal(TNode? node)
    {
        return new TreeIterator(node, TraversalStrategy.PostOrder);
    }

    public IEnumerable<TreeEntry<TKey, TValue>>  InOrderReverse() => InOrderReverseTraversal(Root);

    private IEnumerable<TreeEntry<TKey, TValue>>  InOrderReverseTraversal(TNode? node)
    {
        return new TreeIterator(node, TraversalStrategy.InOrderReverse);
    }

    public IEnumerable<TreeEntry<TKey, TValue>>  PreOrderReverse() => PreOrderReverseTraversal(Root);

    private IEnumerable<TreeEntry<TKey, TValue>>  PreOrderReverseTraversal(TNode? node)
    {
        return new TreeIterator(node, TraversalStrategy.PreOrderReverse);
    }

    public IEnumerable<TreeEntry<TKey, TValue>>  PostOrderReverse() => PostOrderReverseTraversal(Root);

    private IEnumerable<TreeEntry<TKey, TValue>>  PostOrderReverseTraversal(TNode? node)
    {
        return new TreeIterator(node, TraversalStrategy.PostOrderReverse);
    }
    
    /// <summary>
    /// Внутренний класс-итератор. 
    /// Реализует паттерн Iterator вручную, без yield return (ban).
    /// </summary>
    private struct TreeIterator : 
        IEnumerable<TreeEntry<TKey, TValue>>,
        IEnumerator<TreeEntry<TKey, TValue>>
    {
        private readonly TraversalStrategy _strategy;
        private readonly TNode? _startNode;
        private Stack<(TNode node, int depth)>? _stack;
        private Stack<(TNode node, int depth, bool visited)>? _postOrderStack;
        private TreeEntry<TKey, TValue> _current;

        public TreeIterator(TNode? startNode, TraversalStrategy strategy)
        {
            _startNode = startNode;
            _strategy = strategy;
            _stack = null;
            _postOrderStack = null;
            _current = default;
        }

        public IEnumerator<TreeEntry<TKey, TValue>> GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
        
        public TreeEntry<TKey, TValue> Current => _current;
        object IEnumerator.Current => Current;
        
        
        public bool MoveNext()
        {
            if (_stack == null && _postOrderStack == null)
            {
                Reset();
            }

            if (_strategy == TraversalStrategy.PostOrder || _strategy == TraversalStrategy.PostOrderReverse)
            {
                if (_postOrderStack == null) return false;
                
                while (_postOrderStack.Count > 0)
                {
                    var (node, depth, visited) = _postOrderStack.Pop();
                    
                    if (visited)
                    {
                        _current = new TreeEntry<TKey, TValue>(node.Key, node.Value, depth);
                        return true;
                    }
                    
                    _postOrderStack.Push((node, depth, true));
                    
                    if (_strategy == TraversalStrategy.PostOrder)
                    {
                        if (node.Right != null) _postOrderStack.Push((node.Right, depth + 1, false));
                        if (node.Left != null) _postOrderStack.Push((node.Left, depth + 1, false));
                    }
                    else
                    {
                        if (node.Left != null) _postOrderStack.Push((node.Left, depth + 1, false));
                        if (node.Right != null) _postOrderStack.Push((node.Right, depth + 1, false));
                    }
                }
                return false;
            }

            if (_stack == null) return false;

            if (_strategy == TraversalStrategy.PreOrder || _strategy == TraversalStrategy.PreOrderReverse)
            {
                if (_stack.Count == 0) return false;
                
                var (node, depth) = _stack.Pop();
                _current = new TreeEntry<TKey, TValue>(node.Key, node.Value, depth);
                
                if (_strategy == TraversalStrategy.PreOrder)
                {
                    if (node.Right != null) _stack.Push((node.Right, depth + 1));
                    if (node.Left != null) _stack.Push((node.Left, depth + 1));
                }
                else
                {
                    if (node.Left != null) _stack.Push((node.Left, depth + 1));
                    if (node.Right != null) _stack.Push((node.Right, depth + 1));
                }
                
                return true;
            }
            
            if (_strategy == TraversalStrategy.InOrder || _strategy == TraversalStrategy.InOrderReverse)
            {
                if (_stack.Count == 0) return false;
                
                var (node, depth) = _stack.Pop();
                _current = new TreeEntry<TKey, TValue>(node.Key, node.Value, depth);
                
                var curr = _strategy == TraversalStrategy.InOrder ? node.Right : node.Left;
                var currentDepth = depth + 1;
                
                while (curr != null)
                {
                    _stack.Push((curr, currentDepth));
                    curr = _strategy == TraversalStrategy.InOrder ? curr.Left : curr.Right;
                    currentDepth++;
                }
                
                return true;
            }

            return false;
        }
        
        public void Reset()
        {
            if (_startNode == null)
            {
                _stack = new Stack<(TNode, int)>();
                _postOrderStack = new Stack<(TNode, int, bool)>();
                return;
            }

            if (_strategy == TraversalStrategy.PostOrder || _strategy == TraversalStrategy.PostOrderReverse)
            {
                _postOrderStack = new Stack<(TNode node, int depth, bool visited)>();
                _postOrderStack.Push((_startNode, 0, false));
            }
            else if (_strategy == TraversalStrategy.PreOrder || _strategy == TraversalStrategy.PreOrderReverse)
            {
                _stack = new Stack<(TNode node, int depth)>();
                _stack.Push((_startNode, 0));
            }
            else
            {
                _stack = new Stack<(TNode node, int depth)>();
                var curr = _startNode;
                int currentDepth = 0;
                while (curr != null)
                {
                    _stack.Push((curr, currentDepth));
                    curr = _strategy == TraversalStrategy.InOrder ? curr.Left : curr.Right;
                    currentDepth++;
                }
            }
        }

        public void Dispose()
        {
            // TODO release managed resources here
        }
    }
    
    
    private enum TraversalStrategy { InOrder, PreOrder, PostOrder, InOrderReverse, PreOrderReverse, PostOrderReverse }
    
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        throw new NotImplementedException();
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
    public void Clear() { Root = null; Count = 0; }
    public bool Contains(KeyValuePair<TKey, TValue> item) => ContainsKey(item.Key);
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => throw new NotImplementedException();
    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);
}