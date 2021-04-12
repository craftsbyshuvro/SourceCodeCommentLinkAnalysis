using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevisionHistory
{
    public class ProjectDetails
    {
        public string Repository { get; set; }
        public string URL { get; set; }
        public string FilePath { get; set; }
        public int Line { get; set; }
        public string Language { get; set; }
        public string Domain { get; set; }
        public string HTTPStatus { get; set; }
        public string HasURLRevised { get; set; }
    }
}
