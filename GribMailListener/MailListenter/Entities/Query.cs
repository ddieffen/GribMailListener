using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using YellowbrickV6;
using YellowbrickV6.Entities;
using System.Net;

namespace MailListenter
{
    internal class Query
    {
        internal enum QueryType { saildocs, saildocsanser, forward, help, raceinfo, sectioninfo, grib, tweet };

        internal AE.Net.Mail.MailMessage m;
        internal QueryType type;
        internal List<string> awaiting = null;

        /// <summary>
        /// Builds a query from a string
        /// </summary>
        /// <param name="p"></param>
        public Query(AE.Net.Mail.MailMessage message, List<string> awaiting)
        {
            this.m = message;
            this.awaiting = awaiting;
        }

        public Query()
        {
            // TODO: Complete member initialization
        }

        internal bool isValid()
        {
            try
            {
                if (m != null && m.Subject.ToLower().StartsWith("saildocs:"))
                {
                    if (m.Body.Trim("\r\n".ToCharArray()).StartsWith("send") && m.Body.Split(':').Length == 2 && m.Body.Split(',').Length == 4
                        && m.From.Address != "query-reply@saildocs.com")
                    {
                        this.type = QueryType.saildocs;
                        return true;
                    }
                }
                if (m != null && m.From != null && m.From.Address == "query-reply@saildocs.com")
                {
                    this.type = QueryType.saildocsanser;
                    return true;
                }
                if (m != null && m.Subject.ToLower().StartsWith("forward:"))
                {
                    this.type = QueryType.forward;
                    return true;
                }
                if (m != null && String.Equals(m.Subject, "help"))
                {
                    this.type = QueryType.help;
                    return true;
                }
                if(m != null && m.Subject.StartsWith("raceinfo:"))
                {
                    this.type = QueryType.raceinfo;
                    return true;
                }
                if (m != null && m.Subject.StartsWith("sectioninfo:"))
                {
                    this.type = QueryType.sectioninfo;
                    return true;
                }
                if (m != null && m.Subject.StartsWith("nam-conus:"))
                {
                    this.type = QueryType.grib;
                    return true;
                }
                if (m != null && m.Subject.StartsWith("tweet:"))
                {
                    this.type = QueryType.tweet;
                    return true;
                }
                return false;
            }
            catch 
            {
                return false;
            }
        }

        internal string execute()
        {
            if (isValid())
            {
                if (type == QueryType.saildocs)
                {
                    SMTPTools.SendMail("query@saildocs.com", m.From.Address, m.Body.Trim("\r\n".ToCharArray()), false);
                    return m.From.Address + "," + m.Body.Trim("\r\n".ToCharArray());
                }
                else if (type == QueryType.saildocsanser)
                {
                    string recipient = "";
                    string selection = "";
                    if (this.awaiting != null)
                    {
                        foreach (string waiting in awaiting)
                        {
                            string[] split = waiting.Split(new char[] { ',' }, 2);
                            if (split[1].Replace("start ", "").Contains(m.Subject))
                            {
                                recipient = split[0];
                                selection = waiting;
                                break;
                            }
                        }
                        if (!String.IsNullOrEmpty(recipient) && !String.IsNullOrEmpty(selection))
                        {
                            List<string> files = new List<string>();
                            foreach (AE.Net.Mail.Attachment att in m.Attachments)
                            {
                                att.Save(att.Filename);
                                files.Add(att.Filename);
                            }
                            SMTPTools.SendMail(recipient, m.Subject, m.Body, false, files.ToArray());
                            foreach (string file in files)
                                File.Delete(file);
                            awaiting.Remove(selection);
                        }
                    }
                }
                else if (type == QueryType.forward)
                {
                    string[] splitForward = m.Subject.Split(new char[] { ':' }, 3);
                    List<string> filesForward = new List<string>();
                    foreach (AE.Net.Mail.Attachment att in m.Attachments)
                    {
                        att.Save(att.Filename);
                        filesForward.Add(att.Filename);
                    }
                    SMTPTools.SendMail(splitForward[1], "Forwarded from: " + m.From.Address + " => " + splitForward[2], m.Body, true, filesForward.ToArray());
                    foreach (string file in filesForward)
                        File.Delete(file);
                }
                else if (type == QueryType.raceinfo)
                {
                    try
                    {
                        string report = "Race name: ";
                        string[] split = m.Subject.Split(new char[] { ':' }, 2);
                        Race race = YBTracker.getRaceInformation("http://yb.tl", split[1]);
                        report += race.title + "\r\n========================\r\n\r\n";
                        foreach (Tag tag in race.tags)
                        {
                            report += tag.id + " - " + tag.name + "\r\n" + "----------------------\r\n";
                            foreach (Team team in race.teams)
                            {
                                if (team.tags.Contains(tag.id))
                                    report += team.id + " - " + team.name + "\r\n";
                            }
                            report += "\r\n";
                        }
                        SMTPTools.SendMail(m.From.Address, "Race report for " + split[1], report, false, null);
                    }
                    catch
                    { }
                }
                else if (type == QueryType.sectioninfo)
                {
                    try
                    {
                        string[] split = m.Subject.Split(new char[] { ':' }, 4);
                        int sectionId = -1; int referenceTeam = -1;
                        if (Int32.TryParse(split[2], out sectionId)
                            && Int32.TryParse(split[3], out referenceTeam))
                        {
                            Race race = YBTracker.getRaceInformation("http://yb.tl", split[1]);
                            string tagName = "";
                            foreach (Tag t in race.tags)
                            {
                                if (t.id == sectionId)
                                {
                                    tagName = t.name;
                                    break;
                                }
                            }
                            List<Team> teams = YBTracker.getAllPositions("http://yb.tl", split[1]);
                            YBTracker.UpdateTeamsMoments(race.teams, teams);
                            foreach (Team team in race.teams)
                                YBTracker.UpdateMomentsSpeedHeading(team);
                            string report = YBTracker.ReportSelectedTeamsHTML(race, sectionId, referenceTeam);
                            SMTPTools.SendMail(m.From.Address, "Race Positions at " + DateTime.Now.ToString(), report, true, null);
                        }
                    }
                    catch
                    { }
                }
                else if (type == QueryType.help)
                {
                    try
                    {
                        string message = "This service allows you to: \r\n"
                        + "- Request GRIB files from the saildocs.com service\r\n"
                        + "- Forward an email to one or more recipients from this service \r\n"
                        + "- Query a yellowbrick race \r\n"
                        + "- Results for a section of a yellowbrick race \r\n\r\n"
                        + "To request a GRIB file from saildocs service, type in the subject 'saildocs:' only, and in the body your query such as 'send coamps:36N,46N,100W,75W' in order to get the weather for lake michigan\r\n\r\n"
                        + "To request a foreward, type in the subject 'forward:RECIPIENT:SUBJECT' and in the body the body of your message, the message will be delivered to the recipients. If more than one, use comma to separate the email adresses\r\n\r\n"
                        + "To request a yellowbrick race information, put in the subject 'raceinfo:RACE-KEY' where RACE-KEY can be replaced with an existing key\r\n\r\n"
                        + "To request a report for a yellowbrick race section, put in the subject 'sectioninfo:RACE-KEY:SECTION-ID:REFERENCE-TEAM' where race id is a yellowbrick race id, section is a section id, and reference team is the id of the team used as reference for the report.\r\n\r\n"
                        + "To requast a subset of the NAM CONUS weather model, send a message with a subject like this: nam-conus:-90,-84,47,41:0-12/3,15,18-80/6\r\n\r\n";
                        SMTPTools.SendMail(m.From.Address, "Help Response", message);
                    }
                    catch { }
                }
                else if (type == QueryType.grib)
                {
                    try
                    {
                        string[] split = m.Subject.Split(new char[] { ':' }, 2);
                        if (split[0].ToLower().Equals("nam-conus"))
                        {
                            string filename = GRIBTools.DoNam(m.Subject);
                            List<string> filesForward = new List<string>();
                            filesForward.Add(filename);
                            SMTPTools.SendMail(m.From.Address, "NAM-CONUS", "", true, filesForward.ToArray());

                            foreach (string file in filesForward)
                                File.Delete(file);
                        }
                    }
                    catch { }
                }
                else if (type == QueryType.tweet)
                {
                    try 
                    {
                        string[] split = m.Subject.Split(new char[] { ':' }, 2);
                        var newTweet = Tweetinvi.Tweet.CreateTweet(split[1]);
                        newTweet.Publish();
                    }
                    catch { }
                }
            }
            return "";
        }

        public override string ToString()
        {
            return type.ToString();
        }
    
    }
}
