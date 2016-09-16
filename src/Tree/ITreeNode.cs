using System.Collections.Generic;
using System.ComponentModel;

namespace Tree {
    public interface ITreeNode<T> : ITreeNode {
        ITreeNode<T> Root { get; }
        ITreeNode<T> Parent { get; }
        void SetParent(ITreeNode<T> Node, bool UpdateChildNodes = true);

        T Value { get; set; }

        TreeNodeList<T> Children { get; }
    }

    public interface ITreeNode : INotifyPropertyChanged {
        IEnumerable<ITreeNode> Ancestors { get; }
        ITreeNode ParentNode { get; }

        IEnumerable<ITreeNode> ChildNodes { get; }
        IEnumerable<ITreeNode> Descendants { get; }

        void OnAncestorChanged(NodeChangeType changeType, ITreeNode node);
        void OnDescendantChanged(NodeChangeType changeType, ITreeNode node);

        int Depth { get; }
        void OnDepthChanged();

        int Height { get; }
        void OnHeightChanged();
    }
}
