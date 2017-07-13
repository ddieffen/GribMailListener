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

        internal static List<GribTimeFile> DecomposeFromStrings(List<string> files, string modelRequest)
        {
            List<GribTimeFile> list = new List<GribTimeFile>();

            foreach (string file in files)
            {
                string[] split = file.Split(new char[] { '.' });
                GribTimeFile tref = new GribTimeFile();
                if (modelRequest == "nam-conus")
                {
                    tref.full_name = file;
                    tref.cycle = Convert.ToInt16(split[1].Trim('t').Trim('z'));
                    tref.model = split[0];
                    tref.time = Convert.ToInt16(split[2].TrimStart("awphys".ToCharArray()));
                }
                else if (modelRequest == "nam-na")
                {
                    tref.full_name = file;
                    tref.cycle = Convert.ToInt16(split[1].Trim('t').Trim('z'));
                    tref.model = split[0];
                    tref.time = Convert.ToInt16(split[2].Replace("awip32",""));
                }
                else if (modelRequest == "gfs0.25")
                {
                    tref.full_name = file;
                    tref.cycle = Convert.ToInt16(split[1].Trim('t').Trim('z'));
                    tref.model = split[0];
                    if (split[4] == "anl")
                        continue;
                    tref.time = Convert.ToInt16(split[4].TrimStart("f".ToCharArray()));
                }
                else if (modelRequest == "gfs0.5")
                {
                    tref.full_name = file;
                    tref.cycle = Convert.ToInt16(split[1].Trim('t').Trim('z'));
                    tref.model = split[0];
                    if (split[4] == "anl")
                        continue;
                    tref.time = Convert.ToInt16(split[4].TrimStart("f".ToCharArray()));
                }
                else if (modelRequest == "gfs1.0")
                {
                    tref.full_name = file;
                    tref.cycle = Convert.ToInt16(split[1].Trim('t').Trim('z'));
                    tref.model = split[0];
                    if (split[4] == "anl")
                        continue;
                    tref.time = Convert.ToInt16(split[4].TrimStart("f".ToCharArray()));
                }
                else if (modelRequest == "gefs-low-avg")
                {
                    if (split[0] != "geavg")
                        continue;
                    tref.full_name = file;
                    tref.cycle = Convert.ToInt16(split[1].Trim('t').Trim('z'));
                    tref.model = split[0];
                    tref.time = Convert.ToInt16(split[2].Replace("pgrb2a_bcf",""));
                }
                else if (modelRequest == "gefs-low-mode")
                {
                    if (split[0] != "gemode")
                        continue;
                    tref.full_name = file;
                    tref.cycle = Convert.ToInt16(split[1].Trim('t').Trim('z'));
                    tref.model = split[0];
                    tref.time = Convert.ToInt16(split[2].Replace("pgrb2a_bcf", ""));
                }
                else if (modelRequest == "gefs-hi-avg")
                {
                    if (split[0] != "geavg")
                        continue;
                    tref.full_name = file;
                    tref.cycle = Convert.ToInt16(split[1].Trim('t').Trim('z'));
                    tref.model = split[0];
                    tref.time = Convert.ToInt16(split[2].TrimStart("ndgd_conusf".ToCharArray()));
                }
                else if (modelRequest == "gefs-hi-mode")
                {
                    if (split[0] != "gemode")
                        continue;
                    tref.full_name = file;
                    tref.cycle = Convert.ToInt16(split[1].Trim('t').Trim('z'));
                    tref.model = split[0];
                    tref.time = Convert.ToInt16(split[2].TrimStart("ndgd_conusf".ToCharArray()));
                }
                else if (modelRequest == "naefs-low-avg")
                {
                    if (split[0] != "naefs_geavg")
                        continue;
                    tref.full_name = file;
                    tref.cycle = Convert.ToInt16(split[1].Trim('t').Trim('z'));
                    tref.model = split[0];
                    tref.time = Convert.ToInt16(split[2].Replace("pgrb2a_bcf", ""));
                }
                else if (modelRequest == "naefs-low-mode")
                {
                    if (split[0] != "naefs_gemode")
                        continue;
                    tref.full_name = file;
                    tref.cycle = Convert.ToInt16(split[1].Trim('t').Trim('z'));
                    tref.model = split[0];
                    tref.time = Convert.ToInt16(split[2].Replace("pgrb2a_bcf", ""));
                }
                else if (modelRequest == "naefs-hi-avg")
                {
                    if (split[0] != "naefs_geavg")
                        continue;
                    tref.full_name = file;
                    tref.cycle = Convert.ToInt16(split[1].Trim('t').Trim('z'));
                    tref.model = split[0];
                    tref.time = Convert.ToInt16(split[2].Replace("ndgd_conusf", ""));
                }
                else if (modelRequest == "naefs-hi-mode")
                {
                    if (split[0] != "naefs_gemode")
                        continue;
                    tref.full_name = file;
                    tref.cycle = Convert.ToInt16(split[1].Trim('t').Trim('z'));
                    tref.model = split[0];
                    tref.time = Convert.ToInt16(split[2].Replace("ndgd_conusf", ""));
                }
                else if (modelRequest == "hrw-conus")
                {
                    if (split[2].Contains("arw"))
                        continue;
                    tref.full_name = file;
                    tref.cycle = Convert.ToInt16(split[1].Trim('t').Trim('z'));
                    tref.model = split[0];
                    tref.time = Convert.ToInt16(split[3].Replace("f", ""));
                }

                list.Add(tref);
            }

            return list;    
        }

        static int ToTime(string dateTime1, string dateTime2)
        {
            int y1, m1, d1, t1, y2, m2, d2, t2;
            y1 = Convert.ToInt16(dateTime1.Substring(0, 4));
            m1 = Convert.ToInt16(dateTime1.Substring(4, 2));
            d1 = Convert.ToInt16(dateTime1.Substring(6, 2));
            t1 = Convert.ToInt16(dateTime1.Substring(8, 2));
            y2 = Convert.ToInt16(dateTime2.Substring(0, 4));
            m2 = Convert.ToInt16(dateTime2.Substring(4, 2));
            d2 = Convert.ToInt16(dateTime2.Substring(6, 2));
            t2 = Convert.ToInt16(dateTime2.Substring(8, 2));

            TimeSpan ts = new DateTime(y2, m2, d2, t2, 0, 0) - new DateTime(y1, m1, d1, t1, 0, 0);
            return (int)ts.TotalHours;
        }

        public static List<string> KnownModels = new List<string>() { "nam-conus", "nam-na"
            , "gfs0.25", "gfs0.5", "gfs1.0"
            , "gefs-hi-avg", "gefs-low-avg", "naefs-hi-avg", "naefs-low-avg", "gefs-hi-mode", "gefs-low-mode", "naefs-hi-mode", "naefs-low-mode", "hrw-conus"};

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
        internal static string DoFilterGrib(string request, out List<string> errorsAndWarnings)
        {
            errorsAndWarnings = new List<string>();
            string[] splitRequest = request.Split(new char[] { ':' }, 3);
            string modelRequest = splitRequest[0];
            string[] regionRequest = splitRequest[1].Split(new char[] { ',' }, 4);
            string[] timeRequest = splitRequest[2].Split(new char[] { ',' });

            int leftLon = (int)Convert.ToDouble(regionRequest[0]);
            int rightLon = (int)Convert.ToDouble(regionRequest[1]);
            int topLat = (int)Convert.ToDouble(regionRequest[2]);
            int bottomLat = (int)Convert.ToDouble(regionRequest[3]);

            string urlDir = "";
            string urlModel = "";
            string urlParameter = "";
            switch (modelRequest) 
            {
                case "nam-conus":
                    urlDir = "http://nomads.ncep.noaa.gov/cgi-bin/filter_nam.pl";
                    urlModel = urlDir + "?file=";
                    urlParameter = "&lev_10_m_above_ground=on&lev_mean_sea_level=on&var_UGRD=on&var_VGRD=on&var_PRMSL=on&subregion=&leftlon=" 
                        + leftLon + "&rightlon=" + rightLon + "&toplat=" + topLat + "&bottomlat=" + bottomLat + "&dir=";
                    break;
                case "nam-na":
                    urlDir = "http://nomads.ncep.noaa.gov/cgi-bin/filter_nam_na.pl";
                    urlModel = urlDir + "?file=";
                    urlParameter = "&lev_10_m_above_ground=on&lev_mean_sea_level=on&var_UGRD=on&var_VGRD=on&var_PRMSL=on&subregion=&leftlon=" 
                        + leftLon + "&rightlon=" + rightLon + "&toplat=" + topLat + "&bottomlat=" + bottomLat + "&dir=";
                    break;
                case "gfs0.25":
                    urlDir = "http://nomads.ncep.noaa.gov/cgi-bin/filter_gfs_0p25.pl";
                    urlModel = urlDir + "?file=";
                    urlParameter = "&lev_10_m_above_ground=on&lev_mean_sea_level=on&var_UGRD=on&var_VGRD=on&var_PRMSL=on&subregion=&leftlon=" 
                        + leftLon + "&rightlon=" + rightLon + "&toplat=" + topLat + "&bottomlat=" + bottomLat + "&dir=";
                    break;
                case "gfs0.5":
                    urlDir = "http://nomads.ncep.noaa.gov/cgi-bin/filter_gfs_0p50.pl";
                    urlModel = urlDir + "?file=";
                    urlParameter = "&lev_10_m_above_ground=on&lev_mean_sea_level=on&var_UGRD=on&var_VGRD=on&var_PRMSL=on&subregion=&leftlon=" 
                        + leftLon + "&rightlon=" + rightLon + "&toplat=" + topLat + "&bottomlat=" + bottomLat + "&dir=";
                    break;
                case "gfs1.0":
                    urlDir = "http://nomads.ncep.noaa.gov/cgi-bin/filter_gfs_1p00.pl";
                    urlModel = urlDir + "?file=";
                    urlParameter = "&lev_10_m_above_ground=on&lev_mean_sea_level=on&var_UGRD=on&var_VGRD=on&var_PRMSL=on&subregion=&leftlon=" 
                        + leftLon + "&rightlon=" + rightLon + "&toplat=" + topLat + "&bottomlat=" + bottomLat + "&dir=";
                    break;
                case "gefs-low-avg":
                    urlDir = "http://nomads.ncep.noaa.gov/cgi-bin/filter_gensbc.pl";
                    urlModel = urlDir + "?file=";
                    urlParameter = "&lev_10_m_above_ground=on&lev_mean_sea_level=on&var_UGRD=on&var_VGRD=on&var_PRMSL=on&subregion=&leftlon=" 
                        + leftLon + "&rightlon=" + rightLon + "&toplat=" + topLat + "&bottomlat=" + bottomLat + "&dir=";
                    break;
                case "gefs-low-mode":
                    urlDir = "http://nomads.ncep.noaa.gov/cgi-bin/filter_gensbc.pl";
                    urlModel = urlDir + "?file=";
                    urlParameter = "&lev_10_m_above_ground=on&lev_mean_sea_level=on&var_UGRD=on&var_VGRD=on&var_PRMSL=on&subregion=&leftlon="
                        + leftLon + "&rightlon=" + rightLon + "&toplat=" + topLat + "&bottomlat=" + bottomLat + "&dir=";
                    break;
                case "gefs-hi-avg":
                    urlDir = "http://nomads.ncep.noaa.gov/cgi-bin/filter_gensbc_ndgd.pl";
                    urlModel = urlDir + "?file=";
                    urlParameter = "&lev_surface=on&lev_10_m_above_ground=on&var_PRES=on&var_UGRD=on&var_VGRD=on&subregion=&leftlon="
                        + leftLon + "&rightlon=" + rightLon + "&toplat=" + topLat + "&bottomlat=" + bottomLat + "&dir=";
                    break;
                case "gefs-hi-mode":
                    urlDir = "http://nomads.ncep.noaa.gov/cgi-bin/filter_gensbc_ndgd.pl";
                    urlModel = urlDir + "?file=";
                    urlParameter = "&lev_surface=on&lev_10_m_above_ground=on&var_PRES=on&var_UGRD=on&var_VGRD=on&subregion=&leftlon="
                        + leftLon + "&rightlon=" + rightLon + "&toplat=" + topLat + "&bottomlat=" + bottomLat + "&dir=";
                    break;
                case "naefs-low-avg":
                    urlDir = "http://nomads.ncep.noaa.gov/cgi-bin/filter_naefsbc.pl";
                    urlModel = urlDir + "?file=";
                    urlParameter = "&lev_10_m_above_ground=on&lev_mean_sea_level=on&var_UGRD=on&var_VGRD=on&var_PRMSL=on&subregion=&leftlon="
                        + leftLon + "&rightlon=" + rightLon + "&toplat=" + topLat + "&bottomlat=" + bottomLat + "&dir=";
                    break;
                case "naefs-low-mode":
                    urlDir = "http://nomads.ncep.noaa.gov/cgi-bin/filter_naefsbc.pl";
                    urlModel = urlDir + "?file=";
                    urlParameter = "&lev_10_m_above_ground=on&lev_mean_sea_level=on&var_UGRD=on&var_VGRD=on&var_PRMSL=on&subregion=&leftlon="
                        + leftLon + "&rightlon=" + rightLon + "&toplat=" + topLat + "&bottomlat=" + bottomLat + "&dir=";
                    break;
                case "naefs-hi-avg":
                    urlDir = "http://nomads.ncep.noaa.gov/cgi-bin/filter_naefsbc_ndgd.pl";
                    urlModel = urlDir + "?file=";
                    urlParameter = "&lev_surface=on&lev_10_m_above_ground=on&var_PRES=on&var_UGRD=on&var_VGRD=on&subregion=&leftlon="
                        + leftLon + "&rightlon=" + rightLon + "&toplat=" + topLat + "&bottomlat=" + bottomLat + "&dir=";
                    break;
                case "naefs-hi-mode":
                    urlDir = "http://nomads.ncep.noaa.gov/cgi-bin/filter_naefsbc_ndgd.pl";
                    urlModel = urlDir + "?file=";
                    urlParameter = "&lev_surface=on&lev_10_m_above_ground=on&var_PRES=on&var_UGRD=on&var_VGRD=on&subregion=&leftlon="
                        + leftLon + "&rightlon=" + rightLon + "&toplat=" + topLat + "&bottomlat=" + bottomLat + "&dir=";
                    break;
                case "hrw-conus":
                    urlDir = "http://nomads.ncep.noaa.gov/cgi-bin/filter_hiresconus.pl";
                    urlModel = urlDir + "?file=";
                    urlParameter = "&lev_surface=on&lev_10_m_above_ground=on&var_PRES=on&var_UGRD=on&var_VGRD=on&subregion=&leftlon="
                        + leftLon + "&rightlon=" + rightLon + "&toplat=" + topLat + "&bottomlat=" + bottomLat + "&dir=";
                    break;
                default:
                    return "Wrong model";
            }

            if (regionRequest.Length != 4)
                return "Wrong number of lat lon";

            //finding the index page with all models
            WebClient w = new WebClient();
            string s = w.DownloadString(urlDir);

            //find latest simulation day
            LinkItem latest = new LinkItem();
            while(s.Contains("Subdirectory"))
            {
                latest = Finder.FindLinks(s)[0];
                s = w.DownloadString(latest.Href);
            }
            List<string> files = Finder.FindOptionsValues(s);
            if (files.Count == 0)
                errorsAndWarnings.Add("WARNING: Cannot find any files for " + latest + ". Probably the NOAA has not started uploading the files.");
            List<GribTimeFile> gribTimes = GRIBTools.DecomposeFromStrings(files, modelRequest);

            int maxCycle = 0;
            foreach (GribTimeFile time in gribTimes)
                if (time.cycle > maxCycle)
                    maxCycle = time.cycle;

            string simulationDate = DateTime.Now.ToString("yyyyMMdd-Hmm");
            string[] split;
            switch (modelRequest)
            {
                case "nam-conus":
                    simulationDate = latest.Href.Substring(latest.Href.Length-8,8);
                    break;
                case "nam-na":
                    simulationDate = latest.Href.Substring(latest.Href.Length - 8, 8);
                    break;
                case "gfs0.25":
                    simulationDate = latest.Href.Substring(latest.Href.Length - 10, 8);
                    break;
                case "gfs0.5":
                    simulationDate = latest.Href.Substring(latest.Href.Length - 10, 8);
                    break;
                case "gfs1.0":
                    simulationDate = latest.Href.Substring(latest.Href.Length - 10, 8);
                    break;
                case "hrw-conus":
                    simulationDate = latest.Href.Substring(latest.Href.Length - 8, 8);
                    break;
                case "gefs-low-avg":
                    split = latest.Href.Split('%');
                    if (split.Length > 2)
                        simulationDate = split[1].Substring(split[1].Length - 8, 8);
                    break;
                case "gefs-low-mode":
                    split = latest.Href.Split('%');
                    if (split.Length > 2)
                        simulationDate = split[1].Substring(split[1].Length - 8, 8);
                    break;
                case "gefs-hi-avg":
                    split = latest.Href.Split('%');
                    if (split.Length > 2)
                        simulationDate = split[1].Substring(split[1].Length - 8, 8);
                    break;
                case "gefs-hi-mode":
                    split = latest.Href.Split('%');
                    if (split.Length > 2)
                        simulationDate = split[1].Substring(split[1].Length - 8, 8);
                    break;
                case "naefs-low-avg":
                    break;
                case "naefs-low-mode":
                    break;
                case "naefs-hi-avg":
                    break;
                case "naefs-hi-mode":
                    break;
            }


            //download all binary files
            List<GribTimeFile> downloaded = new List<GribTimeFile>();
            int futureStackSize = 0;
            List<int> cycleTimes = CreateTimes(timeRequest);
            List<int> cycleFound = new List<int>();
            foreach (GribTimeFile file in gribTimes.Where(t => t.cycle == maxCycle))
            {
                if (cycleTimes.Contains(file.time))
                {
                    cycleFound.Add(file.time);
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

            if (files.Count != 0 && cycleTimes.Count != cycleFound.Count) {
                string missing = "";
                foreach (int i in cycleTimes)
                {
                    if (!cycleFound.Contains(i))
                        missing += i.ToString() + ", ";
                }
                errorsAndWarnings.Add("Warning required time missing T+:" + missing.TrimEnd(','));
            }

            //stacking grib files
            byte[] stacked = new byte[futureStackSize];
            int currentPosition = 0;
            foreach (GribTimeFile grbTime in downloaded)
            {
                grbTime.buffer.CopyTo(stacked, currentPosition);
                currentPosition += grbTime.buffer.Length;
            }

            string times = "";
            if (cycleFound.Count > 0)
                times += cycleFound.Min().ToString();
            if (cycleFound.Count > 1)
                times += "-" + cycleFound.Max().ToString();

            //writing the file
            string path = modelRequest + "_" + simulationDate + "_T" + maxCycle + "Z_" + times + ".grb";
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
