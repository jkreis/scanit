using System;
using System.Collections.Generic;
using System.Linq;

namespace Tree {
    public class TreeNode<T> : ObservableObject, ITreeNode<T>, IDisposable {

        public ITreeNode<T> Root { get { return (Parent == null) ? this : Parent.Root; } }
        public ITreeNode<T> Parent { get; private set; }
        public TreeNodeList<T> Children { get; set; }

        public TreeNode() {
            Parent = null;
            Children = new TreeNodeList<T>(this);
        }

        public TreeNode(T value) {
            Value = value;
        }

        public TreeNode(T value, TreeNode<T> parent) {
            _Value = value;
            Parent = parent;
            Children = new TreeNodeList<T>(this);
        }

        public ITreeNode ParentNode {
            get { return Parent; }
        }

        public void SetParent(ITreeNode<T> node, bool updateChildren = true) {
            if (node == Parent)
                return;

            var oldParent = Parent;
            var oldParentHeight = Parent != null ? Parent.Height : 0;
            var oldDepth = Depth;

            if (oldParent != null && oldParent.Children.Contains(this))
                oldParent.Children.Remove(this, updateParent: false);

            Parent = node;

            if (Parent != null && updateChildren)
                Parent.Children.Add(this, updateParent: false);

            if (oldParent != null)
                oldParent.OnDescendantChanged(NodeChangeType.NodeRemoved, this);

            if (oldDepth != Depth)
                OnDepthChanged();

            if (Parent != null) {
                var newParentHeight = Parent != null ? Parent.Height : 0;
                if (newParentHeight != oldParentHeight)
                    Parent.OnHeightChanged();

                Parent.OnDescendantChanged(NodeChangeType.NodeAdded, this);
            }

            OnParentChanged(oldParent, Parent);
        }

        protected virtual void OnParentChanged(ITreeNode<T> oldValue, ITreeNode<T> newValue) {
            OnPropertyChanged("Parent");
        }

        public IEnumerable<ITreeNode> ChildNodes {
            get {
                foreach (ITreeNode node in Children)
                    yield return node;

                yield break;
            }
        }

        public IEnumerable<ITreeNode> Descendants {
            get {
                foreach(ITreeNode node in ChildNodes) {
                    yield return node;
                    foreach (ITreeNode descendant in node.Descendants)
                        yield return descendant;
                }
                yield break;
            }
        }

        public IEnumerable<ITreeNode> Subtree {
            get {
                yield return this;

                foreach (ITreeNode node in Descendants)
                    yield return node;

                yield break;
            }
        }

        public IEnumerable<ITreeNode> Ancestors {
            get {
                if (Parent == null)
                    yield break;

                yield return Parent;

                foreach (ITreeNode node in Parent.Ancestors)
                    yield return node;

                yield break;
            }
        }

        public event Action<NodeChangeType, ITreeNode> AncestorChanged;
        public virtual void OnAncestorChanged(NodeChangeType changeType, ITreeNode node) {
            AncestorChanged?.Invoke(changeType, node);

            foreach (ITreeNode<T> child in Children)
                child.OnAncestorChanged(changeType, node);
        }

        public event Action<NodeChangeType, ITreeNode> DescendantChanged;
        public virtual void OnDescendantChanged(NodeChangeType changeType, ITreeNode node) {
            DescendantChanged?.Invoke(changeType, node);

            if (Parent != null)
                Parent.OnDescendantChanged(changeType, node);
        }

        public int Height {
            get { return Children.Count == 0 ? 0 : Children.Max(n => n.Height) + 1; }
        }

        public virtual void OnHeightChanged() {
            OnPropertyChanged("Height");

            foreach(ITreeNode<T> child in Children) {
                child.OnHeightChanged();
            }
        }

        private T _Value;
        public T Value {
            get { return _Value; }
            set {
                if (value == null && _Value == null)
                    return;
                if (value != null && _Value != null && value.Equals(_Value))
                    return;

                _Value = value;
                OnPropertyChanged("Value");
            }
        }

        public int Depth {
            get { return (Parent == null ? 0 : Parent.Depth + 1); }
        }

        public virtual void OnDepthChanged() {
            OnPropertyChanged("Depth");
            if (Parent != null)
                Parent.OnDepthChanged();
        }

        public UpDownTraversalType DisposeTraversal { get; set; }
        public bool IsDisposed { get; private set; }
        public virtual void Dispose() {
            CheckDisposed();
            OnDisposing();

            if(Value is IDisposable) {
                if (DisposeTraversal == UpDownTraversalType.BottomUp) {
                    foreach (TreeNode<T> node in Children)
                        node.Dispose();
                }
            }

            (Value as IDisposable).Dispose();

            if(DisposeTraversal == UpDownTraversalType.TopDown) {
                foreach (TreeNode<T> node in Children)
                    node.Dispose();
            }
            IsDisposed = true;
        }

        public event EventHandler Disposing;

        protected void OnDisposing() {
            if (Disposing != null)
                Disposing(this, EventArgs.Empty);
        }

        public void CheckDisposed() {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        public override string ToString() {
            return "Depth = " + Depth + ", Height = " + Height + ", Children = " + Children.Count;
        }

    }
}