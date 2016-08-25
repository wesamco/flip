using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args == null)
            {
                Console.WriteLine("Please Enter the Required Arguments");
                Console.WriteLine("use the argument --help");
                return;
            }
            foreach (string arg in args)
            {
                Console.WriteLine(arg);
            }
            System.Console.WriteLine(System.IO.Directory.GetCurrentDirectory());
            if (args.Length == 4)
            {
                if (args[0] == "indir")
                {
                    inputFolder = args[1];
                }
                if (args[2] == "outdir")
                {
                    outputFolder = args[3];
                }
            }
            else if (args.Length == 2)
            {
                if (args[0] == "indir")
                {
                    if (args[1] != "outdir")
                    {
                        inputFolder = args[1];
                    }
                    else
                    {
                        Console.WriteLine("Invalid Arguments");
                        return;
                    }
                }
            }
            if (!string.IsNullOrEmpty(inputFolder) && !string.IsNullOrWhiteSpace(inputFolder))
            {
                if (string.IsNullOrEmpty(outputFolder)
                || string.IsNullOrWhiteSpace(outputFolder)
                || !Directory.Exists(outputFolder))
                {
                    int n = 1;
                    outputFolder = $"{inputFolder} {n}";
                    while (Directory.Exists(outputFolder))
                    {
                        outputFolder = $"{outputFolder.Split(' ')[0]} {n}";
                        n++;
                    }
                    Directory.CreateDirectory(outputFolder);
                }
                DirectoryCopy(inputFolder, outputFolder, true);
                Convert(outputFolder);
                return;
            }
            Console.WriteLine(@"LTRTL Command Line Tool
Usage: ltrtl [arguments]

Common Arguments:
  -h|--help             Show help
  -indir                Input Directory
  -outdir (Optional)    Output Directory (if not passed will create new output Folder)");
        }

        private static string inputFolder;
        private static string outputFolder;

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }
            DirectoryInfo[] dirs = dir.GetDirectories();
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
        private static void Convert(string sourceDirName)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }
            ConvertHTML(sourceDirName);
            ConvertCSS(sourceDirName);
            //ConvertJS(sourceDirName);
            var dirs = dir.GetDirectories();
            foreach (DirectoryInfo subdir in dirs)
            {
                Convert(subdir.FullName);
            }
        }
        public static void ConvertHTML(string dirPath)
        {
            string[] filespaths = Directory.GetFiles(dirPath);
            if (filespaths.Length == 0)
                return;
            IEnumerable<string> files = filespaths.ToList()
                .Where(x => x.ToLower().EndsWith(".html")
                || x.ToLower().EndsWith(".htm")
                || x.ToLower().EndsWith(".cshtml")
                || x.ToLower().EndsWith(".asp")
                || x.ToLower().EndsWith(".jsp")
                || x.ToLower().EndsWith(".SHTML")
                || x.ToLower().EndsWith(".xhtml")
                || x.ToLower().EndsWith(".xht")
                || x.ToLower().EndsWith(".xml")
                || x.ToLower().EndsWith(".xhtml")
                || x.ToLower().EndsWith(".xhtml"));
            if (files.Count() == 0)
                return;
            foreach (string FilePath in files)
            {
                string text = File.ReadAllText(FilePath);
                //text = flip(text);
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(text);
                var docnodeChilds = doc.DocumentNode.ChildNodes;
                for (int i = 0; i < docnodeChilds.Count; i++)
                {
                    FlipNodeDir(docnodeChilds.ElementAt(i));
                }
                File.WriteAllText(FilePath, doc.DocumentNode.InnerHtml);
            }
        }
        public static void FlipNodeDir(HtmlNode node)
        {
            if (node == null)
                return;
            if (node.ChildNodes.Count == 0)
            {
                if (node.InnerText.Length > 0)
                {
                    string woNewLines = node.InnerText.Replace(Environment.NewLine, "").Replace("\n",
                        "");
                    if (!string.IsNullOrEmpty(woNewLines)
                        && !string.IsNullOrWhiteSpace(woNewLines))
                    {
                        string s = DumbEnglishToArabic(node.InnerText);
                        if (node.ParentNode != null)
                            node.ParentNode.ReplaceChild(HtmlTextNode.CreateNode(s), node);
                    }
                }
            }
            if (node.Name == "#text")
            {
                string s = DumbEnglishToArabic(node.InnerText);
                if (node.ParentNode != null)
                    node.ParentNode.ReplaceChild(HtmlTextNode.CreateNode(s), node);
            }
            for (int i = 0; i < node.Attributes.Count; i++)
            {
                if (node.Attributes[i].Value.ToString().ToLower().Contains("left"))
                {
                    string a = TextDirflip(node.Attributes[i].Value.ToString());
                }
                node.Attributes[i].Name = TextDirflip(node.Attributes[i].Name.ToString());
                node.Attributes[i].Value = TextDirflip(node.Attributes[i].Value.ToString());
            }
            var childs = node.ChildNodes.ToList();
            var filtered = childs
                .Where(n => n.ParentNode.Name != "script" && n.ParentNode.Name != "style" && n.ParentNode.Name != "#comment" && n.ParentNode.Name != "meta" /*&& n.Name != "#text"*/).ToList();
            int ii = filtered.Count;
            for (int i = 0; i < filtered.Count; i++)
            {
                FlipNodeDir(filtered.ElementAt(i));
            }
        }
        public static void ConvertJS(string dirPath)
        {
            string[] filespaths = Directory.GetFiles(dirPath);
            if (filespaths.Length == 0)
                return;
            IEnumerable<string> files = filespaths.ToList()
                .Where(x => x.ToLower().EndsWith(".js"));
            if (files.Count() == 0)
                return;
            foreach (string FilePath in files)
            {
                string text = File.ReadAllText(FilePath);
                text = TextDirflip(text);
                File.WriteAllText(FilePath, text);
            }
        }

        public static void ConvertCSS(string dirPath)
        {
            string[] filespaths = Directory.GetFiles(dirPath);
            if (filespaths.Length == 0)
                return;
            IEnumerable<string> files = filespaths.ToList()
                .Where(x => x.ToLower().EndsWith(".css")
                || x.ToLower().EndsWith(".sass")
                || x.ToLower().EndsWith(".scss")
                || x.ToLower().EndsWith(".less")
                || x.ToLower().EndsWith(".styl"));
            if (files.Count() == 0)
                return;
            Regex rgx = new Regex("(html\\s+{)|(html{)");
            foreach (string FilePath in files)
            {
                string text = File.ReadAllText(FilePath);
                text = TextDirflip(text);
                string text1 = rgx.Replace(text, "html{direction:rtl;");
                if (text == text1)
                {
                    text = text + "html{direction: rtl;}";
                } else
                {
                    text = text1;
                }
                File.WriteAllText(FilePath, text);
            }
        }
        public static string TextDirflip(string s)
        {
            s = s.Replace("left", "ri*ght");
            s = s.Replace("right", "le*ft");
            s = s.Replace("ri*ght", "right");
            s = s.Replace("le*ft", "left");
            return s;
        }
        public static string DumbEnglishToArabic(string s)
        {
            s = s.Replace('A', 'ا');
            s = s.Replace('a', 'ا');
            s = s.Replace('B', 'ب');
            s = s.Replace('b', 'ب');
            s = s.Replace('C', 'ج');
            s = s.Replace('c', 'ج');
            s = s.Replace('D', 'د');
            s = s.Replace('d', 'د');
            s = s.Replace('E', 'ه');
            s = s.Replace('e', 'ه');
            s = s.Replace('F', 'و');
            s = s.Replace('f', 'و');
            s = s.Replace('G', 'ز');
            s = s.Replace('g', 'ز');
            s = s.Replace('H', 'ح');
            s = s.Replace('h', 'ح');
            s = s.Replace('I', 'ط');
            s = s.Replace('i', 'ط');
            s = s.Replace('J', 'ي');
            s = s.Replace('j', 'ي');
            s = s.Replace('K', 'ظ');
            s = s.Replace('k', 'ظ');
            s = s.Replace('L', 'ك');
            s = s.Replace('l', 'ك');
            s = s.Replace('M', 'ل');
            s = s.Replace('m', 'ل');
            s = s.Replace('N', 'م');
            s = s.Replace('n', 'م');
            s = s.Replace('O', 'ن');
            s = s.Replace('o', 'ن');
            s = s.Replace('P', 'س');
            s = s.Replace('p', 'س');
            s = s.Replace('Q', 'ع');
            s = s.Replace('q', 'ع');
            s = s.Replace('R', 'ف');
            s = s.Replace('r', 'ف');
            s = s.Replace('S', 'ص');
            s = s.Replace('s', 'ص');
            s = s.Replace('T', 'ق');
            s = s.Replace('t', 'ق');
            s = s.Replace('U', 'ر');
            s = s.Replace('u', 'ر');
            s = s.Replace('V', 'ش');
            s = s.Replace('v', 'ش');
            s = s.Replace('W', 'ت');
            s = s.Replace('w', 'ت');
            s = s.Replace('X', 'ث');
            s = s.Replace('x', 'ث');
            s = s.Replace('Y', 'خ');
            s = s.Replace('y', 'خ');
            s = s.Replace('Z', 'ذ');
            s = s.Replace('z', 'ذ');
            return s;
        }
    }
}