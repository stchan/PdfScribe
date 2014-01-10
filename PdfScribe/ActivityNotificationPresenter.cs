using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using SysTimers = System.Timers;
using System.Windows;


namespace PdfScribe
{
    public class ActivityNotificationPresenter
    {

        public ActivityNotificationPresenter()
        {
            this.progressTimer = new SysTimers.Timer();
            this.progressTimer.Enabled = false;
            this.progressTimer.Interval = 250; // Quarter second is default
            this.progressTimer.Elapsed += new SysTimers.ElapsedEventHandler(progressTimer_Elapsed);
            //this.activityWindow = new ActivityNotification();
        }

        
        private Application activityWindowApp;
        private ActivityNotification activityWindow = null;
        private SysTimers.Timer progressTimer;
        private readonly String progressString = "CAPTURING";

        /// <summary>
        /// Displays the floating frameless
        /// activity notification window on
        /// a separate thread
        /// </summary>
        public void ShowActivityNotificationWindow()
        {
            if (this.activityWindow == null)
            {

                if (this.activityWindow.Dispatcher.CheckAccess())
                {
                    this.activityWindow = new ActivityNotification();
                    this.activityWindow.Show();
                    this.progressTimer.Start();
                }
                else
                {
                    this.activityWindow.Dispatcher.Invoke((Action)delegate()
                                                                {
                                                                    this.activityWindow = new ActivityNotification();
                                                                    this.activityWindow.Show();
                                                                    this.progressTimer.Start();
                                                                }
                                                                );
                }
            }
        }
        
        /*
        public void ShowActivityNotificationWindow()
        {
            if (this.activityWindowApp == null)
            {
                activityWindowApp = new Application();
                var activityWindowThread = new Thread(new ThreadStart(() =>
                {
                    activityWindow = new ActivityNotification();
                    activityWindowApp.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                    activityWindowApp.Run(activityWindow);
                }
                ));
                activityWindowThread.SetApartmentState(ApartmentState.STA);
                activityWindowThread.Start();
                this.progressTimer.Enabled = true;
            }
        }
        */

        /// <summary>
        /// Shuts down the WPF Application showing
        /// the ActivityNotification window
        /// </summary>
        public void CloseActivityNotificationWindow()
        {
            //this.progressTimer.Elapsed -= new SysTimers.ElapsedEventHandler(progressTimer_Elapsed);
            this.progressTimer.Stop();
            if (this.activityWindow != null)
            {
                if (this.activityWindow.Dispatcher.CheckAccess())
                {
                    this.activityWindow.Close();
                }
                else
                {
                    this.activityWindow.Dispatcher.Invoke((Action)delegate()
                                                            {
                                                                this.activityWindow.Close();
                                                            }
                    );
                }
                this.activityWindow = null;
            }
            /*
            if (activityWindowApp != null)
            {
                this.progressTimer.Stop();
                // Close windows rather than
                // just bashing the WPF application
                activityWindowApp.Dispatcher.Invoke((Action)delegate()
                        {
                            foreach (Window appWindow in activityWindowApp.Windows)
                            {
                                appWindow.Close();
                            }
                        }
                );
                activityWindowApp.Dispatcher.InvokeShutdown();
                activityWindowApp = null;
                this.progressTimer.Dispose();
                this.progressTimer = null;
            }
             */ 
        }


        private int progressCounter = 0;
        static Object timerElapsedLock = new Object();
        private void progressTimer_Elapsed(object sender, SysTimers.ElapsedEventArgs e)
        {
            ((SysTimers.Timer)sender).Stop();
            if (activityWindow != null)
            {
                if (this.progressCounter >= progressString.Length)
                    this.progressCounter = 0;

                if (activityWindow.labelProgress.Dispatcher.CheckAccess())
                {
                    //EventLog.WriteEntry("PdfScribe", "Timer_No_Invoke");
                    this.activityWindow.labelProgress.Content = this.progressString.Substring(0, progressCounter + 1);
                }
                else
                {
                    //EventLog.WriteEntry("PdfScribe", "Timer_Invoke");
                    this.activityWindow.labelProgress.Dispatcher.Invoke((Action)delegate()
                                                                        {
                                                                            this.activityWindow.labelProgress.Content = this.progressString.Substring(0, progressCounter + 1);
                                                                        }
                                                                    );
                    //EventLog.WriteEntry("PdfScribe", "Timer_Invoked");
                }
                progressCounter++;
            }
            ((SysTimers.Timer)sender).Start();
        }

    }
}
