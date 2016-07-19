using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Download2
{
    class Program
    {
        private static string URL =
            "http://tidesandcurrents.noaa.gov/api/datagetter?product=water_level&application=NOS.COOPS.TAC.WL&begin_date=xxxxxxxx&end_date=yyyyyyyy&datum=MLLW&station=9455760&time_zone=GMT&units=metric&format=csv";
        static void Main(string[] args)
        {
            DateTime endDate = DateTime.Now;
            TimeSpan ts = new TimeSpan(30, 0, 0, 0);
            for (int i = 0; i < 100; i++)
            {
                DateTime startDate = endDate - ts;

                string endDateStr = endDate.ToString("yyyyMMdd");
                string startDateStr = startDate.ToString("yyyyMMdd");

                string url = URL.Replace("xxxxxxxx", startDateStr).Replace("yyyyyyyy", endDateStr);
                string csvData = GetCSV(url);

                string path = @"Z:\Projects\Hydro\FourierMoon\" + i + ".csv";

                // This text is added only once to the file.
                if (!File.Exists(path))
                    File.WriteAllText(path, csvData);

                Console.WriteLine("Downloaded " + startDate + " through " + endDate);

                endDate = startDate;
            }


        }

        static string GetCSV(string url)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            StreamReader sr = new StreamReader(resp.GetResponseStream());
            string results = sr.ReadToEnd();
            sr.Close();

            return results;
        }
    }
}