using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

namespace ScanIT {
    public class FileProcessor {

        public FileInfo File;
        private int FileSize { get; set; }
        private bool IgnoreExtensions { get; set; }

        private ManualResetEvent _doneEvent;

        public FileProcessor(int fileSize, bool ignoreExtensions) {
            FileSize = fileSize;
            IgnoreExtensions = ignoreExtensions;
        }


        public FileProcessor(int fileSize, bool ignoreExtensions, ManualResetEvent doneEvent) {
            FileSize = fileSize;
            IgnoreExtensions = ignoreExtensions;
            _doneEvent = doneEvent;
        }

        public void ThreadPoolCallback(Object threadContext) {
            int threadIndex = (int)threadContext;
            ProcessFiles();
            _doneEvent.Set();
        }
        

        public void ProcessFiles() {
            if (File != null && File.Extension != ".sys") {
                Console.WriteLine("Processing File: " + File.FullName);
                string hash = "";
                if (FileSize != 0 && File.Length >= FileSize) {
                    Console.WriteLine(File.Name + " over " + (FileSize/1024).ToString() + "KB skipping hash");
                    Program.LargeFiles.Add(File);
                } else {
                    hash = Hash(File);
                    try {
                        lock (Program.HashedFiles) {
                            lock (Program.DuplicateFiles) {
                                if (hash != null) {
                                    if (Program.HashedFiles.ContainsKey(hash)) {
                                        if (IgnoreExtensions || (!IgnoreExtensions && Program.HashedFiles[hash].Extension == File.Extension)) {
                                            if (!Program.DuplicateFiles.ContainsKey(Program.HashedFiles[hash]))
                                                Program.DuplicateFiles.Add(Program.HashedFiles[hash], new List<FileInfo>());
                                            Program.DuplicateFiles[Program.HashedFiles[hash]].Add(File);
                                        }
                                    } else {
                                        Program.HashedFiles.Add(hash, File);
                                    }
                                }
                            }
                        }
                    } catch (Exception e) {
                        Console.WriteLine(e.GetBaseException().Message);
                        Console.WriteLine(e.StackTrace);
                        Console.ReadLine();
                    }
                }
            }
        }

        private string Hash(FileInfo file) {
            byte[] fileContents = null;
            try {
                FileStream fs = file.OpenRead();
                MemoryStream ms = new MemoryStream();
                string hashstring = "";
                fs.CopyTo(ms);
                fileContents = ms.ToArray();

                ms.Dispose();
                fs.Dispose();

                SHA1 sha = SHA1.Create();
                byte[] hash = sha.ComputeHash(fileContents);
                sha.Dispose();

                foreach (byte b in hash) {
                    hashstring += b.ToString("X");
                }
                return hashstring;
            } catch (DirectoryNotFoundException e) {
                Console.WriteLine(e.Message);
                return null;
            } catch (UnauthorizedAccessException e) {
                Console.WriteLine(e.Message);
                return null;
            } catch (IOException e) {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
}
