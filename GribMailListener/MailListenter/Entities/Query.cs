using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using YellowbrickV6;
using YellowbrickV6.Entities;
using System.Net;
using Tweetinvi;
using Tweetinvi.Controllers;
using Tweetinvi.Core;
using Tweetinvi.Credentials;
using Tweetinvi.Factories;
using Tweetinvi.Injectinvi;
using Tweetinvi.Json;
using Tweetinvi.Logic;
using Tweetinvi.Security;
using Tweetinvi.Streams;
using Tweetinvi.WebLogic;

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
                if (m != null && m.Subject.StartsWith("tweet:"))
                {
                    this.type = QueryType.tweet;
                    return true;
                }
                if (m != null)
                {
                    bool test = false;
                    foreach (String modelName in GRIBTools.KnownModels)
                        test = test || m.Subject.StartsWith(modelName);
                    if (test)
                    {
                        this.type = QueryType.grib;
                        return true;
                    }
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
                        + "- Request GRIB files \r\n"
                        + "- Forward an email to one or more recipients from this service \r\n"
                        + "- Query a yellowbrick race \r\n"
                        + "- Results for a section of a yellowbrick race \r\n\r\n"
                        + "Weather models supported : " + GRIBTools.KnownModels.Aggregate((i, j) => i + ", " + j)
                        + "To requast a GRIB file for any weather weather model, send a message with a subject like this    nam-conus:-90,-84,47,41:0-12/3,15,18-80/6\r\n\r\n"
                        + "Where: \r\n"
                        + "- nam-comus is the desired model\r\n"
                        + "- -90,-84,47,41 is a rectangle box delimiting the desired resion"
                        + "- 0-12/3,15,18-80/6 represents the desired times, from +0 to +12 hours every 3 hours, then +15, then from +18 to +80 every 6 hours"
                        + "To request a foreward, type in the subject 'forward:RECIPIENT:SUBJECT' and in the body the body of your message, the message will be delivered to the recipients. If more than one, use comma to separate the email adresses\r\n\r\n"
                        + "To request a yellowbrick race information, put in the subject 'raceinfo:RACE-KEY' where RACE-KEY can be replaced with an existing key\r\n\r\n"
                        + "To request a report for a yellowbrick race section, put in the subject 'sectioninfo:RACE-KEY:SECTION-ID:REFERENCE-TEAM' where race id is a yellowbrick race id, section is a section id, and reference team is the id of the team used as reference for the report.\r\n\r\n";
                        SMTPTools.SendMail(m.From.Address, "Help Response", message);
                    }
                    catch { }
                }
                else if (type == QueryType.grib)
                {
                    try
                    {
                        string[] split = m.Subject.Split(new char[] { ':' }, 2);
                        if (GRIBTools.KnownModels.Contains(split[0].ToLower()))
                        {
                            string filename = GRIBTools.DoFilterGrib(m.Subject);
                            List<string> filesForward = new List<string>();
                            filesForward.Add(filename);
                            SMTPTools.SendMail(m.From.Address, split[0].ToLower(), "", true, filesForward.ToArray());

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
                        if (m.Sender.Address == "teamsorcerer@gmail.com")
                            TwitterCredentials.SetCredentials("2654832530-Ryen50pE0Jy3yTXwU5Fm7P09Ur5C5AkWsAkT5ZK", "kdXzccCnDA8S71aKfxMukk8EfUpJpaKbjHs8XSS35xe1J", "amdg77KQolD6GdbhXpsIeGnRZ", "wWTpjJ1hSWTIivRMYgSu2qBpEVwtFk7oQleDNkivFZmV5gZwAA");
                        else
                            return "We do not have tweeter parameter for that email";
                        var newTweet = Tweetinvi.Tweet.CreateTweet(split[1]);
                        foreach(AE.Net.Mail.Attachment att in m.Attachments){
                            newTweet.AddMedia(att.GetData());
                        }
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
