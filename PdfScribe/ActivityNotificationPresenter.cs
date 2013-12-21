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
        public ActivityNotificationPresenter()
        {
            progressTimer = new SysTimers.Timer();
            progressTimer.Enabled = false;
            progressTimer.Interval = 250; // Quarter second is default
            progressTimer.Elapsed += new SysTimers.ElapsedEventHandler(progressTimer_Elapsed);
        }


        
        private Application activityWindowApp = null;
        private ActivityNotification activityWindow;
        private SysTimers.Timer progressTimer;
        readonly String progressString = "CAPTURING";

        /// <summary>
        /// Displays the floating frameless
        /// activity notification window on
        /// a separate thread
        /// </summary>
        public void ShowActivityNotificationWindow()
        {
            if (this.activityWindowApp == null)
            {
                var activityWindowThread = new Thread(new ThreadStart(() =>
                {
                    activityWindowApp = new Application();
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

        /// <summary>
        /// Shuts down the WPF Application showing
        /// the ActivityNotification window
        /// </summary>
        public void CloseActivityNotificationWindow()
        {
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
        }


        private int progressCounter = 0;
        private void progressTimer_Elapsed(object sender, SysTimers.ElapsedEventArgs e)
        {
            ((SysTimers.Timer)sender).Enabled = false;
            activityWindowApp.Dispatcher.Invoke((Action)delegate()
                        {
                            if (this.progressCounter >= progressString.Length)
                                this.progressCounter = 0;
                            activityWindow.labelProgress.Content = progressString.Substring(0, progressCounter + 1);
                            progressCounter++;
                        }
            );
            ((SysTimers.Timer)sender).Enabled = true;
        }

    }
}
