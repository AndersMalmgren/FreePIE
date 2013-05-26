using System;
using System.Collections.Generic;
using System.Linq;

namespace FreePIE.Core.Model
{
    public class Node<T>
    {
        private readonly List<Node<T>> children;

        public Node(T value) : this()
        {
            Value = value;
        }

        public Node()
        {
            this.children = new List<Node<T>>();
        }

        public IEnumerable<Node<T>> AddChildren(IEnumerable<T> children)
        {
            var newChildren = children.Select(child => new Node<T>(child)).ToList();

            this.children.AddRange(newChildren);

            return newChildren;
        }

        public void AddChildren(IEnumerable<Node<T>> nodes)
        {
            this.children.AddRange(nodes);
        }

        public Node<T> AddChild(T value)
        {
            var newChild = new Node<T>(value);

            this.children.Add(newChild);

            return newChild;
        }

        public Node<T> AddChild(Node<T> child)
        {
            this.children.Add(child);

            return child;
        }

        public T Value { get; set; }

        public void SortChildrenRecursive(Comparison<T> comparer)
        {
            children.Sort((a, b) => comparer(a.Value, b.Value));

            children.ForEach(x => x.SortChildrenRecursive(comparer));
        }

        public IEnumerable<Node<T>> Children { get { return children; } }

        public Node<T> FindSequence<TKey>(IEnumerable<TKey> sequence, Func<T, TKey, bool> equals)
        {
            var list = sequence.ToList();

            if (list.Count == 0)
                return this;

            Node<T> current = this;

            foreach (TKey t in list)
            {
                current = current.children.FirstOrDefault(c => equals(c.Value, t));

                if (current == null)
                    return null;
            }

            return current;
        }

        public override string ToString()
        {
            return object.Equals(Value, default(T)) ? "Default value" : Value.ToString();
        }
    }
}
