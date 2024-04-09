using Metrino.Development.Core.TreeSitter;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace TreeSitterTest
{
    // A class to test the TreeSitter methods
    public class TestTreeSitterCPP
    {
        public static TSLanguage lang = new TSLanguage(tree_sitter_c_sharp());

        [DllImport("TreeSitter-CSharp.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tree_sitter_c_sharp();

        public static void PostOrderTraverse(string path, string filetext, TSCursor cursor)
        {
            var rootCursor = cursor.copy();

            for (; ; )
            {
                int so = (int)cursor.CurrentNode.StartOffset;
                int eo = (int)cursor.CurrentNode.EndOffset;
                int sl = (int)cursor.CurrentNode.StartPoint.Row + 1;
                var field = cursor.CurrentField;
                var type = cursor.CurrentSymbol;
                bool hasChildren = cursor.GotoFirstChild();

                var span = filetext.AsSpan(so, eo - so);

                if (hasChildren)
                {
                    continue;
                }

                var preview = span.ToString();
                preview = preview.Substring(0, Math.Min(preview.Length, 25)).Replace("\r\n", "\\r\\n");
                Console.Error.WriteLine("The node type is {0}, symbol is {1}", type, preview);

                if (cursor.GotoNextSibling())
                {
                    continue;
                }

                do
                {
                    cursor.GotoParent();
                    int so_p = (int)cursor.CurrentNode.StartOffset;
                    int eo_p = (int)cursor.CurrentNode.EndOffset;
                    var type_p = cursor.CurrentSymbol;
                    var span_p = filetext.AsSpan(so_p, eo_p - so_p);

                    var preview00 = span_p.ToString();
                    preview00 = preview00.Substring(0, Math.Min(preview00.Length, 55)).Replace("\r\n", "\\r\\n");
                    Console.Error.WriteLine("The node type is {0}, symbol is {1}", type, preview00);

                    if (rootCursor == cursor)
                    {
                        Console.Error.WriteLine("done!");
                        return;
                    }

                } while (!cursor.GotoNextSibling());
            }
        }


        public static void PreOrderTraverse(string path, string filetext, TSNode node, int level)
        {
            //var rootCursor = cursor.copy();

            //for (; ; )
            //{
            int so = (int)node.StartOffset;
            int eo = (int)node.EndOffset;
            int sl = (int)node.StartPoint.Row + 1;
            //var field = node.F;
            var type = node.Type();

            //bool hasChildren = cursor.GotoFirstChild();
            string tabs = new string(' ', level * 2);
            var span = filetext.AsSpan(so, eo - so);
            var preview = span.ToString();
            preview = preview.Substring(0, Math.Min(preview.Length, 25)).Replace("\r\n", "\\r\\n");
            Console.Error.WriteLine($"{tabs}The node type is {type}, symbol is {preview}");


            //enter(node)

            var child_count = node.ChildCount;
            for (uint i = 0; i < child_count; i++)
            {
                TSNode child_node = node.Child(i);//  ts_node_child(node, i);
                PreOrderTraverse(path, filetext, child_node, level + 1);
            }
        }


        public static bool ParseTree(string path, string filetext, TSParser parser)
        {
            parser.Setlanguage(lang);

            using var tree = parser.ParseString(null, filetext);
            if (tree == null)
            {
                return false;
            }

            using var cursor = new TSCursor(tree.RootNode, lang);

            PreOrderTraverse(path, filetext, tree.RootNode, 0);
            return true;
        }

        void visit_tree(TSNode node)
        {
        }

        public static bool TraverseTree(string filename, string filetext)
        {
            using var parser = new TSParser();
            return ParseTree(filename, filetext, parser);
        }

        public static bool PrintTree(List<string> paths)
        {
            bool good = true;
            foreach (var path in paths)
            {
                var filetext = File.ReadAllText(path);
                if (!TraverseTree(path, filetext))
                {
                    good = false;
                }
            }
            return good;
        }

        public static void PrintErrorAt(string path, string error, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            if (Console.CursorLeft != 0)
            {
                Console.Error.WriteLine();
            }

            Console.Error.WriteLine("{0}(): error {1}", path, string.Format(error, args));
            Console.ForegroundColor = Console.ForegroundColor;
        }

        public static List<string> ArgsToPaths(ref int pos, string[] args)
        {
            if (++pos >= args.Length)
            {
                PrintErrorAt("", "XR0100: No input files to process.");
                return null;
            }

            var files = new List<string>();
            var used = new HashSet<string>();
            var options = new EnumerationOptions();
            options.RecurseSubdirectories = false;
            options.ReturnSpecialDirectories = false;
            options.MatchCasing = MatchCasing.CaseInsensitive;
            options.MatchType = MatchType.Simple;
            options.IgnoreInaccessible = false;

            for (; pos < args.Length; pos++)
            {
                var arg = args[pos];

                if (arg[0] == '-')
                {
                    pos--;
                    break;
                }

                if (Directory.Exists(arg))
                {
                    PrintErrorAt(arg, "XR0101: Path is a directory, not a file spec.");
                    return null;
                }

                string directory = Path.GetDirectoryName(arg);
                string pattern = Path.GetFileName(arg);
                if (directory == string.Empty)
                {
                    directory = ".";
                }
                if (directory == null || string.IsNullOrEmpty(pattern))
                {
                    PrintErrorAt(arg, "XR0102: Path is anot a valid file spec.");
                    return null;
                }

                if (!Directory.Exists(directory))
                {
                    PrintErrorAt(directory, "XR0103: Couldn't find direvtory.");
                    return null;
                }

                int cnt = 0;
                foreach (var filepath in Directory.EnumerateFiles(directory, pattern, options))
                {
                    cnt++;
                    if (!used.Contains(filepath))
                    {
                        files.Add(filepath);
                        used.Add(filepath);
                    }
                }
            }

            return files;
        }

        public static void Main(string[] args)
        {
            var files = (List<string>)null;
            int a = 0;

            //// Check if the args have at least two elements and the first one is "-files"
            //if (args.Length < 2 || args[0] != "-files")
            //{
            //    Console.WriteLine("Invalid arguments. Please use -files followed by one or more file paths.");
            //    return;
            //}

            //if ((files = ArgsToPaths(ref a, args)) != null)
            //{
            //}
            PrintTree(new List<string> { @"D:\Work\Git\OTDR\OlmBase\Metrino.Olm\OlxStorage.cs" });
        }
    }
}


