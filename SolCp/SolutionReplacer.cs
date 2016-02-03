using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

// reference : http://stackoverflow.com/questions/707107/parsing-visual-studio-solution-files
namespace SolCp
{
    public class SolutionReplacer
    {
        [DebuggerDisplay("{ParentGuid}, {Name}, {Path}, {Guid}")]
        public class Project
        {
            public string ParentGuid { get; set; }
            public string Name { get; set; }
            public string Path { get; set; }
            public string Guid { get; set; }
            public string RootNamespace { get; set; }
            public string AsSlnString()
            {
                return "Project(\"" + ParentGuid + "\") = \""
                    + Name + "\", \"" + Path + "\", \"" + Guid + "\"";
            }
            private string AsPrjGuidString()
            {
                return "\t<ProjectGuid>" + Guid + "</ProjectGuid>";
            }
            private string AsPrjRootNameSpaceString()
            {
                return "\t<RootNamespace>" + RootNamespace + "</RootNamespace>";
            }
            /// <summary>
            /// Saves project as file.
            /// </summary>
            public void SaveAs(string asFilename)
            {
                List<string> prjLines = new List<string>();
                string prjTxt = File.ReadAllText(asFilename);
                string[] lines = prjTxt.Split('\n');
                // Match string like:
                //    <ProjectGuid>{11111111-2222-3333-4444-555555555555}</ProjectGuid>
                //    <RootNamespace>foo</RootNamespace>
                Regex prjMatcher1 = new Regex("<ProjectGuid>(?<Guid>{[A-F0-9-]+})</ProjectGuid>");
                Regex prjMatcher2 = new Regex("<RootNamespace>(?<NameSpace>{.+})</RootNamespace>");
                Regex.Replace(prjTxt, "^(.*?)[\n\r]*$", new MatchEvaluator(m =>
                {
                    string line = m.Groups[1].Value;
                    Match m2 = prjMatcher1.Match(line);
                    if (m2.Success)
                    {
                        prjLines.Add(AsPrjGuidString());
                        return "";
                    }
                    else
                    {
                        Match m3 = prjMatcher2.Match(line);
                        if (m3.Success)
                        {
                            RootNamespace = m2.Groups[1].ToString();
                            prjLines.Add(AsPrjRootNameSpaceString());
                            return "";
                        }
                        else
                        {
                            prjLines.Add(line);
                            return "";
                        }
                    }
                }),
                    RegexOptions.Multiline
                );
                StringBuilder s = new StringBuilder();
                for (int i = 0; i < prjLines.Count; i++)
                {
                    if (i != prjLines.Count - 1)
                    {
                        s.AppendLine(prjLines[i]);
                    }
                    else
                    {
                        s.Append(prjLines[i]);
                    }
                }
                File.WriteAllText(asFilename, s.ToString(), Encoding.UTF8);
            }
        }

        /// <summary>
        /// .sln loaded into class.
        /// </summary>
        public class Solution
        {
            public List<object> slnLines { get; set; }   // List of either String (line format is not intresting to us), or SolutionProject.
            /// <summary>
            /// Loads visual studio .sln solution
            /// </summary>
            /// <param name="solutionFileName"></param>
            /// <exception cref="System.IO.FileNotFoundException">The file specified in path was not found.</exception>
            public Solution(string solutionFileName)
            {
                slnLines = new List<object>();
                string slnTxt = File.ReadAllText(solutionFileName);
                string[] lines = slnTxt.Split('\n');
                // Match string like: Project("{66666666-7777-8888-9999-AAAAAAAAAAAA}")
                //    = "Name", "Path.csproj", "{11111111-2222-3333-4444-555555555555}"
                // ?<foo> is Named Matched Subexpression
                Regex projMatcher = new Regex(
                    "Project\\(\"(?<ParentGuid>{[A-F0-9-]+})\"\\) "
                    + "= \"(?<Name>.*?)\", \"(?<Path>.*?)\", \"(?<Guid>{[A-F0-9-]+})");

                Regex.Replace(slnTxt, "^(.*?)[\n\r]*$", new MatchEvaluator(m =>
                {
                    string line = m.Groups[1].Value;
                    Match m2 = projMatcher.Match(line);
                    if (m2.Groups.Count < 2)
                    {
                        slnLines.Add(line);
                        return "";
                    }
                    Project p = new Project();
                    foreach (string g in projMatcher.GetGroupNames().Where(x => x != "0")) // "0" - RegEx special kind of group
                    {
                        // Reflection
                        PropertyInfo props = typeof(Project).GetProperty(g);
                        props.SetValue(p, m2.Groups[g].ToString());
                    }
                    slnLines.Add(p);
                    return "";
                }),
                    RegexOptions.Multiline
                );
            }

            /// <summary>
            /// Gets list of sub-projects in solution.
            /// </summary>
            /// <param name="bGetAlsoFolders">true if get also sub-folders.</param>
            public List<Project> GetProjects(bool bGetAlsoFolders = false)
            {
                var q = slnLines.Where(x => x is Project).Select(i => i as Project);
                if (!bGetAlsoFolders)  // Filter away folder names in solution.
                {
                    q = q.Where(x => x.Path != x.Name);
                }
                return q.ToList();
            }

            /// <summary>
            /// Saves solution as file.
            /// </summary>
            public void SaveAs(string asFilename)
            {
                StringBuilder s = new StringBuilder();
                for (int i = 0; i < slnLines.Count; i++)
                {
                    if (slnLines[i] is string)
                    {
                        s.Append(slnLines[i]);
                    }
                    else
                    {
                        s.Append((slnLines[i] as Project).AsSlnString());
                    }
                    if (i != slnLines.Count)
                    {
                        s.AppendLine();
                    }
                }
                File.WriteAllText(asFilename, s.ToString(), Encoding.UTF8);
            }
        }
    }
}
