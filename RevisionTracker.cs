using LibGit2Sharp;
using LinqToExcel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RevisionHistory
{
    public class RevisionTracker
    {
        public readonly string PROJECTS_DIRECTORY = @"D:\University Project\Sample Projects\";
        public readonly string CSV_FILE_PATH = @"D:\University Project\Data\CommentLink.csv";
        public readonly string REVISED_CSV_FILE_PATH = @"D:\University Project\Data\CommentLinkRevised.csv";
        public readonly string CSV_SHEET_NAME = "CommentLink";

        List<ProjectDetails> vCommentURLs = new List<ProjectDetails>();
        public void TrackRevision()
        {
            List<ProjectDetails> updatedProjectDetailsList = new List<ProjectDetails>();
            vCommentURLs = ReadAllProjectInfo();
            //var ass = vCommentURLs.Where(x => string.IsNullOrEmpty(x.HasURLRevised)).Count();

            string GitCommandPrefix = @"git log -L ";
            string GitCommandPostFix = @" --pretty=oneline | findstr '^+' | findstr  /v '++'";
            int processedCommentCount = 0;


            //foreach (var objCommentURL in vCommentURLs)

            Parallel.ForEach(vCommentURLs,(objCommentURL, state) =>
               {

                   if (!string.IsNullOrEmpty(objCommentURL.HasURLRevised))
                   {
                       return;
                   }

                   Console.WriteLine(++processedCommentCount + " Started.");
                   string DirectoryToRunCommand = "'" + PROJECTS_DIRECTORY + objCommentURL.Repository + "'";

                   string GitCommand = @GitCommandPrefix +
                                       objCommentURL.Line + "," +
                                       objCommentURL.Line + ":" + "'" +
                                       PROJECTS_DIRECTORY +
                                       objCommentURL.Repository +
                                       "\\" + objCommentURL.FilePath + "'" +
                                       GitCommandPostFix;
                   //Sample
                   //git log -L 8,8:'D:\University Project\Sample Projects\AM\AM.DM.Article\ArticleModel.cs' --pretty=oneline | findstr '^+' | findstr  /v '++'

                   try
                   {
                       using (PowerShell powershell = PowerShell.Create())
                       {
                           powershell.AddScript($"cd {DirectoryToRunCommand}");
                           //powershell.AddScript(@"git log -L 8,8:'D:\University Project\Sample Projects\AM\AM.DM.Article\ArticleModel.cs' --pretty=oneline | findstr '^+' | findstr  /v '++'");
                           powershell.AddScript(GitCommand);
                           Collection<PSObject> results = powershell.Invoke();
                           List<string> URLRevisions = results.Select(x => x.ToString()).ToList();
                           bool vIsLinkRevised = IsLinkRevised(URLRevisions, objCommentURL.URL);

                           objCommentURL.HasURLRevised = vIsLinkRevised ? "1" : "0";
                       }
                   }
                   catch (Exception ex)
                   {
                       objCommentURL.HasURLRevised = ex.GetBaseException().Message;
                   }

                   updatedProjectDetailsList.Add(objCommentURL);

                   bool start = false;

               });
            //}

            UpdateExcel(vCommentURLs);
        }

        public bool IsLinkRevised(List<string> revisedLinks, string pURL)
        {

            if (revisedLinks.Count < 2)
            {
                return false;
            }

            List<string> vExtractedLinks = new List<string>();

            for (int i = 1; i < revisedLinks.Count; i++)
            {
                vExtractedLinks = ExtractLinks(revisedLinks.ElementAt(i));
                if (vExtractedLinks.Count > 0)
                {
                    break;
                }
            }

            if (vExtractedLinks.Count == 0)
            {
                return false;
            }

            //List<string> vExtractedLinks = ExtractLinks(revisedLinks.ElementAt(1));
            var match = vExtractedLinks.FirstOrDefault(stringToCheck => stringToCheck == pURL);

            if (match == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<string> ExtractLinks(string pInputString)
        {
            var r = new Regex(
            "\\b(((ht|f)tp(s?)\\:\\/\\/)|www.)" +
            "(\\w+:\\w+@)?(([-\\w]+\\.)+(com|org|net|gov" +
            "|mil|biz|info|mobi|name|aero|jobs|museum" +
            "|travel|[a-z]{1}|[a-z]{2}|[a-z]{3}|[a-z]{4}))(:[\\d]{1,5})?" +
            "(((\\/([-\\w~!$+|.,=]|%[a-f\\d]{2})+)+|\\/)+|\\?|#)?" +
            "((\\?([-\\w~!$+|.,*:]|%[a-f\\d{2}])+=?" +
            "([-\\w~!$+|.,*:=]|%[a-f\\d]{2})*)" +
            "(&(?:[-\\w~!$+|.,*:]|%[a-f\\d{2}])+=?" +
            "([-\\w~!$+|.,*:=]|%[a-f\\d]{2})*)*)*" +
            "(#([-\\w~!$+|.,*:=]|%[a-f\\d]{2})*)?\\b", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            var output = r.Matches(pInputString);
            var urls = new List<string>();
            foreach (var item in output)
            {
                urls.Add(item.ToString());
            }

            return urls;
        }

        public void UpdateExcel(List<ProjectDetails> projectDetailsList)
        {
            using (var stream = File.CreateText(REVISED_CSV_FILE_PATH))
            {
                string csvRow = string.Format("{0},{1},{2},{3},{4},{5},{6},{7}", "Repository", "Language", "URL", "Line", "FilePath", "Domain", "HTTPStatus", "HasURLRevised");
                stream.WriteLine(csvRow);

                foreach (var item in projectDetailsList)
                {
                    csvRow = string.Format("{0},{1},{2},{3},{4},{5},{6},{7}", item.Repository, item.Language, item.URL, item.Line, item.FilePath, item.Domain, item.HTTPStatus, item.HasURLRevised);
                    stream.WriteLine(csvRow);
                }
                stream.Close();
            }
        }

        public List<ProjectDetails> ReadAllProjectInfo()
        {
            var projectFile = new LinqToExcel.ExcelQueryFactory(@CSV_FILE_PATH);

            List<ProjectDetails> projectList = (from row in projectFile.Worksheet(CSV_SHEET_NAME)
                                                let item = new ProjectDetails()
                                                {
                                                    Repository = row["Repository"].Cast<string>(),
                                                    Language = row["Language"].Cast<string>(),
                                                    URL = row["URL"].Cast<string>(),
                                                    FilePath = row["FilePath"].Cast<string>(),
                                                    Line = row["Line"].Cast<int>(),
                                                    HTTPStatus = row["HTTPStatus"].Cast<string>(),
                                                    Domain = row["Domain"].Cast<string>(),
                                                    HasURLRevised = row["HasURLRevised"].Cast<string>()
                                                }
                                                select item).ToList();

            return projectList;
        }
    }
}
