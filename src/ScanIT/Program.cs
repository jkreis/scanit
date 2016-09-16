using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScanIT {
    public class Program {

        public static Dictionary<string, FileInfo> HashedFiles = new Dictionary<string, FileInfo>();
        public static Dictionary<FileInfo,List<FileInfo>> DuplicateFiles = new Dictionary<FileInfo,List<FileInfo>>();
        public static List<FileInfo> LargeFiles = new List<FileInfo>();
        private static Folder RootFolder;
        private static StringBuilder report;

        public static DateTime DateFlag { get; private set; }
        public static int FileSizeFlag { get; private set; }
        public static bool ScanOnlyFlag { get; private set; }
        public static bool HashFilesFlag { get; private set; }
        public static bool IgnoreExtensionsFlag { get; private set; }
        public static List<string> Extensions { get; private set; }

        /*  Args:
         *      usage: scanit <directory> [options]
         *      -d, -date               :Date to flag files (YYYY-MM-DD)
         *      -e, -extension          :Specify a list of extensions to only hash those files (Cannot be used with -i)
         *      -f, -filesize           :Size of files to flag in KB
         *      -h, -hash               :Hash files to flag duplicates
         *      -i, -ignore-extensions  :Ignore extensions when hashing files (So different file types could be flagged as duplicates)
         *      -s, -scan               :Scan only, don't flag files, this does not write out a report
         *      -?, -help               :Display a helpful message
         */
        public static void Main(string[] args) {
            //Check for -help and just print help message
            foreach (string arg in args) {
                if (arg == "-?" || arg == "-help") {
                    PrintHelpMessage();
                    return;
                }
            }

            string RootPath = "";

            for (int i = 0; i < args.Length; ) {
                switch (args[i]) {
                    case "-d":
                    case "-date":
                        if(args.Length <= i + 1) {
                            Console.WriteLine("Missing Value for Date");
                            PrintHelpMessage();
                            return;
                        }
                        try {
                            DateFlag = DateTime.Parse(args[i + 1]);
                        } catch (Exception e) {
                            Console.WriteLine(e.Message);
                            PrintHelpMessage();
                            return;
                        }
                        i += 2;
                        break;
                    case "-e":
                    case "-extension":
                        if (IgnoreExtensionsFlag) {
                            Console.WriteLine("-extension cannot be used with -ignore-extensions");
                            PrintHelpMessage();
                            return;
                        }
                        break;
                    case "-f":
                    case "-filesize":
                        FileSizeFlag = int.Parse(args[i + 1]) * 1024;
                        i += 2;
                        break;
                    case "-h":
                    case "-hash":
                        HashFilesFlag = true;
                        i++;
                        break;
                    case "-i":
                    case "-ignore-extensions":
                        IgnoreExtensionsFlag = true;
                        i++;
                        break;
                    case "-s":
                    case "-scan":
                        ScanOnlyFlag = true;
                        i++;
                        break;
                    default:
                        if (args[i].StartsWith("-")) {
                            Console.WriteLine("Invalid Option");
                            PrintHelpMessage();
                            return;
                        } else if (!Directory.Exists(args[i])) {
                            Console.WriteLine("Invalid directory path please try again.");
                            return;
                        }
                        RootPath = args[i];
                        i++;
                        break;
                }
            }
            RootFolder = new Folder(RootPath);
            BuildDirectoryTree(ref RootFolder);
            if (HashFilesFlag) HashFiles(RootFolder);
            if (!ScanOnlyFlag) {
                BuildReport();
                WriteReport("C:\\Users\\jkreis\\Desktop\\Template\\report.htm");
            }
            Console.WriteLine("Done");
        }

        static void PrintHelpMessage() {
            Console.WriteLine("Usage: scanit < directory > [options]");
            Console.WriteLine("\t-d, -date              :Date to flag files(YYYY-MM-DD)");
            Console.WriteLine("\t-e, -extension         :Specify a list of extensions to only hash those files(Cannot be used with - i)");
            Console.WriteLine("\t-f, -filesize          :Size of files to flag in KB");
            Console.WriteLine("\t-h, -hash              :Hash files to flag duplicates");
            Console.WriteLine("\t-i, -ignore-extensions :Ignore extensions when hashing files(So different file types could be flagged as duplicates)");
            Console.WriteLine("\t-s, -scan              :Scan only, don't flag files, this does not write out a report");
            Console.WriteLine("\t-?, -help              :Display this helpful message");
        }

        static void BuildDirectoryTree(ref Folder folder) {

            List<DirectoryInfo> subdirs = folder.Directory.EnumerateDirectories("*").ToList();
            Folder subFolder = null;

            foreach (DirectoryInfo subdir in subdirs) {
                Console.WriteLine(subdir.FullName);
                try {
                    subFolder = new Folder(subdir.FullName);
                    folder.Children.Add(subFolder);
                    BuildDirectoryTree(ref subFolder);
                } catch (UnauthorizedAccessException e) {
                    Console.WriteLine(e.Message);
                } catch (DirectoryNotFoundException e) {
                    Console.WriteLine(e.Message);
                }
            }
        }

        static bool HashFiles (Folder folder) {
            if (folder.Files.Count > 0) {
                List<Task> tasks = new List<Task>();
                foreach(FileInfo file in folder.Files) {
                    Task t = Task.Run(() => {
                        FileProcessor p = new FileProcessor(FileSizeFlag, IgnoreExtensionsFlag);
                        p.File = file;
                        p.ProcessFiles();
                    });
                    tasks.Add(t);
                }
                try {
                    Task.WaitAll(tasks.ToArray());
                } catch(Exception e) {
                    Console.WriteLine(e.Message);
                }
            }
            foreach(Folder f in folder.ChildNodes) {
                HashFiles(f);
            }
            return true;
        }

        public static void BuildReport() {
            report = new StringBuilder();
            report.AppendLine("<HTML>");
            report.AppendLine("<head>");
            report.AppendLine("<link rel=\"stylesheet\" href=\"bootstrap/css/bootstrap.css\">");
            report.AppendLine("</head>");
            report.AppendLine("<body class=\"panel panel-body\">");
            report.AppendLine("<h1>Report For: " + RootFolder.Directory + "</h1>");
            report.AppendLine("<div id=\"Root\">");
            report.AppendLine("<div class=\"panel\">");
            report.AppendLine("<ul class=\"nav nav-tabs\">");
            report.AppendLine("<li role=\"presentation\" class=\"active\"><a href=\"#largefiles\" role=\"tab\" data-toggle=\"tab\">Large Files</a></li>");
            report.AppendLine("<li role=\"presentation\"><a href=\"#duplicatefiles\" role=\"tab\" data-toggle=\"tab\">Duplicates</a></li>");
            report.AppendLine("<li role=\"presentation\"><a href=\"#oldfiles\" role=\"tab\" data-toggle=\"tab\">Old Files</a></li>");
            report.AppendLine("<li role=\"presentation\"><a href=\"#directorystats\" role=\"tab\" data-toggle=\"tab\">Directory Statistics</a></li>");
            report.AppendLine("</ul>");
            report.AppendLine("<div class=\"tab-content\">");
            if (FileSizeFlag != 0) {
                report.AppendLine("<div role=\"tabpanel\" class=\"tab-pane active\" id=\"largefiles\">");
                report.AppendLine("<table class=\"table table-bordered table-striped\" id=\"LargeFiles\">");
                report.AppendLine("<thead>");
                report.AppendLine("<tr>");
                report.AppendLine("<th>File Name</th>");
                report.AppendLine("<th>Size</th>");
                report.AppendLine("<th>Date Modified</th>");
                report.AppendLine("</tr>");
                report.AppendLine("</thead>");
                report.AppendLine("<tbody>");

                foreach(FileInfo file in LargeFiles) {
                    report.AppendLine("<tr>");
                    report.AppendLine("<td>" + file.FullName + "</td>");
                    report.AppendLine("<td>" + file.Length + "</td>");
                    report.AppendLine("<td>" + file.LastAccessTime + "</td>");
                    report.AppendLine("</tr>");
                }

                report.AppendLine("</tbody>");
                report.AppendLine("</table>");
                report.AppendLine("</div>");
            }
            report.AppendLine("<div role=\"tabpanel\" class=\"tab-pane\" id=\"duplicatefiles\">");
            //Make a separate table for each set of duplicate files.
                int counter = 0;
                report.AppendLine("<div class=\"panel-group\" id=\"accordion\" role=\"tablist\" aria-multiselectable=\"true\">");
                    foreach (FileInfo file in DuplicateFiles.Keys) {
                        report.AppendLine("<div class=\"panel panel-default\">");
                            report.AppendLine("<div class=\"panel-heading\" role=\"tab\" id=\"heading-" + counter + "\">");
                                report.AppendLine("<h4 class=\"panel-title\">");
                                    report.AppendLine("<a role=\"button\" data-toggle=\"collapse\" data-parent=\"#accordion\" href=\"#collapse-" + counter + "\" aria-expanded=\"true\" aria-controls=\"collapse-" + counter + "\">");
                                        report.AppendLine(file.Name + " -- " + (file.Length / 1024).ToString() + "KB");
                                    report.AppendLine("</a>");
                                report.AppendLine("</h4>");
                            report.AppendLine("</div>");
                            report.AppendLine("<div id=\"collapse-" + counter + "\" class=\"panel-collapse collapse in\" role=\"tabpanel\" aria-labelledby=\"heading-" + counter+"\">");
                                report.AppendLine("<div class=\"panel-body\">");
                                    report.AppendLine("<table class=\"table table-bordered table-striped\" id=\"Duplicates-" + counter + "\">");
                                        report.AppendLine("<thead>");
                                            report.AppendLine("<tr>");
                                                report.AppendLine("<th>File Name</th>");
                                                report.AppendLine("<th>Duplicate File Name</th>");
                                                report.AppendLine("<th>Size</th>");
                                                report.AppendLine("<th>Date Modified</th>");
                                            report.AppendLine("</tr>");
                                        report.AppendLine("</thead>");
                                        report.AppendLine("<tbody>");

                                        foreach (FileInfo f in DuplicateFiles[file]) {
                                                report.AppendLine("<tr>");
                                                report.AppendLine("<td>" + file.FullName + "</td>");
                                                report.AppendLine("<td>" + f.FullName + "</td>");
                                                report.AppendLine("<td>" + f.Length + "</td>");
                                                report.AppendLine("<td>" + f.LastAccessTime + "</td>");
                                                report.AppendLine("</tr>");
                                            }

                                        report.AppendLine("</tbody>");
                                    report.AppendLine("</table>");
                                report.AppendLine("</div>");
                            report.AppendLine("</div>");
                        report.AppendLine("</div>");
                        counter++;
                    }
                report.AppendLine("</div>");
                report.AppendLine("</div>");

                report.AppendLine("<div role=\"tabpanel\" class=\"tab-pane\" id=\"oldfiles\">");
                report.AppendLine("<table class=\"table table-bordered table-striped\" id=\"OldFiles\">");
                report.AppendLine("<thead>");
                report.AppendLine("<tr>");
                report.AppendLine("<th>File Name</th>");
                report.AppendLine("<th>Size</th>");
                report.AppendLine("<th>Date Modified</th>");
                report.AppendLine("</tr>");
                report.AppendLine("</thead>");
                report.AppendLine("<tbody>");

                ExpandTree(RootFolder);

                report.AppendLine("</tbody>");
                report.AppendLine("</table>");
                report.AppendLine("</div>");
            
            report.AppendLine("<div role=\"tabpanel\" class=\"tab-pane\" id=\"directorystats\">");
            report.AppendLine("<div>");
            report.AppendLine("<details>");
            report.AppendLine("<summary class=\"h4\">" + RootFolder.Directory.FullName + "</summary>");
            report.AppendLine("Total Files:");
            report.AppendLine("Total Size:");
            report.AppendLine("</details>");
            report.AppendLine("</div>");
            report.AppendLine("</div>");
            report.AppendLine("</div>");
            report.AppendLine("</div>");
            report.AppendLine("</body>");
            report.AppendLine("<footer>");
            report.AppendLine("<script src=\"https://ajax.googleapis.com/ajax/libs/jquery/1.11.3/jquery.min.js\"></script>");
            report.AppendLine("<script src=\"bootstrap/js/bootstrap.js\"></script>");
            report.AppendLine("</footer>");
            report.AppendLine("</HTML>");
        }

        public static void WriteReport(string path) {
            using (FileStream file = new FileStream(path, FileMode.Create)) {
                using (StreamWriter writer = new StreamWriter(file)) {
                    writer.Write(report.ToString());
                };
            };
        }

        private static void ExpandTree(Folder Node) {
            foreach (FileInfo f in Node.Files) {
                if (f.LastAccessTime.CompareTo(DateFlag) < 0) {
                    report.AppendLine("<tr>");
                    report.AppendLine("<td>" + f.FullName + "</td>");
                    report.AppendLine("<td>" + f.Length + "</td>");
                    report.AppendLine("<td>" + f.LastAccessTime + "</td>");
                    report.AppendLine("</tr>");
                }
            }
            foreach (Folder f in Node.Children) {
                ExpandTree(f);
            }
        }

    }
}
