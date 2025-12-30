namespace SyntaxAnalyzer.Implementions
{
    public class XmlOutputWriter : IDisposable
    {
        private readonly StreamWriter _writer;
        private int _indentLevel = 0;

        public XmlOutputWriter(string outputPath)
        {
            _writer = new StreamWriter(outputPath);
        }

        public void OpenTag(string tag)
        {
            WriteLine($"<{tag}>");
            _indentLevel++;
        }

        public void CloseTag(string tag)
        {
            _indentLevel--;
            WriteLine($"</{tag}>");
        }

        public void WriteToken(string tag, string value)
        {
            value = Escape(value);
            WriteLine($"<{tag}> {value} </{tag}>");
        }

        private void WriteLine(string line)
        {
            _writer.WriteLine(new string(' ', _indentLevel * 2) + line);
        }

        private static string Escape(string value)
        {
            return value
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;");
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }

}
