using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolCp
{
    public static class FileUtil
    {
        static readonly string[] RenameExtensions = {
            ".sln",
            ".vcxproj",
            ".vcxproj.filters",
            ".vcxproj.user",
            ".csproj",
            ".csproj.filters",
            ".csproj.user"
        };
        public static bool IsDirectory(string str)
        {
            FileAttributes uAttribute;
            try
            {
                uAttribute = File.GetAttributes(str);
            }
            catch (IOException)
            {
                throw new SolutionCopyException(string.Format("Solution src [{0}] is not exist.", str));
            }
            // is Directory?
            if ((uAttribute & FileAttributes.Directory) == FileAttributes.Directory)
            {
                return true;
            }
            return false;
        }

        public static List<string> FindFilesRecursively(string rootPath, string pattern)
        {
            List<string> foundfiles = new List<string>();
            // search files in this directory
            foreach (string filePath in Directory.GetFiles(rootPath, pattern))
            {
                foundfiles.Add(filePath);
            }

            // search sub directories in this directory (recursively)
            foreach (string dirPath in Directory.GetDirectories(rootPath))
            {
                List<string> f = FindFilesRecursively(dirPath, pattern);
                // if the condition mathes then add into ArrayList
                if (f.Count > 0)
                {
                    foundfiles.AddRange(f);
                }
            }
            return foundfiles;
        }

        public static void CopyAndReplace(string srcPath, string dstPath
            , string srcName, string dstName
            , System.Text.RegularExpressions.Regex ignoreFile
            , System.Text.RegularExpressions.Regex ignoreDirectory)
        {
            // if the directory already exists, delete it
            Delete(dstPath);
            Directory.CreateDirectory(dstPath);

            // copy files
            foreach (var file in Directory.GetFiles(srcPath))
            {
                if (!ignoreFile.IsMatch(file))
                {
                    bool isRename = false;
                    foreach (string s in RenameExtensions)
                    {
                        if (Path.GetFileName(file) == srcName + s)
                        {
                            File.Copy(file, Path.Combine(dstPath, dstName + s));
                            isRename = true;
                            break;
                        }
                    }
                    if (!isRename)
                    {
                        File.Copy(file, Path.Combine(dstPath, Path.GetFileName(file)));
                    }
                }
            }
            // copy directory recursively
            foreach (var dir in Directory.GetDirectories(srcPath))
            {
                if (!ignoreDirectory.IsMatch(dir))
                {
                    if (Path.GetFileName(dir) == srcName)
                    {
                        CopyAndReplace(dir, Path.Combine(dstPath, Path.GetFileName(dstName)),
                            srcName, dstName, ignoreFile, ignoreDirectory);
                    }
                    else
                    {
                        CopyAndReplace(dir, Path.Combine(dstPath, Path.GetFileName(dir)),
                            srcName, dstName, ignoreFile, ignoreDirectory);
                    }
                }
            }
        }

        public static void Delete(string targetDirectoryPath)
        {
            if (!Directory.Exists(targetDirectoryPath))
            {
                return;
            }
            // delete files exclude directories.
            foreach (string filePath in Directory.GetFiles(targetDirectoryPath))
            {
                File.SetAttributes(filePath, FileAttributes.Normal);
                File.Delete(filePath);
            }
            // delete sub directory recursively
            foreach (string directoryPath in Directory.GetDirectories(targetDirectoryPath))
            {
                Delete(directoryPath);
            }
            // if empty, delete the directory.
            Directory.Delete(targetDirectoryPath, false);
        }
    }
}
