using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevisionHistory
{
    public class LinkTargetDownloadInfo
    {
        public string Id { get; set; }
        public string URL { get; set; }
        public string TargetLocation { get; set; }
        public bool IsDownloadedSuccessful{ get; set; }
        public string DownloadTime { get; set; }
        public string DownloadException { get; set; }
        public string TargetChanged { get; set; }
    }
}
