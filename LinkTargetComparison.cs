using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevisionHistory
{
    public class LinkTargetComparison
    {
        public readonly string LINK_TARGET_LOCATION = @"D:\University Project\Data\LinkTargetDownloads";
        public readonly string CSV_FILE_PATH = @"D:\University Project\Data\CommentLink.csv";
        public readonly string DOWNLOADED_CONTENT_INFO_CSV = @"D:\University Project\Data\LinkTargetDownloadInfo.csv";
        public readonly string CSV_SHEET_NAME = "CommentLink";
        public readonly string DOWNLOAD_CSV_SHEET_NAME = "LinkTargetDownloadInfo";
        public int processCount = 0;
        public void CompareLinkTarget()
        {
            var vAllProjectsInfo = DownloadLinkTargetInfo();


            List<LinkTargetDownloadInfo> vLinkTargetDownloadInfo = new List<LinkTargetDownloadInfo>();

            Parallel.ForEach(vAllProjectsInfo, (objCommentURL, state) =>
              {

                  if (objCommentURL.IsDownloadedSuccessful == false)
                  {
                      goto AddToList;
                  }

                  Console.WriteLine("Processed: " + ++processCount);
                  string filePath = objCommentURL.TargetLocation + "\\" + objCommentURL.Id + ".txt";
                  string OldFIleText = string.Empty;

                  try
                  {
                      OldFIleText = File.ReadAllText(filePath);

                  }
                  catch (Exception ex)
                  {
                      goto AddToList;
                  }

    

                  var result = string.Empty;
                  using (var webClient = new System.Net.WebClient())
                  {

                      try
                      {
                          result = webClient.DownloadString(objCommentURL.URL);

                          if(OldFIleText == result)
                          {
                              objCommentURL.TargetChanged = "No";
                          }
                          else
                          {
                              objCommentURL.TargetChanged = "Yes";
                          }
                      }
                      catch (Exception ex)
                      {
                          Console.WriteLine(ex.GetBaseException().Message);
                      }
                  }

                  AddToList:
                  vLinkTargetDownloadInfo.Add(objCommentURL);
              });
            UpdateExcel(vLinkTargetDownloadInfo);
        }

        public List<LinkTargetDownloadInfo> DownloadLinkTargetInfo()
        {
            var projectFile = new LinqToExcel.ExcelQueryFactory(DOWNLOADED_CONTENT_INFO_CSV);

            List<LinkTargetDownloadInfo> projectList = (from row in projectFile.Worksheet(DOWNLOAD_CSV_SHEET_NAME)
                                                let item = new LinkTargetDownloadInfo()
                                                {
                                                    Id = row["Id"].Cast<string>(),
                                                    URL = row["URL"].Cast<string>(),
                                                    TargetLocation = row["TargetLocation"].Cast<string>(),
                                                    IsDownloadedSuccessful = row["IsDownloadedSuccessful"].Cast<bool>(),
                                                    DownloadTime = row["DownloadTime"].Cast<string>(),
                                                    DownloadException = row["DownloadException"].Cast<string>(),
                                                    TargetChanged = row["TargetChanged"].Cast<string>()
                                                }
                                                select item).ToList();

            return projectList;
        }

        public void UpdateExcel(List<LinkTargetDownloadInfo> projectDetailsList)
        {
            using (var stream = File.CreateText(DOWNLOADED_CONTENT_INFO_CSV))
            {
                string csvRow = string.Format("{0},{1},{2},{3},{4},{5},{6}", "Id", "URL", "TargetLocation", "IsDownloadedSuccessful", "DownloadTime", "DownloadException", "TargetChanged");
                stream.WriteLine(csvRow);

                foreach (var item in projectDetailsList)
                {
                    csvRow = string.Format("{0},{1},{2},{3},{4},{5},{6}", item.Id, item.URL, item.TargetLocation, item.IsDownloadedSuccessful, item.DownloadTime,item.DownloadException,item.TargetChanged);
                    stream.WriteLine(csvRow);
                }
                stream.Close();
            }
        }
    }
}
