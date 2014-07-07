using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AE.Net.Mail;
using System.Threading;
using System.Security;
using System.Runtime.InteropServices;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            // Connect to the IMAP server. The 'true' parameter specifies to use SSL
            // which is important (for Gmail at least)
            ImapClient ic = TryConnect();
            if (ic == null)
            {
                Console.WriteLine("Can't connect, exiting...");
                Thread.Sleep(1000);
                return;
            }

            // Select a mailbox. Case-insensitive
            ic.SelectMailbox("INBOX");

            int count = ic.GetMessageCount();

            // Get the first *11* messages. 0 is the first message;
            // and it also includes the 10th message, which is really the eleventh ;)
            // MailMessage represents, well, a message in your mailbox
            MailMessage[] mm = ic.GetMessages(MailListenter.Properties.Settings.Default.lastfetchuid, "*", true);

            foreach (MailMessage m in mm)
            {
                Console.WriteLine(m.Subject);
                MailListenter.Properties.Settings.Default.lastfetchuid = m.Uid;
            }

            // Probably wiser to use a using statement
            ic.Dispose();
            Console.ReadLine();
        }

        static ImapClient TryConnect()
        {
            ImapClient ic = null;
            int errorsCount = 0;
            while (ic == null && errorsCount < 3)
            {
                try
                {
                    ic = new ImapClient(
                        MailListenter.Properties.Settings.Default.imapserver, 
                        MailListenter.Properties.Settings.Default.imapuser,
                        ToInsecureString(DecryptString(MailListenter.Properties.Settings.Default.password)),
                        AuthMethods.Login,
                        MailListenter.Properties.Settings.Default.port,
                        true);
                    Console.WriteLine("Connection to IMAP server Succeeded!");
                    return ic;
                }
                catch
                {
                    errorsCount++;
                    Console.WriteLine("Invalid imap configuration, please re-enter information");
                    Console.WriteLine("Please enter imap server address (example: imap.gmail.com):");
                    MailListenter.Properties.Settings.Default.imapserver = Console.ReadLine();
                    Console.WriteLine("Please enter imap server port (example: 993):");
                    MailListenter.Properties.Settings.Default.port = Convert.ToInt32(Console.ReadLine());
                    Console.WriteLine("Please enter user name (example: name@gmail.com):");
                    MailListenter.Properties.Settings.Default.imapuser = Console.ReadLine();
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
                    MailListenter.Properties.Settings.Default.password = EncryptString(ToSecureString(pass));
                    MailListenter.Properties.Settings.Default.Save();
                }
            }
            return null;
        }

        //http://msdn.microsoft.com/en-us/library/system.security.cryptography.dataprotectionscope(v=vs.100).aspx
        static byte[] entropy = System.Text.Encoding.Unicode.GetBytes("Just some salty entropy");
        public static string EncryptString(System.Security.SecureString input)
        {
            byte[] encryptedData = System.Security.Cryptography.ProtectedData.Protect(
                System.Text.Encoding.Unicode.GetBytes(ToInsecureString(input)),
                entropy,
                System.Security.Cryptography.DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedData);
        }
        public static SecureString DecryptString(string encryptedData)
        {
            try
            {
                byte[] decryptedData = System.Security.Cryptography.ProtectedData.Unprotect(
                    Convert.FromBase64String(encryptedData),
                    entropy,
                    System.Security.Cryptography.DataProtectionScope.CurrentUser);
                return ToSecureString(System.Text.Encoding.Unicode.GetString(decryptedData));
            }
            catch
            {
                return new SecureString();
            }
        }
        public static SecureString ToSecureString(string input)
        {
            SecureString secure = new SecureString();
            foreach (char c in input)
            {
                secure.AppendChar(c);
            }
            secure.MakeReadOnly();
            return secure;
        }
        public static string ToInsecureString(SecureString input)
        {
            string returnValue = string.Empty;
            IntPtr ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(input);
            try
            {
                returnValue = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);
            }
            return returnValue;
        }
    }
}
