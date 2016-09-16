using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tree;

namespace ScanIT {
    public class Folder : TreeNode<Folder> {

        public DirectoryInfo Directory { get; set; }
        public List<FileInfo> Files { get; set; }

        public Folder(string path) {
            Directory = new DirectoryInfo(path);
            Files = Directory.GetFiles().ToList();
        }
    }
}
