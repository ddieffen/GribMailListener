using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace MailListenter
{
    public class GribTimeFile
    {
        public string model;
        public int cycle;
        public int time;
        public string full_name;
        public byte[] buffer;
    }


    internal static class GRIBTools
    {

        internal static List<GribTimeFile> DecomposeFromStrings(List<string> files)
        {
            List<GribTimeFile> list = new List<GribTimeFile>();

            foreach (string file in files)
            {
                string[] split = file.Split(new char[] { '.' });
                GribTimeFile tref = new GribTimeFile();
                tref.full_name = file;
                tref.cycle = Convert.ToInt16(split[1].Trim('t').Trim('z'));
                tref.model = split[0];
                tref.time =  Convert.ToInt16(split[2].TrimStart("awphys".ToCharArray()));
                list.Add(tref);
            }

            return list;    
        }

        public static List<string> KnownModels = new List<string>() { "nam-comus", "nam-na", "gfs0.25", "gfs0.5", "gfs1.0", "gefs-hi", "gefs-low", "naefs-hi", "naefs-low" };

        /// <summary>
        /// Parses a string for a NAM-CONUS weather request
        /// 
        /// The request must be a string a that forms like this nam-conus:leftlon,rightlon,toplat,bottomlat:0-12/3,15,18-80/6
        /// Where nam-conus is the name of the model
        /// Left lon, right lon, top lat, bottom lat are the desired region
        /// 0-12 indicate that we want all times between 0 to 12 hours after the forecast was issued with a step of 3 hours
        /// 15 indicates that we also want the forecast for 15 hours
        /// and 18-80 indicates that we want the forecast for 18 to 80 hours with a step of 6 hours
        /// </summary>
        /// <param name="request">Properly formated request</param>
        internal static string DoFilterGrib(string request)
        {
            string[] splitRequest = request.Split(new char[] { ':' }, 3);
            string modelRequest = splitRequest[0];
            string[] regionRequest = splitRequest[1].Split(new char[] { ',' }, 4);
            string[] timeRequest = splitRequest[2].Split(new char[] { ',' });

            string urlDir = "";
            string urlModel = "";
            switch (modelRequest) 
            {
                case "nam-comus":
                    urlDir = "http://nomads.ncep.noaa.gov/cgi-bin/filter_nam.pl";
                    urlModel = urlDir + "?file=";
                    break;
                case "nam-na":
                    urlDir = "http://nomads.ncep.noaa.gov/cgi-bin/filter_nam_na.pl";
                    urlModel = urlDir + "?file=";
                    break;
                case "gfs0.25":
                    urlDir = "http://nomads.ncep.noaa.gov/cgi-bin/filter_gfs_0p25.pl";
                    urlModel = urlDir + "?file=";
                    break;
                case "gfs0.5":
                    urlDir = "http://nomads.ncep.noaa.gov/cgi-bin/filter_gfs_0p50.pl";
                    urlModel = urlDir + "?file=";
                    break;
                case "gfs1.0":
                    urlDir = "http://nomads.ncep.noaa.gov/cgi-bin/filter_gfs_1p00.pl";
                    urlModel = urlDir + "?file=";
                    break;
                case "gefs-hi":
                    urlDir = "http://nomads.ncep.noaa.gov/cgi-bin/filter_gensbc.pl";
                    urlModel = urlDir + "?file=";
                    break;
                case "gefs-low":
                    urlDir = "http://nomads.ncep.noaa.gov/cgi-bin/filter_gensbc_ndgd.pl";
                    urlModel = urlDir + "?file=";
                    break;
                case "naefs-hi":
                    urlDir = "http://nomads.ncep.noaa.gov/cgi-bin/filter_naefsbc.pl";
                    urlModel = urlDir + "?file=";
                    break;
                case "naefs-low":
                    urlDir = "http://nomads.ncep.noaa.gov/cgi-bin/filter_naefsbc_ndgd.pl";
                    urlModel = urlDir + "?file=";
                    break;
                default:
                    return "Wrong model";
            }

            if (regionRequest.Length != 4)
                return "Wrong number of lat lon";

            int leftLon = (int)Convert.ToDouble(regionRequest[0]);
            int rightLon = (int)Convert.ToDouble(regionRequest[1]);
            int topLat = (int)Convert.ToDouble(regionRequest[2]);
            int bottomLat = (int)Convert.ToDouble(regionRequest[3]);

           
            string urlParameter = "&lev_10_m_above_ground=on&lev_mean_sea_level=on&var_UGRD=on&var_VGRD=on&var_PRMSL=on&subregion=&leftlon=" 
                + leftLon + "&rightlon=" + rightLon + "&toplat=" + topLat + "&bottomlat=" + bottomLat + "&dir=";

            //finding the index page with all models
            WebClient w = new WebClient();
            string s = w.DownloadString(urlDir);

            //find latest simulation day
            LinkItem latest = Finder.FindLinks(s)[0];

            //find files
            s = w.DownloadString(latest.Href);
            List<string> files = Finder.FindOptionsValues(s);
            List<GribTimeFile> gribTimes = GRIBTools.DecomposeFromStrings(files);

            int maxCycle = 0;
            foreach (GribTimeFile time in gribTimes)
                if (time.cycle > maxCycle)
                    maxCycle = time.cycle;

            //download all binary files
            List<GribTimeFile> downloaded = new List<GribTimeFile>();
            int futureStackSize = 0;
            List<int> cycleTimes = CreateTimes(timeRequest);
            foreach (GribTimeFile file in gribTimes.Where(t => t.cycle == maxCycle))
            {
                if (cycleTimes.Contains(file.time))
                {
                    string url = urlModel
                        + file.full_name
                        + urlParameter
                        + latest.Href.Split(new string[] { "dir=" }, StringSplitOptions.None)[1];
                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                    webRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows 5.1;";
                    HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                    Stream responseStream = webResponse.GetResponseStream();
                    MemoryStream memStream = new MemoryStream();
                    responseStream.CopyTo(memStream);
                    byte[] buffer = memStream.ToArray();
                    file.buffer = buffer;
                    downloaded.Add(file);
                    futureStackSize += buffer.Length;
                }
            }

            //stacking grib files
            byte[] stacked = new byte[futureStackSize];
            int currentPosition = 0;
            foreach (GribTimeFile grbTime in downloaded)
            {
                grbTime.buffer.CopyTo(stacked, currentPosition);
                currentPosition += grbTime.buffer.Length;
            }

            //writing the file
            string path = "nam-" + DateTime.Now.ToString("yyyyMMdd-Hmm") + ".grb";
            File.WriteAllBytes(path, stacked);
            return path;
        }


        private static List<int> CreateTimes(string[] time)
        {
            List<int> times = new List<int>();
            double temp;
            foreach(string t in time)
            {
                if (t.Contains("-"))
                {//parse interval
                    int step = 1;
                    string[] dash = t.Split(new char[]{'-'},2);
                    int start = (int)Convert.ToDouble(dash[0]);
                    int finish = 0;
                    int increment = 1;
                    if (dash[1].Contains("/"))
                    {
                        string[] dashslash = dash[1].Split(new char[] { '/' });
                        finish = (int)Convert.ToDouble(dashslash[0]);
                        increment = (int)Convert.ToDouble(dashslash[1]);
                    }
                    else
                        finish = (int)Convert.ToDouble(dash[1]);

                    for (int i = start; i <= finish; i+=increment)
                    {
                        times.Add(i);
                    }
                }
                else if (Double.TryParse(t, out temp))
                {
                    if (!times.Contains((int)temp))
                        times.Add((int)temp);
                }

            }
            return times;
        }
    }
}
