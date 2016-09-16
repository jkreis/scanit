using System.Collections.Generic;
using System.ComponentModel;

namespace Tree {
    public interface ITreeNodeList<T> : IList<ITreeNode<T>>, INotifyPropertyChanged {
        new ITreeNode<T> Add(ITreeNode<T> node);
    }
}
