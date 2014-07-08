﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using YellowbrickV6;
using YellowbrickV6.Entities;

namespace MailListenter
{
    internal class Query
    {
        internal enum QueryType { saildocs, saildocsanser, forward, help, raceinfo, sectioninfo };

        private AE.Net.Mail.MailMessage m;
        private QueryType type;
        private List<string> awaiting = null;

        /// <summary>
        /// Builds a query from a string
        /// </summary>
        /// <param name="p"></param>
        public Query(AE.Net.Mail.MailMessage message, List<string> awaiting)
        {
            this.m = message;
            this.awaiting = awaiting;
        }

        internal bool isValid()
        {
            try
            {
                if (m != null && m.Subject.ToLower().StartsWith("saildocs:"))
                {
                    if (m.Body.StartsWith("send") && m.Body.Split(':').Length == 2 && m.Body.Split(',').Length == 4
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
                switch (type) 
                {
                    case QueryType.saildocs:
                        SMTPTools.SendMail("query@saildocs.com", m.From.Address, m.Body, false);
                        return m.From.Address + "," + m.Body;
                    case QueryType.saildocsanser:
                        string recipient = "";
                        string selection = "";
                        if (this.awaiting != null)
                        {
                            foreach (string waiting in awaiting)
                            {
                                string[] split = waiting.Split(new char[] {','}, 2);
                                if (split[1].Replace("start ","").Contains(m.Subject))
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
                        break;
                    case QueryType.forward:
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
                        break;
                    case QueryType.raceinfo:
                        try 
                        {
                            string report = "Race name: ";
                            string[] split = m.Subject.Split(new char[]{':'},2);
                            Race race = YBTracker.getRaceInformation("http://yb.tl", split[1]);
                            report += race.title + "\r\n========================\r\n\r\n";
                            foreach (Tag tag in race.tags)
                            {
                                report += tag.id + " - " + tag.name + "\r\n" + "----------------------\r\n";
                                foreach (Team team in race.teams)
                                { 
                                    if(team.tags.Contains(tag.id))
                                        report += team.id + " - " + team.name + "\r\n";
                                }
                                report += "\r\n";
                            }
                            SMTPTools.SendMail(m.From.Address, "Race report for " + split[1], report, false, null);
                        }
                        catch 
                        { }
                        break;
                    case QueryType.sectioninfo:
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
                                SMTPTools.SendMail(m.From.Address, "Section report for " + tagName, report, true, null);
                            }
                        }
                        catch
                        { }
                        break;
                    case QueryType.help:
                        string message = "This service allows you to: \r\n"
                        + "- Request GRIB files from the saildocs.com service\r\n"
                        + "- Forward an email to one or more recipients from this service \r\n"
                        + "- Query a yellowbrick race \r\n"
                        + "- Results for a section of a yellowbrick race \r\n\r\n"
                        + "To request a GRIB file, type in the subject 'saildocs:' only, and in the body your query such as 'send coamps:36N,46N,100W,75W' in order to get the weather for lake michigan\r\n\r\n"
                        + "To request a foreward, type in the subject 'forward:RECIPIENT:SUBJECT' and in the body the body of your message, the message will be delivered to the recipients. If more than one, use comma to separate the email adresses\r\n\r\n"
                        + "To request a yellowbrick race information, put in the subject 'raceinfo:RACE-KEY' where RACE-KEY can be replaced with an existing key\r\n\r\n"
                        + "To request a report for a yellowbrick race section, put in the subject 'sectioninfo:RACE-KEY:SECTION-ID:REFERENCE-TEAM' where race id is a yellowbrick race id, section is a section id, and reference team is the id of the team used as reference for the report.\r\n\r\n";
                        SMTPTools.SendMail(m.From.Address, "Help Response", message);
                        break;
                    default:
                        break;
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