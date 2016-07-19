using System;
using System.Text;
using System.Net.Mail;
using System.Net;
using System.Net.Mime;
using System.IO;

namespace MailListenter
{
    internal static class SMTPTools
    {
        internal static void SendMail(
            string emailTo,
            string subject,
            string body,
            bool isBodyHTML = false,
            string[] attachements = null)
        {
            bool enableSSL = true;

            if (attachements != null && attachements.Length > 0
                && emailTo.Contains("@mailasail.com"))
                emailTo = emailTo.Replace("@mailasail.com", "+attach@mailasail.com");

            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(Properties.Settings.Default.smtpuser);
                mail.To.Add(emailTo);
                mail.Subject = subject;
                mail.Body = body;
                mail.BodyEncoding = Encoding.UTF8;
                mail.IsBodyHtml = isBodyHTML;

                if (attachements != null)
                {
                    foreach (string file in attachements)
                    {
                        Attachment attachment = new Attachment(file, MediaTypeNames.Application.Octet);
                        ContentDisposition disposition = attachment.ContentDisposition;
                        disposition.CreationDate = File.GetCreationTime(file);
                        disposition.ModificationDate = File.GetLastWriteTime(file);
                        disposition.ReadDate = File.GetLastAccessTime(file);
                        disposition.FileName = Path.GetFileName(file);
                        disposition.Size = new FileInfo(file).Length;
                        disposition.DispositionType = DispositionTypeNames.Attachment;
                        mail.Attachments.Add(attachment);
                    }
                }

                using (SmtpClient smtp = new SmtpClient(Properties.Settings.Default.smtpserver, Properties.Settings.Default.smtpport))
                {
                    smtp.Credentials = new NetworkCredential(Properties.Settings.Default.smtpuser, SecurityTools.ToInsecureString(SecurityTools.DecryptString(Properties.Settings.Default.smtppassword)));
                    smtp.EnableSsl = enableSSL;
                    smtp.Send(mail);
                }
            }
        }

        internal static void TrySMTP()
        {
            bool haveInfo = false;
            while (!haveInfo)
            {
                if (!String.IsNullOrEmpty(Properties.Settings.Default.smtpserver)
                       && !String.IsNullOrEmpty(Properties.Settings.Default.smtpport.ToString())
                       && !String.IsNullOrEmpty(Properties.Settings.Default.smtpuser)
                       && !String.IsNullOrEmpty(Properties.Settings.Default.smtppassword))
                {
                    haveInfo = true;
                }
                else
                {
                    Console.WriteLine("Invalid SMTP configuration, please re-enter information");
                    Console.WriteLine("Please enter SMTP server address (example: smtp.gmail.com):");
                    Properties.Settings.Default.smtpserver = Console.ReadLine();
                    Console.WriteLine("Please enter SMTP server port (example: 587):");
                    Properties.Settings.Default.smtpport = Convert.ToInt32(Console.ReadLine());
                    Console.WriteLine("Please enter user name (example: name@gmail.com):");
                    Properties.Settings.Default.smtpuser = Console.ReadLine();
                    Console.WriteLine("Please enter password:");
                    ConsoleKeyInfo key; String pass = "";
                    do
                    {//https://stackoverflow.com/questions/3404421/password-masking-console-application
                        key = Console.ReadKey(true);

                        // Backspace Should Not Work
                        if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                        {
                            pass += key.KeyChar;
                            Console.Write("*");
                        }
                        else
                        {
                            if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                            {
                                pass = pass.Substring(0, (pass.Length - 1));
                                Console.Write("\b \b");
                            }
                        }
                    }
                    // Stops Receving Keys Once Enter is Pressed
                    while (key.Key != ConsoleKey.Enter);
                    Properties.Settings.Default.smtppassword = SecurityTools.EncryptString(SecurityTools.ToSecureString(pass));
                    Properties.Settings.Default.Save();
                }
            }
        }
    }
}
