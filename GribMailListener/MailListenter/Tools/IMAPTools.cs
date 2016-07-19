using System;


namespace MailListenter
{
    internal static class IMAPTools
    {
        internal static void TryIMAP()
        {
            bool haveInfo = false;
            while (!haveInfo)
            {
                if(!String.IsNullOrEmpty(Properties.Settings.Default.imapserver)
                    && !String.IsNullOrEmpty(Properties.Settings.Default.imapport.ToString())
                    && !String.IsNullOrEmpty(Properties.Settings.Default.imapuser)
                    && !String.IsNullOrEmpty(Properties.Settings.Default.imappassword))
                {
                    haveInfo = true;
                }
                else
                {
                    Console.WriteLine("Invalid IMAP configuration, please re-enter information");
                    Console.WriteLine("Please enter imap server address (example: imap.gmail.com):");
                    Properties.Settings.Default.imapserver = Console.ReadLine();
                    Console.WriteLine("Please enter imap server port (example: 993):");
                    Properties.Settings.Default.imapport = Convert.ToInt32(Console.ReadLine());
                    Console.WriteLine("Please enter user name (example: name@gmail.com):");
                    Properties.Settings.Default.imapuser = Console.ReadLine();
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
                    Properties.Settings.Default.imappassword = SecurityTools.EncryptString(SecurityTools.ToSecureString(pass));
                    Properties.Settings.Default.Save();
                }
            }
        }
    }
}
