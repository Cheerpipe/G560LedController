
using Microsoft.Win32;
using G560Led;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace G560LedListener
{
    // For use with Aurora Project Scripted Device or any other program using NamedPipes
    // https://github.com/antonpup/Aurora 

    internal static class Program
    {
        internal static Mutex _singleInstanceMutex;
        static private int devIndex;
        private static List<Listener> _listeners = new List<Listener>();
        static void Main(string[] args)
        {
            if (args.Contains("--ngen"))
            {
                Util.ngen(Util.NgenOperation.Install);
                return;
            }
            if (args.Contains("--unngen"))
            {
                Util.ngen(Util.NgenOperation.Uninstall);
                return;
            }

            Util.SetPriorityProcessAndThreads(Process.GetCurrentProcess().ProcessName, ProcessPriorityClass.AboveNormal, ThreadPriorityLevel.AboveNormal);

            _singleInstanceMutex = new Mutex(true, "{7073739c-532d-5b06-bde3-19732814ee74}-" + devIndex);

            //No more than one instance runing
            if (!_singleInstanceMutex.WaitOne(TimeSpan.Zero, true))
                return;



            G560LedControllerLoader.InitDevice(2).Wait();

            if (G560LedControllerLoader.Devices.Length == 0)
            {
                throw new Exception("No G560 device found");
            }

            Listener _listener = new Listener(G560LedControllerLoader.Devices[2]);
            _listeners.Add(_listener);

            if (args.Contains("--shutdown"))
            {
                G560LedControllerLoader.Devices[devIndex].SetColor(Color.FromArgb(255, 0, 0, 0), 0);
                G560LedControllerLoader.Devices[devIndex].SetColor(Color.FromArgb(255, 0, 0, 0), 1);
                G560LedControllerLoader.Devices[devIndex].SetColor(Color.FromArgb(255, 0, 0, 0), 2);
                G560LedControllerLoader.Devices[devIndex].SetColor(Color.FromArgb(255, 0, 0, 0), 3);

            }
            else
            {
                SystemEvents.SessionEnding += SystemEvents_SessionEnding;
                SystemEvents.SessionEnded += SystemEvents_SessionEnded;
                Application.Run();
            }
        }

        private static void SystemEvents_SessionEnded(object sender, SessionEndedEventArgs e)
        {
            foreach (Listener l in _listeners)
            {
                l.Setter(new byte[] { 0, 0, 0 });
            }
        }

        private static void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            foreach (Listener l in _listeners)
            {
                l.Setter(new byte[] { 0, 0, 0 });
            }
        }
    }
}
