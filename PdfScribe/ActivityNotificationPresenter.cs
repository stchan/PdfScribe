using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SysTimers = System.Timers;
using System.Windows;


namespace PdfScribe
{
    public class ActivityNotificationPresenter
    {
        public ActivityNotificationPresenter(Application guiApplication)
        {
            this.activityWindowApp = guiApplication;
            this.progressTimer = new SysTimers.Timer();
            this.progressTimer.Enabled = false;
            this.progressTimer.Interval = 250; // Quarter second is default
            this.progressTimer.Elapsed += new SysTimers.ElapsedEventHandler(progressTimer_Elapsed);
            this.activityWindow = new ActivityNotification();
            /*
            if (guiApplication.Dispatcher.CheckAccess())
            {
                this.activityWindow = new ActivityNotification();
            }
            else
            {
                guiApplication.Dispatcher.Invoke((Action)delegate()
                                                    {
                                                        this.activityWindow = new ActivityNotification();
                                                        progressTimer = new SysTimers.Timer();
                                                        progressTimer.Enabled = false;
                                                        progressTimer.Interval = 250; // Quarter second is default
                                                        progressTimer.Elapsed += new SysTimers.ElapsedEventHandler(progressTimer_Elapsed);
                                                    }
                                                );
            }
             */ 
        }


        
        private Application activityWindowApp;
        private ActivityNotification activityWindow = null;
        private SysTimers.Timer progressTimer;
        readonly String progressString = "CAPTURING";

        /// <summary>
        /// Displays the floating frameless
        /// activity notification window on
        /// a separate thread
        /// </summary>
        public void ShowActivityNotificationWindow()
        {
            if (this.activityWindow != null)
            {
                if (this.activityWindow.Dispatcher.CheckAccess())
                {
                    this.activityWindow.Show();
                }
                else
                {
                    this.activityWindow.Dispatcher.Invoke((Action)delegate()
                                                                {
                                                                    this.activityWindow.Show();
                                                                }
                                                             );
                }
                this.progressTimer.Start();
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
            this.progressTimer.Enabled = false;
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
        private void progressTimer_Elapsed(object sender, SysTimers.ElapsedEventArgs e)
        {
            ((SysTimers.Timer)sender).Enabled = false;
            if (activityWindow != null)
            {
                if (this.progressCounter >= progressString.Length)
                    this.progressCounter = 0;

                if (activityWindow.labelProgress.Dispatcher.CheckAccess())
                {
                    activityWindow.labelProgress.Content = progressString.Substring(0, progressCounter + 1);
                }
                else
                {
                    activityWindow.labelProgress.Dispatcher.Invoke((Action)delegate()
                                                                        {
                                                                            activityWindow.labelProgress.Content = progressString.Substring(0, progressCounter + 1);
                                                                        }
                                                                    );
                }
                progressCounter++;
                ((SysTimers.Timer)sender).Enabled = true;
            }
        }

    }
}
