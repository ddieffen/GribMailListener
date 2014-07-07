using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MailListenter
{
    internal class Query
    {
        internal enum QueryType { saildocs, saildocsanser, forward, help };

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
                if (m != null && m.Subject.ToLower().Contains("saildocs query"))
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
                if (m != null && m.Subject.ToLower().Contains("forward-to:"))
                {
                    this.type = QueryType.forward;
                    return true;
                }
                if (m != null && String.Equals(m.Subject, "help"))
                {
                    this.type = QueryType.help;
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
                    case QueryType.help:
                        string message = "This service allows you to: \r\n"
                        + "- Request GRIB files from the saildocs.com service\r\n"
                        + "- Forward an email to one or more recipients from this service \r\n\r\n"
                        + "To request a GRIB file, type in the subject 'saildocs query' only, and in the body your query such as 'send coamps:36N,46N,100W,75W' in order to get the weather for lake michigan\r\n\r\n"
                        + "To request a foreard, type in the subject 'forward-to:RECIPIENT:SUBJECT' and in the body the body of your message, the message will be delivered to the recipients. If more than one, use comma to separate the email adresses";
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
