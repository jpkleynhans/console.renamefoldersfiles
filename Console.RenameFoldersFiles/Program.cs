using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Renaming
{
    class Program
    {
        private readonly static List<string> bannedDirectories = new List<string>() { "bin", "obj" };
        private readonly static List<string> mimeTypes = new List<string>()
        {
            ".ts",
            ".html",
            ".scss",
            ".cs",
            ".sln",
            ".config",
            ".txt",
            ".csproj",
            ".user",
            ".nuspec",
            ".xdt",
            ".sql"
        };

        static void Main(string[] args)
        {
            Run();
        }

        static void Run()
        {
            Console.WriteLine("What would you like to do?");
            Console.WriteLine("");
            Console.WriteLine("1. Rename");
            Console.WriteLine("2. Create from file");
            Console.WriteLine("");

            switch (Console.ReadLine())
            {
                case "1":
                    Init();
                    break;
                case "2":
                    CreateFromFile();
                    break;
                default:
                    break;
            }
        }

        static void Init()
        {
            var directory = "";
            var wordToReplace = "";
            var wordToReplaceWith = "";

            while (string.IsNullOrEmpty(directory))
            {
                Console.WriteLine("Enter directory:");
                directory = Console.ReadLine();
            }

            while (string.IsNullOrEmpty(wordToReplace))
            {
                Console.WriteLine("Enter word to replace:");
                wordToReplace = Console.ReadLine();
            }

            while (string.IsNullOrEmpty(wordToReplaceWith))
            {
                Console.WriteLine("Enter word to replace with:");
                wordToReplaceWith = Console.ReadLine();
            }
            RenameBaseFiles(directory, wordToReplace, wordToReplaceWith);
            RenameFolder(directory, wordToReplace, wordToReplaceWith);

            Console.WriteLine("Press Enter");

            if (Console.ReadKey().Key == ConsoleKey.Enter)
            {
                Run();
            }
        }

        static void CreateFromFile(bool reInit = false, string directory = "", string wordToReplace = "")
        {
            var wordToReplaceWith = "";

            if (!reInit)
            {
                while (string.IsNullOrEmpty(directory))
                {
                    Console.WriteLine("Enter directory and filename with extension:");
                    directory = Console.ReadLine();
                }

                while (string.IsNullOrEmpty(wordToReplace))
                {
                    Console.WriteLine("Enter word to replace:");
                    wordToReplace = Console.ReadLine();
                }
            }

            while (string.IsNullOrEmpty(wordToReplaceWith))
            {
                Console.WriteLine("Enter word to replace with:");
                wordToReplaceWith = Console.ReadLine();
            }
            CreateAndRenameBaseFiles(directory, wordToReplace, wordToReplaceWith);

            Console.WriteLine("Press Enter to Continue or Y to do another");

            if (Console.ReadKey().Key == ConsoleKey.Y)
            {
                Console.WriteLine("");

                CreateFromFile(true, directory, wordToReplace);
            }
            if (Console.ReadKey().Key == ConsoleKey.Enter)
            {
                Run();
            }
        }

        private static void CreateAndRenameBaseFiles(string path, string source, string dest)
        {
            var fi = new FileInfo(path);
            var newDirectory = $"{fi.Directory}\\{dest}";
            var fileNameReplaced = fi.Name.Replace(source, dest);
            var code = File.ReadAllText(fi.FullName);
            code = Regex.Replace(code, @"\b" + Regex.Escape(source) + @"\b", $"{dest}");

            if (!Directory.Exists(newDirectory))
                Directory.CreateDirectory(newDirectory);

            File.WriteAllText($"{newDirectory}\\{fileNameReplaced}", code);
        }

        private static void RenameBaseFiles(string path, string source, string dest)
        {
            DirectoryInfo di = new DirectoryInfo(path);

            foreach (FileInfo item in di.GetFiles("*").Where(x => mimeTypes.Contains(x.Extension)))
            {
                var code = File.ReadAllText(item.FullName);
                code = code.Replace(source, dest);
                File.WriteAllText(item.FullName, code);
            }
        }

        private static void RenameFolder(string path, string source, string dest)
        {
            //Console.WriteLine("{0} {1} {2}", path, source, dest); 

            var di = new DirectoryInfo(path);

            foreach (DirectoryInfo si in di.GetDirectories("*", SearchOption.TopDirectoryOnly).Where(x => !bannedDirectories.Contains(x.Name)))
            {
                RenameFiles(si.FullName, source, dest);
                RenameFolder(si.Parent.FullName + @"\" + si.Name, source, dest);

                string strFoldername = si.Name;

                if (strFoldername.Contains(source))
                {
                    strFoldername = strFoldername.Replace(source, dest);
                    string strFolderRoot = si.Parent.FullName + "\\" + strFoldername;

                    si.MoveTo(strFolderRoot);
                }
                Console.WriteLine("{0} renamed to {1}", si.Parent.FullName, si.Name);
            }
        }

        private static void RenameFiles(string path, string source, string dest)
        {
            DirectoryInfo di = new DirectoryInfo(path);

            foreach (FileInfo si in di.GetFiles("*"))
            {
                string fileName = si.Name;
                var fileUrl = $"{di}\\{fileName}";

                if (si.Name.Contains(source) && mimeTypes.Contains(si.Extension))
                {
                    RenameFileContents(fileUrl, source, dest);
                    File.Move(fileUrl, $"{path}\\{fileName.Replace(source, dest)}");
                }
                else if (mimeTypes.Contains(si.Extension))
                {
                    RenameFileContents(fileUrl, source, dest);
                }

                Console.WriteLine("{0} renamed to {1}", si.FullName, si.Name);
            }
        }

        private static void RenameFileContents(string path, string source, string dest)
        {
            var code = File.ReadAllText(path);
            code = code.Replace(source, dest);
            File.WriteAllText(path, code);
        }
    }
}
