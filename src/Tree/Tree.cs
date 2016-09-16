namespace Tree {
    public class Tree<T> : TreeNode<T> {
        public Tree() { }
        public Tree(T RootValue) {
            Value = RootValue;
        }
    }
}
