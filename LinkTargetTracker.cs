using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevisionHistory
{
    public class LinkTargetTracker
    {
        public readonly string LINK_TARGET_LOCATION = @"D:\University Project\Data\LinkTargetDownloads";
        public readonly string CSV_FILE_PATH = @"D:\University Project\Data\CommentLink.csv";
        public readonly string DOWNLOADED_CONTENT_INFO_CSV = @"D:\University Project\Data\LinkTargetDownloadInfo.csv";
        public readonly string CSV_SHEET_NAME = "CommentLink";
        public int downloadCount = 0;
        public void DownloadLinkTarget()
        {
            var vAllProjectsInfo = ReadAllProjectInfo();
            var random = new Random();
            var vDownloadableProjectInfo = vAllProjectsInfo.Where(x => x.HTTPStatus == "200")
                                                           .GroupBy(p => p.URL)
                                                           .Select(g => g.First())
                                                           .OrderBy(x => random.Next())
                                                           .Take(1000).ToList();



            List<LinkTargetDownloadInfo> vLinkTargetDownloadInfo = new List<LinkTargetDownloadInfo>();

            Parallel.ForEach(vDownloadableProjectInfo,(objCommentURL, state) =>
              {
                  Console.WriteLine(++downloadCount + " Started.");

                  LinkTargetDownloadInfo linkTargetDownloadInfo = new LinkTargetDownloadInfo();
                  linkTargetDownloadInfo.Id = Guid.NewGuid().ToString();
                  linkTargetDownloadInfo.URL = objCommentURL.URL;

                  var result = string.Empty;
                  using (var webClient = new System.Net.WebClient())
                  {

                      try
                      {
                          result = webClient.DownloadString(objCommentURL.URL);

                          string vFileLocation = LINK_TARGET_LOCATION + @"\"+linkTargetDownloadInfo.Id+".txt";
                          File.WriteAllText(vFileLocation, result);
                          linkTargetDownloadInfo.TargetLocation = LINK_TARGET_LOCATION;
                          linkTargetDownloadInfo.IsDownloadedSuccessful = true;
                          linkTargetDownloadInfo.DownloadTime = DateTime.Now.ToString();
                      }
                      catch (Exception ex)
                      {
                          linkTargetDownloadInfo.IsDownloadedSuccessful = false;
                          linkTargetDownloadInfo.DownloadException = ex.GetBaseException().Message;
                      }
                  }

                  vLinkTargetDownloadInfo.Add(linkTargetDownloadInfo);
              });
            UpdateExcel(vLinkTargetDownloadInfo);
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

        public void UpdateExcel(List<LinkTargetDownloadInfo> projectDetailsList)
        {
            using (var stream = File.CreateText(DOWNLOADED_CONTENT_INFO_CSV))
            {
                string csvRow = string.Format("{0},{1},{2},{3},{4},{5}", "Id", "URL", "TargetLocation", "IsDownloadedSuccessful", "DownloadTime", "DownloadException");
                stream.WriteLine(csvRow);

                foreach (var item in projectDetailsList)
                {
                    csvRow = string.Format("{0},{1},{2},{3},{4},{5}", item.Id, item.URL, item.TargetLocation, item.IsDownloadedSuccessful, item.DownloadTime,item.DownloadException);
                    stream.WriteLine(csvRow);
                }
                stream.Close();
            }
        }
    }
}
