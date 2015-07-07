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
using Tweetinvi;

namespace MailListenter
{
    class Program
    {
        static void Main(string[] args)
        {
            ImapClient ic = null;
            while (true)
            {
                try
                {
                    // Connect to the IMAP server. The 'true' parameter specifies to use SSL
                    // which is important (for Gmail at least)
                    IMAPTools.TryIMAP();
                    SMTPTools.TrySMTP();

                    ic = new ImapClient(
                               MailListenter.Properties.Settings.Default.imapserver,
                               MailListenter.Properties.Settings.Default.imapuser,
                               SecurityTools.ToInsecureString(SecurityTools.DecryptString(MailListenter.Properties.Settings.Default.imappassword)),
                               AuthMethods.Login,
                               MailListenter.Properties.Settings.Default.imapport,
                               true);

                    ic.SelectMailbox("INBOX");// Select a mailbox. Case-insensitive

                    int count = ic.GetMessageCount();
                    List<string> awaiting = new List<string>();
                    DateTime lastUpdate = new DateTime(1970, 1, 1);

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
                                    if (m != null && m.From != null && m.From.Address != null && (m.From.Address == "teamsorcerer@gmail.com"
                                        || m.From.Address == "mikepanacek@hotmail.com"))
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
                                }
                                MailListenter.Properties.Settings.Default.lastfetchuid = m.Uid;
                                MailListenter.Properties.Settings.Default.Save();
                            }
                           
                            lastUpdate = DateTime.Now;
                            timeLapsed = (DateTime.Now - lastUpdate).TotalSeconds;
                            Console.Write("\r" + "                 ");
                            Console.Write("\r" + Math.Floor(5 - timeLapsed).ToString());
                        }
                        else
                        {
                            Console.Write("\r" + Math.Ceiling(5 - timeLapsed).ToString());
                        }
                        Thread.Sleep(1000);
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(DateTime.Now.ToString() + " Something went wrong: " + e.Message);
                }
                finally
                {
                    if (ic == null)
                        Console.WriteLine("Cannot connect IMAP Client...");
                }
                Thread.Sleep(5000);
            }
        }
    }
}
