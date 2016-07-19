using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using YellowbrickV8;
using YellowbrickV8.Entities;
using Tweetinvi;
using Tweetinvi.Parameters;
using Tweetinvi.Models;

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
            m = message;
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
                        type = QueryType.saildocs;
                        return true;
                    }
                }
                if (m != null && m.From != null && m.From.Address == "query-reply@saildocs.com")
                {
                    type = QueryType.saildocsanser;
                    return true;
                }
                if (m != null && m.Subject.ToLower().StartsWith("forward:"))
                {
                    type = QueryType.forward;
                    return true;
                }
                if (m != null && string.Equals(m.Subject, "help"))
                {
                    type = QueryType.help;
                    return true;
                }
                if(m != null && m.Subject.StartsWith("raceinfo:"))
                {
                    type = QueryType.raceinfo;
                    return true;
                }
                if (m != null && m.Subject.StartsWith("sectioninfo:"))
                {
                    type = QueryType.sectioninfo;
                    return true;
                }
                if (m != null && m.Subject.StartsWith("tweet:"))
                {
                    type = QueryType.tweet;
                    return true;
                }
                if (m != null)
                {
                    bool test = false;
                    foreach (String modelName in GRIBTools.KnownModels)
                        test = test || m.Subject.StartsWith(modelName);
                    if (test)
                    {
                        type = QueryType.grib;
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
                    if (awaiting != null)
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
                        report += race.title + "\r\n========================"+Environment.NewLine+Environment.NewLine;
                        foreach (Tag tag in race.tags)
                        {
                            report += tag.id + " - " + tag.name +Environment.NewLine + "----------------------"+Environment.NewLine;
                            foreach (Team team in race.teams)
                            {
                                if (team.tags.Contains(tag.id))
                                    report += team.id + " - " + team.name +Environment.NewLine;
                            }
                            report += ""+Environment.NewLine;
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
                        if (int.TryParse(split[2], out sectionId)
                            && int.TryParse(split[3], out referenceTeam))
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
                        string message = "This service allows you to: " + Environment.NewLine
                        + "- Request GRIB files " + Environment.NewLine
                        + "- Forward an email to one or more recipients from this service " + Environment.NewLine
                        + "- Query a yellowbrick race " + Environment.NewLine
                        + "- Results for a section of a yellowbrick race " + Environment.NewLine
                        + "- Send tweet messages" + Environment.NewLine + Environment.NewLine
                        + "Weather models supported : " + GRIBTools.KnownModels.Aggregate((i, j) => i + ", " + j)
                        + "To request a GRIB file from the NOAA servers for any weather weather model, send a message with a subject like this nam-conus:-90,-84,47,41:0-12/3,15,18-80/6" + Environment.NewLine + Environment.NewLine
                        + "Where: " + Environment.NewLine
                        + "- nam-comus is the desired model" + Environment.NewLine
                        + "- -90,-84,47,41 is a rectangle box delimiting the desired region"
                        + "- 0-12/3,15,18-80/6 represents the desired times, from +0 to +12 hours every 3 hours, then +15, then from +18 to +80 every 6 hours" + Environment.NewLine + Environment.NewLine
                        + "To request a GRIB file from the SAILDOCS server, send a message with a the subject containing saildocs: and the body of the message containing model and box coordinates like this send coamps:36N,46N,100W,75W" + Environment.NewLine + Environment.NewLine
                        + "To request a foreward, type in the subject 'forward:RECIPIENT:SUBJECT' and in the body the body of your message, the message will be delivered to the recipients. If more than one, use comma to separate the email adresses" + Environment.NewLine + Environment.NewLine
                        + "To request a yellowbrick race information, put in the subject 'raceinfo:RACE-KEY' where RACE-KEY can be replaced with an existing key" + Environment.NewLine + Environment.NewLine
                        + "To request a report for a yellowbrick race section, put in the subject 'sectioninfo:RACE-KEY:SECTION-ID:REFERENCE-TEAM' where race id is a yellowbrick race id, section is a section id, and reference team is the id of the team used as reference for the report." + Environment.NewLine + Environment.NewLine
                        + "To send a tweet, enter in the subject tweet: followed by the tweet you want to post. For example tweet:Hello World! in the subject" + Environment.NewLine + "Attach an image to the email in order to post an image with the tweet";
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
                            List<string> errorAndWarnings = new List<string>();
                            string filename = GRIBTools.DoFilterGrib(m.Subject, out errorAndWarnings);
                            FileInfo fi = new FileInfo(filename);
                            string bodyMessage = "OK";
                            if(errorAndWarnings.Count>0)
                                bodyMessage = errorAndWarnings.Aggregate((i, j) => i + Environment.NewLine + j);
                            List<string> filesForward = new List<string>();
                            filesForward.Add(filename);

                            SMTPTools.SendMail(m.From.Address, "GRIB Requested: " + m.Subject, bodyMessage, true, filesForward.ToArray());

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
                        string consumerKey = "amdg77KQolD6GdbhXpsIeGnRZ";
                        string consumerSectet = "wWTpjJ1hSWTIivRMYgSu2qBpEVwtFk7oQleDNkivFZmV5gZwAA";
                        string accessToken = "2654832530-Ryen50pE0Jy3yTXwU5Fm7P09Ur5C5AkWsAkT5ZK";
                        string accessTokenSecret = "kdXzccCnDA8S71aKfxMukk8EfUpJpaKbjHs8XSS35xe1J";

                        string[] split = m.Subject.Split(new char[] { ':' }, 2);
                        ITwitterCredentials credentials;
                        if (m.From.Address == "teamsorcerer@mailasail.com" 
                            || m.From.Address == "chicago.beercanracing@gmail.com"
                            || m.From.Address == "lahendo@gmail.com"
                            || m.From.Address == "dondraper1@yahoo.com")
                            credentials = Auth.SetUserCredentials(consumerKey, consumerSectet, accessToken, accessTokenSecret);
                        else
                            return "We do not have tweeter parameters for that email";

                        var user = User.GetAuthenticatedUser(credentials);

                        if (m.Attachments != null && m.Attachments.Count != 0)
                            foreach (AE.Net.Mail.Attachment att in m.Attachments)
                            {
                                var media = Upload.UploadImage(att.GetData());
                                var tweet = Tweet.PublishTweet(split[1], new PublishTweetOptionalParameters { Medias = new List<IMedia> { media } });
                            }
                        else
                        {
                            var newTweet = Tweet.PublishTweet(split[1]);
                        }
                    }
                    catch (Exception e)
                    { }
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
