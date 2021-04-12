using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RevisionHistory
{
    class Program
    {
        public static void Main(string[] args)
        {
            //For Revision
            //RevisionTracker rt = new RevisionTracker();
            //Console.WriteLine("Process Started");
            //rt.TrackRevision();
            //Console.WriteLine("Process Finished!!!");

            //HTTPStatusTrack
            //HTTPStatusTrack objHTTPStatusTrack = new HTTPStatusTrack();
            //objHTTPStatusTrack.TrackHTTPStatusAsync();
            //Console.WriteLine("Process Finished!!!");

            //LinkTargetTracker ltt = new LinkTargetTracker();
            //ltt.DownloadLinkTarget();
            //Console.WriteLine("Process Finished!!!");
            //Console.ReadLine();

            LinkTargetComparison ltt = new LinkTargetComparison();
            ltt.CompareLinkTarget();
            Console.WriteLine("Process Finished!!!");
            Console.ReadLine();
        }

    }
}
