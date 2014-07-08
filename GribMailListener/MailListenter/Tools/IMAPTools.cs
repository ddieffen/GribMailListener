﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AE.Net.Mail;

namespace MailListenter
{
    internal static class IMAPTools
    {
        internal static void TryIMAP()
        {
            bool haveInfo = false;
            while (!haveInfo)
            {
                if(!String.IsNullOrEmpty(MailListenter.Properties.Settings.Default.imapserver)
                    && !String.IsNullOrEmpty(MailListenter.Properties.Settings.Default.imapport.ToString())
                    && !String.IsNullOrEmpty(MailListenter.Properties.Settings.Default.imapuser)
                    && !String.IsNullOrEmpty(MailListenter.Properties.Settings.Default.imappassword))
                {
                    haveInfo = true;
                }
                else
                {
                    Console.WriteLine("Invalid IMAP configuration, please re-enter information");
                    Console.WriteLine("Please enter imap server address (example: imap.gmail.com):");
                    MailListenter.Properties.Settings.Default.imapserver = Console.ReadLine();
                    Console.WriteLine("Please enter imap server port (example: 993):");
                    MailListenter.Properties.Settings.Default.imapport = Convert.ToInt32(Console.ReadLine());
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
                    MailListenter.Properties.Settings.Default.imappassword = SecurityTools.EncryptString(SecurityTools.ToSecureString(pass));
                    MailListenter.Properties.Settings.Default.Save();
                }
            }
        }
    }
}