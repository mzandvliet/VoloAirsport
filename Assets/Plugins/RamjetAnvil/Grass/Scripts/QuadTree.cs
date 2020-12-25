using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

/*
 - Todo: Tree traversal using stack
 - Todo: Tree as flat array
 - Todo: Bonus points: Is it worth the effort to support worlds larger than quadtree range, or do we stick with one QT that covers the entire level. (Is the only other solution a sparse quad tree?)
    - I guess you could store a sparse quadtree in world coordinates, and then for streaming convert to local scene coordinates. The key is a sparse tree.
    - Sparse trees would also really cut down on tree traversal time!!!
*/

namespace RamjetAnvil.Unity.Utility {
    
    public static class QuadTreeUtils {

        public static QuadTree<T> Create<T>(int size, int patchSize, Vector2 posMin, Func<QtNode<T>, T> constructor) {
            QuadTree<T> tree = new QuadTree<T>(posMin, size, patchSize, constructor);

            return tree;
        }

        public static void Traverse<T, TData>(this QuadTree<T> tree, QtNodeVisitor<T, TData> visitor, TData data) {
            Traverse(tree, 0, visitor, data);
        }

        public static void Traverse<T, TData>(QuadTree<T> tree, int nodeIndex, QtNodeVisitor<T, TData> visitor, TData data) {
            var node = tree[nodeIndex];
            if (!visitor(node, data) || !node.HasChildren) {
                return;
            }

            for (int i = 0; i < 4; i++) {
                Traverse(tree, node.Children[i], visitor, data);
            }
        }

        private static readonly int[] Stack = new int[512];
        private static int _stackPointer = -1;

        private static void Push(int val) {
            _stackPointer++;
            Stack[_stackPointer] = val;
        }

        private static int Pop() {
            int val = Stack[_stackPointer];
            _stackPointer--;
            return val;
        }

        private static void Clear() {
            _stackPointer = -1;
        }

        public static void TraverseWithStack<T, TData>(this QuadTree<T> tree, QtNodeVisitor<T, TData> visitor, TData data) {
            Clear();
            Push(0);

            while (_stackPointer >= 0) {
                var nodeIndex = Pop();
                var node = tree[nodeIndex];

                if (!visitor(node, data) || !node.HasChildren) {
                    continue;
                }

                for (int i = 0; i < 4; i++) {
                    Push(node.Children[i]);
                }
            }
        }
    }

    public delegate bool QtNodeVisitor<T, Y>(QtNode<T> node, Y data);

    public class QuadTree<T> {
        private readonly QtNode<T>[] _nodes;
        public readonly int MaxDepth;
        private readonly Func<QtNode<T>, T> _constructor;

        public QtNode<T> Root {
            get { return _nodes[0]; }
        }

        public QtNode<T> this[int i] {
            get { return _nodes[i]; }
        } 

        public QuadTree(Vector2 posMin, int size, int patchSize, Func<QtNode<T>, T> constructor) {
            //log2(1024*8/32)
            int numPatchesAtLod0 = size / patchSize;
            if (!Mathf.IsPowerOfTwo(numPatchesAtLod0)) {
                throw new ArgumentException("Ratio of size/patchSize must be power-of-two");
            }
            MaxDepth = (int)Math.Log(numPatchesAtLod0, 2);
            _constructor = constructor;

            int totalNodes = CalculateNumNodes(MaxDepth);

            Debug.Log(
                "QuadTree || Depth: " + MaxDepth +
                ", Total Nodes: " + totalNodes +
                ", Total Mem: " + totalNodes * QtNode<T>.SizeBytes / 1024 + "KB");

            _nodes = new QtNode<T>[totalNodes];

            int i = 0;
            QtNode<T> root = CreateNode(ref i, 0, IntVector2.Zero, posMin, size);
            ExpandRecursively(root, ref i);
        }

        private static int CalculateNumNodes(int maxDepth) {
            // Examples:
            // sum 4^j, j=0 to 8

            if (maxDepth < 1) {
                maxDepth = 1;
                Debug.LogWarning("Tree always has at least 1 layer; the root node");
            }
            
            int totalNodes = 0;
            for (int i = 0; i <= maxDepth; i++) {
                totalNodes += (int)Math.Pow(4, i);
            }
            return totalNodes;
        }

        private void ExpandRecursively(QtNode<T> node, ref int i) {
            if (node.Depth == MaxDepth) {
                return;
            }

            int depth = node.Depth + 1;
            float size = node.Size / 2f;

            for (int x = 0; x < 2; x++) {
                for (int y = 0; y < 2; y++) {
                    var childCoord = node.Coord * 2 + new IntVector2(x, y);
                    var childPosition = node.Position + new Vector2(size * x, size * y);

                    var child = CreateNode(ref i, depth, childCoord, childPosition, size);

                    int localChildIndex = x * 2 + y;
                    node.Children[localChildIndex] = i-1;

                    ExpandRecursively(child, ref i);
                }
            }
        }

        private QtNode<T> CreateNode(ref int i, int depth, IntVector2 childCoord, Vector2 childPosition, float size) {
            var child = new QtNode<T>(depth, childCoord, childPosition, size);
            child.Value = _constructor(child);
            _nodes[i++] = child;
            return child;
        }
    }

    public class QtNode<T> {
        // Note: ALL data except T value could potentially be made implicit and evaluated on traversal
        public readonly int[] Children; 
        private T _value;
        public int Depth;
        public IntVector2 Coord;
        public Vector2 Position;
        public float Size;

        public const int SizeBytes = 44;

        public bool HasChildren {
            get { return Children[0] != -1; }
        }

        public T Value {
            get { return _value; }
            set { _value = value; }
        }

        public Vector2 GetCenter() {
            float halfSize = Size * 0.5f;
            return new Vector2(Position.x + halfSize, Position.y + halfSize);
        }

        public QtNode(int depth, IntVector2 coord, Vector2 position, float size) {
            Depth = depth;
            Coord = coord;
            Position = position;
            Size = size;

            Children = new [] {-1, -1, -1, -1};
        }

    }
}
