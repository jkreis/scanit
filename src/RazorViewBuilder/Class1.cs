using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.Parser;
using Microsoft.AspNetCore.Razor.Parser.Internal;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using System.IO;

namespace RazorViewBuilder {
    public class Class1 {
        public void TestParse() {
            string path = "test.cshtml";
            Stream stream = new FileStream(path, FileMode.Open);
            StreamReader text = new StreamReader(stream);

            var resolver = new TagHelperDescriptorResolver(false);
            RazorParser parser = new RazorParser(new CSharpCodeParser(), new HtmlMarkupParser(), resolver);
            ParserResults res = parser.Parse(text);
            if (res.Success) {
                string doc = res.ToString();
            }

            text.Dispose();
            stream.Dispose();
        }
    }
}
