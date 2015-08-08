using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Veeam.IntroductoryAssignment.Util
{
    class AVLTree<T>
        where T : IComparable<T>
    {
        private Node _root;

        class Node
        {
            private T _key;

            public Node LeftChild { get; set; }
            public Node RightChild { get; set; }
            public int Height { get; set; }

            public Node(T key)
            {
                _key = key;
                Height = 1;
            }

            public T GetKey()
            {
                return _key;
            }
        }

        public int Count { get; private set; }

        private int GetHeight(Node node)
        {
            return node != null ? node.Height : 0;
        }

        private int GetBalanceFactor(Node node)
        {
            return GetHeight(node.RightChild) - GetHeight(node.LeftChild);
        }

        private void FixHeight(Node node)
        {
            var leftHeight = GetHeight(node.LeftChild);
            var rightHeight = GetHeight(node.RightChild);
            node.Height = Math.Max(leftHeight, rightHeight) + 1;
        }

        private Node RotateRight(Node p)
        {
            var q = p.LeftChild;
            p.LeftChild = q.RightChild;
            q.RightChild = p;
            FixHeight(p);
            FixHeight(q);
            return q;
        }

        private Node RotateLeft(Node p)
        {
            var q = p.RightChild;
            p.RightChild = q.LeftChild;
            q.LeftChild = p;
            FixHeight(p);
            FixHeight(q);
            return q;
        }

        private Node Balance(Node node)
        {
            FixHeight(node);
            if (GetBalanceFactor(node) == 2)
            {
                if (GetBalanceFactor(node.RightChild) < 0)
                {
                    node.RightChild = RotateRight(node.RightChild);
                }
                return RotateLeft(node);
            }
            if (GetBalanceFactor(node) == -2)
            {
                if (GetBalanceFactor(node.LeftChild) > 0)
                {
                    node.LeftChild = RotateLeft(node.LeftChild);
                }
                return RotateRight(node);
            }
            return node;
        }

        private Node Insert(Node node, T key)
        {
            if (node == null) return new Node(key);
            if (key.CompareTo(node.GetKey()) < 0)
            {
                node.LeftChild = Insert(node.LeftChild, key);
            }
            else
            {
                node.RightChild = Insert(node.RightChild, key);
            }
            return Balance(node);
        }

        public void Insert(T key)
        {
            _root = Insert(_root, key);
            Count++;
        }

        private Node FindMin(Node node)
        {
            return node.LeftChild != null ? FindMin(node.LeftChild) : node;
        }

        private Node RemoveMin(Node node)
        {
            if (node == null) return null;
            if (node.LeftChild == null)
            {
                Count--;
                return node.RightChild;
            }
            node.LeftChild = RemoveMin(node.LeftChild);
            return Balance(node);
        }

        public void RemoveMin()
        {
            _root = RemoveMin(_root);
        }

        private Node Remove(Node node, T key)
        {
            if (node == null) return null;
            if (key.CompareTo(node.GetKey()) < 0)
            {
                node.LeftChild = Remove(node.LeftChild, key);
            }
            else if (key.CompareTo(node.GetKey()) > 0)
            {
                node.RightChild = Remove(node.RightChild, key);
            }
            else
            {
                var leftChild = node.LeftChild;
                var rightChild = node.RightChild;
                if (rightChild == null) return leftChild;
                var min = FindMin(rightChild);
                min.RightChild = RemoveMin(rightChild);
                min.LeftChild = leftChild;
                return Balance(min);
            }
            return Balance(node);
        }

        public void Remove(T key)
        {
            _root = Remove(_root, key);
        }

        public T Min
        {
            get
            {
                return _root != null ? FindMin(_root).GetKey() : default(T);
            }
        }

        private void Traverse(Node node, IList<T> keys)
        {
            if (node == null) return;
            Traverse(node.RightChild, keys);
            keys.Add(node.GetKey());
            Traverse(node.LeftChild, keys);
        }

        public List<T> GetKeys()
        {
            var keys = new List<T>();
            Traverse(_root, keys);
            return keys;
        }
    }
}
