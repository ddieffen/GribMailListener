using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using YellowbrickV8.Entities;

namespace YellowbrickV8
{
    public static class YBTracker
    {
        /// <summary>
        /// Retreives all positions and put them into instances of Team and Moments
        /// </summary>
        /// <param name="serverName">YB server name</param>
        /// <param name="raceKey">Race key to select data</param>
        /// <returns>List of Team containing all Moments known</returns>
        public static List<Team> getAllPositions(string serverName, string raceKey)
        {
            #region http request
            Random rnd = new Random();
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(serverName + "/BIN/" + raceKey + "/AllPositions?r=" + rnd.NextDouble());
            webRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.106 Safari/537.36";
            webRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            webRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
            Stream responseStream = webResponse.GetResponseStream();
            MemoryStream m = new MemoryStream();
            responseStream.CopyTo(m);
            byte[] buffer = m.ToArray();
            #endregion

            short confByte = buffer[0];
            var doAltitude = ((confByte & 0x01) == 0x01);
            var doDTF = ((confByte & 0x02) == 0x02);

            var startOff = buffer.getUint32(1);

            var cursor = 5;

            List<Team> teams = new List<Team>();

            while (cursor < buffer.Length)
            {
                Team team = new Team();

                //read first 2 bytes - this is the team id
                var teamId = buffer.getUint16(cursor);
                cursor += 2;
                team.id = (int)teamId;

                var teamMoments = team.moments;

                #region team positions
                var numberOfPositions = buffer.getUint16(cursor);
                cursor += 2;

                Moment lastMoment = new Moment();

                for (var i = 0; i < numberOfPositions; i++)
                {
                    var firstByte = buffer.getUint8(cursor);

                    Moment moment = new Moment();

                    if ((firstByte & 0x80) == 0x80)
                    {
                        #region relative mode
                        var atDelta = buffer.getUint16(cursor);
                        cursor += 2;
                        var latDelta = buffer.getInt16(cursor);
                        cursor += 2;
                        var lonDelta = buffer.getInt16(cursor);
                        cursor += 2;

                        if (doAltitude)
                        {
                            moment.altMeters = buffer.getInt16(cursor);
                            cursor += 2;
                        }

                        if (doDTF)
                        {
                            // expecting a 2-byte short, in metres
                            var dtfDelta = buffer.getInt16(cursor);
                            cursor += 2;
                            moment.dtfMeters = lastMoment.dtfMeters + dtfDelta;
                        }

                        atDelta = atDelta & 0x7fff;

                        moment.lat = lastMoment.lat + ((double)latDelta / 100000);
                        moment.lon = lastMoment.lon + ((double)lonDelta / 100000);
                        moment.at = lastMoment.at - atDelta;
                        #endregion
                    }
                    else
                    {//
                        #region absolute mode
                        var at = buffer.getUint32(cursor);
                        cursor += 4;
                        var lat = buffer.getInt32(cursor);
                        cursor += 4;
                        var lon = buffer.getInt32(cursor);
                        cursor += 4;

                        if (doAltitude)
                        {
                            moment.altMeters = buffer.getInt16(cursor);
                            cursor += 2;
                        }

                        if (doDTF)
                        {
                            // expecting a 4-byte int, in metres
                            var dtf = buffer.getInt32(cursor);
                            cursor += 4;
                            moment.dtfMeters = dtf;
                        }

                        moment.lat = (double)lat / 100000;
                        moment.lon = (double)lon / 100000;
                        moment.at = startOff + at;
                        #endregion
                    }

                    teamMoments.Add(moment);
                    lastMoment = moment;

                }
                #endregion

                teams.Add(team);
            }

            return teams;
        }

        /// <summary>
        /// Retreives only latest positions and put them into instances of Team and Moments
        /// </summary>
        /// <param name="serverName">YB server name</param>
        /// <param name="raceKey">Race key to select data</param>
        /// <returns>List of Team containing the latest Moments only</returns>
        public static List<Team> getNewPositions(string serverName, string raceKey)
        {
            #region http request
            Random rnd = new Random();
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(serverName + "/BIN/" + raceKey + "/LatestPositions?r=" + rnd.NextDouble());
            HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
            Stream responseStream = webResponse.GetResponseStream();
            MemoryStream m = new MemoryStream();
            responseStream.CopyTo(m);
            byte[] buffer = m.ToArray();
            #endregion

            short confByte = buffer[0];
            var doAltitude = ((confByte & 0x01) == 0x01);
            var doDTF = ((confByte & 0x02) == 0x02);

            var startOff = buffer.getUint32(1);

            var cursor = 5;

            List<Team> teams = new List<Team>();

            while (cursor < buffer.Length)
            {
                Team team = new Team();

                //read first 2 bytes - this is the team id
                var teamId = buffer.getUint16(cursor);
                cursor += 2;
                team.id = (int)teamId;

                var teamMoments = team.moments;

                #region team positions
                var numberOfPositions = buffer.getUint16(cursor);
                cursor += 2;

                Moment lastMoment = new Moment();

                for (var i = 0; i < numberOfPositions; i++)
                {
                    var firstByte = buffer.getUint8(cursor);

                    Moment moment = new Moment();

                    if ((firstByte & 0x80) == 0x80)
                    {
                        #region relative mode
                        var atDelta = buffer.getUint16(cursor);
                        cursor += 2;
                        var latDelta = buffer.getInt16(cursor);
                        cursor += 2;
                        var lonDelta = buffer.getInt16(cursor);
                        cursor += 2;

                        if (doAltitude)
                        {
                            moment.altMeters = buffer.getInt16(cursor);
                            cursor += 2;
                        }

                        if (doDTF)
                        {
                            // expecting a 2-byte short, in metres
                            var dtfDelta = buffer.getInt16(cursor);
                            cursor += 2;
                            moment.dtfMeters = lastMoment.dtfMeters + dtfDelta;
                        }

                        atDelta = atDelta & 0x7fff;

                        moment.lat = lastMoment.lat + ((double)latDelta / 100000);
                        moment.lon = lastMoment.lon + ((double)lonDelta / 100000);
                        moment.at = lastMoment.at - atDelta;
                        #endregion
                    }
                    else
                    {//
                        #region absolute mode
                        var at = buffer.getUint32(cursor);
                        cursor += 4;
                        var lat = buffer.getInt32(cursor);
                        cursor += 4;
                        var lon = buffer.getInt32(cursor);
                        cursor += 4;

                        if (doAltitude)
                        {
                            moment.altMeters = buffer.getInt16(cursor);
                            cursor += 2;
                        }

                        if (doDTF)
                        {
                            // expecting a 4-byte int, in metres
                            var dtf = buffer.getInt32(cursor);
                            cursor += 4;
                            moment.dtfMeters = dtf;
                        }

                        moment.lat = (double)lat / 100000;
                        moment.lon = (double)lon / 100000;
                        moment.at = startOff + at;
                        #endregion
                    }

                    teamMoments.Add(moment);
                    lastMoment = moment;
                }

                #endregion

                teams.Add(team);
            }

            return teams;
        }

        /// <summary>
        /// Gets all the race informations into an instance of a Race object
        /// </summary>
        /// <param name="serverName">YB server name</param>
        /// <param name="raceKey">Race key to select data</param>
        /// <returns>Instance of a Race with all information known</returns>
        public static Race getRaceInformation(string serverName, string raceKey)
        {
            #region http request
            WebClient webClient = new WebClient();
            string url = serverName + "/JSON/" + raceKey + "/RaceSetup?_=" + Math.Ceiling(TimeTools.DateTimeToUnixTime(DateTime.Now) * 1000);
            string jsonRace = webClient.DownloadString(url);
            #endregion

            //dynamic race = JsonConvert.DeserializeObject(jsonRace);
            //string title = raceKey.title;
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.CheckAdditionalContent = false;
            settings.NullValueHandling = NullValueHandling.Ignore;
            Race race = JsonConvert.DeserializeObject<Race>(jsonRace, settings);
            return race;
        }

        /// <summary>
        /// This method uses the teams information available to update the race instance
        /// </summary>
        /// <param name="raceDst">Instance to be updated</param>
        /// <param name="teamsSrc">Partial teams information that will be used to update the race</param>
        public static void UpdateTeamsMoments(List<Team> teamsDst, List<Team> teamsSrc)
        {
            foreach (Team teamSrc in teamsSrc)
            {
                Team teamDst = teamsDst.SingleOrDefault(i => i.id == teamSrc.id);
                if (teamDst == null)
                {//if the race does not contains the team we add it
                    teamsDst.Add(teamSrc);
                    teamDst = teamSrc;
                }
                else
                {//if the race already contains a team with the same id, we then update the moments for that team
                    foreach (Moment momentSrc in teamSrc.moments)
                    {
                        if (!teamDst.moments.Any(m => m.at == momentSrc.at))
                            teamDst.moments.Add(momentSrc);
                    }
                }
            }
        }

        /// <summary>
        /// Looks thought all the known moments and calculates speed and heading when available
        /// </summary>
        /// <param name="team">Team for which all speeds and headings are going to be calculated</param>
        public static void UpdateMomentsSpeedHeading(Team team)
        { 
            Moment previous = null;
            foreach (Moment moment in team.moments.OrderBy(m => m.at))
            {
                if (previous != null)
                {
                    double distance = CoordinateTools.HaversineDistanceNauticalMiles(previous.lat, previous.lon, moment.lat, moment.lon);
                    double timeDiff = (double)(moment.at - previous.at) / 3600;
                    moment.distNm = distance;
                    moment.spdKn = distance / timeDiff;
                    team.maxSpeedKn = Math.Max(team.maxSpeedKn, moment.spdKn);
                    moment.heading = CoordinateTools.HaversineHeadingDegrees(previous.lat, previous.lon, moment.lat, moment.lon);
                }
                previous = moment;
            }
        }

        /// <summary>
        /// Generates a human readable report for a few selected teams
        /// </summary>
        /// <param name="race">Race containing the necessary data</param>
        /// <param name="teamsIDs">Teams selected for the report</param>
        /// <returns>Report for the selected teams</returns>
        public static string ReportSelectedTeams(Race race, List<int> teamsIDs, int referenceTeam = -1)
        {
            string report = "";

            Dictionary<int, Moment> latestMoments = new Dictionary<int, Moment>();
            foreach (Team team in race.teams)
            {
                if (teamsIDs == null || teamsIDs.Count == 0 || teamsIDs.Contains(team.id))
                {
                    Moment latest = new Moment();
                    latest.dtfMeters = (int)(race.course.distance * 1000);
                    foreach (Moment moment in team.moments)
                    {
                        if (moment.at > latest.at)
                            latest = moment;
                    }
                    latestMoments.Add(team.id, latest);
                }
            }

            Team refTeam;
            if (race.teams.Any(t => t.id == referenceTeam))
                refTeam = race.teams.Single(t => t.id == referenceTeam);
            else
            {
                int winningTeam = latestMoments.OrderBy(t => t.Value.dtfMeters).First().Key;
                refTeam = race.teams.Single(t => t.id == winningTeam);
            }

            Moment reference = latestMoments[refTeam.id];

            report += "Reference Team: " + refTeam.name + "\r\n\r\n";
            report += "\tNAME\tSPEED\tToFinish\tRelToF\tRel A\tRel D\tLat\tLon\tAt\r\n";
            int position = 1;
            foreach (KeyValuePair<int, Moment> teamPair in latestMoments.OrderBy(p => p.Value.dtfMeters))
            {
                Team team = race.teams.Single(t => t.id == teamPair.Key);
                if (team.status == "RACING")
                {
                    Moment moment = teamPair.Value;

                    report += position + ")\t"
                        + team.name.Lenght(18) + "\t"
                        + moment.spdKn.ToString("00.0") + "kn\t"
                        + UnitTools.M2Nm(moment.dtfMeters).ToString("0000.0") + "mn\t"
                        + UnitTools.M2Nm(reference.dtfMeters - moment.dtfMeters).ToString("0000.0") + "mn\t"
                        + CoordinateTools.HaversineHeadingDegrees(reference.lat, reference.lon, moment.lat, moment.lon).ToString("000.0") + "\t"
                        + CoordinateTools.HaversineDistanceNauticalMiles(reference.lat, reference.lon, moment.lat, moment.lon).ToString("0000.0") + "nm\t"
                        + moment.lat.ToString("000.000000") + "\t"
                        + moment.lon.ToString("000.000000") + "\t"
                        + TimeTools.UnixTimeStampToDateTime(moment.at).ToString("u")
                        + "\r\n";
                    position++;
                }
            }

            return report;
        }

        /// <summary>
        /// Generates a human readable report for a few selected teams
        /// </summary>
        /// <param name="race">Race containing the necessary data</param>
        /// <param name="teamsIDs">Teams selected for the report</param>
        /// <returns>Report for the selected teams</returns>
        public static string ReportSelectedTeamsHTML(Race race, int section, int referenceTeam = -1)
        {
            string report = "";

            Dictionary<int, Moment> latestMoments = new Dictionary<int, Moment>();
            foreach (Team team in race.teams)
            {
                if (team.tags != null)
                {
                    if (team.tags.Contains(section))
                    {
                        Moment latest = new Moment();
                        latest.dtfMeters = (int)(race.course.distance * 1000);
                        foreach (Moment moment in team.moments)
                        {
                            if (moment.at > latest.at)
                                latest = moment;
                        }
                        latestMoments.Add(team.id, latest);
                    }
                }
            }

            Team refTeam;
            if (race.teams.Any(t => t.id == referenceTeam))
                refTeam = race.teams.Single(t => t.id == referenceTeam);
            else
            {
                if (latestMoments.Count > 0)
                {
                    int winningTeam = latestMoments.OrderBy(t => t.Value.dtfMeters).First().Key;
                    refTeam = race.teams.Single(t => t.id == winningTeam);
                }
                else
                    refTeam = null;
            }
            Moment reference = null;
            if (refTeam != null)
                reference = latestMoments[refTeam.id];
            
            report += "Reference Team: " + (refTeam!=null?refTeam.name:"Unknown Team") + "</br></br>";
            report += "<table cellpadding=5 border=1 cellspacing=0>";
            report += "<tr><td>Pos</td><td>NAME</td><td>SPEED</td><td>ToFinish</td><td>RelToF</td><td>Rel Ang</td><td>Rel Dst</td><td>Lat</td><td>Lon</td><td>At</td></tr>\r\n";
            int position = 1;
            
            foreach (KeyValuePair<int, Moment> teamPair in latestMoments.OrderBy(p => p.Value.dtfMeters))
            {
                Team team = race.teams.Single(t => t.id == teamPair.Key);
                if (team.status == "RACING")
                {
                    Moment moment = teamPair.Value;

                    if ((double)position % 2 == 0)
                        report += "<tr bgcolor=#dbeeff align=right>";
                    else
                        report += "<tr align=right>";

                    report += "<td>" + position + "</td>"
                        + "<td>" + (team.id == (refTeam != null ? refTeam.id:-1) ? ">>" : "") + team.name.Lenght(18) + "</td>"
                        + "<td>" + moment.spdKn.ToString("0.0") + "kn</td>"
                        + "<td>" + UnitTools.M2Nm(moment.dtfMeters).ToString("0.0") + "mn</td>"
                        + "<td>" + UnitTools.M2Nm(moment.dtfMeters - (reference!=null?reference.dtfMeters:0)).ToString("0.0") + "mn</td>"
                        + "<td>" + CoordinateTools.HaversineHeadingDegrees(reference.lat, (reference != null ? reference.lon:0), moment.lat, moment.lon).ToString("0.0") + "</td>"
                        + "<td>" + CoordinateTools.HaversineDistanceNauticalMiles(reference.lat, (reference != null ? reference.lon:0), moment.lat, moment.lon).ToString("0.0") + "nm</td>"
                        + "<td>" + moment.lat.ToString("0.000000") + "</td>"
                        + "<td>" + moment.lon.ToString("0.000000") + "</td>"
                        + "<td>" + TimeTools.UnixTimeStampToDateTime(moment.at).ToString("u") + "</td>"
                        + "</tr>";
                    position++;
                }
            }
            report += "</table><br><br>";

            foreach (KeyValuePair<int, Moment> teamPair in latestMoments.OrderBy(p => p.Value.dtfMeters))
            {
                Team team = race.teams.Single(t => t.id == teamPair.Key);
                if (team.status != "RACING")
                    report += team.name + " - " + team.status + "<br>";
            }

            return report;
        }
    }
}
