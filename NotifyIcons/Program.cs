using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using NDesk.Options;

namespace NotifyIcons
{

    class Program
    {

        static int Main(string[] args)
        {
            string exeName = null;
            int preference = 2;
            bool list = false;
            bool show_help = false;

            var options = new OptionSet()
            {
                {"n|name=", "Exec name",  
                    v => exeName = v},
                {"p=", "Change preference to. Default 2. \n0: Only show notifications. \n1: Hide icon and notifications. \n2: Show icon and notifications.", 
                    (int v) => {if(v>=0 && v <=2) preference = v;}},
                {"l", "List all notify items with preference.", 
                    v => list = v != null},
                { "h|help",  "show this message and exit", 
                    v => show_help = v != null },
            };

            List<string> extra;
            try
            {
                extra = options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("NotifyIcons: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `NotifyIcons --help' for more information.");
                return 1;
            }

            if (show_help)
            {
                ShowHelp(options);
                return 0;
            }

            if (list)
            {
                bool grep = !string.IsNullOrWhiteSpace(exeName);
                RegistryKey myKey = Registry.CurrentUser.OpenSubKey(TrayHelper.trayNotifyPath, false);
                byte[] all = (byte[])myKey.GetValue("IconStreams");
                NotifyItem[] items = TrayHelper.getNotifyItems(all);
                int count = 0;

                if (grep)
                {
                    foreach (NotifyItem item in items)
                    {
                        if (item.ExePath.IndexOf(exeName, StringComparison.OrdinalIgnoreCase) > 0)
                        {
                            Console.WriteLine(string.Format("{0} preferenc: {1}", item.ExePath, item.preference));
                            ++count;
                        }
                    }
                }
                else
                {
                    foreach (NotifyItem item in items)
                    {
                        Console.WriteLine(string.Format("{0} preferenc: {1}", item.ExePath, item.preference));
                        ++count;
                    }
                }
                Console.WriteLine();
                Console.WriteLine("Count: " + count.ToString());

                return 0;
            }

            if (!string.IsNullOrWhiteSpace(exeName))
            {
                RegistryKey myKey = Registry.CurrentUser.OpenSubKey(TrayHelper.trayNotifyPath, true);
                byte[] all = (byte[])myKey.GetValue("IconStreams");
                NotifyItem[] items = TrayHelper.getNotifyItems(all);
                bool rewrite = false;
                int count = 0;

                List<int> changes = new List<int>();
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i].ExePath.IndexOf(exeName, StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        changes.Add(i);
                    }
                }
                foreach (int change in changes)
                {

                    int offset = TrayHelper.getOffSet(change);
                    if (items[change].preference != preference)
                    {
                        byte[] bb = BitConverter.GetBytes((int)(preference));
                        Buffer.BlockCopy(bb, 0, all, offset, 4);
                        rewrite = true;
                        ++count;
                    }
                }
                if (rewrite)
                {
                    myKey.SetValue("IconStreams", all);
                }

                Console.WriteLine("Change items: " + count.ToString());
                return 0;
            }

            return 0;
        }

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Change notify icons visible status.");
            Console.WriteLine("Usage: NotifyIcons [OPTIONS]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}
