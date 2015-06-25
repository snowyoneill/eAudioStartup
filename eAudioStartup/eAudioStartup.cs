using System;
//Namespaces we need to use
using System.Diagnostics;
using System.Reflection;
using Microsoft.Win32;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;

namespace eAudioStartup
{
    class eAudioStartup
    {
        private const String EAUDIOAPPNAME = "eAudio";
        private static NotifyIcon tray;
        
        public delegate bool eAudioSettingsDelegate();

        public bool checkeAudioSettings()
        {
            //MessageBox.Show("I was called by delegate ...");
            //Thread.Sleep(1000);
            //MessageBox.Show("Finished ...");

            bool eAudioRunning = IsProcessOpen();
            DateTime initialTime = DateTime.Now;

            bool messageShown = false;

            while (!eAudioRunning && (DateTime.Now - initialTime).TotalSeconds < 10)
            {
                Thread.Sleep(1000);
                eAudioRunning = IsProcessOpen();
                if ((DateTime.Now - initialTime).TotalSeconds > 3 && messageShown == false)
                {
                    tray.Visible = false;
                    tray.Visible = true;
                    messageShown = true;
                }
            }

            return eAudioRunning;
        }
        

        static void Main(string[] args)
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            new eAudioStartup();
            Application.Run();
        }
 
        // call back method to capture results
        private static void ResultsReturned( IAsyncResult iar )
        {
            eAudioSettingsDelegate eAudioRunning = (eAudioSettingsDelegate) iar.AsyncState;

            // get the return value from that method call
            bool returnValue = eAudioRunning.EndInvoke(iar);

            if (returnValue)
            {
                RegistryKey eAudioKey = Registry.CurrentUser.OpenSubKey(@"Software\acer\eAudio", true);

                string[] valnames = eAudioKey.GetValueNames();
                Int32 val0 = (Int32)eAudioKey.GetValue(valnames[0]);
                String value0 = System.Convert.ToString(eAudioKey.GetValue(valnames[0]));
                Int32 val1 = (Int32)eAudioKey.GetValue(valnames[1]);
                String value1 = System.Convert.ToString(eAudioKey.GetValue(valnames[1]));
                //Int32 val2 = (Int32)eAudioKey.GetValue(valnames[2]);
                //String value2 = System.Convert.ToString(eAudioKey.GetValue(valnames[2]));

                Console.WriteLine(val0);
                Console.WriteLine(val1);

                //Set to default rock EQ.
                eAudioKey.SetValue("EQ", 4);
                eAudioKey.SetValue("AudioMode", 2);

                tray.ShowBalloonTip(1000, "eAudioStartup Manager",
                    "eAudio is now running EQ reset to Rock...",
                    ToolTipIcon.Info);

                Thread.Sleep(2000);
            }
            else
            {
                tray.ShowBalloonTip(1000, "eAudioStartup Manager",
                    "eAudio has not booted...closing",
                    ToolTipIcon.Info);

                Thread.Sleep(2000);
            }

            tray.Visible = false;
            Application.Exit();
        }

        public eAudioStartup()
        {
            eAudioSettingsDelegate eAudioDelegate = new eAudioSettingsDelegate(checkeAudioSettings);
            IAsyncResult result = eAudioDelegate.BeginInvoke(new AsyncCallback(ResultsReturned), eAudioDelegate);

            Assembly a = Assembly.GetExecutingAssembly();
            System.IO.Stream eAudioIcon = a.GetManifestResourceStream("eAudioStartup.eAudio.ico");

            tray = new NotifyIcon();
            tray.Icon = new Icon(eAudioIcon);
            tray.Visible = true;
            tray.ShowBalloonTip(500, "eAudioStartup Manager",
                    "Waiting for eAudio...",
                    ToolTipIcon.Info);

            tray.Click += new EventHandler(delegate
                                              {
                                                  tray.ShowBalloonTip(1000, "eAudioStartup Manager",
                                                                      "eAudioManager running...",
                                                                      ToolTipIcon.Info);
                                              });

            //Thread.Sleep(20000);
        }

        public bool IsProcessOpen()
        {
	        //here we're going to get a list of all running processes on
	        //the computer
	        foreach (Process clsProcess in Process.GetProcesses()) {
		        //now we're going to see if any of the running processes
		        //match the currently running processes. Be sure to not
		        //add the .exe to the name you provide, i.e: NOTEPAD,
		        //not NOTEPAD.EXE or false is always returned even if
		        //notepad is running.
		        //Remember, if you have the process running more than once, 
		        //say IE open 4 times the loop thr way it is now will close all 4,
		        //if you want it to just close the first one it finds
		        //then add a return; after the Kill
		        if (clsProcess.ProcessName.Equals(EAUDIOAPPNAME))
		        {
			        //if the process is found to be running then we
			        //return a true
			        return true;
		        }
	        }
	        //otherwise we return a false
	        return false;
        }
    }
}
