using System;
using System.Collections.Generic;

namespace YellowbrickV8.Entities
{
    [Serializable]
    public class Poi
    {
        public List<object> lines { get; set; }
    }

    [Serializable]
    public class Node
    {
        public double lon { get; set; }
        public string name { get; set; }
        public double lat { get; set; }
    }

    [Serializable]
    public class Course
    {
        /// <summary>
        /// Distance in Kilometers
        /// </summary>
        public double distance { get; set; }
        public List<Node> nodes { get; set; }
    }

    [Serializable]
    public class Tag
    {
        public int id { get; set; }
        public int sort { get; set; }
        public int start { get; set; }
        public string name { get; set; }
        public string handicap { get; set; }
        public int show { get; set; }
        public bool lb { get; set; }

        public override string ToString()
        {
            return name;
        }
    }

    [Serializable]
    public class Logo
    {
        public object href { get; set; }
        public int y { get; set; }
        public string url { get; set; }
        public int x { get; set; }
    }

    [Serializable]
    public class Team
    {
        public Team()
        {
            this.moments = new List<Moment>();
        }

        public List<int> tags { get; set; }
        public string model { get; set; }
        public string status { get; set; }
        public string img { get; set; }
        public string tcf3 { get; set; }
        public string type { get; set; }
        public string tcf2 { get; set; }
        public string url { get; set; }
        public string captain { get; set; }
        public string tcf1 { get; set; }
        public int id { get; set; }
        public string colour { get; set; }
        public string flag { get; set; }
        public int start { get; set; }
        public string name { get; set; }
        public string owner { get; set; }
        public string sail { get; set; }
        public string explain { get; set; }
        public List<Moment> moments { get; set; }

        //Calculated value to know what was the max speed of that boat and normalize speeds on scatter display, in knots
        public double maxSpeedKn { get; set; }

        public override string ToString()
        {
            return name;
        }
    }

    [Serializable]
    public class Advert
    {
        public int duration { get; set; }
        public string href { get; set; }
        public int y { get; set; }
        public string url { get; set; }
        public int x { get; set; }
    }

    [Serializable]
    public class Race
    {
        public int stop { get; set; }
        public string viewerMode { get; set; }
        public int tzOffset { get; set; }
        public int loadingAdvertSeconds { get; set; }
        public string zoom { get; set; }
        public string mapTypeGoogle { get; set; }
        public string mapUnits { get; set; }
        public string lang { get; set; }
        public Poi poi { get; set; }
        public Course course { get; set; }
        public string showTracksDefault { get; set; }
        public string title { get; set; }
        public object mapCustomCss { get; set; }
        public string tab_leaderboard { get; set; }
        public int zoomLevel { get; set; }
        public string tab_teams { get; set; }
        public int fadeFullTracksAfterTime { get; set; }
        public List<Tag> tags { get; set; }
        public bool showFadedTracks { get; set; }
        public Logo logo { get; set; }
        public object hashtag { get; set; }
        public bool showEstimatedFinish { get; set; }
        public string url { get; set; }
        public List<Team> teams { get; set; }
        public string tz { get; set; }
        public List<object> associated { get; set; }
        public int start { get; set; }
        public string tab_social { get; set; }
        public object loadingAdvertUrl { get; set; }
        public string zoomArea { get; set; }
        public List<Advert> adverts { get; set; }
    }
}
