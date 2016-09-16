using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Tree {
    public class TreeNodeList<T> : List<ITreeNode<T>>, ITreeNodeList<T>, INotifyPropertyChanged {
        private ITreeNode<T> Parent { get; set; }
        
        public TreeNodeList(ITreeNode<T> parent) {
            Parent = parent;
        }

        public new ITreeNode<T> Add(ITreeNode<T> node) {
            return Add(node, true);
        }
        
        protected internal ITreeNode<T> Add(ITreeNode<T> node, bool updateParent) {
            if (updateParent) {
                node.SetParent(Parent, UpdateChildNodes: true);
                return node;
            }

            base.Add(node);
            OnPropertyChanged("Count");
            return node;
        }

        public new bool Remove(ITreeNode<T> node) {
            return Remove(node, true);
        }

        protected internal bool Remove(ITreeNode<T> node, bool updateParent) {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!Contains(node))
                return false;

            if (updateParent) {
                node.SetParent(null, UpdateChildNodes: false);
                return !Contains(node);
            }

            var result = base.Remove(node);
            OnPropertyChanged("Count");
            return result;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string PropertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
        public override string ToString() {
            return "Count = " + Count;
        }
    }
}
