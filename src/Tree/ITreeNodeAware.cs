namespace Tree {
    public interface ITreeNodeAware<T> where T : new() {
        TreeNode<T> Node { get; set; }
    }
}
