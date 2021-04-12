using LibGit2Sharp;
using LinqToExcel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RevisionHistory
{
    public class HTTPStatusTrack
    {
        public readonly string PROJECTS_DIRECTORY = @"D:\University Project\Sample Projects\";
        public readonly string CSV_FILE_PATH = @"D:\University Project\Data\CommentLink.csv";
        public readonly string REVISED_CSV_FILE_PATH = @"D:\University Project\Data\CommentLinkRevised.csv";
        public readonly string CSV_SHEET_NAME = "CommentLink";

        List<ProjectDetails> vCommentURLs = new List<ProjectDetails>();
        public void TrackHTTPStatusAsync()
        {
            List<ProjectDetails> updatedProjectDetailsList = new List<ProjectDetails>();
            vCommentURLs = ReadAllProjectInfo();
            //vCommentURLs = vCommentURLs.Where(s=>string.IsNullOrEmpty(s.HTTPStatus)).ToList();
            var xyz = vCommentURLs.Where(s => string.IsNullOrEmpty(s.HTTPStatus));
            var xyasdsaz = vCommentURLs.Where(s => string.IsNullOrEmpty(s.HTTPStatus) && s.ToString().StartsWith("ftp"));
            var wwww = vCommentURLs.Where(s =>  (s.HTTPStatus ?? "") .ToString().StartsWith("Haider")).ToList();
            var xyeez = xyz.Count();
            int processedCommentCount = 0;


            Parallel.ForEach(vCommentURLs, (objCommentURL, state) =>
            {

                if (!string.IsNullOrEmpty(objCommentURL.HTTPStatus))
                {
                    return;
                }



                Console.WriteLine(++processedCommentCount + " Started.");
                try
                {
                    using (var cilent = new HttpClient())
                    {
                        try
                        {
                            var task = Task.Run(() =>
                                cilent.GetAsync(objCommentURL.URL)
                            );

                            task.Wait();
                            var response = task.Result;
                            objCommentURL.HTTPStatus = ((int)response.StatusCode).ToString();
                        }
                        catch (Exception ex)
                        {
                            objCommentURL.HTTPStatus = ex.GetBaseException().Message;
                        }
                        cilent.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    objCommentURL.HTTPStatus = ex.GetBaseException().Message;
                }

            });

            UpdateExcel(vCommentURLs);
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
