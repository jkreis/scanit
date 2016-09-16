using System.IO;
using System.Text;

namespace ScanIT {
    public static class ReportBuilder {

        private static StringBuilder report;

        public static void BuildReport(Folder folder, bool large, bool dups, bool old) {
            report = new StringBuilder();
            report.Append("<HTML>");
                report.Append("<head>");
                    report.Append("<link rel=\"stylesheet\" href=\"bootstrap/css/bootstrap.css\">");
                report.Append("</head>");
                report.Append("<body class=\"panel panel-body\">");
                    report.Append("<h1>Report For ");
                    report.Append(folder.Directory + "</h1>");
                    report.Append("<div id=\"Root\">");
                        report.Append("<div class=\"panel\">");
                            report.Append("<ul class=\"nav nav-tabs\">");
                                report.Append("<li role=\"presentation\" class=\"active\"><a href=\"#largefiles\" role=\"tab\" data-toggle=\"tab\">Large Files</a></li>");
                                report.Append("<li role=\"presentation\"><a href=\"#duplicatefiles\" role=\"tab\" data-toggle=\"tab\">Duplicates</a></li>");
                                report.Append("<li role=\"presentation\"><a href=\"#oldfiles\" role=\"tab\" data-toggle=\"tab\">Old Files</a></li>");
                                report.Append("<li role=\"presentation\"><a href=\"#directorystats\" role=\"tab\" data-toggle=\"tab\">Directory Statistics</a></li>");
                            report.Append("</ul>");
                            report.Append("<div class=\"tab-content\">");
                            if (large) {
                                report.Append("<div role=\"tabpanel\" class=\"tab-pane active\" id=\"largefiles\">");
                                    report.Append("<table class=\"table table-bordered table-striped\" id=\"LargeFiles\">");
                                        report.Append("<thead>");
                                                report.Append("<tr>");
                                                report.Append("<th>File Name</th>");
                                                report.Append("<th>Size</th>");
                                                report.Append("<th>Date Modified</th>");
                                            report.Append("</tr>");
                                        report.Append("</thead>");
                                        report.Append("<tbody>");

                                            ExpandTree(folder);

                                        report.Append("</tbody>");
                                    report.Append("</table>");
                                report.Append("</div>");
                            }
                            if (dups) {
                                report.Append("<div role=\"tabpanel\" class=\"tab-pane\" id=\"duplicatefiles\">");
                                    report.Append("<table class=\"table table-bordered table-striped\" id=\"DuplicateFiles\">");
                                        report.Append("<thead>");
                                            report.Append("<tr>");
                                                report.Append("<th>File Name</th>");
                                                report.Append("<th>Size</th>");
                                                report.Append("<th>Date Modified</th>");
                                            report.Append("</tr>");
                                        report.Append("</thead>");
                                        report.Append("<tbody>");
                                        //For Loop add rows to Table
                                        report.Append("</tbody>");
                                    report.Append("</table>");
                                report.Append("</div>");
                            }
                            if (old) {
                                report.Append("<div role=\"tabpanel\" class=\"tab-pane\" id=\"oldfiles\">");
                                    report.Append("<table class=\"table table-bordered table-striped\" id=\"OldFiles\">");
                                        report.Append("<thead>");
                                            report.Append("<tr>");
                                                report.Append("<th>File Name</th>");
                                                report.Append("<th>Size</th>");
                                                report.Append("<th>Date Modified</th>");
                                            report.Append("</tr>");
                                        report.Append("</thead>");
                                        report.Append("<tbody>");

                                        ExpandTree(folder);

                                        report.Append("</tbody>");
                                    report.Append("</table>");
                                report.Append("</div>");
                            }
                                report.Append("<div role=\"tabpanel\" class=\"tab-pane\" id=\"directorystats\">");
                                    report.Append("<div>");
                                        report.Append("<details>");
                                            report.Append("<summary class=\"h4\">" + folder.Directory.FullName +"</summary>");
                                            report.Append("Total Files:");
                                            report.Append("Total Size:");
                                        report.Append("</details>");
                                    report.Append("</div>");
                                report.Append("</div>");
                            report.Append("</div>");
                        report.Append("</div>");
                    report.Append("</div>");
                report.Append("</body>");
                report.Append("<footer>");
		            report.Append("<script src=\"https://ajax.googleapis.com/ajax/libs/jquery/1.11.3/jquery.min.js\"></script>");
                    report.Append("<script src=\"bootstrap/js/bootstrap.js\"></script>");
                report.Append("</footer>");
            report.Append("</HTML>");
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
                    report.Append("<tr>");
                    report.Append("<td>" + f.FullName + "</td>");
                    report.Append("<td>" + f.Length + "</td>");
                    report.Append("<td>" + f.LastAccessTime + "</td>");
                    report.Append("</tr>");
            }
            foreach(Folder f in Node.Children) {
                ExpandTree(f);
            }
        }
    }
}
