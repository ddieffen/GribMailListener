using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AE.Net.Mail;
using System.Threading;
using System.Security;
using System.Runtime.InteropServices;
using MailListenter;
using System.Net.Mail;
using System.Net;

namespace MailListenter
{
    class Program
    {
        static void Main(string[] args)
        {
            // Connect to the IMAP server. The 'true' parameter specifies to use SSL
            // which is important (for Gmail at least)
            ImapClient ic = IMAPTools.TryIMAP();
            if (ic == null)
            {
                Console.WriteLine("Can't connect to IMAP Server, exiting...");
                Thread.Sleep(1000);
                return;
            }

            SmtpClient sc = SMTPTools.TrySMTP();
            if (sc == null)
            {
                Console.WriteLine("Can't connect to SMTP Server, exiting...");
                Thread.Sleep(1000);
                return;
            }

            // Select a mailbox. Case-insensitive
            ic.SelectMailbox("INBOX");

            int count = ic.GetMessageCount();
            List<string> awaiting = new List<string>();
            DateTime lastUpdate = new DateTime(1970,1,1);

            while (true)
            {
                double timeLapsed = (DateTime.Now - lastUpdate).TotalSeconds;
                if (timeLapsed > 5)
                {
                    Console.Write("\r" + "Checking inbox...");
                    // Get the first *11* messages. 0 is the first message;
                    // and it also includes the 10th message, which is really the eleventh ;)
                    // MailMessage represents, well, a message in your mailbox
                    AE.Net.Mail.MailMessage[] mm = ic.GetMessages(MailListenter.Properties.Settings.Default.lastfetchuid, "*", false);

                    foreach (AE.Net.Mail.MailMessage m in mm)
                    {
                        if (!m.Uid.Equals(MailListenter.Properties.Settings.Default.lastfetchuid))
                        {
                            Query q = new Query(m, awaiting);
                            if (q.isValid())
                            {
                                string result = q.execute();
                                Console.WriteLine("\r" + DateTime.Now.ToString() + " Executing Request:" + q.ToString());
                                if (result != "")
                                    awaiting.Add(result);
                            }
                        }
                        MailListenter.Properties.Settings.Default.lastfetchuid = m.Uid;
                        lastUpdate = DateTime.Now;
                    }
                    Console.Write("\r" + "                 ");
                }
                else
                {
                    Console.Write("\r" + Math.Floor(5 - timeLapsed).ToString());
                }
                Thread.Sleep(1000);
            }
        }
    }
}
