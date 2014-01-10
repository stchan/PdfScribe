using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using PdfScribeCore;

namespace PdfScribe
{
    public class Program
    {


        #region Message constants
        
        const string errorDialogCaption = "PDF Scribe"; // Error taskdialog caption text
        
        const string errorDialogInstructionPDFGeneration = "There was a PDF generation error.";
        const string errorDialogInstructionCouldNotWrite = "Could not create the output file.";
        const string errorDialogInstructionUnexpectedError = "There was an unhandled error in PDF Scribe. Enable tracing for details.";

        const string errorDialogTextFileInUse = "{0} is being used by another process.";
        const string errorDialogTextGhostScriptConversion = "Ghostscript error code {0}.";

        const string warnFileNotDeleted = "{0} could not be deleted.";

        #endregion

        #region Other constants
        const string traceSourceName = "PdfScribe";

        const string defaultOutputFilename = "OAISISSOFTSCAN.PDF";

        #endregion

        static Application guiApplication = null;
        static Dispatcher guiDispatcher = null;
        static ActivityNotificationPresenter activityNotification = null;
        static TraceSource logEventSource = new TraceSource(traceSourceName);

        [STAThread]
        static void Main(string[] args)
        {
            // Install the global exception handler
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Application_UnhandledException);

            //Dispatcher.Run();
            LaunchApplication();
            //LaunchActivityNotification();

            //Thread.Sleep(10000);

            String standardInputFilename = Path.GetTempFileName();
            String outputFilename = Path.Combine(Path.GetTempPath(), defaultOutputFilename);

            // Only set absolute minimum parameters, let the postscript input
            // dictate as much as possible
            String[] ghostScriptArguments = { "-dBATCH", "-dNOPAUSE", "-dSAFER",  "-sDEVICE=pdfwrite",
                                              String.Format("-sOutputFile={0}", outputFilename), standardInputFilename };

            try
            {
                // Remove the existing OAISISSOFTSCAN.PDF file if present
                File.Delete(outputFilename);

                using (BinaryReader standardInputReader = new BinaryReader(Console.OpenStandardInput()))
                {
                    using (FileStream standardInputFile = new FileStream(standardInputFilename, FileMode.Create, FileAccess.ReadWrite))
                    {
                        standardInputReader.BaseStream.CopyTo(standardInputFile);
                    }
                }
                GhostScript64.CallAPI(ghostScriptArguments);
            }
            catch (IOException ioEx)
            {
                // We couldn't delete, or create a file
                // because it was in use
                logEventSource.TraceEvent(TraceEventType.Error, 
                                          (int)TraceEventType.Error,
                                          errorDialogInstructionCouldNotWrite +
                                          Environment.NewLine +
                                          "Exception message: " + ioEx.Message);
                /*ErrorDialogPresenter errorDialog = new ErrorDialogPresenter(errorDialogCaption,
                                                                            errorDialogInstructionCouldNotWrite,
                                                                            String.Format("{0} is in use.", outputFilename));*/

                DispatchToGUIApp(
                    (Action)delegate()
                        {
                            ErrorDialogPresenter errorDialog = new ErrorDialogPresenter(errorDialogCaption,
                                                                                        errorDialogInstructionCouldNotWrite,
                                                                                        String.Format("{0} is in use.", outputFilename));
                        }
                    );
            }
            catch (UnauthorizedAccessException unauthorizedEx)
            {
                // Couldn't delete a file
                // because it was set to readonly
                // or couldn't create a file
                // because of permissions issues
                logEventSource.TraceEvent(TraceEventType.Error, 
                                          (int)TraceEventType.Error, 
                                          errorDialogInstructionCouldNotWrite +
                                          Environment.NewLine +
                                          "Exception message: " + unauthorizedEx.Message);
                ErrorDialogPresenter errorDialog = new ErrorDialogPresenter(errorDialogCaption,
                                                                            errorDialogInstructionCouldNotWrite,
                                                                            String.Format("Insufficient privileges to either create or delete {0}", outputFilename));
            }
            catch (ExternalException ghostscriptEx)
            {
                // Ghostscript error
                logEventSource.TraceEvent(TraceEventType.Error, 
                                          (int)TraceEventType.Error, 
                                          String.Format(errorDialogTextGhostScriptConversion, ghostscriptEx.ErrorCode.ToString()) +
                                          Environment.NewLine +
                                          "Exception message: " + ghostscriptEx.Message);
                ErrorDialogPresenter errorDialog = new ErrorDialogPresenter(errorDialogCaption,
                                                                            errorDialogInstructionPDFGeneration,
                                                                            String.Format(errorDialogTextGhostScriptConversion, ghostscriptEx.ErrorCode.ToString()));

            }
            finally
            {
                try
                {
                    File.Delete(standardInputFilename);
                }
                catch 
                {
                    logEventSource.TraceEvent(TraceEventType.Warning,
                                              (int)TraceEventType.Warning,
                                              String.Format(warnFileNotDeleted, standardInputFilename));
                }
                //ShutdownApplication();
            }
        }

        /// <summary>
        /// http://stackoverflow.com/questions/8047610/re-open-wpf-window-from-a-console-application
        /// 
        /// All unhandled exceptions will bubble their way up here -
        /// a final error dialog will be displayed before the crash and burn
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Application_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            logEventSource.TraceEvent(TraceEventType.Critical,
                                      (int)TraceEventType.Critical,
                                      ((Exception)e.ExceptionObject).Message + Environment.NewLine +
                                                                        ((Exception)e.ExceptionObject).StackTrace);
            ErrorDialogPresenter errorDialog = new ErrorDialogPresenter(errorDialogCaption,
                                                                        errorDialogInstructionUnexpectedError,
                                                                        String.Empty);
        }

        static void LaunchActivityNotification()
        {
            if (guiApplication != null)
            {
                DispatchToGUIApp((Action)delegate()
                    {
                        ActivityNotificationPresenter notificationPresenter = new ActivityNotificationPresenter();
                        notificationPresenter.ShowActivityNotificationWindow();
                        System.Windows.Threading.Dispatcher.Run();
                    }
                );
            }
        }

        static void LaunchApplication()
        {

            if (guiApplication == null)
            {
                var guiApplicationThread = new Thread(new ThreadStart(() =>
                                {
                                    guiApplication = new Application();
                                    guiApplication.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                                    activityNotification = new ActivityNotificationPresenter();
                                    activityNotification.ShowActivityNotificationWindow();
                                    guiApplication.Run();
                                }
                            ));
                guiApplicationThread.SetApartmentState(ApartmentState.STA);
                guiApplicationThread.IsBackground = true;
                guiApplicationThread.Start();
            }
        }

        static void DispatchToGUIApp(Action guiAction)
        {
            if (guiApplication != null)
            {
                if (guiApplication.Dispatcher.Thread != System.Windows.Threading.Dispatcher.CurrentDispatcher.Thread)
                {
                    guiApplication.Dispatcher.Invoke(guiAction);
                }
                else
                {
                    guiAction.Invoke();
                }
            }
        }

        static void ShutdownApplication()
        {
            if (guiApplication != null)
            {
                if (guiApplication.Windows != null && guiApplication.Windows.Count > 0)
                {
                    foreach (Window appWindow in guiApplication.Windows)
                    {
                        appWindow.Close();
                    }
                }
                guiApplication.Shutdown();
                guiApplication = null;
                /*
                guiApplication.Dispatcher.Invoke((Action)delegate()
                    {
                        if (guiApplication.Windows != null && guiApplication.Windows.Count > 0)
                        {
                            foreach (Window appWindow in guiApplication.Windows)
                            {
                                appWindow.Close();
                            }
                        }
                        guiApplication.Shutdown();
                        //guiApplication.Dispatcher.InvokeShutdown();

                    }
                );
                */
                //guiApplication.Dispatcher.BeginInvokeShutdown(System.Windows.Threading.DispatcherPriority.Send);
                //guiApplication.Shutdown();
                //guiApplication = null;
            }
        }
    }
}
