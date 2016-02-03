using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace SolCp
{
    public class SolutionCopyException : Exception
    {
        public SolutionCopyException()
        {
        }
        public SolutionCopyException(string message)
            : base(message)
        {
        }
        public SolutionCopyException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class SolutionCopy
    {
        static readonly string IgnoreFile = @"(.+)\.sdf|(.+)\.suo|(.+)\.psess|(.+)\.vspx";
        static readonly string IgnoreDir = @"Debug|Release|obj|bin|\.vs";

        public SolutionCopy(string[] args)
        {
            if (args.Length != 2)
            {
                Usage();
                throw new SolutionCopyException("Invalid arguments.");
            }
            arg1 = args[0];
            arg2 = args[1];
        }

        private void Usage()
        {
            Console.WriteLine("solcp [sol src path] [sol dst path]");
        }

        public void Copy() {
            // check if the src == dst
            if (arg1 == arg2)
            {
                throw new SolutionCopyException("Errro occurred. The same path is designated.");
            }
            src = CreateFilePath(arg1);
            dst = CreateFilePath(arg2);
            // check if directory exists
            if (!FileUtil.IsDirectory(src))
            {
                throw new SolutionCopyException(
                    string.Format("Solution src [{0}] is not directory.", src)
                );
            }
            Console.WriteLine("src : " + src);
            Console.WriteLine("dst : " + dst);
            srcName = GetLastName(arg1);
            dstName = GetLastName(arg2);
            // check solution file
            string slnSrcFileName = System.IO.Path.GetFileName(src) + ".sln";
            // find solution file
            List<string> tmpSlnSrc = FileUtil.FindFilesRecursively(src, slnSrcFileName);
            if (tmpSlnSrc.Count == 0)
            {
                throw new SolutionCopyException("Solution file is not found.");
            }
            System.Text.RegularExpressions.Regex ignoreFile
                = new System.Text.RegularExpressions.Regex(IgnoreFile);
            System.Text.RegularExpressions.Regex ignoreDirectory
                = new System.Text.RegularExpressions.Regex(IgnoreDir);
            // execute copy
            FileUtil.CopyAndReplace(src, dst, srcName, dstName, ignoreFile, ignoreDirectory);
            // find solution file (destination)
            string slnDstFileName = System.IO.Path.GetFileName(dst) + ".sln";
            List<string> tmpSlnDst = FileUtil.FindFilesRecursively(dst, slnDstFileName);
            string dstSln;
            if (tmpSlnDst.Count == 1)
            {
                dstSln = tmpSlnDst[0];
            }
            else
            {
                throw new SolutionCopyException("Solution file is not found.");
            }
            // parse solution file
            var solution = new SolutionReplacer.Solution(dstSln);
            foreach(var p in solution.GetProjects())
            {
                p.Name = dstName;
                p.Path = p.Path.Replace(srcName, dstName);
                p.Guid = Guid.NewGuid().ToString("D");
                p.RootNamespace = p.Name;
                p.SaveAs(Path.Combine(dst, p.Path));
            }
            solution.SaveAs(Path.Combine(dst, dstName + ".sln"));
        }

        private string CreateFilePath(string str)
        {
            // if str is NOT full-path
            if (!Path.IsPathRooted(str))
            {
                return Path.Combine(Directory.GetCurrentDirectory(), Path.GetFileName(str));
            }
            // if str is full-path
            else
            {
                return str;
            }
        }
        private string GetLastName(string str)
        {
            return Path.GetFileName(str);
        }
        private string arg1;
        private string arg2;
        public string src { get; private set; }
        public string dst { get; private set; }
        public string srcName { get; private set; }
        public string dstName { get; private set; }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            SolutionCopy sc;
            try
            {
                sc = new SolutionCopy(args);
                sc.Copy();
            }
            catch(SolutionCopyException e)
            {
                Console.Error.WriteLine(e.Message);
            }
        }
    }
}
